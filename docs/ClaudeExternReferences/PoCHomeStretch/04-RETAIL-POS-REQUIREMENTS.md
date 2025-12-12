# 04 - RETAIL POS REQUIREMENTS SPECIFICATION

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: PoC Specification - Market Entry Focus (85-90%)
**Related Documents**:
- [00-MAIN-SPECIFICATION.md](00-MAIN-SPECIFICATION.md) - Central specification index
- [01-MODULE-DESCRIPTIONS.md](01-MODULE-DESCRIPTIONS.md) - All module overviews
- [02-INVENTORY-MODULE-SPEC.md](02-INVENTORY-MODULE-SPEC.md) - Inventory foundation
- [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md) - Movement system backbone
- [10-GLOSSARY.md](10-GLOSSARY.md) - Terminology reference

---

## DOCUMENT PURPOSE

This document provides the **enterprise-grade specification** for the **Retail POS Module** - the primary market entry point for Project420. The Retail POS module enables cannabis dispensaries to:

- **Process customer sales** with full compliance tracking
- **Generate serial numbers** for pre-roll products (retail production integration)
- **Track batch-to-customer traceability** (seed-to-sale compliance)
- **Handle returns and refunds** with proper movement reversal
- **Integrate with payment systems** (cash, card, digital)
- **Generate compliant receipts** (SAHPRA, DALRRD, VAT)
- **Support loyalty programs** (future enhancement)

**PoC Completeness Target**: 85-90% (Market Entry Ready)

**Critical Success Factors**:
1. Fast transaction processing (<5 seconds from scan to receipt)
2. 100% accurate inventory movements on every sale
3. Full batch/serial number traceability
4. Cannabis compliance (ID verification, daily limits, tax calculations)
5. Reliable operation (99.9% uptime during business hours)

---

## TABLE OF CONTENTS

1. [Executive Summary](#1-executive-summary)
2. [Module Architecture](#2-module-architecture)
3. [Core Entities](#3-core-entities)
4. [Transaction Workflow](#4-transaction-workflow)
5. [Product Scanning & Selection](#5-product-scanning--selection)
6. [Pricing & Discounts](#6-pricing--discounts)
7. [Tax Calculation (VAT)](#7-tax-calculation-vat)
8. [Payment Processing](#8-payment-processing)
9. [Receipt Generation](#9-receipt-generation)
10. [Returns & Refunds](#10-returns--refunds)
11. [Cannabis Compliance](#11-cannabis-compliance)
12. [Customer Management](#12-customer-management)
13. [Retail Production Integration](#13-retail-production-integration)
14. [Reporting & Analytics](#14-reporting--analytics)
15. [Data Access Layer](#15-data-access-layer)
16. [Business Logic Layer](#16-business-logic-layer)
17. [API Endpoints](#17-api-endpoints)
18. [Validation Rules](#18-validation-rules)
19. [Testing Strategy](#19-testing-strategy)
20. [Implementation Roadmap](#20-implementation-roadmap)

---

## 1. EXECUTIVE SUMMARY

### 1.1 Module Overview

The **Retail POS Module** is Project420's **primary revenue driver** and **market entry point**. It transforms the system from an internal inventory tracker into a **customer-facing sales platform** for cannabis dispensaries.

**Key Capabilities**:
- **Fast Checkout**: Barcode scanning, quick product lookup, <5 second transaction time
- **Compliance**: ID verification, daily purchase limits, batch traceability, compliant receipts
- **Inventory Integration**: Every sale creates OUT movements (reduces SOH)
- **Retail Production**: On-demand serial number generation for pre-rolls
- **Multi-Payment**: Cash, card, split payments, change calculation
- **Returns**: Full refund workflow with movement reversal

**Value Proposition**:
> "Project420 Retail POS enables South African cannabis dispensaries to sell legally and compliantly, with full seed-to-sale traceability and real-time inventory visibility."

### 1.2 Current Implementation Status

**Implemented (✅)**:
- Basic transaction model (header + details)
- Product lookup by code
- Basic pricing calculations
- Transaction number generation
- Simple receipt generation

**Partially Implemented (🟡)**:
- Movement generation (needs consistency with Option A architecture)
- Batch/serial number tracking (basic, needs enhancement)
- Tax calculations (VAT implemented, needs verification)
- Payment processing (basic, needs multi-payment support)

**Not Yet Implemented (❌)**:
- Barcode scanning (UI integration)
- Discount engine (promotions, bulk discounts)
- Returns workflow (movement reversal)
- Customer daily limit tracking (compliance)
- ID verification integration
- Loyalty program
- Advanced reporting (sales analytics, product performance)

### 1.3 PoC Completion Target (85-90%)

To achieve 85-90% completeness by Phase 12 (Week 6), the Retail POS Module must deliver:

**Must Have (85-90% Target)**:
- ✅ Fast transaction processing (<5 seconds)
- ✅ Barcode scanning for product lookup
- ✅ Accurate pricing with discounts (line-level and transaction-level)
- ✅ VAT calculation (15% SA standard rate)
- ✅ Multi-payment support (cash, card, split payments)
- ✅ Compliant receipt generation (SAHPRA, DALRRD, VAT)
- ✅ Returns and refunds workflow
- ✅ Movement generation (OUT movements on sale, IN movements on return)
- ✅ Batch/serial number capture (traceability)
- ✅ Basic customer tracking (optional registration)
- ✅ Daily sales reporting
- ✅ Cashier performance tracking
- ✅ End-of-day reconciliation

**Nice to Have (Beyond 90%)**:
- ⚪ Loyalty program (points, rewards)
- ⚪ Gift cards / vouchers
- ⚪ Layaway / hold transactions
- ⚪ Advanced promotions (buy-one-get-one, tiered discounts)
- ⚪ Customer purchase history analytics
- ⚪ Real-time sales dashboard

---

## 2. MODULE ARCHITECTURE

### 2.1 Architectural Principles

The Retail POS Module follows **Project420's 3-tier architecture** with **Option A Movement Architecture**:

```
┌─────────────────────────────────────────────────────────────┐
│                        UI LAYER                              │
│          (Blazor Server - POS Terminal Interface)           │
│                                                              │
│  ┌────────────────┬──────────────────┬───────────────────┐ │
│  │ Product Scan   │ Transaction View │ Payment Screen    │ │
│  │ - Barcode input│ - Line items     │ - Cash/Card/Split │ │
│  │ - Quick search │ - Totals         │ - Change calc     │ │
│  │ - Price display│ - Customer info  │ - Receipt print   │ │
│  └────────────────┴──────────────────┴───────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LOGIC LAYER                       │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │RetailTransService│ PricingService   │PaymentService   │ │
│  │                  │                  │                 │ │
│  │- CreateSale()    │- CalculatePrice()│- ProcessCash()  │ │
│  │- AddLineItem()   │- ApplyDiscount() │- ProcessCard()  │ │
│  │- ProcessReturn() │- CalculateVAT()  │- SplitPayment() │ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │MovementService   │ReceiptService    │ComplianceService│ │
│  │                  │                  │                 │ │
│  │- CreateMovement()│- GenerateReceipt()│- VerifyID()    │ │
│  │- ReverseMovement│- PrintReceipt()  │- CheckLimits()  │ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                         │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │RetailTransRepo   │TransDetailRepo   │PaymentRepo      │ │
│  │                  │                  │                 │ │
│  │IRetailTransRepo  │ITransDetailRepo  │IPaymentRepo     │ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │MovementRepo      │CustomerRepo      │ReceiptRepo      │ │
│  │                  │                  │                 │ │
│  │IMovementRepo     │ICustomerRepo     │IReceiptRepo     │ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       MODEL LAYER                            │
│                                                              │
│  RetailTransactionHeader, TransactionDetails, Payments,     │
│  Customers, Receipts, Movements                              │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       DATABASE                               │
│                    PostgreSQL 17                             │
│              Database: project420_retail                     │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Key Design Decisions

**Decision 1: Option A Movement Architecture**
- **Rationale**: Retail transactions use `RetailTransactionHeaders` + unified `TransactionDetails` table
- **Impact**: Consistent movement generation across all modules
- **Implementation**: See [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md)

**Decision 2: Serial Number Generation at POS**
- **Rationale**: Pre-rolls are produced in bulk but serialized at point-of-sale
- **Impact**: POS system must integrate with `SerialNumberService`
- **Implementation**: Generate 13-digit short serial numbers on-demand during checkout

**Decision 3: Multi-Payment Support**
- **Rationale**: Customers may pay with cash + card, or split payments
- **Impact**: Payments stored in separate `Payments` table (1-to-many with transaction)
- **Implementation**: Transaction not complete until `SUM(Payments) = TotalAmount`

**Decision 4: Returns as Reverse Transactions**
- **Rationale**: Returns are separate transactions (not modifications of original)
- **Impact**: Original transaction remains immutable; return creates new transaction with negative values
- **Implementation**: Return transaction links to original via `OriginalTransactionId`

**Decision 5: Soft Delete + 7-Year Retention**
- **Rationale**: Tax compliance (SARS requires 7-year financial record retention)
- **Impact**: Transactions never hard deleted; `IsDeleted` flag + retention policy
- **Implementation**: Scheduled job archives old transactions after 7 years

---

## 3. CORE ENTITIES

### 3.1 RetailTransactionHeader

**Purpose**: Header for every retail sale (POS transaction).

**Schema**:
```sql
CREATE TABLE RetailTransactionHeaders (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    TransactionNumber NVARCHAR(100) NOT NULL UNIQUE, -- e.g., "SALE-2024-12345"

    -- Transaction details
    TransactionDate DATETIME NOT NULL DEFAULT GETDATE(),
    TransactionType NVARCHAR(50) NOT NULL DEFAULT 'Sale', -- 'Sale', 'Return', 'Void'
    OriginalTransactionId INT NULL, -- For returns, links to original sale

    -- Location & Staff
    SiteId INT NOT NULL,
    RegisterId INT NULL, -- POS terminal ID
    CashierId INT NOT NULL, -- Staff member processing sale

    -- Customer (optional for walk-in)
    CustomerId INT NULL,
    CustomerName NVARCHAR(200) NULL, -- For non-registered customers

    -- Financial totals
    SubTotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    VATAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,

    -- Payment status
    AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    AmountDue DECIMAL(18,2) NOT NULL, -- TotalAmount - AmountPaid
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Paid', 'PartiallyPaid', 'Refunded'

    -- Compliance (cannabis)
    CustomerIDVerified BIT NOT NULL DEFAULT 0,
    CustomerIDNumber NVARCHAR(50) NULL,
    DailyLimitChecked BIT NOT NULL DEFAULT 0,

    -- Receipt
    ReceiptNumber NVARCHAR(100) NULL,
    ReceiptPrinted BIT NOT NULL DEFAULT 0,

    -- Notes
    Notes NVARCHAR(1000) NULL,

    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- 'Active', 'Void', 'Returned'
    IsDeleted BIT NOT NULL DEFAULT 0,

    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_RetailTrans_Site FOREIGN KEY (SiteId) REFERENCES Sites(SiteId),
    CONSTRAINT FK_RetailTrans_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_RetailTrans_Original FOREIGN KEY (OriginalTransactionId)
        REFERENCES RetailTransactionHeaders(TransactionId)
);

CREATE INDEX IX_RetailTrans_Number ON RetailTransactionHeaders(TransactionNumber);
CREATE INDEX IX_RetailTrans_Date ON RetailTransactionHeaders(TransactionDate);
CREATE INDEX IX_RetailTrans_Customer ON RetailTransactionHeaders(CustomerId);
CREATE INDEX IX_RetailTrans_Cashier ON RetailTransactionHeaders(CashierId);
CREATE INDEX IX_RetailTrans_Status ON RetailTransactionHeaders(Status, PaymentStatus);
```

**C# Entity**:
```csharp
public class RetailTransactionHeader : AuditableEntity
{
    public int TransactionId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;

    // Transaction details
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public TransactionType TransactionType { get; set; } = TransactionType.Sale;
    public int? OriginalTransactionId { get; set; }
    public RetailTransactionHeader? OriginalTransaction { get; set; }

    // Location & Staff
    public int SiteId { get; set; }
    public Site Site { get; set; } = null!;
    public int? RegisterId { get; set; }
    public int CashierId { get; set; }

    // Customer
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerName { get; set; }

    // Financial totals
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VATAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Payment status
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    // Compliance
    public bool CustomerIDVerified { get; set; }
    public string? CustomerIDNumber { get; set; }
    public bool DailyLimitChecked { get; set; }

    // Receipt
    public string? ReceiptNumber { get; set; }
    public bool ReceiptPrinted { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Status
    public string Status { get; set; } = "Active";

    // Navigation properties
    public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum TransactionType
{
    Sale,
    Return,
    Void
}

public enum PaymentStatus
{
    Pending,
    PartiallyPaid,
    Paid,
    Refunded
}
```

### 3.2 TransactionDetail (Unified - Option A)

**Purpose**: Line items for ALL transaction types (Retail, Purchase, Production, etc.).

**Schema**:
```sql
CREATE TABLE TransactionDetails (
    TransactionDetailId INT IDENTITY(1,1) PRIMARY KEY,

    -- Transaction linkage
    HeaderId INT NOT NULL, -- TransactionId from RetailTransactionHeaders, PurchaseHeaders, etc.
    TransactionType NVARCHAR(50) NOT NULL, -- 'Retail', 'Purchase', 'Production', 'StockAdjustment'

    -- Product details
    ProductId INT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Pricing
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    VATAmount DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL, -- (UnitPrice * Quantity) - DiscountAmount + VATAmount

    -- Traceability
    BatchNumber NVARCHAR(100) NULL,
    SerialNumber NVARCHAR(50) NULL,

    -- Notes
    Notes NVARCHAR(500) NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_TransDetail_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE INDEX IX_TransDetail_Header ON TransactionDetails(HeaderId, TransactionType);
CREATE INDEX IX_TransDetail_Product ON TransactionDetails(ProductId);
CREATE INDEX IX_TransDetail_Batch ON TransactionDetails(BatchNumber) WHERE BatchNumber IS NOT NULL;
CREATE INDEX IX_TransDetail_Serial ON TransactionDetails(SerialNumber) WHERE SerialNumber IS NOT NULL;
```

**C# Entity**:
```csharp
public class TransactionDetail : AuditableEntity
{
    public int TransactionDetailId { get; set; }

    // Transaction linkage
    public int HeaderId { get; set; }
    public string TransactionType { get; set; } = string.Empty;

    // Product details
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Pricing
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VATAmount { get; set; }
    public decimal LineTotal { get; set; }

    // Traceability
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Notes
    public string? Notes { get; set; }
}
```

### 3.3 Payment

**Purpose**: Tracks all payments for a transaction (supports multi-payment).

**Schema**:
```sql
CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    TransactionId INT NOT NULL,

    -- Payment details
    PaymentMethod NVARCHAR(50) NOT NULL, -- 'Cash', 'Card', 'DigitalWallet', 'GiftCard'
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),

    -- Cash-specific
    CashTendered DECIMAL(18,2) NULL,
    CashChange DECIMAL(18,2) NULL,

    -- Card-specific
    CardType NVARCHAR(50) NULL, -- 'Visa', 'Mastercard', 'Amex'
    CardLast4Digits NVARCHAR(4) NULL,
    AuthorizationCode NVARCHAR(100) NULL,
    TransactionReference NVARCHAR(200) NULL,

    -- Digital wallet
    WalletProvider NVARCHAR(100) NULL, -- 'SnapScan', 'Zapper', 'PayPal'

    -- Status
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Completed', -- 'Pending', 'Completed', 'Failed', 'Refunded'

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_Payment_Transaction FOREIGN KEY (TransactionId)
        REFERENCES RetailTransactionHeaders(TransactionId)
);

CREATE INDEX IX_Payment_Transaction ON Payments(TransactionId);
CREATE INDEX IX_Payment_Method ON Payments(PaymentMethod);
CREATE INDEX IX_Payment_Date ON Payments(PaymentDate);
```

**C# Entity**:
```csharp
public class Payment : AuditableEntity
{
    public int PaymentId { get; set; }
    public int TransactionId { get; set; }
    public RetailTransactionHeader Transaction { get; set; } = null!;

    // Payment details
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    // Cash-specific
    public decimal? CashTendered { get; set; }
    public decimal? CashChange { get; set; }

    // Card-specific
    public string? CardType { get; set; }
    public string? CardLast4Digits { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? TransactionReference { get; set; }

    // Digital wallet
    public string? WalletProvider { get; set; }

    // Status
    public string PaymentStatus { get; set; } = "Completed";
}

public enum PaymentMethod
{
    Cash,
    Card,
    DigitalWallet,
    GiftCard,
    Other
}
```

### 3.4 Customer

**Purpose**: Optional customer registration (loyalty, purchase history, compliance tracking).

**Schema**:
```sql
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerNumber NVARCHAR(100) NOT NULL UNIQUE, -- e.g., "CUST-000001"

    -- Personal details
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NULL,
    PhoneNumber NVARCHAR(20) NULL,

    -- Address
    AddressLine1 NVARCHAR(200) NULL,
    AddressLine2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,

    -- ID verification (compliance)
    IDNumber NVARCHAR(50) NULL,
    IDType NVARCHAR(50) NULL, -- 'SA ID', 'Passport', 'DriverLicense'
    DateOfBirth DATE NULL,
    IDVerified BIT NOT NULL DEFAULT 0,
    IDVerifiedDate DATETIME NULL,

    -- Loyalty (future)
    LoyaltyPoints INT NOT NULL DEFAULT 0,
    LoyaltyTier NVARCHAR(50) NULL, -- 'Bronze', 'Silver', 'Gold'

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL,

    CONSTRAINT UK_Customer_Email UNIQUE (Email) WHERE Email IS NOT NULL,
    CONSTRAINT UK_Customer_IDNumber UNIQUE (IDNumber) WHERE IDNumber IS NOT NULL
);

CREATE INDEX IX_Customer_Number ON Customers(CustomerNumber);
CREATE INDEX IX_Customer_Name ON Customers(LastName, FirstName);
CREATE INDEX IX_Customer_Phone ON Customers(PhoneNumber);
```

---

## 4. TRANSACTION WORKFLOW

### 4.1 Standard Sale Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                    RETAIL SALE WORKFLOW                      │
└─────────────────────────────────────────────────────────────┘

1. START TRANSACTION
   ├── Cashier logs in
   ├── Create new RetailTransactionHeader (Status = 'Pending')
   └── Generate TransactionNumber

2. ADD LINE ITEMS
   ├── Scan barcode / search product
   ├── Check SOH (sufficient stock?)
   ├── Get price (StandardSellPrice or promotion price)
   ├── Create TransactionDetail
   │   ├── ProductId, Quantity, UnitPrice
   │   ├── Apply discounts (if any)
   │   ├── Calculate VAT (15%)
   │   ├── Calculate LineTotal
   │   └── Capture BatchNumber (and SerialNumber if applicable)
   └── Update Transaction totals (SubTotal, VATAmount, TotalAmount)

3. FINALIZE TRANSACTION
   ├── Apply transaction-level discounts (if any)
   ├── Recalculate totals
   └── Display total to customer

4. PROCESS PAYMENT
   ├── Enter payment method (Cash / Card / Split)
   ├── For Cash:
   │   ├── Enter amount tendered
   │   ├── Calculate change
   │   └── Create Payment record
   ├── For Card:
   │   ├── Process card authorization (external gateway)
   │   ├── Store authorization code
   │   └── Create Payment record
   └── Update TransactionHeader:
       ├── AmountPaid = SUM(Payments.Amount)
       ├── AmountDue = TotalAmount - AmountPaid
       └── PaymentStatus = 'Paid' (if AmountDue = 0)

5. GENERATE MOVEMENTS
   ├── For each TransactionDetail:
   │   └── Create OUT movement:
   │       ├── ProductId, SiteId
   │       ├── MovementType = 'Sale'
   │       ├── MovementDirection = 'OUT'
   │       ├── Quantity, BatchNumber, SerialNumber
   │       └── TransactionId, TransactionDetailId
   └── SOH automatically reduced

6. GENERATE RECEIPT
   ├── Create receipt data (header + lines + payments + compliance info)
   ├── Generate receipt number
   ├── Print receipt
   └── Update TransactionHeader (ReceiptPrinted = true)

7. COMPLETE TRANSACTION
   └── Update TransactionHeader (Status = 'Completed')

END TRANSACTION
```

### 4.2 Transaction State Machine

```
┌──────────┐
│ Pending  │ ← Transaction created
└────┬─────┘
     │ Add line items
     ↓
┌──────────┐
│ In Progress │
└────┬─────┘
     │ Process payment
     ↓
┌──────────┐
│   Paid   │ ← Payment successful
└────┬─────┘
     │ Generate movements + receipt
     ↓
┌──────────┐
│Completed │ ← Transaction finalized
└────┬─────┘
     │
     ├──→ (If returned) ┌──────────┐
     │                  │ Returned │
     │                  └──────────┘
     │
     └──→ (If cancelled)┌──────────┐
                        │   Void   │
                        └──────────┘
```

---

## 5. PRODUCT SCANNING & SELECTION

### 5.1 Barcode Scanning

**Barcode Types Supported**:
- **EAN-13**: Standard product barcodes (13 digits)
- **Short Serial Number**: Project420 custom 13-digit serial (for pre-rolls)
- **QR Code**: Batch number + product code (future)

**Barcode Lookup Logic**:
```csharp
public async Task<Product?> LookupProductByBarcodeAsync(string barcode)
{
    // 1. Try lookup by ProductCode (standard barcode)
    var product = await _productRepo.GetByCodeAsync(barcode);
    if (product != null) return product;

    // 2. Try lookup by SerialNumber (if 13-digit serial)
    if (barcode.Length == 13 && IsValidSerialNumber(barcode))
    {
        var serial = await _serialRepo.GetBySerialNumberAsync(barcode);
        if (serial != null)
        {
            product = await _productRepo.GetByIdAsync(serial.ProductId);
            return product;
        }
    }

    // 3. Not found
    return null;
}
```

### 5.2 Quick Product Search

**Search Criteria**:
- Product code (exact or partial match)
- Product name (fuzzy search)
- Category (filter by cannabis type: Indica, Sativa, Hybrid)
- Strain name

**Search Implementation**:
```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
{
    return await _productRepo.SearchAsync(p =>
        p.ProductCode.Contains(searchTerm) ||
        p.ProductName.Contains(searchTerm) ||
        (p.StrainName != null && p.StrainName.Contains(searchTerm))
    );
}
```

---

## 6. PRICING & DISCOUNTS

### 6.1 Pricing Model

**Price Hierarchy** (highest to lowest priority):
1. **Promotion Price**: Active promotion for product (time-limited)
2. **Customer Tier Price**: Loyalty tier pricing (Gold, Silver, Bronze)
3. **Bulk Discount Price**: Quantity-based discount (e.g., 10+ units = 10% off)
4. **Standard Sell Price**: `Product.StandardSellPrice`

**Pricing Logic**:
```csharp
public async Task<decimal> GetPriceAsync(int productId, int quantity, int? customerId = null)
{
    var product = await _productRepo.GetByIdAsync(productId);
    if (product == null)
        throw new NotFoundException($"Product {productId} not found");

    // 1. Check for active promotion
    var promotion = await _promotionService.GetActivePromotionAsync(productId);
    if (promotion != null)
        return promotion.PromotionPrice;

    // 2. Check customer tier pricing
    if (customerId.HasValue)
    {
        var customer = await _customerRepo.GetByIdAsync(customerId.Value);
        if (customer?.LoyaltyTier != null)
        {
            var tierPrice = await GetTierPriceAsync(productId, customer.LoyaltyTier);
            if (tierPrice.HasValue)
                return tierPrice.Value;
        }
    }

    // 3. Check bulk discount
    var bulkPrice = await GetBulkPriceAsync(productId, quantity);
    if (bulkPrice.HasValue)
        return bulkPrice.Value;

    // 4. Default to standard price
    return product.StandardSellPrice;
}
```

### 6.2 Discount Types

**Line-Level Discounts**:
- Applied to individual line items
- Examples: "10% off this product", "R50 off per unit"
- Stored in `TransactionDetail.DiscountAmount`

**Transaction-Level Discounts**:
- Applied to entire transaction
- Examples: "R100 off total", "5% off entire purchase"
- Stored in `RetailTransactionHeader.DiscountAmount`

**Discount Calculation**:
```csharp
public void ApplyLineDiscount(TransactionDetail line, decimal discountPercent)
{
    var originalLineTotal = line.UnitPrice * line.Quantity;
    var discountAmount = originalLineTotal * (discountPercent / 100m);

    line.DiscountAmount = discountAmount;
    line.LineTotal = originalLineTotal - discountAmount + line.VATAmount;
}

public void ApplyTransactionDiscount(RetailTransactionHeader transaction, decimal discountAmount)
{
    transaction.DiscountAmount = discountAmount;
    transaction.TotalAmount = transaction.SubTotal - discountAmount + transaction.VATAmount;
}
```

---

## 7. TAX CALCULATION (VAT)

### 7.1 South African VAT

**Standard Rate**: 15% (as of 2025)

**VAT Calculation Method**: **Tax-Inclusive Pricing**
- Shelf prices include VAT
- VAT extracted from total: `VAT = Total / 1.15 * 0.15`

**Alternative Method**: **Tax-Exclusive Pricing**
- Shelf prices exclude VAT
- VAT added to total: `VAT = SubTotal * 0.15`

**Project420 Implementation**: **Tax-Inclusive** (standard for retail)

### 7.2 VAT Calculation Logic

**Line-Level VAT**:
```csharp
public decimal CalculateLineVAT(decimal unitPrice, decimal quantity, decimal discountAmount)
{
    const decimal VAT_RATE = 0.15m;

    // Gross amount (includes VAT)
    var grossAmount = (unitPrice * quantity) - discountAmount;

    // Extract VAT (tax-inclusive)
    var vatAmount = grossAmount / (1 + VAT_RATE) * VAT_RATE;

    return Math.Round(vatAmount, 2);
}
```

**Transaction-Level VAT Recalculation**:
```csharp
public void RecalculateTransactionTotals(RetailTransactionHeader transaction)
{
    const decimal VAT_RATE = 0.15m;

    // Sum all line totals (excluding VAT)
    var subTotal = transaction.TransactionDetails.Sum(d =>
    {
        var grossAmount = (d.UnitPrice * d.Quantity) - d.DiscountAmount;
        var netAmount = grossAmount / (1 + VAT_RATE);
        return netAmount;
    });

    // Apply transaction-level discount
    subTotal -= transaction.DiscountAmount;

    // Calculate total VAT
    var vatAmount = subTotal * VAT_RATE;

    // Update transaction
    transaction.SubTotal = Math.Round(subTotal, 2);
    transaction.VATAmount = Math.Round(vatAmount, 2);
    transaction.TotalAmount = transaction.SubTotal + transaction.VATAmount;
}
```

### 7.3 VAT Compliance

**Receipt Requirements** (SARS):
- Vendor name and VAT registration number
- Invoice number (transaction number)
- Date of supply
- Description of goods
- **VAT amount clearly shown**
- Total amount including VAT

---

## 8. PAYMENT PROCESSING

### 8.1 Payment Methods

**Supported Payment Methods**:
1. **Cash**: Manual entry, change calculation
2. **Card** (Credit/Debit): External payment gateway integration
3. **Digital Wallet**: SnapScan, Zapper, PayPal (future)
4. **Gift Card**: Store credit (future)
5. **Split Payment**: Combination of above

### 8.2 Cash Payment

**Cash Payment Workflow**:
```csharp
public async Task<Payment> ProcessCashPaymentAsync(int transactionId, decimal cashTendered, string userId)
{
    var transaction = await _transactionRepo.GetByIdAsync(transactionId);
    if (transaction == null)
        throw new NotFoundException($"Transaction {transactionId} not found");

    // Calculate change
    var amountDue = transaction.TotalAmount - transaction.AmountPaid;
    if (cashTendered < amountDue)
        throw new ValidationException($"Insufficient payment. Due: R{amountDue}, Tendered: R{cashTendered}");

    var changeAmount = cashTendered - amountDue;

    // Create payment record
    var payment = new Payment
    {
        TransactionId = transactionId,
        PaymentMethod = PaymentMethod.Cash,
        Amount = amountDue,
        CashTendered = cashTendered,
        CashChange = changeAmount,
        PaymentStatus = "Completed",
        CreatedBy = userId
    };

    await _paymentRepo.AddAsync(payment);

    // Update transaction
    transaction.AmountPaid += payment.Amount;
    transaction.AmountDue = transaction.TotalAmount - transaction.AmountPaid;
    transaction.PaymentStatus = transaction.AmountDue == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;

    await _transactionRepo.UpdateAsync(transaction);
    await _transactionRepo.SaveChangesAsync();

    return payment;
}
```

### 8.3 Card Payment

**Card Payment Workflow** (external gateway integration):
```csharp
public async Task<Payment> ProcessCardPaymentAsync(int transactionId, CardPaymentDto dto, string userId)
{
    var transaction = await _transactionRepo.GetByIdAsync(transactionId);
    if (transaction == null)
        throw new NotFoundException($"Transaction {transactionId} not found");

    var amountDue = transaction.TotalAmount - transaction.AmountPaid;

    // Call external payment gateway (e.g., Yoco, PayGate, Peach Payments)
    var gatewayResponse = await _paymentGateway.ProcessPaymentAsync(new PaymentRequest
    {
        Amount = amountDue,
        CardNumber = dto.CardNumber,
        ExpiryDate = dto.ExpiryDate,
        CVV = dto.CVV,
        TransactionReference = transaction.TransactionNumber
    });

    if (!gatewayResponse.IsSuccessful)
        throw new PaymentException($"Card payment failed: {gatewayResponse.ErrorMessage}");

    // Create payment record
    var payment = new Payment
    {
        TransactionId = transactionId,
        PaymentMethod = PaymentMethod.Card,
        Amount = amountDue,
        CardType = dto.CardType,
        CardLast4Digits = dto.CardNumber.Substring(dto.CardNumber.Length - 4),
        AuthorizationCode = gatewayResponse.AuthorizationCode,
        TransactionReference = gatewayResponse.TransactionReference,
        PaymentStatus = "Completed",
        CreatedBy = userId
    };

    await _paymentRepo.AddAsync(payment);

    // Update transaction
    transaction.AmountPaid += payment.Amount;
    transaction.AmountDue = transaction.TotalAmount - transaction.AmountPaid;
    transaction.PaymentStatus = transaction.AmountDue == 0 ? PaymentStatus.Paid : PaymentStatus.PartiallyPaid;

    await _transactionRepo.UpdateAsync(transaction);
    await _transactionRepo.SaveChangesAsync();

    return payment;
}
```

### 8.4 Split Payment

**Split Payment Example**: Customer pays R500 cash + R300 card (total R800)

```csharp
public async Task ProcessSplitPaymentAsync(int transactionId, SplitPaymentDto dto, string userId)
{
    var transaction = await _transactionRepo.GetByIdAsync(transactionId);
    if (transaction == null)
        throw new NotFoundException($"Transaction {transactionId} not found");

    var totalPaymentAmount = dto.Payments.Sum(p => p.Amount);
    var amountDue = transaction.TotalAmount - transaction.AmountPaid;

    if (totalPaymentAmount != amountDue)
        throw new ValidationException($"Payment amount mismatch. Due: R{amountDue}, Provided: R{totalPaymentAmount}");

    // Process each payment
    foreach (var paymentDto in dto.Payments)
    {
        if (paymentDto.PaymentMethod == PaymentMethod.Cash)
        {
            await ProcessCashPaymentAsync(transactionId, paymentDto.Amount, userId);
        }
        else if (paymentDto.PaymentMethod == PaymentMethod.Card)
        {
            await ProcessCardPaymentAsync(transactionId, paymentDto.CardDetails, userId);
        }
    }

    // Transaction status updated in individual payment methods
}
```

---

## 9. RECEIPT GENERATION

### 9.1 Receipt Requirements (South Africa)

**SARS Compliance**:
- Vendor name, address, VAT number
- Invoice number (transaction number)
- Date and time of sale
- Cashier name/ID
- Item descriptions with quantities and prices
- **VAT amount clearly shown**
- Total amount
- Payment method
- Change given (if cash)

**Cannabis Compliance**:
- SAHPRA license number
- DALRRD permit number
- Batch numbers for all cannabis products
- Customer ID verification notice
- Daily purchase limit notice

### 9.2 Receipt Template

```
================================================================
               GREEN LEAF DISPENSARY
        123 Main Street, Johannesburg, 2000
            Tel: 011-123-4567
      VAT No: 1234567890 | SAHPRA Lic: SL-2024-001
================================================================

RECEIPT: SALE-2024-12345
Date: 2024-12-11 14:32:15
Cashier: John Doe (EMP-001)
Register: POS-01

----------------------------------------------------------------
ITEMS
----------------------------------------------------------------
Indica Pre-Roll 1g (Batch: 0101202412110001)
  1 x R120.00                                        R120.00

Sativa Flower 3.5g (Batch: 0102202412100005)
  1 x R450.00                                        R450.00

----------------------------------------------------------------
                                          Sub-Total:   R495.65
                                           Discount:      R0.00
                                        VAT (15%):     R74.35
----------------------------------------------------------------
                                              TOTAL:   R570.00
================================================================

PAYMENT
Cash Tendered:                                        R600.00
Change:                                                R30.00

================================================================
CANNABIS COMPLIANCE NOTICE
----------------------------------------------------------------
- Valid ID verified: SA ID ***6789
- This purchase counts towards your daily limit
- SAHPRA License: SL-2024-001
- DALRRD Permit: DP-2024-JHB-001
- Keep this receipt for traceability
================================================================

          Thank you for your purchase!
     Please consume responsibly and legally.

================================================================
```

### 9.3 Receipt Generation Code

```csharp
public async Task<string> GenerateReceiptAsync(int transactionId)
{
    var transaction = await _transactionRepo.GetByIdWithDetailsAsync(transactionId);
    if (transaction == null)
        throw new NotFoundException($"Transaction {transactionId} not found");

    var site = transaction.Site;
    var cashier = await _userService.GetByIdAsync(transaction.CashierId);

    var sb = new StringBuilder();

    // Header
    sb.AppendLine("================================================================");
    sb.AppendLine($"               {site.SiteName.ToUpper()}");
    sb.AppendLine($"        {site.AddressLine1}");
    sb.AppendLine($"            Tel: {site.PhoneNumber}");
    sb.AppendLine($"      VAT No: {site.VATNumber} | SAHPRA Lic: {site.LicenseNumber}");
    sb.AppendLine("================================================================");
    sb.AppendLine();

    sb.AppendLine($"RECEIPT: {transaction.TransactionNumber}");
    sb.AppendLine($"Date: {transaction.TransactionDate:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine($"Cashier: {cashier.FullName} ({cashier.EmployeeCode})");
    sb.AppendLine($"Register: {transaction.RegisterId}");
    sb.AppendLine();

    // Line items
    sb.AppendLine("----------------------------------------------------------------");
    sb.AppendLine("ITEMS");
    sb.AppendLine("----------------------------------------------------------------");

    foreach (var line in transaction.TransactionDetails)
    {
        sb.AppendLine($"{line.Product.ProductName} (Batch: {line.BatchNumber})");
        sb.AppendLine($"  {line.Quantity} x R{line.UnitPrice:F2}                                        R{line.LineTotal:F2}");
        sb.AppendLine();
    }

    // Totals
    sb.AppendLine("----------------------------------------------------------------");
    sb.AppendLine($"                                          Sub-Total:   R{transaction.SubTotal:F2}");
    sb.AppendLine($"                                           Discount:   R{transaction.DiscountAmount:F2}");
    sb.AppendLine($"                                        VAT (15%):   R{transaction.VATAmount:F2}");
    sb.AppendLine("----------------------------------------------------------------");
    sb.AppendLine($"                                              TOTAL:   R{transaction.TotalAmount:F2}");
    sb.AppendLine("================================================================");
    sb.AppendLine();

    // Payment
    sb.AppendLine("PAYMENT");
    foreach (var payment in transaction.Payments)
    {
        sb.AppendLine($"{payment.PaymentMethod}:                                        R{payment.Amount:F2}");
        if (payment.PaymentMethod == PaymentMethod.Cash)
        {
            sb.AppendLine($"Cash Tendered:                                        R{payment.CashTendered:F2}");
            sb.AppendLine($"Change:                                                R{payment.CashChange:F2}");
        }
    }

    sb.AppendLine();
    sb.AppendLine("================================================================");
    sb.AppendLine("CANNABIS COMPLIANCE NOTICE");
    sb.AppendLine("----------------------------------------------------------------");
    sb.AppendLine($"- Valid ID verified: {transaction.CustomerIDNumber}");
    sb.AppendLine("- This purchase counts towards your daily limit");
    sb.AppendLine($"- SAHPRA License: {site.LicenseNumber}");
    sb.AppendLine($"- DALRRD Permit: {site.DalrrdPermit}");
    sb.AppendLine("- Keep this receipt for traceability");
    sb.AppendLine("================================================================");
    sb.AppendLine();

    sb.AppendLine("          Thank you for your purchase!");
    sb.AppendLine("     Please consume responsibly and legally.");
    sb.AppendLine();
    sb.AppendLine("================================================================");

    return sb.ToString();
}
```

---

## 10. RETURNS & REFUNDS

### 10.1 Return Policy

**Allowed Returns**:
- Defective products (quality issues)
- Incorrect products sold
- Within 7 days of purchase (with receipt)

**NOT Allowed**:
- Opened cannabis products (health & safety)
- Products without batch numbers (traceability requirement)
- Products past expiry date

### 10.2 Return Workflow

**Return Transaction Process**:
```
1. Verify original transaction (by receipt number)
2. Validate return eligibility (within 7 days, unopened, etc.)
3. Create new RetailTransactionHeader (Type = 'Return')
   └── Link to original: OriginalTransactionId
4. Create TransactionDetails (negative quantities)
5. Generate IN movements (reverse original OUT movements)
6. Process refund payment (cash or card reversal)
7. Generate refund receipt
```

### 10.3 Return Implementation

```csharp
public async Task<RetailTransactionHeader> ProcessReturnAsync(ReturnRequestDto dto, string userId)
{
    // 1. Get original transaction
    var originalTrans = await _transactionRepo.GetByNumberAsync(dto.OriginalTransactionNumber);
    if (originalTrans == null)
        throw new NotFoundException($"Original transaction {dto.OriginalTransactionNumber} not found");

    // 2. Validate return eligibility
    if ((DateTime.UtcNow - originalTrans.TransactionDate).TotalDays > 7)
        throw new ValidationException("Returns must be within 7 days of purchase");

    if (originalTrans.Status == "Returned")
        throw new ValidationException("Transaction already returned");

    // 3. Create return transaction
    var returnTrans = new RetailTransactionHeader
    {
        TransactionNumber = await _numberService.GenerateTransactionNumberAsync(),
        TransactionDate = DateTime.UtcNow,
        TransactionType = TransactionType.Return,
        OriginalTransactionId = originalTrans.TransactionId,
        SiteId = originalTrans.SiteId,
        CashierId = int.Parse(userId),
        CustomerId = originalTrans.CustomerId,
        SubTotal = -originalTrans.SubTotal,
        DiscountAmount = -originalTrans.DiscountAmount,
        VATAmount = -originalTrans.VATAmount,
        TotalAmount = -originalTrans.TotalAmount,
        PaymentStatus = PaymentStatus.Pending,
        Status = "Active",
        CreatedBy = userId
    };

    await _transactionRepo.AddAsync(returnTrans);

    // 4. Create return line items (negative quantities)
    foreach (var originalLine in originalTrans.TransactionDetails)
    {
        var returnLine = new TransactionDetail
        {
            HeaderId = returnTrans.TransactionId,
            TransactionType = "Retail",
            ProductId = originalLine.ProductId,
            Quantity = -originalLine.Quantity, // Negative!
            UnitOfMeasure = originalLine.UnitOfMeasure,
            UnitPrice = originalLine.UnitPrice,
            DiscountAmount = -originalLine.DiscountAmount,
            VATAmount = -originalLine.VATAmount,
            LineTotal = -originalLine.LineTotal,
            BatchNumber = originalLine.BatchNumber,
            SerialNumber = originalLine.SerialNumber,
            CreatedBy = userId
        };

        await _transactionDetailRepo.AddAsync(returnLine);

        // 5. Create IN movement (reverse original OUT)
        await _movementService.CreateMovementAsync(new CreateMovementDto
        {
            ProductId = originalLine.ProductId,
            SiteId = returnTrans.SiteId,
            MovementType = MovementType.SaleReturn,
            MovementDirection = MovementDirection.IN, // IN (stock comes back)
            Quantity = originalLine.Quantity, // Positive quantity for IN
            BatchNumber = originalLine.BatchNumber,
            SerialNumber = originalLine.SerialNumber,
            TransactionType = "Retail",
            TransactionId = returnTrans.TransactionId,
            TransactionDetailId = returnLine.TransactionDetailId,
            ReferenceNumber = returnTrans.TransactionNumber,
            Notes = $"Return of {originalTrans.TransactionNumber}",
            CreatedBy = userId
        });
    }

    // 6. Process refund payment
    var refundPayment = new Payment
    {
        TransactionId = returnTrans.TransactionId,
        PaymentMethod = PaymentMethod.Cash, // Or reverse to original card
        Amount = -originalTrans.TotalAmount, // Negative (refund)
        PaymentStatus = "Completed",
        CreatedBy = userId
    };

    await _paymentRepo.AddAsync(refundPayment);

    // Update return transaction
    returnTrans.AmountPaid = refundPayment.Amount;
    returnTrans.PaymentStatus = PaymentStatus.Refunded;

    // Update original transaction status
    originalTrans.Status = "Returned";
    await _transactionRepo.UpdateAsync(originalTrans);

    await _transactionRepo.SaveChangesAsync();

    return returnTrans;
}
```

---

## 11. CANNABIS COMPLIANCE

### 11.1 ID Verification

**Requirements** (Cannabis Act 2024):
- Customers must be **21+ years old**
- Valid SA ID, passport, or driver's license required
- ID must be scanned/verified for every cannabis purchase

**ID Verification Integration**:
```csharp
public async Task<IDVerificationResult> VerifyCustomerIDAsync(string idNumber)
{
    // 1. Validate ID number format
    if (!IsValidSAIDNumber(idNumber))
        return new IDVerificationResult { IsValid = false, ErrorMessage = "Invalid SA ID number format" };

    // 2. Extract date of birth from ID
    var dob = ExtractDOBFromIDNumber(idNumber);
    var age = CalculateAge(dob);

    // 3. Check minimum age (21)
    if (age < 21)
        return new IDVerificationResult { IsValid = false, ErrorMessage = "Customer must be 21 or older" };

    // 4. Optional: Call external ID verification service (e.g., Home Affairs)
    // var verificationResponse = await _idVerificationService.VerifyAsync(idNumber);

    return new IDVerificationResult
    {
        IsValid = true,
        IDNumber = idNumber,
        DateOfBirth = dob,
        Age = age
    };
}

private bool IsValidSAIDNumber(string idNumber)
{
    if (idNumber.Length != 13 || !idNumber.All(char.IsDigit))
        return false;

    // Luhn algorithm validation (checksum)
    return ValidateLuhnChecksum(idNumber);
}

private DateTime ExtractDOBFromIDNumber(string idNumber)
{
    var year = int.Parse(idNumber.Substring(0, 2));
    var month = int.Parse(idNumber.Substring(2, 2));
    var day = int.Parse(idNumber.Substring(4, 2));

    // Determine century (00-24 = 2000s, 25-99 = 1900s)
    year += year <= 24 ? 2000 : 1900;

    return new DateTime(year, month, day);
}
```

### 11.2 Daily Purchase Limits

**Regulations** (DALRRD):
- Maximum **100g dried flower** per customer per day
- Maximum **10g concentrate** per customer per day
- Tracking required across all dispensaries (future: centralized registry)

**Daily Limit Tracking**:
```csharp
public async Task<DailyLimitCheckResult> CheckDailyLimitsAsync(string idNumber, List<TransactionDetail> items)
{
    const decimal MAX_FLOWER_GRAMS = 100m;
    const decimal MAX_CONCENTRATE_GRAMS = 10m;

    // Get today's purchases for this customer
    var todaysPurchases = await _transactionRepo.GetTodaysTransactionsByIDNumberAsync(idNumber);

    // Calculate total purchased today
    var flowerTotal = todaysPurchases
        .SelectMany(t => t.TransactionDetails)
        .Where(d => d.Product.ProductType == ProductType.FinishedGood && d.Product.ProductCategory.CategoryName.Contains("Flower"))
        .Sum(d => d.Quantity);

    var concentrateTotal = todaysPurchases
        .SelectMany(t => t.TransactionDetails)
        .Where(d => d.Product.ProductCategory.CategoryName.Contains("Concentrate"))
        .Sum(d => d.Quantity);

    // Add current transaction items
    var currentFlower = items
        .Where(d => d.Product.ProductType == ProductType.FinishedGood && d.Product.ProductCategory.CategoryName.Contains("Flower"))
        .Sum(d => d.Quantity);

    var currentConcentrate = items
        .Where(d => d.Product.ProductCategory.CategoryName.Contains("Concentrate"))
        .Sum(d => d.Quantity);

    // Check limits
    var totalFlower = flowerTotal + currentFlower;
    var totalConcentrate = concentrateTotal + currentConcentrate;

    if (totalFlower > MAX_FLOWER_GRAMS)
        return new DailyLimitCheckResult
        {
            IsWithinLimit = false,
            ErrorMessage = $"Daily limit exceeded: Flower {totalFlower}g / {MAX_FLOWER_GRAMS}g"
        };

    if (totalConcentrate > MAX_CONCENTRATE_GRAMS)
        return new DailyLimitCheckResult
        {
            IsWithinLimit = false,
            ErrorMessage = $"Daily limit exceeded: Concentrate {totalConcentrate}g / {MAX_CONCENTRATE_GRAMS}g"
        };

    return new DailyLimitCheckResult
    {
        IsWithinLimit = true,
        FlowerTotal = totalFlower,
        FlowerLimit = MAX_FLOWER_GRAMS,
        ConcentrateTotal = totalConcentrate,
        ConcentrateLimit = MAX_CONCENTRATE_GRAMS
    };
}
```

---

## 12. CUSTOMER MANAGEMENT

### 12.1 Customer Registration

**Optional Registration**:
- Walk-in customers: No registration required (but ID still verified)
- Registered customers: Loyalty points, purchase history, faster checkout

**Registration Workflow**:
```csharp
public async Task<Customer> RegisterCustomerAsync(RegisterCustomerDto dto, string userId)
{
    // Validate ID number
    var idVerification = await VerifyCustomerIDAsync(dto.IDNumber);
    if (!idVerification.IsValid)
        throw new ValidationException(idVerification.ErrorMessage);

    // Check for duplicate
    var existing = await _customerRepo.GetByIDNumberAsync(dto.IDNumber);
    if (existing != null)
        throw new ValidationException("Customer already registered");

    // Create customer
    var customer = new Customer
    {
        CustomerNumber = await _numberService.GenerateCustomerNumberAsync(),
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        IDNumber = dto.IDNumber,
        IDType = "SA ID",
        DateOfBirth = idVerification.DateOfBirth,
        IDVerified = true,
        IDVerifiedDate = DateTime.UtcNow,
        IsActive = true,
        CreatedBy = userId
    };

    await _customerRepo.AddAsync(customer);
    await _customerRepo.SaveChangesAsync();

    return customer;
}
```

### 12.2 Customer Lookup

**Quick Lookup Methods**:
- By customer number (barcode on loyalty card)
- By phone number (last 4 digits)
- By ID number (partial match)
- By name (fuzzy search)

```csharp
public async Task<Customer?> QuickLookupAsync(string searchTerm)
{
    // Try customer number
    var customer = await _customerRepo.GetByNumberAsync(searchTerm);
    if (customer != null) return customer;

    // Try phone number (last 4 digits)
    if (searchTerm.Length == 4 && searchTerm.All(char.IsDigit))
    {
        customer = await _customerRepo.GetByPhoneLast4Async(searchTerm);
        if (customer != null) return customer;
    }

    // Try ID number
    customer = await _customerRepo.GetByIDNumberAsync(searchTerm);
    if (customer != null) return customer;

    // Try name search
    var customers = await _customerRepo.SearchByNameAsync(searchTerm);
    return customers.FirstOrDefault();
}
```

---

## 13. RETAIL PRODUCTION INTEGRATION

### 13.1 Serial Number Generation at POS

**Scenario**: Customer purchases a pre-roll from bulk inventory.

**Workflow**:
1. Cashier scans pre-roll product (by batch number)
2. System checks if product is serial-tracked
3. If yes, generate serial number on-demand
4. Capture serial number on transaction detail
5. Create movement with serial number

**Implementation**:
```csharp
public async Task<string> GenerateSerialNumberForSaleAsync(int productId, string batchNumber)
{
    var product = await _productRepo.GetByIdAsync(productId);
    if (!product.IsSerialTracked)
        return null; // Not a serial-tracked product

    // Generate 13-digit short serial number
    var serialNumber = await _serialNumberService.GenerateShortSerialNumberAsync(
        siteId: 1, // POS site
        productId: productId,
        batchNumber: batchNumber
    );

    // Record serial number in SerialNumbers table
    var serial = new SerialNumber
    {
        SerialNumber = serialNumber,
        ProductId = productId,
        BatchNumber = batchNumber,
        Status = "Sold",
        CreatedBy = "POS-System"
    };

    await _serialNumberRepo.AddAsync(serial);
    await _serialNumberRepo.SaveChangesAsync();

    return serialNumber;
}
```

**Transaction Detail with Serial Number**:
```csharp
var serialNumber = await GenerateSerialNumberForSaleAsync(productId, batchNumber);

var detail = new TransactionDetail
{
    ProductId = productId,
    Quantity = 1,
    UnitPrice = 120m,
    BatchNumber = batchNumber,
    SerialNumber = serialNumber, // Captured!
    TransactionType = "Retail",
    CreatedBy = userId
};
```

---

## 14. REPORTING & ANALYTICS

### 14.1 Daily Sales Reports

**End-of-Day Sales Summary**:
```sql
-- Daily sales by cashier
SELECT
    c.FullName AS Cashier,
    COUNT(t.TransactionId) AS TransactionCount,
    SUM(t.TotalAmount) AS TotalSales,
    SUM(t.VATAmount) AS TotalVAT,
    SUM(t.DiscountAmount) AS TotalDiscounts
FROM RetailTransactionHeaders t
JOIN Users c ON c.UserId = t.CashierId
WHERE t.TransactionDate >= @StartOfDay
  AND t.TransactionDate < @EndOfDay
  AND t.Status = 'Completed'
  AND t.IsDeleted = 0
GROUP BY c.FullName
ORDER BY TotalSales DESC;
```

**Payment Method Breakdown**:
```sql
-- Sales by payment method
SELECT
    p.PaymentMethod,
    COUNT(DISTINCT p.TransactionId) AS TransactionCount,
    SUM(p.Amount) AS TotalAmount
FROM Payments p
JOIN RetailTransactionHeaders t ON t.TransactionId = p.TransactionId
WHERE t.TransactionDate >= @StartOfDay
  AND t.TransactionDate < @EndOfDay
  AND t.Status = 'Completed'
GROUP BY p.PaymentMethod;
```

### 14.2 Product Performance

**Top Selling Products**:
```sql
SELECT
    p.ProductCode,
    p.ProductName,
    SUM(d.Quantity) AS TotalQuantitySold,
    COUNT(DISTINCT d.HeaderId) AS TransactionCount,
    SUM(d.LineTotal) AS TotalRevenue
FROM TransactionDetails d
JOIN Products p ON p.ProductId = d.ProductId
JOIN RetailTransactionHeaders t ON t.TransactionId = d.HeaderId AND d.TransactionType = 'Retail'
WHERE t.TransactionDate >= @StartDate
  AND t.TransactionDate < @EndDate
  AND t.Status = 'Completed'
GROUP BY p.ProductCode, p.ProductName
ORDER BY TotalRevenue DESC
LIMIT 20;
```

### 14.3 Cannabis Compliance Reports

**Batch Traceability Report**:
```sql
-- All sales for a specific batch
SELECT
    t.TransactionNumber,
    t.TransactionDate,
    p.ProductName,
    d.Quantity,
    d.BatchNumber,
    d.SerialNumber,
    t.CustomerIDNumber
FROM TransactionDetails d
JOIN RetailTransactionHeaders t ON t.TransactionId = d.HeaderId AND d.TransactionType = 'Retail'
JOIN Products p ON p.ProductId = d.ProductId
WHERE d.BatchNumber = @BatchNumber
  AND t.Status = 'Completed'
ORDER BY t.TransactionDate;
```

---

## 15. DATA ACCESS LAYER

### 15.1 IRetailTransactionRepository

```csharp
public interface IRetailTransactionRepository
{
    // CRUD
    Task<RetailTransactionHeader?> GetByIdAsync(int transactionId);
    Task<RetailTransactionHeader?> GetByNumberAsync(string transactionNumber);
    Task<RetailTransactionHeader?> GetByIdWithDetailsAsync(int transactionId);
    Task<IEnumerable<RetailTransactionHeader>> GetAllAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task AddAsync(RetailTransactionHeader transaction);
    Task UpdateAsync(RetailTransactionHeader transaction);
    Task DeleteAsync(int transactionId); // Soft delete
    Task SaveChangesAsync();

    // Queries
    Task<IEnumerable<RetailTransactionHeader>> GetByCashierAsync(int cashierId, DateTime? date = null);
    Task<IEnumerable<RetailTransactionHeader>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<RetailTransactionHeader>> GetBySiteAsync(int siteId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<RetailTransactionHeader>> GetTodaysTransactionsByIDNumberAsync(string idNumber);

    // Reporting
    Task<decimal> GetTotalSalesByCashierAsync(int cashierId, DateTime date);
    Task<decimal> GetTotalSalesBySiteAsync(int siteId, DateTime fromDate, DateTime toDate);
    Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(DateTime fromDate, DateTime toDate, int topN = 20);
}
```

---

## 16. BUSINESS LOGIC LAYER

### 16.1 IRetailTransactionService

```csharp
public interface IRetailTransactionService
{
    // Transaction management
    Task<RetailTransactionDto> CreateTransactionAsync(CreateTransactionDto dto);
    Task<RetailTransactionDto> GetTransactionByIdAsync(int transactionId);
    Task<RetailTransactionDto> GetTransactionByNumberAsync(string transactionNumber);

    // Line items
    Task<TransactionDetailDto> AddLineItemAsync(int transactionId, AddLineItemDto dto);
    Task RemoveLineItemAsync(int transactionId, int lineItemId);
    Task UpdateLineItemAsync(int transactionId, int lineItemId, UpdateLineItemDto dto);

    // Discounts
    Task ApplyLineDiscountAsync(int lineItemId, decimal discountPercent);
    Task ApplyTransactionDiscountAsync(int transactionId, decimal discountAmount);

    // Payment
    Task<PaymentDto> ProcessCashPaymentAsync(int transactionId, decimal cashTendered, string userId);
    Task<PaymentDto> ProcessCardPaymentAsync(int transactionId, CardPaymentDto dto, string userId);

    // Completion
    Task CompleteTransactionAsync(int transactionId, string userId);

    // Returns
    Task<RetailTransactionDto> ProcessReturnAsync(ReturnRequestDto dto, string userId);

    // Compliance
    Task<IDVerificationResult> VerifyCustomerIDAsync(string idNumber);
    Task<DailyLimitCheckResult> CheckDailyLimitsAsync(string idNumber, List<int> productIds);
}
```

---

## 17. API ENDPOINTS

### 17.1 Transaction Controller

```csharp
[ApiController]
[Route("api/retail/transactions")]
public class RetailTransactionController : ControllerBase
{
    private readonly IRetailTransactionService _transactionService;

    // Create new transaction
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        var transaction = await _transactionService.CreateTransactionAsync(dto);
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transaction);
    }

    // Get transaction by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(int id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    // Add line item
    [HttpPost("{id}/lines")]
    public async Task<IActionResult> AddLineItem(int id, [FromBody] AddLineItemDto dto)
    {
        var lineItem = await _transactionService.AddLineItemAsync(id, dto);
        return Ok(lineItem);
    }

    // Process payment
    [HttpPost("{id}/payments/cash")]
    public async Task<IActionResult> ProcessCashPayment(int id, [FromBody] CashPaymentDto dto)
    {
        var payment = await _transactionService.ProcessCashPaymentAsync(id, dto.CashTendered, User.Identity.Name);
        return Ok(payment);
    }

    // Complete transaction
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTransaction(int id)
    {
        await _transactionService.CompleteTransactionAsync(id, User.Identity.Name);
        return NoContent();
    }

    // Process return
    [HttpPost("returns")]
    public async Task<IActionResult> ProcessReturn([FromBody] ReturnRequestDto dto)
    {
        var returnTransaction = await _transactionService.ProcessReturnAsync(dto, User.Identity.Name);
        return Ok(returnTransaction);
    }
}
```

---

## 18. VALIDATION RULES

### 18.1 Transaction Validation

```csharp
public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.SiteId)
            .GreaterThan(0).WithMessage("Site ID is required");

        RuleFor(x => x.CashierId)
            .GreaterThan(0).WithMessage("Cashier ID is required");

        When(x => x.CustomerId.HasValue, () =>
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Invalid customer ID");
        });
    }
}

public class AddLineItemValidator : AbstractValidator<AddLineItemDto>
{
    public AddLineItemValidator(IProductRepository productRepo, ISOHService sohService)
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        // Check sufficient stock
        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                var soh = await sohService.GetSOHAsync(dto.ProductId, dto.SiteId, dto.BatchNumber);
                return soh >= dto.Quantity;
            })
            .WithMessage("Insufficient stock");
    }
}
```

---

## 19. TESTING STRATEGY

### 19.1 Unit Tests

```csharp
public class RetailTransactionServiceTests
{
    [Fact]
    public async Task CreateTransaction_ValidData_CreatesSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IRetailTransactionRepository>();
        var service = new RetailTransactionService(mockRepo.Object, null, null);

        var dto = new CreateTransactionDto
        {
            SiteId = 1,
            CashierId = 1
        };

        // Act
        var result = await service.CreateTransactionAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.SiteId);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<RetailTransactionHeader>()), Times.Once);
    }

    [Fact]
    public async Task ProcessCashPayment_SufficientPayment_CompletesSuccessfully()
    {
        // Arrange
        var transaction = new RetailTransactionHeader
        {
            TransactionId = 1,
            TotalAmount = 100m,
            AmountPaid = 0m
        };

        var mockRepo = new Mock<IRetailTransactionRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(transaction);

        var service = new RetailTransactionService(mockRepo.Object, null, null);

        // Act
        var payment = await service.ProcessCashPaymentAsync(1, 150m, "testuser");

        // Assert
        Assert.Equal(100m, payment.Amount);
        Assert.Equal(50m, payment.CashChange);
        Assert.Equal(PaymentStatus.Paid, transaction.PaymentStatus);
    }
}
```

---

## 20. IMPLEMENTATION ROADMAP

### 20.1 Phase 8: Retail POS Core (Week 2)

**Tasks**:
- [ ] Refactor transaction entities to use Option A architecture
- [ ] Implement `RetailTransactionService` with movement generation
- [ ] Implement pricing engine (standard + discounts)
- [ ] Implement VAT calculation (15%)
- [ ] Unit test transaction workflow (20+ tests)

### 20.2 Phase 9: Payment & Receipts (Week 3)

**Tasks**:
- [ ] Implement multi-payment support (cash, card, split)
- [ ] Integrate card payment gateway (Yoco/PayGate)
- [ ] Implement receipt generation service
- [ ] Create compliant receipt template
- [ ] Unit test payment processing (15+ tests)

### 20.3 Phase 10: Returns & Compliance (Week 4)

**Tasks**:
- [ ] Implement returns workflow with movement reversal
- [ ] Implement ID verification service
- [ ] Implement daily limit tracking
- [ ] Create compliance reporting
- [ ] Unit test returns (10+ tests)

### 20.4 Phase 11: UI & Integration (Week 5)

**Tasks**:
- [ ] Create Blazor POS terminal UI
- [ ] Implement barcode scanning
- [ ] Integrate with serial number generation
- [ ] Create cashier dashboard
- [ ] End-to-end testing (20+ scenarios)

### 20.5 Phase 12: Reporting & Final Testing (Week 6)

**Tasks**:
- [ ] Implement daily sales reports
- [ ] Implement product performance analytics
- [ ] Create end-of-day reconciliation
- [ ] Full integration testing
- [ ] User acceptance testing (UAT)

---

**END OF RETAIL POS REQUIREMENTS SPECIFICATION**
