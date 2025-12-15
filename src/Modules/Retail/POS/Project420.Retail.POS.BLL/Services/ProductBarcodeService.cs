using Microsoft.Extensions.Logging;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.DAL.Repositories;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.Services;

/// <summary>
/// Service implementation for product barcode operations including POS scanning and serial number management.
/// </summary>
/// <remarks>
/// SAHPRA Compliance:
/// - Supports seed-to-sale traceability via serial numbers
/// - Enables batch recall queries
/// - Validates serial number availability before sale
///
/// POS Integration:
/// - Barcode scanning at checkout (returns product with pricing)
/// - Duplicate sale prevention for unique items
/// - Integration with TransactionService for sale recording
/// </remarks>
public class ProductBarcodeService : IProductBarcodeService
{
    private readonly IProductBarcodeRepository _repository;
    private readonly ILogger<ProductBarcodeService> _logger;

    public ProductBarcodeService(
        IProductBarcodeRepository repository,
        ILogger<ProductBarcodeService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================
    // BARCODE SCANNING (POS Primary Use Case)
    // ========================================

    /// <inheritdoc/>
    public async Task<BarcodeScanResultDto> ScanBarcodeAsync(string barcodeValue)
    {
        var result = new BarcodeScanResultDto
        {
            BarcodeValue = barcodeValue
        };

        if (string.IsNullOrWhiteSpace(barcodeValue))
        {
            result.Success = false;
            result.ErrorMessage = "Barcode value cannot be empty";
            return result;
        }

        try
        {
            var barcode = await _repository.GetByBarcodeValueAsync(barcodeValue);

            if (barcode == null)
            {
                result.Success = false;
                result.ErrorMessage = $"Barcode '{barcodeValue}' not found";
                _logger.LogWarning("Barcode not found: {BarcodeValue}", barcodeValue);
                return result;
            }

            // Populate result from barcode and product
            result.Success = true;
            result.ProductId = barcode.ProductId;
            result.ProductSku = barcode.Product.SKU;
            result.ProductName = barcode.Product.Name;
            result.UnitPriceInclVAT = barcode.Product.Price;
            result.CostPrice = barcode.Product.CostPrice;
            result.BarcodeType = barcode.BarcodeType;
            result.IsUniqueItem = barcode.IsUnique;
            result.StockOnHand = barcode.Product.StockOnHand;

            // Handle unique serial number items
            if (barcode.IsUnique)
            {
                result.ProductBarcodeId = barcode.Id;
                result.SerialNumber = barcode.SerialNumber;
                result.BatchNumber = barcode.BatchNumber;
                result.IsAlreadySold = barcode.IsSold;

                if (barcode.IsSold)
                {
                    result.WarningMessage = $"WARNING: This serial number has already been sold " +
                        $"(Transaction #{barcode.SoldInTransactionId} on {barcode.SoldDate:yyyy-MM-dd})";
                    _logger.LogWarning(
                        "Attempted to scan already sold serial number: {SerialNumber}, " +
                        "Transaction: {TransactionId}, SoldDate: {SoldDate}",
                        barcode.SerialNumber, barcode.SoldInTransactionId, barcode.SoldDate);
                }
            }
            else
            {
                // Standard barcode - use product batch number
                result.BatchNumber = barcode.Product.BatchNumber;
            }

            _logger.LogDebug(
                "Barcode scanned successfully: {BarcodeValue} -> Product: {ProductName} (ID: {ProductId})",
                barcodeValue, barcode.Product.Name, barcode.ProductId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning barcode: {BarcodeValue}", barcodeValue);
            result.Success = false;
            result.ErrorMessage = "An error occurred while scanning the barcode";
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<SerialNumberValidationResultDto> ValidateSerialNumberAsync(string serialNumber)
    {
        var result = new SerialNumberValidationResultDto();

        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            result.IsValid = false;
            result.ErrorMessage = "Serial number cannot be empty";
            return result;
        }

        try
        {
            var barcode = await _repository.GetBySerialNumberAsync(serialNumber);

            if (barcode == null)
            {
                result.IsValid = false;
                result.Exists = false;
                result.IsAvailable = false;
                result.ErrorMessage = $"Serial number '{serialNumber}' not found";
                return result;
            }

            result.Exists = true;
            result.ProductBarcodeId = barcode.Id;
            result.ProductId = barcode.ProductId;
            result.ProductName = barcode.Product.Name;
            result.BatchNumber = barcode.BatchNumber;

            if (barcode.IsSold)
            {
                result.IsValid = false;
                result.IsAvailable = false;
                result.SoldInTransactionId = barcode.SoldInTransactionId;
                result.SoldDate = barcode.SoldDate;
                result.ErrorMessage = $"Serial number already sold in transaction #{barcode.SoldInTransactionId}";
            }
            else
            {
                result.IsValid = true;
                result.IsAvailable = true;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating serial number: {SerialNumber}", serialNumber);
            result.IsValid = false;
            result.ErrorMessage = "An error occurred while validating the serial number";
            return result;
        }
    }

    // ========================================
    // CART ITEM CREATION
    // ========================================

    /// <inheritdoc/>
    public async Task<CartItemDto?> CreateCartItemFromBarcodeAsync(string barcodeValue, int? pricelistId = null)
    {
        var scanResult = await ScanBarcodeAsync(barcodeValue);

        if (!scanResult.Success || !scanResult.ProductId.HasValue)
        {
            _logger.LogWarning(
                "Cannot create cart item - barcode scan failed: {BarcodeValue}, Error: {Error}",
                barcodeValue, scanResult.ErrorMessage);
            return null;
        }

        // Don't create cart item for already sold unique items
        if (scanResult.IsUniqueItem && scanResult.IsAlreadySold)
        {
            _logger.LogWarning(
                "Cannot create cart item - serial number already sold: {SerialNumber}",
                scanResult.SerialNumber);
            return null;
        }

        // TODO: If pricelistId is provided, look up customer-specific pricing
        // For now, use product default price

        var cartItem = new CartItemDto
        {
            ProductId = scanResult.ProductId.Value,
            ProductSku = scanResult.ProductSku ?? string.Empty,
            ProductName = scanResult.ProductName ?? string.Empty,
            UnitPriceInclVAT = scanResult.UnitPriceInclVAT ?? 0,
            CostPrice = scanResult.CostPrice ?? 0,
            Quantity = 1, // Default quantity, can be adjusted by POS
            BatchNumber = scanResult.BatchNumber,
            SerialNumber = scanResult.SerialNumber,
            BarcodeValue = barcodeValue,
            BarcodeType = scanResult.BarcodeType,
            IsUniqueItem = scanResult.IsUniqueItem,
            ProductBarcodeId = scanResult.ProductBarcodeId
        };

        _logger.LogDebug(
            "Cart item created from barcode: {BarcodeValue} -> {ProductName} @ R{Price}",
            barcodeValue, cartItem.ProductName, cartItem.UnitPriceInclVAT);

        return cartItem;
    }

    // ========================================
    // SERIAL NUMBER LIFECYCLE
    // ========================================

    /// <inheritdoc/>
    public async Task<bool> MarkSerialNumberAsSoldAsync(int productBarcodeId, int transactionId, DateTime soldDate)
    {
        try
        {
            var success = await _repository.MarkAsSoldAsync(productBarcodeId, transactionId, soldDate);

            if (success)
            {
                _logger.LogInformation(
                    "Serial number marked as sold. ProductBarcodeId: {ProductBarcodeId}, " +
                    "TransactionId: {TransactionId}, SoldDate: {SoldDate}",
                    productBarcodeId, transactionId, soldDate);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to mark serial number as sold. ProductBarcodeId: {ProductBarcodeId}, " +
                    "TransactionId: {TransactionId}",
                    productBarcodeId, transactionId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error marking serial number as sold. ProductBarcodeId: {ProductBarcodeId}, " +
                "TransactionId: {TransactionId}",
                productBarcodeId, transactionId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UnmarkSerialNumberAsSoldAsync(int productBarcodeId)
    {
        try
        {
            var success = await _repository.UnmarkAsSoldAsync(productBarcodeId);

            if (success)
            {
                _logger.LogInformation(
                    "Serial number unmarked as sold (refund). ProductBarcodeId: {ProductBarcodeId}",
                    productBarcodeId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to unmark serial number as sold. ProductBarcodeId: {ProductBarcodeId}",
                    productBarcodeId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error unmarking serial number as sold. ProductBarcodeId: {ProductBarcodeId}",
                productBarcodeId);
            return false;
        }
    }

    // ========================================
    // BARCODE MANAGEMENT
    // ========================================

    /// <inheritdoc/>
    public async Task<int> RegisterBarcodeAsync(RegisterBarcodeDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        // Validate unique barcode value
        if (await _repository.BarcodeValueExistsAsync(dto.BarcodeValue))
        {
            throw new InvalidOperationException($"Barcode value '{dto.BarcodeValue}' already exists");
        }

        var barcode = new ProductBarcode
        {
            ProductId = dto.ProductId,
            BarcodeValue = dto.BarcodeValue,
            BarcodeType = dto.BarcodeType,
            IsUnique = dto.IsUnique,
            SerialNumber = dto.SerialNumber,
            BatchNumber = dto.BatchNumber,
            IsPrimaryBarcode = dto.IsPrimaryBarcode,
            IsActive = true,
            IsSold = false
        };

        var created = await _repository.AddAsync(barcode);

        _logger.LogInformation(
            "Barcode registered. Id: {Id}, ProductId: {ProductId}, BarcodeValue: {BarcodeValue}, " +
            "IsUnique: {IsUnique}, SerialNumber: {SerialNumber}",
            created.Id, dto.ProductId, dto.BarcodeValue, dto.IsUnique, dto.SerialNumber);

        return created.Id;
    }

    /// <inheritdoc/>
    public async Task<int> RegisterBatchSerialNumbersAsync(RegisterBatchSerialNumbersDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.SerialNumbers == null || !dto.SerialNumbers.Any())
            throw new ArgumentException("Serial numbers list cannot be empty", nameof(dto));

        var barcodes = dto.SerialNumbers.Select(sn => new ProductBarcode
        {
            ProductId = dto.ProductId,
            BarcodeValue = sn, // Use serial number as barcode value
            BarcodeType = dto.BarcodeType,
            IsUnique = true,
            SerialNumber = sn,
            BatchNumber = dto.BatchNumber,
            IsPrimaryBarcode = false,
            IsActive = true,
            IsSold = false
        }).ToList();

        var count = await _repository.AddRangeAsync(barcodes);

        _logger.LogInformation(
            "Batch serial numbers registered. ProductId: {ProductId}, BatchNumber: {BatchNumber}, " +
            "Count: {Count}",
            dto.ProductId, dto.BatchNumber, count);

        return count;
    }

    /// <inheritdoc/>
    public async Task<bool> DeactivateBarcodeAsync(int productBarcodeId)
    {
        try
        {
            var success = await _repository.DeactivateAsync(productBarcodeId);

            if (success)
            {
                _logger.LogInformation("Barcode deactivated. ProductBarcodeId: {ProductBarcodeId}", productBarcodeId);
            }
            else
            {
                _logger.LogWarning("Failed to deactivate barcode. ProductBarcodeId: {ProductBarcodeId}", productBarcodeId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating barcode. ProductBarcodeId: {ProductBarcodeId}", productBarcodeId);
            return false;
        }
    }

    // ========================================
    // QUERIES
    // ========================================

    /// <inheritdoc/>
    public async Task<List<ProductBarcodeDto>> GetBarcodesByProductIdAsync(int productId)
    {
        var barcodes = await _repository.GetByProductIdAsync(productId);
        return barcodes.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductBarcodeDto>> GetSerialNumbersByBatchAsync(string batchNumber)
    {
        var barcodes = await _repository.GetByBatchNumberAsync(batchNumber);
        return barcodes.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductBarcodeDto>> GetAvailableSerialNumbersAsync(int productId)
    {
        var barcodes = await _repository.GetUnsoldSerialNumbersAsync(productId);
        return barcodes.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<ProductBarcodeDto>> GetSoldSerialNumbersAsync(
        int productId,
        DateTime startDate,
        DateTime endDate)
    {
        var barcodes = await _repository.GetSoldSerialNumbersAsync(productId, startDate, endDate);
        return barcodes.Select(MapToDto).ToList();
    }

    // ========================================
    // PRIVATE HELPERS
    // ========================================

    /// <summary>
    /// Maps ProductBarcode entity to DTO.
    /// </summary>
    private static ProductBarcodeDto MapToDto(ProductBarcode entity)
    {
        return new ProductBarcodeDto
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductSku = entity.Product?.SKU ?? string.Empty,
            ProductName = entity.Product?.Name ?? string.Empty,
            BarcodeValue = entity.BarcodeValue,
            BarcodeType = entity.BarcodeType,
            IsUnique = entity.IsUnique,
            SerialNumber = entity.SerialNumber,
            BatchNumber = entity.BatchNumber,
            IsActive = entity.IsActive,
            IsPrimaryBarcode = entity.IsPrimaryBarcode,
            IsSold = entity.IsSold,
            SoldInTransactionId = entity.SoldInTransactionId,
            SoldDate = entity.SoldDate,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };
    }
}
