using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database.Services;

/// <summary>
/// Service for generating and managing inventory movements (Option A - Movement Architecture).
/// All stock-affecting transactions MUST generate movements through this service.
/// </summary>
/// <remarks>
/// Architecture Principles:
/// - SOH (Stock on Hand) is calculated from movements, NEVER stored directly
/// - SOH = SUM(Quantity WHERE Direction = IN) - SUM(Quantity WHERE Direction = OUT)
/// - Movements are immutable once created (soft delete only)
/// - All movements link back to source TransactionDetail
///
/// Usage Pattern:
/// 1. Transaction is created and saved (Sale, GRV, Refund, etc.)
/// 2. TransactionDetails are created and saved
/// 3. MovementService.GenerateMovementsAsync() is called
/// 4. Movements are created automatically based on transaction type
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Full audit trail for all stock movements
/// - Batch/serial number tracking for seed-to-sale traceability
/// - Weight tracking for reconciliation
/// - Movement reasons documented for compliance reporting
/// </remarks>
public interface IMovementService
{
    // ============================================================
    // MOVEMENT GENERATION
    // ============================================================

    /// <summary>
    /// Generate movements from transaction details.
    /// Called AFTER transaction is saved to database.
    /// </summary>
    /// <param name="transactionType">Type of transaction (Sale, GRV, Refund, etc.)</param>
    /// <param name="headerId">ID of the transaction header</param>
    /// <returns>Number of movements created</returns>
    Task<int> GenerateMovementsAsync(TransactionType transactionType, int headerId);

    /// <summary>
    /// Generate a single movement directly (for non-transaction-based movements like adjustments).
    /// </summary>
    /// <param name="movement">Movement entity to create</param>
    /// <returns>The created movement with ID populated</returns>
    Task<Movement> CreateMovementAsync(Movement movement);

    // ============================================================
    // MOVEMENT REVERSAL
    // ============================================================

    /// <summary>
    /// Reverse movements (soft delete) when transaction is cancelled or voided.
    /// </summary>
    /// <param name="transactionType">Type of transaction</param>
    /// <param name="headerId">ID of the transaction header</param>
    /// <param name="reason">Reason for reversal (for audit trail)</param>
    /// <returns>Number of movements reversed</returns>
    Task<int> ReverseMovementsAsync(TransactionType transactionType, int headerId, string reason);

    // ============================================================
    // SOH CALCULATION
    // ============================================================

    /// <summary>
    /// Calculate Stock on Hand for a product.
    /// SOH = SUM(IN movements) - SUM(OUT movements)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="asOfDate">Optional: Calculate SOH as of a specific date (for historical queries)</param>
    /// <param name="locationId">Optional: Calculate SOH for a specific location</param>
    /// <returns>Current SOH quantity</returns>
    Task<decimal> CalculateSOHAsync(int productId, DateTime? asOfDate = null, int? locationId = null);

    /// <summary>
    /// Calculate SOH for a specific batch.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="batchNumber">Batch number</param>
    /// <param name="asOfDate">Optional: Calculate SOH as of a specific date</param>
    /// <returns>SOH for the specific batch</returns>
    Task<decimal> CalculateBatchSOHAsync(int productId, string batchNumber, DateTime? asOfDate = null);

    /// <summary>
    /// Calculate SOH for multiple products efficiently (batch query).
    /// </summary>
    /// <param name="productIds">List of product IDs</param>
    /// <param name="asOfDate">Optional: Calculate SOH as of a specific date</param>
    /// <returns>Dictionary of ProductId to SOH</returns>
    Task<Dictionary<int, decimal>> CalculateSOHBatchAsync(IEnumerable<int> productIds, DateTime? asOfDate = null);

    // ============================================================
    // MOVEMENT QUERIES
    // ============================================================

    /// <summary>
    /// Get movement history for a product.
    /// </summary>
    Task<IEnumerable<Movement>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get movements by batch number (seed-to-sale traceability).
    /// </summary>
    Task<IEnumerable<Movement>> GetMovementsByBatchAsync(string batchNumber);

    /// <summary>
    /// Get movements by serial number (individual unit tracking).
    /// </summary>
    Task<IEnumerable<Movement>> GetMovementsBySerialNumberAsync(string serialNumber);

    /// <summary>
    /// Get movements for a specific transaction.
    /// </summary>
    Task<IEnumerable<Movement>> GetMovementsByTransactionAsync(TransactionType transactionType, int headerId);

    // ============================================================
    // UTILITY METHODS
    // ============================================================

    /// <summary>
    /// Determine movement direction for a transaction type.
    /// </summary>
    MovementDirection GetMovementDirection(TransactionType transactionType);

    /// <summary>
    /// Get descriptive movement type name for a transaction type.
    /// </summary>
    string GetMovementTypeName(TransactionType transactionType);

    /// <summary>
    /// Check if a transaction type generates stock movements.
    /// </summary>
    bool IsStockAffectingTransaction(TransactionType transactionType);
}
