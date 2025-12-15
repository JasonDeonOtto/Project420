using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.Models.Entities;

namespace Project420.Retail.POS.DAL.Repositories;

/// <summary>
/// Repository implementation for product barcode database operations.
/// </summary>
/// <remarks>
/// SAHPRA Compliance:
/// - Supports seed-to-sale traceability via serial numbers
/// - Enables batch recall queries
/// - Tracks individual item lifecycle (created → sold → returned)
///
/// POS Integration:
/// - Barcode scanning at checkout
/// - Duplicate sale prevention for unique items
/// - Batch/serial number recording on transactions
/// </remarks>
public class ProductBarcodeRepository : IProductBarcodeRepository
{
    private readonly PosDbContext _context;

    public ProductBarcodeRepository(PosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ========================================
    // BARCODE LOOKUP (POS Scanning)
    // ========================================

    /// <inheritdoc/>
    public async Task<ProductBarcode?> GetByBarcodeValueAsync(string barcodeValue)
    {
        if (string.IsNullOrWhiteSpace(barcodeValue))
            throw new ArgumentException("Barcode value cannot be empty", nameof(barcodeValue));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .FirstOrDefaultAsync(pb => pb.BarcodeValue == barcodeValue && pb.IsActive);
    }

    /// <inheritdoc/>
    public async Task<ProductBarcode?> GetBySerialNumberAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number cannot be empty", nameof(serialNumber));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .FirstOrDefaultAsync(pb => pb.SerialNumber == serialNumber && pb.IsUnique);
    }

    /// <inheritdoc/>
    public async Task<ProductBarcode?> GetPrimaryBarcodeAsync(int productId)
    {
        if (productId <= 0)
            throw new ArgumentException("Invalid product ID", nameof(productId));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .FirstOrDefaultAsync(pb => pb.ProductId == productId && pb.IsPrimaryBarcode && pb.IsActive);
    }

    /// <inheritdoc/>
    public async Task<List<ProductBarcode>> GetByProductIdAsync(int productId)
    {
        if (productId <= 0)
            throw new ArgumentException("Invalid product ID", nameof(productId));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .Where(pb => pb.ProductId == productId)
            .OrderByDescending(pb => pb.IsPrimaryBarcode)
            .ThenBy(pb => pb.BarcodeValue)
            .ToListAsync();
    }

    // ========================================
    // AVAILABILITY CHECKS (Duplicate Prevention)
    // ========================================

    /// <inheritdoc/>
    public async Task<bool> IsSerialNumberAvailableAsync(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number cannot be empty", nameof(serialNumber));

        var barcode = await _context.ProductBarcodes
            .FirstOrDefaultAsync(pb => pb.SerialNumber == serialNumber && pb.IsUnique);

        // Available if exists and not sold
        return barcode != null && !barcode.IsSold;
    }

    /// <inheritdoc/>
    public async Task<bool> BarcodeValueExistsAsync(string barcodeValue)
    {
        if (string.IsNullOrWhiteSpace(barcodeValue))
            throw new ArgumentException("Barcode value cannot be empty", nameof(barcodeValue));

        return await _context.ProductBarcodes
            .AnyAsync(pb => pb.BarcodeValue == barcodeValue);
    }

    // ========================================
    // BATCH OPERATIONS (Traceability)
    // ========================================

    /// <inheritdoc/>
    public async Task<List<ProductBarcode>> GetByBatchNumberAsync(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            throw new ArgumentException("Batch number cannot be empty", nameof(batchNumber));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .Where(pb => pb.BatchNumber == batchNumber)
            .OrderBy(pb => pb.SerialNumber)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ProductBarcode>> GetUnsoldSerialNumbersAsync(int productId)
    {
        if (productId <= 0)
            throw new ArgumentException("Invalid product ID", nameof(productId));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .Where(pb => pb.ProductId == productId && pb.IsUnique && !pb.IsSold)
            .OrderBy(pb => pb.SerialNumber)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ProductBarcode>> GetSoldSerialNumbersAsync(
        int productId,
        DateTime startDate,
        DateTime endDate)
    {
        if (productId <= 0)
            throw new ArgumentException("Invalid product ID", nameof(productId));

        if (endDate < startDate)
            throw new ArgumentException("End date must be >= start date", nameof(endDate));

        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .Where(pb => pb.ProductId == productId
                && pb.IsUnique
                && pb.IsSold
                && pb.SoldDate.HasValue
                && pb.SoldDate.Value >= startDate
                && pb.SoldDate.Value <= endDate)
            .OrderBy(pb => pb.SoldDate)
            .ToListAsync();
    }

    // ========================================
    // LIFECYCLE MANAGEMENT
    // ========================================

    /// <inheritdoc/>
    public async Task<bool> MarkAsSoldAsync(int productBarcodeId, int transactionId, DateTime soldDate)
    {
        if (productBarcodeId <= 0)
            throw new ArgumentException("Invalid product barcode ID", nameof(productBarcodeId));

        if (transactionId <= 0)
            throw new ArgumentException("Invalid transaction ID", nameof(transactionId));

        var barcode = await _context.ProductBarcodes
            .FirstOrDefaultAsync(pb => pb.Id == productBarcodeId && pb.IsUnique);

        if (barcode == null)
            return false;

        // Check if already sold
        if (barcode.IsSold)
            return false;

        barcode.IsSold = true;
        barcode.SoldInTransactionId = transactionId;
        barcode.SoldDate = soldDate;

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UnmarkAsSoldAsync(int productBarcodeId)
    {
        if (productBarcodeId <= 0)
            throw new ArgumentException("Invalid product barcode ID", nameof(productBarcodeId));

        var barcode = await _context.ProductBarcodes
            .FirstOrDefaultAsync(pb => pb.Id == productBarcodeId && pb.IsUnique);

        if (barcode == null)
            return false;

        // Check if actually sold
        if (!barcode.IsSold)
            return false;

        barcode.IsSold = false;
        barcode.SoldInTransactionId = null;
        barcode.SoldDate = null;

        await _context.SaveChangesAsync();
        return true;
    }

    // ========================================
    // CRUD OPERATIONS
    // ========================================

    /// <inheritdoc/>
    public async Task<ProductBarcode> AddAsync(ProductBarcode productBarcode)
    {
        if (productBarcode == null)
            throw new ArgumentNullException(nameof(productBarcode));

        _context.ProductBarcodes.Add(productBarcode);
        await _context.SaveChangesAsync();

        // Reload with product navigation
        return await GetByIdAsync(productBarcode.Id) ?? productBarcode;
    }

    /// <inheritdoc/>
    public async Task<int> AddRangeAsync(IEnumerable<ProductBarcode> productBarcodes)
    {
        if (productBarcodes == null)
            throw new ArgumentNullException(nameof(productBarcodes));

        var barcodeList = productBarcodes.ToList();
        if (!barcodeList.Any())
            return 0;

        _context.ProductBarcodes.AddRange(barcodeList);
        await _context.SaveChangesAsync();

        return barcodeList.Count;
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ProductBarcode productBarcode)
    {
        if (productBarcode == null)
            throw new ArgumentNullException(nameof(productBarcode));

        _context.ProductBarcodes.Update(productBarcode);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> DeactivateAsync(int productBarcodeId)
    {
        if (productBarcodeId <= 0)
            throw new ArgumentException("Invalid product barcode ID", nameof(productBarcodeId));

        var barcode = await _context.ProductBarcodes
            .FirstOrDefaultAsync(pb => pb.Id == productBarcodeId);

        if (barcode == null)
            return false;

        // Soft delete
        barcode.IsDeleted = true;
        barcode.DeletedAt = DateTime.UtcNow;
        barcode.IsActive = false;

        await _context.SaveChangesAsync();
        return true;
    }

    // ========================================
    // PRIVATE HELPERS
    // ========================================

    /// <summary>
    /// Get barcode by ID with product navigation.
    /// </summary>
    private async Task<ProductBarcode?> GetByIdAsync(int productBarcodeId)
    {
        return await _context.ProductBarcodes
            .Include(pb => pb.Product)
            .FirstOrDefaultAsync(pb => pb.Id == productBarcodeId);
    }
}
