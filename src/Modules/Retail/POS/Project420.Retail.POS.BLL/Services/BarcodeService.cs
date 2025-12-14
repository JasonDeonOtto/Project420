using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project420.Retail.POS.BLL.DTOs;
using Project420.Retail.POS.DAL;
using Project420.Shared.Core.Enums;
using System.Text.RegularExpressions;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service for barcode scanning, serial number lookup, and product search
    /// Phase 9.1: Barcode & Serial Number Scanning
    /// </summary>
    public class BarcodeService : IBarcodeService
    {
        private readonly PosDbContext _context;
        private readonly ILogger<BarcodeService> _logger;

        // Regular expressions for barcode format detection
        private static readonly Regex EAN13Regex = new(@"^\d{13}$", RegexOptions.Compiled);
        private static readonly Regex UPCRegex = new(@"^\d{12}$", RegexOptions.Compiled);
        private static readonly Regex FullSerialNumberRegex = new(@"^\d{30,31}$", RegexOptions.Compiled); // 30-31 digit full SN
        private static readonly Regex ShortSerialNumberRegex = new(@"^\d{13}$", RegexOptions.Compiled); // 13 digit short SN (same as EAN-13)
        private static readonly Regex SKURegex = new(@"^[A-Z0-9]{3,20}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public BarcodeService(PosDbContext context, ILogger<BarcodeService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<BarcodeScanResultDto> ProcessScanAsync(string scannedValue)
        {
            if (string.IsNullOrWhiteSpace(scannedValue))
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ErrorMessage = "Scanned value is empty"
                };
            }

            var trimmedValue = scannedValue.Trim();
            var barcodeType = DetectBarcodeType(trimmedValue);

            _logger.LogInformation("Processing scan: {Value}, Detected type: {Type}", trimmedValue, barcodeType);

            // Try to find by different methods based on detected type
            return barcodeType switch
            {
                "FullSerialNumber" => await ProcessFullSerialNumberAsync(trimmedValue),
                "ShortSerialNumber" or "EAN13" => await ProcessShortSerialOrEAN13Async(trimmedValue),
                "UPC" => await ProcessStandardBarcodeAsync(trimmedValue),
                "SKU" => await ProcessSKUAsync(trimmedValue),
                _ => await ProcessUnknownAsync(trimmedValue)
            };
        }

        private async Task<BarcodeScanResultDto> ProcessFullSerialNumberAsync(string fullSN)
        {
            _logger.LogInformation("Looking up full serial number: {FullSN}", fullSN);

            // Validate Luhn check digit
            if (!ValidateLuhnCheckDigit(fullSN))
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = fullSN,
                    BarcodeType = "FullSerialNumber",
                    ErrorMessage = "Invalid serial number check digit"
                };
            }

            // Look up in SerialNumbers table (Phase 8)
            var serialNumber = await _context.SerialNumbers
                .FirstOrDefaultAsync(sn => sn.FullSerialNumber == fullSN);

            if (serialNumber == null)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = fullSN,
                    BarcodeType = "FullSerialNumber",
                    ErrorMessage = "Serial number not found in system"
                };
            }

            // Check if already sold
            if (serialNumber.Status == SerialStatus.Sold)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = fullSN,
                    BarcodeType = "FullSerialNumber",
                    IsSerializedItem = true,
                    ErrorMessage = $"This item has already been sold on {serialNumber.SoldAt?.ToString("yyyy-MM-dd HH:mm")}"
                };
            }

            // Check if destroyed
            if (serialNumber.Status == SerialStatus.Destroyed)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = fullSN,
                    BarcodeType = "FullSerialNumber",
                    IsSerializedItem = true,
                    ErrorMessage = "This item has been destroyed and cannot be sold"
                };
            }

            // Get the associated product
            if (!serialNumber.ProductId.HasValue)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = fullSN,
                    BarcodeType = "FullSerialNumber",
                    IsSerializedItem = true,
                    ErrorMessage = "Serial number is not assigned to a product"
                };
            }

            var product = await _context.Products.FindAsync(serialNumber.ProductId.Value);
            if (product == null)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = fullSN,
                    BarcodeType = "FullSerialNumber",
                    IsSerializedItem = true,
                    ErrorMessage = "Product not found for this serial number"
                };
            }

            return new BarcodeScanResultDto
            {
                Success = true,
                ScannedValue = fullSN,
                BarcodeType = "FullSerialNumber",
                IsSerializedItem = true,
                CartItem = new CartItemDto
                {
                    ProductId = product.Id,
                    ProductSku = product.SKU,
                    ProductName = product.Name,
                    UnitPriceInclVAT = product.Price,
                    CostPrice = product.CostPrice,
                    Quantity = 1, // Serialized items are always qty 1
                    BatchNumber = serialNumber.BatchNumber,
                    SerialNumber = fullSN
                }
            };
        }

        private async Task<BarcodeScanResultDto> ProcessShortSerialOrEAN13Async(string value)
        {
            _logger.LogInformation("Looking up short serial or EAN-13: {Value}", value);

            // First, try as short serial number (Phase 8)
            var serialNumber = await _context.SerialNumbers
                .FirstOrDefaultAsync(sn => sn.ShortSerialNumber == value);

            if (serialNumber != null)
            {
                // Found as serial number - process same as full SN
                return await ProcessFullSerialNumberAsync(serialNumber.FullSerialNumber);
            }

            // Not a serial number - try as EAN-13 barcode
            return await ProcessStandardBarcodeAsync(value);
        }

        private async Task<BarcodeScanResultDto> ProcessStandardBarcodeAsync(string barcode)
        {
            _logger.LogInformation("Looking up standard barcode: {Barcode}", barcode);

            // Look up in ProductBarcodes table
            var productBarcode = await _context.ProductBarcodes
                .Include(pb => pb.Product)
                .FirstOrDefaultAsync(pb => pb.BarcodeValue == barcode && pb.IsActive);

            if (productBarcode == null)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = barcode,
                    BarcodeType = DetectBarcodeType(barcode),
                    ErrorMessage = "Barcode not found in system"
                };
            }

            // Check if this is a unique/serialized barcode that's already sold
            if (productBarcode.IsUnique && productBarcode.IsSold)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = barcode,
                    BarcodeType = productBarcode.BarcodeType.ToString(),
                    IsSerializedItem = true,
                    ErrorMessage = $"This item has already been sold on {productBarcode.SoldDate?.ToString("yyyy-MM-dd HH:mm")}"
                };
            }

            var product = productBarcode.Product;
            string? warningMessage = null;

            // Check stock level
            if (product.StockOnHand <= 0)
            {
                warningMessage = "Warning: Product is out of stock";
            }
            else if (product.StockOnHand < 5)
            {
                warningMessage = $"Warning: Low stock ({product.StockOnHand} remaining)";
            }

            return new BarcodeScanResultDto
            {
                Success = true,
                ScannedValue = barcode,
                BarcodeType = productBarcode.BarcodeType.ToString(),
                IsSerializedItem = productBarcode.IsUnique,
                WarningMessage = warningMessage,
                CartItem = new CartItemDto
                {
                    ProductId = product.Id,
                    ProductSku = product.SKU,
                    ProductName = product.Name,
                    UnitPriceInclVAT = product.Price,
                    CostPrice = product.CostPrice,
                    Quantity = 1,
                    BatchNumber = productBarcode.BatchNumber,
                    SerialNumber = productBarcode.IsUnique ? productBarcode.SerialNumber : null
                }
            };
        }

        private async Task<BarcodeScanResultDto> ProcessSKUAsync(string sku)
        {
            _logger.LogInformation("Looking up SKU: {SKU}", sku);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU.ToUpper() == sku.ToUpper());

            if (product == null)
            {
                return new BarcodeScanResultDto
                {
                    Success = false,
                    ScannedValue = sku,
                    BarcodeType = "SKU",
                    ErrorMessage = "Product SKU not found"
                };
            }

            string? warningMessage = null;
            if (product.StockOnHand <= 0)
            {
                warningMessage = "Warning: Product is out of stock";
            }

            return new BarcodeScanResultDto
            {
                Success = true,
                ScannedValue = sku,
                BarcodeType = "SKU",
                IsSerializedItem = false,
                WarningMessage = warningMessage,
                CartItem = new CartItemDto
                {
                    ProductId = product.Id,
                    ProductSku = product.SKU,
                    ProductName = product.Name,
                    UnitPriceInclVAT = product.Price,
                    CostPrice = product.CostPrice,
                    Quantity = 1,
                    BatchNumber = null // Will be assigned at checkout based on FIFO
                }
            };
        }

        private async Task<BarcodeScanResultDto> ProcessUnknownAsync(string value)
        {
            _logger.LogInformation("Processing unknown format: {Value}", value);

            // Try all lookup methods in order: barcode -> serial -> SKU -> name search
            var barcodeResult = await ProcessStandardBarcodeAsync(value);
            if (barcodeResult.Success)
                return barcodeResult;

            var skuResult = await ProcessSKUAsync(value);
            if (skuResult.Success)
                return skuResult;

            // Try partial name search
            var searchResult = await SearchProductsAsync(value, 1);
            if (searchResult.TotalCount == 1)
            {
                var product = searchResult.Products.First();
                return new BarcodeScanResultDto
                {
                    Success = true,
                    ScannedValue = value,
                    BarcodeType = "NameSearch",
                    IsSerializedItem = false,
                    WarningMessage = product.StockOnHand <= 0 ? "Warning: Product is out of stock" : null,
                    CartItem = new CartItemDto
                    {
                        ProductId = product.ProductId,
                        ProductSku = product.SKU,
                        ProductName = product.Name,
                        UnitPriceInclVAT = product.Price,
                        CostPrice = product.CostPrice,
                        Quantity = 1
                    }
                };
            }

            return new BarcodeScanResultDto
            {
                Success = false,
                ScannedValue = value,
                BarcodeType = "Unknown",
                ErrorMessage = searchResult.TotalCount > 1
                    ? $"Multiple products found ({searchResult.TotalCount}). Please refine your search."
                    : "No matching product found"
            };
        }

        /// <inheritdoc />
        public async Task<SerialNumberValidationDto> ValidateSerialNumberAsync(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                return new SerialNumberValidationDto
                {
                    IsValid = false,
                    ErrorMessage = "Serial number is required"
                };
            }

            var trimmed = serialNumber.Trim();

            // Determine if full or short format
            bool isFullFormat = trimmed.Length >= 30;

            var sn = isFullFormat
                ? await _context.SerialNumbers.FirstOrDefaultAsync(s => s.FullSerialNumber == trimmed)
                : await _context.SerialNumbers.FirstOrDefaultAsync(s => s.ShortSerialNumber == trimmed);

            if (sn == null)
            {
                return new SerialNumberValidationDto
                {
                    IsValid = false,
                    ErrorMessage = "Serial number not found"
                };
            }

            return new SerialNumberValidationDto
            {
                IsValid = true,
                FullSerialNumber = sn.FullSerialNumber,
                ShortSerialNumber = sn.ShortSerialNumber,
                ProductId = sn.ProductId,
                BatchNumber = sn.BatchNumber,
                Status = sn.Status.ToString(),
                IsSold = sn.Status == SerialStatus.Sold,
                WeightGrams = sn.WeightGrams
            };
        }

        /// <inheritdoc />
        public async Task<ProductSearchResultDto> SearchProductsAsync(string searchTerm, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            {
                return new ProductSearchResultDto { TotalCount = 0, Products = new() };
            }

            var term = searchTerm.Trim().ToLower();

            var query = _context.Products
                .Where(p => p.Name.ToLower().Contains(term) ||
                            p.SKU.ToLower().Contains(term) ||
                            (p.StrainName != null && p.StrainName.ToLower().Contains(term)));

            var totalCount = await query.CountAsync();

            var rawProducts = await query
                .OrderBy(p => p.Name)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.SKU,
                    p.Name,
                    p.Price,
                    p.CostPrice,
                    p.StockOnHand,
                    p.StrainName,
                    p.THCPercentage,
                    p.CBDPercentage
                })
                .ToListAsync();

            var products = rawProducts.Select(p => new ProductSearchItemDto
            {
                ProductId = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Category = null, // Category not implemented yet
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockOnHand = (decimal)p.StockOnHand,
                StrainName = p.StrainName,
                THCPercentage = ParseDecimal(p.THCPercentage),
                CBDPercentage = ParseDecimal(p.CBDPercentage),
                RequiresSerialNumber = false // TODO: Add RequiresSerialTracking to Product entity
            }).ToList();

            // Get primary barcodes for each product
            var productIds = products.Select(p => p.ProductId).ToList();
            var barcodes = await _context.ProductBarcodes
                .Where(pb => productIds.Contains(pb.ProductId) && pb.IsPrimaryBarcode && pb.IsActive)
                .Select(pb => new { pb.ProductId, pb.BarcodeValue })
                .ToListAsync();

            foreach (var product in products)
            {
                product.PrimaryBarcode = barcodes
                    .FirstOrDefault(b => b.ProductId == product.ProductId)?.BarcodeValue;
            }

            return new ProductSearchResultDto
            {
                TotalCount = totalCount,
                Products = products
            };
        }

        /// <inheritdoc />
        public async Task<CartItemDto?> GetProductForCartAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return null;

            return new CartItemDto
            {
                ProductId = product.Id,
                ProductSku = product.SKU,
                ProductName = product.Name,
                UnitPriceInclVAT = product.Price,
                CostPrice = product.CostPrice,
                Quantity = 1
            };
        }

        /// <inheritdoc />
        public async Task<List<SerialNumberValidationDto>> GetAvailableSerialNumbersAsync(int productId, int limit = 50)
        {
            var serialNumbers = await _context.SerialNumbers
                .Where(sn => sn.ProductId == productId &&
                             (sn.Status == SerialStatus.Created || sn.Status == SerialStatus.Assigned))
                .OrderBy(sn => sn.ProductionDate)
                .ThenBy(sn => sn.UnitSequence)
                .Take(limit)
                .Select(sn => new SerialNumberValidationDto
                {
                    IsValid = true,
                    FullSerialNumber = sn.FullSerialNumber,
                    ShortSerialNumber = sn.ShortSerialNumber,
                    ProductId = sn.ProductId,
                    BatchNumber = sn.BatchNumber,
                    Status = sn.Status.ToString(),
                    IsSold = false,
                    WeightGrams = sn.WeightGrams
                })
                .ToListAsync();

            return serialNumbers;
        }

        /// <inheritdoc />
        public string DetectBarcodeType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";

            var trimmed = value.Trim();

            // Check for full serial number first (30-31 digits)
            if (FullSerialNumberRegex.IsMatch(trimmed))
            {
                // Validate Luhn to confirm it's a serial number
                if (ValidateLuhnCheckDigit(trimmed))
                    return "FullSerialNumber";
            }

            // Check for EAN-13 or short serial number (13 digits)
            if (EAN13Regex.IsMatch(trimmed))
            {
                // Could be EAN-13 or short serial - we'll try both
                return ValidateEAN13CheckDigit(trimmed) ? "EAN13" : "ShortSerialNumber";
            }

            // Check for UPC (12 digits)
            if (UPCRegex.IsMatch(trimmed))
                return "UPC";

            // Check for SKU format
            if (SKURegex.IsMatch(trimmed))
                return "SKU";

            return "Unknown";
        }

        /// <inheritdoc />
        public bool ValidateEAN13CheckDigit(string ean13)
        {
            if (string.IsNullOrEmpty(ean13) || ean13.Length != 13 || !ean13.All(char.IsDigit))
                return false;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = ean13[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit == (ean13[12] - '0');
        }

        /// <inheritdoc />
        public bool ValidateLuhnCheckDigit(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.All(char.IsDigit))
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = value.Length - 1; i >= 0; i--)
            {
                int digit = value[i] - '0';

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        /// <summary>
        /// Parse a string percentage value to decimal
        /// </summary>
        private static decimal? ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Remove % symbol if present
            var cleaned = value.Replace("%", "").Trim();

            if (decimal.TryParse(cleaned, out var result))
                return result;

            return null;
        }
    }
}
