using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating and managing inventory movements (Option A - Movement Architecture).
/// </summary>
/// <remarks>
/// Implementation of Movement Architecture (Option A):
/// - SOH (Stock on Hand) is calculated from movements, NEVER stored directly
/// - SOH = SUM(Quantity WHERE Direction = IN) - SUM(Quantity WHERE Direction = OUT)
/// - Movements are immutable once created (soft delete only)
/// - All movements link back to source TransactionDetail
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Full audit trail for all stock movements via AuditableEntity
/// - Batch/serial number tracking for seed-to-sale traceability
/// - Weight tracking for reconciliation
/// - Movement reasons documented for compliance reporting
///
/// Architecture:
/// - Uses IBusinessDbContext interface to access business data tables
/// - IBusinessDbContext is implemented by PosDbContext in POS.DAL
/// - This avoids circular dependency between Shared.Database and POS.DAL
/// </remarks>
public class MovementService : IMovementService
{
    private readonly IBusinessDbContext _context;
    private readonly ILogger<MovementService> _logger;

    // Configuration constants for retry logic (Phase 9.9)
    private const int MaxRetryAttempts = 3;
    private const int BaseRetryDelayMs = 100;
    private const int PerformanceWarningThresholdMs = 500;
    private const int LargeBatchThreshold = 50;

    /// <summary>
    /// Initializes a new instance of the MovementService.
    /// </summary>
    /// <param name="context">The business database context</param>
    /// <param name="logger">Logger for tracking operations</param>
    /// <exception cref="ArgumentNullException">Thrown when context or logger is null</exception>
    public MovementService(IBusinessDbContext context, ILogger<MovementService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ============================================================
    // MOVEMENT GENERATION
    // ============================================================

    /// <inheritdoc />
    /// <remarks>
    /// Phase 9.9: Optimized with performance profiling and retry logic.
    /// - Uses Stopwatch to measure execution time
    /// - Logs performance warnings for slow operations (>500ms)
    /// - Implements exponential backoff retry on transient failures
    /// - Uses AddRangeAsync for efficient batch insert
    /// </remarks>
    public async Task<int> GenerateMovementsAsync(TransactionType transactionType, int headerId)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Generating movements for {TransactionType} transaction {HeaderId}",
            transactionType, headerId);

        // Skip non-stock-affecting transactions
        if (!IsStockAffectingTransaction(transactionType))
        {
            _logger.LogInformation(
                "Transaction type {TransactionType} does not affect stock. No movements generated.",
                transactionType);
            return 0;
        }

        try
        {
            // Get transaction details for this header
            var detailQueryStart = stopwatch.ElapsedMilliseconds;
            var details = await _context.TransactionDetails
                .Where(d => d.HeaderId == headerId && d.TransactionType == transactionType && !d.IsDeleted)
                .ToListAsync();
            var detailQueryTime = stopwatch.ElapsedMilliseconds - detailQueryStart;

            if (!details.Any())
            {
                _logger.LogWarning(
                    "No transaction details found for {TransactionType} transaction {HeaderId}",
                    transactionType, headerId);
                return 0;
            }

            // Log if we have a large batch
            if (details.Count >= LargeBatchThreshold)
            {
                _logger.LogInformation(
                    "Large batch detected: {Count} items for {TransactionType} transaction {HeaderId}",
                    details.Count, transactionType, headerId);
            }

            // Get movement direction and type name
            var direction = GetMovementDirection(transactionType);
            var movementTypeName = GetMovementTypeName(transactionType);

            // Create movements for each detail (pre-allocate list capacity)
            var movements = new List<Movement>(details.Count);
            var transactionDate = DateTime.UtcNow;

            foreach (var detail in details)
            {
                var movement = new Movement
                {
                    ProductId = detail.ProductId,
                    ProductSKU = detail.ProductSKU,
                    ProductName = detail.ProductName,
                    MovementType = movementTypeName,
                    Direction = direction,
                    Quantity = detail.Quantity,
                    Mass = detail.WeightGrams ?? 0,
                    Value = detail.LineTotal,
                    BatchNumber = detail.BatchNumber,
                    SerialNumber = detail.SerialNumber,
                    TransactionType = transactionType,
                    HeaderId = headerId,
                    DetailId = detail.Id,
                    MovementReason = $"{movementTypeName} transaction #{headerId}",
                    TransactionDate = transactionDate,
                    UserId = detail.CreatedBy // Inherit user from transaction detail
                };

                movements.Add(movement);
            }

            // Save all movements with retry logic
            var saveResult = await SaveMovementsWithRetryAsync(movements, transactionType, headerId);

            stopwatch.Stop();

            // Log performance metrics
            LogPerformanceMetrics(stopwatch.ElapsedMilliseconds, detailQueryTime, movements.Count, transactionType, headerId);

            _logger.LogInformation(
                "Generated {Count} {Direction} movements for {TransactionType} transaction {HeaderId} in {ElapsedMs}ms",
                movements.Count, direction, transactionType, headerId, stopwatch.ElapsedMilliseconds);

            return saveResult;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Failed to generate movements for {TransactionType} transaction {HeaderId} after {ElapsedMs}ms",
                transactionType, headerId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Save movements with exponential backoff retry logic (Phase 9.9).
    /// </summary>
    private async Task<int> SaveMovementsWithRetryAsync(List<Movement> movements, TransactionType transactionType, int headerId)
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                // Use AddRange for efficient batch insert
                _context.Movements.AddRange(movements);
                await _context.SaveChangesAsync();

                if (attempt > 1)
                {
                    _logger.LogInformation(
                        "Movement save succeeded on attempt {Attempt} for {TransactionType} transaction {HeaderId}",
                        attempt, transactionType, headerId);
                }

                return movements.Count;
            }
            catch (DbUpdateException ex) when (IsTransientException(ex) && attempt < MaxRetryAttempts)
            {
                lastException = ex;
                var delayMs = BaseRetryDelayMs * (int)Math.Pow(2, attempt - 1); // Exponential backoff

                _logger.LogWarning(ex,
                    "Transient error on movement save attempt {Attempt}/{MaxAttempts} for {TransactionType} transaction {HeaderId}. Retrying in {DelayMs}ms",
                    attempt, MaxRetryAttempts, transactionType, headerId, delayMs);

                // Detach the entities so they can be re-added on retry
                foreach (var movement in movements)
                {
                    var entry = _context.Movements.Entry(movement);
                    if (entry != null)
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                // Non-transient exception, don't retry
                _logger.LogError(ex,
                    "Non-transient error saving movements for {TransactionType} transaction {HeaderId}",
                    transactionType, headerId);
                throw;
            }
        }

        // All retries exhausted
        _logger.LogError(lastException,
            "All {MaxAttempts} retry attempts exhausted for {TransactionType} transaction {HeaderId}",
            MaxRetryAttempts, transactionType, headerId);
        throw lastException!;
    }

    /// <summary>
    /// Check if an exception is transient and worth retrying.
    /// Supports PostgreSQL (Npgsql) and common database errors.
    /// </summary>
    private static bool IsTransientException(Exception ex)
    {
        // Check the exception type name (to avoid direct assembly reference)
        var exceptionTypeName = ex.InnerException?.GetType().FullName ?? string.Empty;

        // Check for Npgsql (PostgreSQL) transient errors
        if (exceptionTypeName.Contains("Npgsql") || exceptionTypeName.Contains("Postgres"))
        {
            var message = ex.InnerException?.Message?.ToLowerInvariant() ?? string.Empty;
            return message.Contains("connection") ||
                   message.Contains("timeout") ||
                   message.Contains("deadlock") ||
                   message.Contains("serialization failure") ||
                   message.Contains("could not connect") ||
                   message.Contains("broken pipe") ||
                   message.Contains("40001"); // PostgreSQL serialization failure code
        }

        // Check for generic SQL Server errors (by type name, no assembly reference)
        if (exceptionTypeName.Contains("SqlException"))
        {
            var message = ex.InnerException?.Message?.ToLowerInvariant() ?? string.Empty;
            return message.Contains("timeout") ||
                   message.Contains("deadlock") ||
                   message.Contains("connection") ||
                   message.Contains("transport-level error");
        }

        // Check the main exception message for common transient patterns
        var mainMessage = ex.Message?.ToLowerInvariant() ?? string.Empty;
        return mainMessage.Contains("timeout") ||
               mainMessage.Contains("deadlock") ||
               mainMessage.Contains("connection") ||
               mainMessage.Contains("network") ||
               mainMessage.Contains("transient");
    }

    /// <summary>
    /// Log performance metrics and warnings (Phase 9.9).
    /// </summary>
    private void LogPerformanceMetrics(long totalMs, long queryMs, int movementCount, TransactionType transactionType, int headerId)
    {
        var saveMs = totalMs - queryMs;
        var avgPerMovement = movementCount > 0 ? (double)totalMs / movementCount : 0;

        // Log performance warning if slow
        if (totalMs >= PerformanceWarningThresholdMs)
        {
            _logger.LogWarning(
                "PERFORMANCE: Movement generation took {TotalMs}ms for {Count} movements " +
                "(query: {QueryMs}ms, save: {SaveMs}ms, avg: {AvgMs:F2}ms/movement) " +
                "for {TransactionType} transaction {HeaderId}",
                totalMs, movementCount, queryMs, saveMs, avgPerMovement, transactionType, headerId);
        }
        else
        {
            _logger.LogDebug(
                "Performance: Movement generation took {TotalMs}ms for {Count} movements " +
                "(query: {QueryMs}ms, save: {SaveMs}ms, avg: {AvgMs:F2}ms/movement)",
                totalMs, movementCount, queryMs, saveMs, avgPerMovement);
        }
    }

    /// <inheritdoc />
    public async Task<Movement> CreateMovementAsync(Movement movement)
    {
        if (movement == null)
            throw new ArgumentNullException(nameof(movement));

        _logger.LogInformation(
            "Creating direct movement for ProductId {ProductId}, Type {MovementType}, Direction {Direction}, Qty {Quantity}",
            movement.ProductId, movement.MovementType, movement.Direction, movement.Quantity);

        // Validate required fields
        if (movement.ProductId <= 0)
            throw new ArgumentException("ProductId must be greater than 0", nameof(movement));

        if (movement.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(movement));

        if (string.IsNullOrWhiteSpace(movement.MovementReason))
            throw new ArgumentException("MovementReason is required for audit compliance", nameof(movement));

        // Set transaction date if not provided
        if (movement.TransactionDate == default)
            movement.TransactionDate = DateTime.UtcNow;

        _context.Movements.Add(movement);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created movement {MovementId} for ProductId {ProductId}",
            movement.Id, movement.ProductId);

        return movement;
    }

    // ============================================================
    // MOVEMENT REVERSAL
    // ============================================================

    /// <inheritdoc />
    public async Task<int> ReverseMovementsAsync(TransactionType transactionType, int headerId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required for reversal audit trail", nameof(reason));

        _logger.LogInformation(
            "Reversing movements for {TransactionType} transaction {HeaderId}. Reason: {Reason}",
            transactionType, headerId, reason);

        // Get movements to reverse (bypassing soft delete filter)
        var movements = await _context.Movements
            .IgnoreQueryFilters()
            .Where(m => m.TransactionType == transactionType && m.HeaderId == headerId && !m.IsDeleted)
            .ToListAsync();

        if (!movements.Any())
        {
            _logger.LogWarning(
                "No movements found to reverse for {TransactionType} transaction {HeaderId}",
                transactionType, headerId);
            return 0;
        }

        // Soft delete movements
        var now = DateTime.UtcNow;
        foreach (var movement in movements)
        {
            movement.IsDeleted = true;
            movement.DeletedAt = now;
            movement.DeletedBy = "SYSTEM"; // TODO: Get from auth context

            // Update reason to include reversal info
            movement.MovementReason = $"{movement.MovementReason} [REVERSED: {reason}]";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Reversed {Count} movements for {TransactionType} transaction {HeaderId}",
            movements.Count, transactionType, headerId);

        return movements.Count;
    }

    // ============================================================
    // SOH CALCULATION
    // ============================================================

    /// <inheritdoc />
    public async Task<decimal> CalculateSOHAsync(int productId, DateTime? asOfDate = null, int? locationId = null)
    {
        _logger.LogDebug(
            "Calculating SOH for ProductId {ProductId}, AsOfDate {AsOfDate}, LocationId {LocationId}",
            productId, asOfDate, locationId);

        var query = _context.Movements
            .Where(m => m.ProductId == productId && !m.IsDeleted);

        // Apply date filter if specified
        if (asOfDate.HasValue)
        {
            query = query.Where(m => m.TransactionDate <= asOfDate.Value);
        }

        // Apply location filter if specified
        if (locationId.HasValue)
        {
            query = query.Where(m => m.LocationId == locationId.Value);
        }

        // Calculate SOH: SUM(IN) - SUM(OUT)
        // Using conditional aggregation for efficiency
        var soh = await query
            .GroupBy(m => 1) // Group all into single result
            .Select(g => new
            {
                TotalIn = g.Where(m => m.Direction == MovementDirection.In).Sum(m => m.Quantity),
                TotalOut = g.Where(m => m.Direction == MovementDirection.Out).Sum(m => m.Quantity)
            })
            .FirstOrDefaultAsync();

        var result = (soh?.TotalIn ?? 0) - (soh?.TotalOut ?? 0);

        _logger.LogDebug(
            "SOH for ProductId {ProductId}: IN={TotalIn}, OUT={TotalOut}, SOH={SOH}",
            productId, soh?.TotalIn ?? 0, soh?.TotalOut ?? 0, result);

        return result;
    }

    /// <inheritdoc />
    public async Task<decimal> CalculateBatchSOHAsync(int productId, string batchNumber, DateTime? asOfDate = null)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("BatchNumber is required", nameof(batchNumber));

        _logger.LogDebug(
            "Calculating Batch SOH for ProductId {ProductId}, Batch {BatchNumber}, AsOfDate {AsOfDate}",
            productId, batchNumber, asOfDate);

        var query = _context.Movements
            .Where(m => m.ProductId == productId && m.BatchNumber == batchNumber && !m.IsDeleted);

        if (asOfDate.HasValue)
        {
            query = query.Where(m => m.TransactionDate <= asOfDate.Value);
        }

        var soh = await query
            .GroupBy(m => 1)
            .Select(g => new
            {
                TotalIn = g.Where(m => m.Direction == MovementDirection.In).Sum(m => m.Quantity),
                TotalOut = g.Where(m => m.Direction == MovementDirection.Out).Sum(m => m.Quantity)
            })
            .FirstOrDefaultAsync();

        return (soh?.TotalIn ?? 0) - (soh?.TotalOut ?? 0);
    }

    /// <inheritdoc />
    public async Task<Dictionary<int, decimal>> CalculateSOHBatchAsync(IEnumerable<int> productIds, DateTime? asOfDate = null)
    {
        var productIdList = productIds?.ToList() ?? new List<int>();

        if (!productIdList.Any())
            return new Dictionary<int, decimal>();

        _logger.LogDebug(
            "Calculating batch SOH for {Count} products, AsOfDate {AsOfDate}",
            productIdList.Count, asOfDate);

        var query = _context.Movements
            .Where(m => productIdList.Contains(m.ProductId) && !m.IsDeleted);

        if (asOfDate.HasValue)
        {
            query = query.Where(m => m.TransactionDate <= asOfDate.Value);
        }

        // Group by product and calculate SOH for each
        var results = await query
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalIn = g.Where(m => m.Direction == MovementDirection.In).Sum(m => m.Quantity),
                TotalOut = g.Where(m => m.Direction == MovementDirection.Out).Sum(m => m.Quantity)
            })
            .ToListAsync();

        var sohDictionary = new Dictionary<int, decimal>();

        // Initialize all requested products with 0
        foreach (var productId in productIdList)
        {
            sohDictionary[productId] = 0;
        }

        // Update with calculated values
        foreach (var result in results)
        {
            sohDictionary[result.ProductId] = result.TotalIn - result.TotalOut;
        }

        return sohDictionary;
    }

    // ============================================================
    // MOVEMENT QUERIES
    // ============================================================

    /// <inheritdoc />
    public async Task<IEnumerable<Movement>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug(
            "Getting movement history for ProductId {ProductId} from {StartDate} to {EndDate}",
            productId, startDate, endDate);

        return await _context.Movements
            .Where(m => m.ProductId == productId
                && m.TransactionDate >= startDate
                && m.TransactionDate <= endDate
                && !m.IsDeleted)
            .OrderByDescending(m => m.TransactionDate)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Movement>> GetMovementsByBatchAsync(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("BatchNumber is required", nameof(batchNumber));

        _logger.LogDebug("Getting movements for batch {BatchNumber}", batchNumber);

        return await _context.Movements
            .Where(m => m.BatchNumber == batchNumber && !m.IsDeleted)
            .OrderByDescending(m => m.TransactionDate)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Movement>> GetMovementsBySerialNumberAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("SerialNumber is required", nameof(serialNumber));

        _logger.LogDebug("Getting movements for serial number {SerialNumber}", serialNumber);

        return await _context.Movements
            .Where(m => m.SerialNumber == serialNumber && !m.IsDeleted)
            .OrderByDescending(m => m.TransactionDate)
            .ThenByDescending(m => m.Id)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Movement>> GetMovementsByTransactionAsync(TransactionType transactionType, int headerId)
    {
        _logger.LogDebug(
            "Getting movements for {TransactionType} transaction {HeaderId}",
            transactionType, headerId);

        return await _context.Movements
            .Where(m => m.TransactionType == transactionType && m.HeaderId == headerId && !m.IsDeleted)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    // ============================================================
    // UTILITY METHODS
    // ============================================================

    /// <inheritdoc />
    public MovementDirection GetMovementDirection(TransactionType transactionType)
    {
        return transactionType switch
        {
            // IN transactions (increase SOH)
            TransactionType.GRV => MovementDirection.In,
            TransactionType.Refund => MovementDirection.In,
            TransactionType.WholesaleRefund => MovementDirection.In,
            TransactionType.ProductionOutput => MovementDirection.In,
            TransactionType.TransferIn => MovementDirection.In,
            TransactionType.AdjustmentIn => MovementDirection.In,

            // OUT transactions (decrease SOH)
            TransactionType.Sale => MovementDirection.Out,
            TransactionType.RTS => MovementDirection.Out,
            TransactionType.WholesaleSale => MovementDirection.Out,
            TransactionType.ProductionInput => MovementDirection.Out,
            TransactionType.TransferOut => MovementDirection.Out,
            TransactionType.AdjustmentOut => MovementDirection.Out,

            // StocktakeVariance handled separately (can be either)
            TransactionType.StocktakeVariance => MovementDirection.In, // Default, actual handled by caller

            // Non-stock transactions
            TransactionType.AccountPayment => MovementDirection.In, // N/A but need a default
            TransactionType.Layby => MovementDirection.Out, // Reserved stock
            TransactionType.Quote => MovementDirection.In, // N/A but need a default

            _ => throw new ArgumentException($"Unknown transaction type: {transactionType}", nameof(transactionType))
        };
    }

    /// <inheritdoc />
    public string GetMovementTypeName(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Sale => "Retail Sale",
            TransactionType.Refund => "Customer Refund",
            TransactionType.AccountPayment => "Account Payment",
            TransactionType.Layby => "Layby",
            TransactionType.Quote => "Quote",
            TransactionType.GRV => "Goods Received",
            TransactionType.RTS => "Return to Supplier",
            TransactionType.WholesaleSale => "Wholesale Sale",
            TransactionType.WholesaleRefund => "Wholesale Refund",
            TransactionType.ProductionInput => "Production Input",
            TransactionType.ProductionOutput => "Production Output",
            TransactionType.TransferOut => "Stock Transfer Out",
            TransactionType.TransferIn => "Stock Transfer In",
            TransactionType.AdjustmentIn => "Stock Adjustment (In)",
            TransactionType.AdjustmentOut => "Stock Adjustment (Out)",
            TransactionType.StocktakeVariance => "Stocktake Variance",
            _ => transactionType.ToString()
        };
    }

    /// <inheritdoc />
    public bool IsStockAffectingTransaction(TransactionType transactionType)
    {
        return transactionType switch
        {
            // Non-stock transactions
            TransactionType.AccountPayment => false, // Payment only, no product movement
            TransactionType.Quote => false, // Estimate only, no commitment

            // All other transaction types affect stock
            _ => true
        };
    }
}
