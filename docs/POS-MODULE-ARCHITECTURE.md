# POS Module Architecture & Scaffolding
**Project**: Project420 - Cannabis Management System
**Module**: Retail.POS (Point of Sale)
**Created**: 2025-12-05
**Status**: Architecture Design / Scaffolding Phase

---

## 🎯 Executive Summary

This document outlines the complete architecture for the POS (Point of Sale) module, addressing:
- Financial document construction (detail-first approach)
- VAT calculation strategies (inclusive vs exclusive)
- Rounding and precision handling
- Multi-tender payment processing
- Barcode scanning (unique SN + standard EAN)
- PinPad device integration
- Service layer scaffolding

---

## 📐 Core Architecture Principles

### 1. **Detail-First Financial Document Construction**

**Principle**: Build transaction details first, then aggregate to header.

**Why?**
- Line items are the source of truth
- Prevents header/detail mismatch
- Easier to handle complex scenarios (discounts, returns, multiple tax rates)
- Audit trail starts at detail level

**Flow**:
```
1. User scans/selects products → Create POSTransactionDetail records
2. Calculate per-line amounts (Subtotal, Tax, Total)
3. Aggregate all details → Calculate POSTransactionHeader totals
4. Apply transaction-level discounts (if any)
5. Recalculate header totals
6. Accept payments
7. Finalize transaction (Status = Completed)
```

**Benefits**:
- ✅ Accurate to the cent
- ✅ Handles split lines (different tax rates, discounts)
- ✅ Easy to add/remove items during transaction
- ✅ Clear audit trail

---

## 💰 VAT Calculation Strategy

### South African VAT Context
- **Standard VAT Rate**: 15%
- **VAT Registration Threshold**: R1,000,000 annual turnover
- **Price Display**: VAT-inclusive (standard practice in SA retail)
- **Reporting**: VAT201 returns to SARS

### **Challenge: VAT-Inclusive Pricing Precision**

When prices include VAT, extracting the VAT amount can cause rounding issues:

```
Example: Product priced at R10.00 (VAT-inclusive)

Method 1 (Standard):
Subtotal = R10.00 / 1.15 = R8.695652...
VAT = R10.00 - R8.70 (rounded) = R1.30
Total = R8.70 + R1.30 = R10.00 ✅

Method 2 (VAT-first):
VAT = R10.00 * (15/115) = R1.304348...
Subtotal = R10.00 - R1.30 (rounded) = R8.70
Total = R8.70 + R1.30 = R10.00 ✅

Problem: Large invoice with many items
100 items @ R10.00 = R1000.00 total
But: 100 * R8.70 subtotal = R870.00
     100 * R1.30 VAT = R130.00
     Total = R1000.00 ✅

However: 100 * (R10.00 / 1.15) = R869.57 (exact)
         Rounding: R870.00
         Difference: R0.43 variance
```

### **Recommended VAT Calculation Approach**

#### **Option 1: Line-by-Line with Final Adjustment (RECOMMENDED)**

**Implementation**:
```csharp
// Per Line Item Calculation
public class POSTransactionDetailCalculator
{
    private const decimal VAT_RATE = 0.15m;
    private const decimal VAT_DIVISOR = 1.15m;

    public POSTransactionDetail CalculateLineItem(
        decimal unitPriceInclVAT,
        int quantity)
    {
        // All calculations use VAT-inclusive prices
        decimal lineTotal = unitPriceInclVAT * quantity;

        // Calculate VAT portion (rounded to 2 decimals per line)
        decimal vatAmount = Math.Round(
            lineTotal - (lineTotal / VAT_DIVISOR),
            2,
            MidpointRounding.AwayFromZero
        );

        // Subtotal is the difference
        decimal subtotal = lineTotal - vatAmount;

        return new POSTransactionDetail
        {
            Quantity = quantity,
            UnitPrice = unitPriceInclVAT,
            Total = lineTotal,
            TaxAmount = vatAmount,
            Subtotal = subtotal
        };
    }
}

// Header Aggregation
public class POSTransactionHeaderCalculator
{
    public void CalculateHeaderTotals(POSTransactionHeader header)
    {
        // Sum all detail lines
        decimal detailTotalSum = header.TransactionDetails.Sum(d => d.Total);
        decimal detailTaxSum = header.TransactionDetails.Sum(d => d.TaxAmount);
        decimal detailSubtotalSum = header.TransactionDetails.Sum(d => d.Subtotal);

        // Check for rounding variance
        decimal calculatedTotal = detailSubtotalSum + detailTaxSum;
        decimal variance = detailTotalSum - calculatedTotal;

        if (Math.Abs(variance) > 0.01m) // More than 1 cent off
        {
            // Adjust VAT by variance amount (keep total correct)
            detailTaxSum += variance;
        }

        header.Subtotal = detailSubtotalSum;
        header.TaxAmount = detailTaxSum;
        header.TotalAmount = detailTotalSum;
        header.DiscountAmount = 0.00m; // Apply discounts separately
    }
}
```

**Benefits**:
- ✅ Final total always matches line items
- ✅ VAT variance absorbed in tax amount (acceptable per SARS)
- ✅ No surprise rounding errors at checkout
- ✅ Simple to understand and audit

#### **Option 2: Rounding Write-Off Line Item (ALTERNATIVE)**

For very large invoices (e.g., wholesale with 1000+ line items):

```csharp
public void FinalizeTransaction(POSTransactionHeader header)
{
    // Calculate totals
    CalculateHeaderTotals(header);

    // Check for variance
    decimal expectedTotal = header.TransactionDetails.Sum(d => d.Total);
    decimal calculatedTotal = header.Subtotal + header.TaxAmount;
    decimal variance = expectedTotal - calculatedTotal;

    // If variance > 5 cents, create rounding adjustment line
    if (Math.Abs(variance) > 0.05m)
    {
        var roundingLine = new POSTransactionDetail
        {
            ProductId = ROUNDING_PRODUCT_ID, // Special "Rounding Adjustment" product
            ProductSKU = "ROUND-ADJ",
            ProductName = "Rounding Adjustment",
            Quantity = 1,
            UnitPrice = variance,
            Total = variance,
            TaxAmount = 0.00m,
            Subtotal = variance,
            Notes = "VAT calculation rounding adjustment"
        };

        header.TransactionDetails.Add(roundingLine);

        // Recalculate header
        CalculateHeaderTotals(header);
    }
}
```

**Use Case**:
- Large wholesale invoices (1000+ lines)
- Transparency required (customer sees adjustment)
- Auditor-friendly (adjustment is explicit)

**Standard Retail POS**: Use **Option 1** (silent VAT adjustment)
**Wholesale Invoices**: Consider **Option 2** (explicit rounding line)

---

## 🧾 Financial Document Structure

### **What Goes in Header vs Detail?**

#### **POSTransactionHeader (Aggregated Data)**
```csharp
public class POSTransactionHeader
{
    // Identification
    public string TransactionNumber { get; set; }    // "SALE-20251205-001"
    public DateTime TransactionDate { get; set; }
    public TransactionType Type { get; set; }        // Sale, Refund, etc.
    public TransactionStatus Status { get; set; }    // Pending, Completed, etc.

    // Customer (Optional - walk-ins can be null)
    public int? DebtorId { get; set; }
    public string? CustomerName { get; set; }        // Denormalized for speed

    // Pricing Context
    public int? PricelistId { get; set; }            // Which pricelist was used

    // **FINANCIAL TOTALS (Aggregated from Details)**
    public decimal Subtotal { get; set; }            // Sum of all detail subtotals
    public decimal TaxAmount { get; set; }           // Sum of all detail taxes
    public decimal TotalAmount { get; set; }         // Sum of all detail totals
    public decimal DiscountAmount { get; set; }      // Transaction-level discount

    // Metadata
    public string ProcessedBy { get; set; }          // Cashier username
    public string? Notes { get; set; }
    public int? OriginalTransactionId { get; set; }  // For refunds

    // Relationships
    public ICollection<POSTransactionDetail> TransactionDetails { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
```

**Header Financial Fields Are Calculated, Not Entered**
- Subtotal = Sum(Detail.Subtotal)
- TaxAmount = Sum(Detail.TaxAmount)
- TotalAmount = Sum(Detail.Total) - DiscountAmount
- DiscountAmount = Optional transaction-level discount

#### **POSTransactionDetail (Line Item - Source of Truth)**
```csharp
public class POSTransactionDetail
{
    // Parent Reference
    public int POSTransactionHeaderId { get; set; }

    // Product Reference
    public int ProductId { get; set; }

    // **PRODUCT SNAPSHOT (Denormalized - historical record)**
    public string ProductSKU { get; set; }           // Copy from Product at sale time
    public string ProductName { get; set; }          // Copy from Product at sale time
    public string? BatchNumber { get; set; }         // Cannabis compliance

    // **LINE ITEM CALCULATIONS (Source of Truth)**
    public int Quantity { get; set; }                // How many sold
    public decimal UnitPrice { get; set; }           // Price per unit (VAT-incl)
    public decimal LineDiscountAmount { get; set; }  // Per-line discount

    public decimal Subtotal { get; set; }            // Calculated: Qty × Price, excl VAT
    public decimal TaxAmount { get; set; }           // Calculated: VAT portion
    public decimal Total { get; set; }               // Calculated: Subtotal + Tax

    // Optional
    public decimal? CostPrice { get; set; }          // For profit analysis
    public string? Notes { get; set; }
}
```

**Detail Fields Are the Source of Truth**
- User enters: ProductId, Quantity
- System calculates: UnitPrice (from Product/Pricelist), Subtotal, TaxAmount, Total
- System copies: ProductSKU, ProductName, BatchNumber (denormalization)

---

## 💳 Multi-Tender Payment Processing

### **Tender Types (PaymentMethod Enum)**

**Current Tender Types**:
```csharp
public enum PaymentMethod
{
    Cash = 1,           // Physical money
    Card = 2,           // Credit/Debit (via PinPad)
    EFT = 3,            // Electronic bank transfer
    MobilePayment = 4,  // SnapScan, Zapper, etc.
    OnAccount = 5,      // Customer credit terms
    Voucher = 6         // Gift cards, vouchers
}
```

**ADDITION NEEDED**: Add **LoyaltyPoints** tender type

```csharp
public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    EFT = 3,
    MobilePayment = 4,
    OnAccount = 5,
    Voucher = 6,
    LoyaltyPoints = 7  // NEW: Customer loyalty program points
}
```

### **Split Payment Architecture**

**Principle**: One transaction can have multiple payments

**Example Scenario**:
```
Transaction Total: R500.00

Payment 1: Cash         R200.00
Payment 2: Card         R250.00
Payment 3: LoyaltyPoints R50.00
           -----------
Total Paid:             R500.00
```

**Implementation**:
```csharp
public class POSTransaction
{
    public POSTransactionHeader Header { get; set; }
    public List<POSTransactionDetail> Details { get; set; }
    public List<Payment> Payments { get; set; }

    public decimal TotalDue => Header.TotalAmount;
    public decimal TotalPaid => Payments.Where(p => p.IsSuccessful).Sum(p => p.Amount);
    public decimal Balance => TotalDue - TotalPaid;
    public bool IsFullyPaid => Balance <= 0.00m;
}
```

**Payment Workflow**:
1. Calculate transaction total
2. Accept first payment (e.g., Cash R200)
3. Show remaining balance (R300)
4. Accept second payment (e.g., Card R250)
5. Show remaining balance (R50)
6. Accept third payment (e.g., LoyaltyPoints R50)
7. Balance = R0.00 → Finalize transaction

**Change Calculation**:
```csharp
public class ChangeCalculator
{
    public decimal CalculateChange(decimal totalDue, List<Payment> payments)
    {
        decimal totalTendered = payments
            .Where(p => p.PaymentMethod == PaymentMethod.Cash)
            .Sum(p => p.Amount);

        decimal change = totalTendered - totalDue;
        return change > 0 ? change : 0.00m;
    }
}
```

---

## 📦 Barcode Scanning Architecture

### **Dual Barcode Strategy**

Cannabis retail needs to support:
1. **Standard EAN/UPC barcodes** - Product master (e.g., "Blue Dream 3.5g")
2. **Unique Serial Numbers** - Individual item tracking (e.g., "Batch BDR-2024-001, Item #42")

### **Product Barcode Table Structure**

```csharp
public class ProductBarcode : AuditableEntity
{
    public int Id { get; set; }

    // Product Reference
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }

    // Barcode Information
    public string BarcodeValue { get; set; }         // The scannable code
    public BarcodeType BarcodeType { get; set; }     // EAN13, UPC, Serial, QR, etc.
    public bool IsUnique { get; set; }               // True for serial numbers

    // Serial Number Tracking (if IsUnique = true)
    public string? SerialNumber { get; set; }        // Unique item identifier
    public string? BatchNumber { get; set; }         // Manufacturing batch
    public bool IsSold { get; set; }                 // Has this item been sold?
    public int? SoldInTransactionId { get; set; }    // Which transaction sold it
    public DateTime? SoldDate { get; set; }

    // Standard Barcode (if IsUnique = false)
    public bool IsActive { get; set; }               // Can this barcode be used?
    public bool IsPrimaryBarcode { get; set; }       // Default barcode for product
}

public enum BarcodeType
{
    EAN13 = 1,      // Standard retail barcode (13 digits)
    UPC = 2,        // Universal Product Code (12 digits)
    EAN8 = 3,       // Short EAN (8 digits)
    Code128 = 4,    // Alphanumeric barcode
    QRCode = 5,     // QR code (2D)
    DataMatrix = 6, // 2D matrix barcode
    Serial = 7      // Custom serial number format
}
```

### **Barcode Scanning Service**

```csharp
public interface IBarcodeScanService
{
    /// <summary>
    /// Scan a barcode and return product information
    /// </summary>
    Task<BarcodeScanResult> ScanBarcodeAsync(string barcodeValue);

    /// <summary>
    /// Check if a serial number has already been sold
    /// </summary>
    Task<bool> IsSerialNumberAvailableAsync(string serialNumber);

    /// <summary>
    /// Mark a serial number as sold (during transaction finalization)
    /// </summary>
    Task MarkSerialNumberAsSoldAsync(string serialNumber, int transactionId);
}

public class BarcodeScanResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    // Product Information
    public int ProductId { get; set; }
    public string ProductSKU { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    // Barcode Context
    public bool IsUniqueItem { get; set; }           // Serial number or standard?
    public string? SerialNumber { get; set; }
    public string? BatchNumber { get; set; }

    // Stock Availability
    public int StockOnHand { get; set; }
    public bool IsInStock { get; set; }

    // Compliance Warnings
    public bool HasExpiryWarning { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool RequiresAgeVerification { get; set; }
}
```

### **Scanning Workflow**

#### **Scenario 1: Standard Product Barcode (EAN)**
```
1. Cashier scans barcode "6001234567890"
2. System: Lookup ProductBarcode where BarcodeValue = "6001234567890"
3. Found: Product #42 "Blue Dream 3.5g"
4. Check: IsUnique = false (standard product barcode)
5. Check: StockOnHand = 10 (available)
6. Return: Product info + Price
7. Add to cart: Product #42, Qty = 1
8. On finalize: Deduct 1 from StockOnHand
```

#### **Scenario 2: Unique Serial Number Scan**
```
1. Cashier scans barcode "BD-2024-001-00042"
2. System: Lookup ProductBarcode where BarcodeValue = "BD-2024-001-00042"
3. Found: Product #42 "Blue Dream 3.5g", Serial #00042, Batch BD-2024-001
4. Check: IsUnique = true (unique item)
5. Check: IsSold = false (not yet sold)
6. Return: Product info + Serial info
7. Add to cart: Product #42, Qty = 1, SerialNumber = "00042"
8. On finalize: Mark ProductBarcode.IsSold = true, ProductBarcode.SoldInTransactionId = 123
```

#### **Scenario 3: Serial Number Already Sold**
```
1. Cashier scans "BD-2024-001-00042"
2. System: Found ProductBarcode
3. Check: IsSold = true, SoldDate = 2024-12-01, SoldInTransactionId = 99
4. Return: Error "This item was already sold on 2024-12-01 (Transaction #SALE-20241201-099)"
5. Alert cashier: Possible duplicate scan or stolen item
```

---

## 💳 PinPad Integration Architecture

### **PinPad Integration Service**

**Purpose**: Communicate with physical card payment devices (Yoco, PinPad, etc.)

```csharp
public interface IPinPadService
{
    /// <summary>
    /// Initialize connection to PinPad device
    /// </summary>
    Task<bool> ConnectAsync(string devicePort);

    /// <summary>
    /// Request payment from PinPad
    /// </summary>
    Task<PinPadPaymentResult> RequestPaymentAsync(decimal amount, string reference);

    /// <summary>
    /// Cancel pending payment request
    /// </summary>
    Task CancelPaymentAsync();

    /// <summary>
    /// Get device status (connected, ready, busy, error)
    /// </summary>
    Task<PinPadStatus> GetDeviceStatusAsync();

    /// <summary>
    /// Print receipt on PinPad printer (if available)
    /// </summary>
    Task<bool> PrintReceiptAsync(string receiptText);
}

public class PinPadPaymentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    // Payment Details
    public decimal AmountAuthorized { get; set; }
    public string AuthorizationCode { get; set; }      // Bank auth code
    public string TransactionReference { get; set; }   // PinPad transaction ID

    // Card Details (PCI-DSS Safe - masked only)
    public string MaskedCardNumber { get; set; }       // "****1234"
    public string CardType { get; set; }               // "Visa", "Mastercard"
    public string BankName { get; set; }

    // Compliance
    public DateTime TransactionDateTime { get; set; }
    public string TerminalId { get; set; }
    public string MerchantId { get; set; }
}

public enum PinPadStatus
{
    Disconnected,
    Connecting,
    Ready,
    Busy,
    Error,
    Offline
}
```

### **PinPad Workflow Integration**

```csharp
public class POSPaymentService
{
    private readonly IPinPadService _pinPadService;

    public async Task<Payment> ProcessCardPaymentAsync(
        POSTransactionHeader transaction,
        decimal amount)
    {
        // 1. Check PinPad status
        var status = await _pinPadService.GetDeviceStatusAsync();
        if (status != PinPadStatus.Ready)
        {
            throw new InvalidOperationException($"PinPad not ready: {status}");
        }

        // 2. Request payment from PinPad
        var reference = $"TXN-{transaction.TransactionNumber}";
        var result = await _pinPadService.RequestPaymentAsync(amount, reference);

        // 3. Create Payment record
        var payment = new Payment
        {
            TransactionHeaderId = transaction.Id,
            PaymentReference = GeneratePaymentReference(),
            PaymentDate = DateTime.Now,
            PaymentMethod = PaymentMethod.Card,
            Amount = result.AmountAuthorized,
            IsSuccessful = result.Success,

            // PinPad response data
            ExternalReference = result.AuthorizationCode,
            MaskedCardNumber = result.MaskedCardNumber,
            BankOrProvider = result.BankName,

            ProcessedBy = GetCurrentCashier(),
            Notes = $"Card: {result.CardType}, Auth: {result.AuthorizationCode}"
        };

        return payment;
    }
}
```

### **PinPad Device Configuration**

```json
{
  "PinPadSettings": {
    "Enabled": true,
    "DeviceType": "Yoco",
    "ConnectionType": "USB",
    "Port": "COM3",
    "Timeout": 60000,
    "AutoConnect": true,
    "RetryAttempts": 3,
    "PrintReceipt": true
  }
}
```

---

## 🏗️ Service Layer Scaffolding

### **Required BLL Services**

#### **1. POSTransactionService**
```csharp
public interface IPOSTransactionService
{
    // Transaction Management
    Task<POSTransactionDto> CreateTransactionAsync();
    Task<POSTransactionDto> GetTransactionAsync(int id);
    Task<bool> CancelTransactionAsync(int id, string reason);
    Task<bool> VoidTransactionAsync(int id, string reason);

    // Line Items
    Task<POSTransactionDetailDto> AddLineItemAsync(int transactionId, AddLineItemDto dto);
    Task<bool> UpdateLineItemQuantityAsync(int lineItemId, int newQuantity);
    Task<bool> RemoveLineItemAsync(int lineItemId);
    Task<decimal> ApplyLineDiscountAsync(int lineItemId, decimal discountAmount);

    // Calculations
    Task<POSTransactionTotalsDto> RecalculateTransactionAsync(int transactionId);
    Task<decimal> ApplyTransactionDiscountAsync(int transactionId, decimal discountPercentage);

    // Finalization
    Task<POSTransactionDto> FinalizeTransactionAsync(int transactionId, List<PaymentDto> payments);

    // Refunds
    Task<POSTransactionDto> CreateRefundAsync(int originalTransactionId, List<RefundLineDto> lines);
}
```

#### **2. POSPaymentService**
```csharp
public interface IPOSPaymentService
{
    // Payment Processing
    Task<PaymentDto> ProcessCashPaymentAsync(int transactionId, decimal amount, decimal tendered);
    Task<PaymentDto> ProcessCardPaymentAsync(int transactionId, decimal amount);
    Task<PaymentDto> ProcessEFTPaymentAsync(int transactionId, decimal amount, string reference);
    Task<PaymentDto> ProcessLoyaltyPointsAsync(int transactionId, decimal amount, int customerId);

    // Payment Management
    Task<List<PaymentDto>> GetTransactionPaymentsAsync(int transactionId);
    Task<decimal> GetTotalPaidAsync(int transactionId);
    Task<decimal> GetBalanceAsync(int transactionId);
    Task<bool> VoidPaymentAsync(int paymentId, string reason);

    // Change Calculation
    Task<ChangeDto> CalculateChangeAsync(int transactionId, List<PaymentDto> payments);
}
```

#### **3. BarcodeScanService**
```csharp
public interface IBarcodeScanService
{
    Task<BarcodeScanResult> ScanBarcodeAsync(string barcodeValue);
    Task<bool> IsSerialNumberAvailableAsync(string serialNumber);
    Task MarkSerialNumberAsSoldAsync(string serialNumber, int transactionId);
    Task<List<ProductBarcodeDto>> GetProductBarcodesAsync(int productId);
    Task<ProductBarcodeDto> RegisterBarcodeAsync(RegisterBarcodeDto dto);
}
```

#### **4. PinPadService**
```csharp
public interface IPinPadService
{
    Task<bool> ConnectAsync(string devicePort);
    Task<PinPadPaymentResult> RequestPaymentAsync(decimal amount, string reference);
    Task CancelPaymentAsync();
    Task<PinPadStatus> GetDeviceStatusAsync();
    Task<bool> PrintReceiptAsync(string receiptText);
    Task DisconnectAsync();
}
```

#### **5. POSCalculationService**
```csharp
public interface IPOSCalculationService
{
    // Line Item Calculations
    POSTransactionDetail CalculateLineItem(decimal unitPriceInclVAT, int quantity);
    decimal CalculateLineSubtotal(decimal unitPrice, int quantity);
    decimal CalculateLineVAT(decimal lineTotal);

    // Header Calculations
    void CalculateHeaderTotals(POSTransactionHeader header);
    decimal CalculateSubtotal(List<POSTransactionDetail> details);
    decimal CalculateTotalVAT(List<POSTransactionDetail> details);
    decimal CalculateTotalAmount(List<POSTransactionDetail> details);

    // Discount Calculations
    decimal ApplyPercentageDiscount(decimal amount, decimal percentage);
    decimal ApplyFixedDiscount(decimal amount, decimal discountAmount);

    // Rounding
    decimal RoundToNearestCent(decimal amount);
    decimal CalculateRoundingAdjustment(decimal expectedTotal, decimal calculatedTotal);
}
```

### **Required DAL Repositories**

```csharp
// POSTransactionHeader Repository
public interface IPOSTransactionHeaderRepository : IRepository<POSTransactionHeader>
{
    Task<POSTransactionHeader?> GetByTransactionNumberAsync(string transactionNumber);
    Task<List<POSTransactionHeader>> GetTransactionsByDateAsync(DateTime date);
    Task<List<POSTransactionHeader>> GetPendingTransactionsAsync();
}

// POSTransactionDetail Repository
public interface IPOSTransactionDetailRepository : IRepository<POSTransactionDetail>
{
    Task<List<POSTransactionDetail>> GetByTransactionIdAsync(int transactionId);
    Task<decimal> GetTransactionTotalAsync(int transactionId);
}

// Payment Repository
public interface IPaymentRepository : IRepository<Payment>
{
    Task<List<Payment>> GetByTransactionIdAsync(int transactionId);
    Task<List<Payment>> GetCashPaymentsByDateAsync(DateTime date);
    Task<List<Payment>> GetFICReportingRequiredAsync();
}

// ProductBarcode Repository
public interface IProductBarcodeRepository : IRepository<ProductBarcode>
{
    Task<ProductBarcode?> GetByBarcodeValueAsync(string barcodeValue);
    Task<List<ProductBarcode>> GetByProductIdAsync(int productId);
    Task<ProductBarcode?> GetBySerialNumberAsync(string serialNumber);
    Task<List<ProductBarcode>> GetUnsoldSerialNumbersAsync(int productId);
}
```

---

## 📋 Implementation Checklist

### Phase 1: Foundation (Week 1)
- [ ] Add `LoyaltyPoints` to PaymentMethod enum
- [ ] Create `ProductBarcode` entity and table
- [ ] Create `BarcodeType` enum
- [ ] Update database migration
- [ ] Create `POSCalculationService` (VAT calculations)
- [ ] Write unit tests for calculation service

### Phase 2: Repositories (Week 1-2)
- [ ] Implement `IPOSTransactionHeaderRepository`
- [ ] Implement `IPOSTransactionDetailRepository`
- [ ] Implement `IPaymentRepository`
- [ ] Implement `IProductBarcodeRepository`
- [ ] Write repository unit tests

### Phase 3: Core Services (Week 2-3)
- [ ] Implement `IPOSTransactionService`
- [ ] Implement `IPOSPaymentService`
- [ ] Implement `IBarcodeScanService`
- [ ] Create DTOs for all services
- [ ] Create FluentValidation validators
- [ ] Write service unit tests

### Phase 4: PinPad Integration (Week 3)
- [ ] Implement `IPinPadService` (mock version first)
- [ ] Create PinPad configuration settings
- [ ] Implement device connection/status checks
- [ ] Implement payment request flow
- [ ] Add error handling and timeouts
- [ ] Write integration tests

### Phase 5: UI Components (Week 4-5)
- [ ] Create POS transaction page (product selection)
- [ ] Create barcode scanner input component
- [ ] Create shopping cart component
- [ ] Create payment tender selection component
- [ ] Create split payment dialog
- [ ] Create receipt preview/print component
- [ ] Create age verification dialog

### Phase 6: Testing & Refinement (Week 5-6)
- [ ] End-to-end transaction flow testing
- [ ] VAT calculation accuracy testing
- [ ] Barcode scanning integration testing
- [ ] PinPad device testing (with real hardware)
- [ ] Performance testing (large transactions)
- [ ] Cannabis compliance verification

---

## 🎯 Success Criteria

### Technical
- [ ] Transaction totals accurate to the cent (zero rounding errors)
- [ ] Support for 10+ payment tenders per transaction
- [ ] Barcode scanning < 500ms response time
- [ ] PinPad payment < 30 seconds timeout
- [ ] VAT calculation variance < R0.05 per transaction

### Functional
- [ ] Can process walk-in cash sale
- [ ] Can process account customer sale
- [ ] Can apply multiple pricelists
- [ ] Can split payments (cash + card + points)
- [ ] Can scan standard EAN barcodes
- [ ] Can scan unique serial number barcodes
- [ ] Can process card payments via PinPad
- [ ] Can handle refunds
- [ ] Can void transactions

### Compliance
- [ ] Age verification enforced (18+ cannabis sales)
- [ ] Batch numbers tracked per line item
- [ ] FIC reporting flagged for cash > R25,000
- [ ] Audit trail complete (who sold what when)
- [ ] VAT calculations SARS-compliant

---

## 📚 References

### South African Regulations
- **VAT Act**: https://www.sars.gov.za/types-of-tax/value-added-tax/
- **FIC Act**: Cash reporting requirements
- **Cannabis Act 2024**: Compliance requirements
- **POPIA**: Data protection and audit trails

### Technical Standards
- **PCI-DSS**: Payment card security
- **EAN/UPC**: Barcode standards
- **ISO 8601**: Date/time formats

---

**Document Status**: Ready for Review & Approval
**Next Step**: Begin Phase 1 implementation after approval
**Estimated Timeline**: 6 weeks to full POS module completion
