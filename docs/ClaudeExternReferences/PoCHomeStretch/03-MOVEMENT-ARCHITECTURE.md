# Project420 - Movement Architecture
## System Backbone: Critical Movement and Transaction Design

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Part of**: PoC Home Stretch Specification Suite
**Status**: ⚠️ CRITICAL - If movement tracking is flawed, the entire platform fails

---

## ⚠️ CRITICAL IMPORTANCE

The Movement Architecture is the **most important design decision** in Project420.

**Why?**
- Stock-on-hand (SOH) is calculated entirely from movements
- Traceability (SAHPRA compliance) depends on movement records
- Financial reporting (SARS) depends on movement accuracy
- If movements are wrong, SOH is wrong, compliance fails, business fails

**This document defines**:
- How transactions are structured (Header/Detail pattern)
- How movements are generated from transactions
- Why Option A (specialized headers + unified details) is the correct choice
- How batch and serial number traceability flows through movements

---

## 📋 TABLE OF CONTENTS

1. [Architectural Mandates](#1-architectural-mandates)
2. [Transaction vs Movement](#2-transaction-vs-movement)
3. [Header-Detail Pattern](#3-header-detail-pattern)
4. [Option A vs Option B Analysis](#4-option-a-vs-option-b-analysis)
5. [Movement Table Structure](#5-movement-table-structure)
6. [Movement Generation Patterns](#6-movement-generation-patterns)
7. [Batch Transformation Model](#7-batch-transformation-model)
8. [Performance Considerations](#8-performance-considerations)
9. [Implementation Examples](#9-implementation-examples)

---

## 1. ARCHITECTURAL MANDATES

These are **non-negotiable** principles for Project420:

### 1.1 SOH is NEVER Stored Directly

**WRONG**:
```sql
UPDATE Products
SET StockOnHand = StockOnHand - @Quantity
WHERE ProductId = @ProductId;
```

**Problems with direct SOH storage**:
- ❌ No audit trail (can't prove why SOH changed)
- ❌ Concurrency issues (two sales at same time = data loss)
- ❌ No traceability (can't link SOH to source transactions)
- ❌ Can't recalculate historical SOH (for audits or error correction)
- ❌ Violates SAHPRA traceability requirements

**CORRECT**:
```sql
-- SOH is always calculated from movements
SELECT
    ProductId,
    SUM(CASE
        WHEN MovementType IN ('GRV', 'ProductionOutput', 'TransferIn', 'AdjustmentIn', 'ReturnFromCustomer')
        THEN Quantity
        ELSE -Quantity
    END) AS StockOnHand
FROM Movements
WHERE ProductId = @ProductId
  AND IsDeleted = 0
  AND TransactionDate <= @AsOfDate
GROUP BY ProductId;
```

**Benefits**:
- ✅ Complete audit trail (every stock change recorded)
- ✅ No concurrency issues (movements are immutable, append-only)
- ✅ Full traceability (each movement links to source transaction)
- ✅ Can recalculate SOH for any point in time
- ✅ SAHPRA compliant (7-year movement history)

### 1.2 Movements Must Be Created by Every Module

**All modules that affect stock must create movements**:

| Module | Transaction | Movement Type | Direction |
|--------|------------|---------------|-----------|
| **Purchasing** | GRV (Goods Received) | GRV | IN |
| **Purchasing** | RTS (Return to Supplier) | RTS | OUT |
| **Retail POS** | Sale | Sale | OUT |
| **Retail POS** | Refund | ReturnFromCustomer | IN |
| **Wholesale** | Invoice | WholesaleSale | OUT |
| **Wholesale** | Credit Note | ReturnFromCustomer | IN |
| **Production** | Input consumption | ProductionInput | OUT |
| **Production** | Output creation | ProductionOutput | IN |
| **Inventory** | Transfer (send) | TransferOut | OUT |
| **Inventory** | Transfer (receive) | TransferIn | IN |
| **Inventory** | Adjustment (positive) | AdjustmentIn | IN |
| **Inventory** | Adjustment (negative) | AdjustmentOut | OUT |
| **Inventory** | Stocktake variance | Adjustment | IN/OUT |

### 1.3 Movements Must Be Immutable

**Immutability Rules**:
- ✅ Once created, movements are **never updated** (only soft-deleted if transaction reversed)
- ✅ Corrections create **new movements** (not edit existing)
- ✅ Audit trail remains intact (POPIA 7-year retention)

**Example** (incorrect transaction):
```
Transaction 1 (incorrect):
├── Sale: 10 units of Product A
└── Movement: OUT -10 units, TransactionId=1

User realizes error: should have been 5 units

WRONG: UPDATE Movements SET Quantity = 5 WHERE TransactionId = 1
RIGHT:
├── Cancel Transaction 1 (soft delete movements)
│   └── Mark movements as IsDeleted = true
└── Create Transaction 2 (correct transaction)
    └── Movement: OUT -5 units, TransactionId=2
```

### 1.4 Movement Table Must Scale

**Expected Volume** (typical dispensary + production facility):
- **Retail POS**: 500 transactions/day x 5 items/transaction = 2,500 movements/day
- **Production**: 50 batches/month x 10 steps x 5 inputs/outputs = 2,500 movements/month
- **Purchasing**: 100 GRVs/month x 10 items = 1,000 movements/month
- **Inventory**: 50 transfers/month x 5 items = 250 movements/month
- **Total**: ~80,000 movements/month = **960,000 movements/year**

**Performance Strategy** (Phase 11):
1. **Partitioning**: Partition `Movements` table by `TransactionDate` (monthly or quarterly)
2. **Indexing**: Indexes on `(ProductId, TransactionDate)`, `(BatchNumber)`, `(SerialNumber)`, `(TransactionType, HeaderId)`
3. **Caching**: Cache SOH in `StockLevelsCache` table (refreshed incrementally)
4. **Archival**: Move movements older than 2 years to cold storage (retain 7 years for SAHPRA/POPIA)

---

## 2. TRANSACTION VS MOVEMENT

**Key Concept**: Transactions and Movements are **different things**.

### 2.1 Transaction

A **transaction** is a business event:
- **Examples**: Sale, GRV, Production Run, Transfer, Adjustment
- **Structure**: Header (summary) + Details (line items)
- **Purpose**: Capture business logic (who, what, when, why)

**Transaction Header** contains:
- TransactionId, TransactionNumber, TransactionType
- Date, Time, User
- Customer/Supplier reference
- Status (Pending, Completed, Cancelled)
- Financial totals (Subtotal, VAT, Total)

**Transaction Details** contain:
- DetailId, HeaderId
- ProductId, Quantity, UnitPrice
- Discounts, VAT, LineTotal
- BatchNumber, SerialNumber
- Notes

### 2.2 Movement

A **movement** is a physical stock change:
- **Examples**: Stock IN, Stock OUT
- **Structure**: Flat record (one row per stock-affecting unit)
- **Purpose**: Track inventory changes for SOH calculation

**Movement Record** contains:
- MovementId
- ProductId, Quantity, Mass
- MovementType, Direction (IN/OUT)
- BatchNumber, SerialNumber
- TransactionType, HeaderId, DetailId (link back to source)
- MovementReason
- Timestamp, UserId

### 2.3 Transaction → Movement Flow

```
1. User creates transaction (e.g., POS Sale)
   ↓
2. Transaction saved to database
   ├── Insert into RetailTransactionHeaders
   └── Insert into TransactionDetails (one per line item)
   ↓
3. MovementService.GenerateMovements(transactionId)
   ├── Read transaction details
   ├── For each detail:
   │   ├── Create movement record
   │   ├── Set MovementType based on TransactionType
   │   ├── Set Direction (IN/OUT)
   │   ├── Copy ProductId, Quantity, BatchNumber, SerialNumber
   │   └── Link to source (TransactionType, HeaderId, DetailId)
   └── Save movements to database
   ↓
4. SOH automatically updated (via calculation)
```

**Why separate?**
- **Transaction**: Business logic, user-facing, editable (draft mode), cancelable
- **Movement**: Data logic, system-facing, immutable, used for SOH calculation

---

## 3. HEADER-DETAIL PATTERN

All transactions in Project420 follow the **Header-Detail pattern**.

### 3.1 Pattern Structure

```
Header (one record per transaction)
    ├── TransactionId (PK)
    ├── TransactionNumber (unique, user-friendly)
    ├── TransactionType (Sale, GRV, Production, etc.)
    ├── Date, Time, User
    ├── Customer/Supplier/Batch reference
    ├── Status (Pending, Completed, Cancelled)
    └── Totals (Subtotal, VAT, Discount, Total)
        ↓
Details (many records per transaction)
    ├── DetailId (PK)
    ├── HeaderId (FK to header)
    ├── ProductId
    ├── Quantity, UnitPrice
    ├── Discounts, VAT, LineTotal
    ├── BatchNumber, SerialNumber
    └── Notes
        ↓
Movements (generated from details)
    ├── MovementId (PK)
    ├── ProductId
    ├── Quantity, Mass
    ├── MovementType, Direction
    ├── BatchNumber, SerialNumber
    ├── TransactionType, HeaderId, DetailId (link back)
    └── MovementReason
```

### 3.2 Why Header-Detail?

**Benefits**:
- ✅ **Efficient**: One header, many details (vs. repeating header data on every line)
- ✅ **Consistent**: Same pattern across all modules (Retail, Wholesale, Purchasing, Production)
- ✅ **Queryable**: Easy to query all lines for a transaction (JOIN on HeaderId)
- ✅ **Auditable**: Clear relationship between transaction and movements

**Example** (Retail Sale):
```sql
-- Header: SALE-20250101-001
INSERT INTO RetailTransactionHeaders (TransactionNumber, CustomerId, Date, Status, Total)
VALUES ('SALE-20250101-001', 123, '2025-01-01', 'Completed', 470.00);

-- Detail Line 1: Product A x2
INSERT INTO TransactionDetails (HeaderId, TransactionType, ProductId, Quantity, UnitPrice, LineTotal)
VALUES (1, 'Sale', 101, 2, 150.00, 300.00);

-- Detail Line 2: Product B x1
INSERT INTO TransactionDetails (HeaderId, TransactionType, ProductId, Quantity, UnitPrice, LineTotal)
VALUES (1, 'Sale', 102, 1, 80.00, 80.00);

-- Detail Line 3: Product C x1 (discounted)
INSERT INTO TransactionDetails (HeaderId, TransactionType, ProductId, Quantity, UnitPrice, DiscountAmount, LineTotal)
VALUES (1, 'Sale', 103, 1, 100.00, 10.00, 90.00);

-- Movements generated from details
INSERT INTO Movements (ProductId, MovementType, Direction, Quantity, TransactionType, HeaderId, DetailId, Reason)
VALUES
    (101, 'Sale', 'OUT', 2, 'Sale', 1, 1, 'Retail sale SALE-20250101-001'),
    (102, 'Sale', 'OUT', 1, 'Sale', 1, 2, 'Retail sale SALE-20250101-001'),
    (103, 'Sale', 'OUT', 1, 'Sale', 1, 3, 'Retail sale SALE-20250101-001');
```

---

## 4. OPTION A VS OPTION B ANALYSIS

**The Question**: How should we structure headers and details?

### 4.1 Option A: Specialized Headers + Unified Detail Table ✅ RECOMMENDED

**Structure**:
```
Specialized Header Tables:
├── RetailTransactionHeaders (POS-specific fields)
├── WholesaleTransactionHeaders (B2B-specific fields)
├── PurchaseHeaders (GRV/RTS-specific fields)
├── ProductionHeaders (batch-specific fields)
└── TransferHeaders (location-specific fields)
        ↓ all link to ↓
TransactionDetails (unified)
├── DetailId (PK)
├── HeaderId (FK)
├── TransactionType (discriminator: 'Sale', 'GRV', 'Production', etc.)
├── ProductId
├── Quantity, UnitPrice, DiscountAmount, VATAmount, LineTotal
├── BatchNumber, SerialNumber
└── Notes
        ↓ generates ↓
Movements (unified ledger)
```

**Benefits**:
- ✅ **Single detail model**: One table for all transaction details (reduces duplication)
- ✅ **Consistent structure**: All modules use same detail fields
- ✅ **Easy to add new transaction types**: Just add new header table, details stay same
- ✅ **Efficient reporting**: Single JOIN to get all details across all transaction types
- ✅ **Centralized movement generation**: One `MovementService` handles all types
- ✅ **Easier to maintain**: Changes to detail structure only affect one table
- ✅ **Scalable**: Proven pattern in enterprise ERP systems

**Drawbacks**:
- ⚠️ **Slight complexity**: Need `TransactionType` discriminator to link to correct header
- ⚠️ **Referential integrity**: Must enforce via triggers or application logic (can't use simple FK)

**Implementation**:
```csharp
public class TransactionDetail : AuditableEntity
{
    public int TransactionDetailId { get; set; }

    [Required]
    public int HeaderId { get; set; }

    [Required]
    public TransactionType TransactionType { get; set; } // Discriminator

    [Required]
    public int ProductId { get; set; }
    public virtual Product? Product { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VATAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    [StringLength(100)]
    public string? BatchNumber { get; set; }

    [StringLength(50)]
    public string? SerialNumber { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties (based on TransactionType)
    // Loaded conditionally via application logic
}

public enum TransactionType
{
    Sale = 1,
    Refund = 2,
    GRV = 3,
    RTS = 4,
    WholesaleSale = 5,
    WholesaleRefund = 6,
    ProductionInput = 7,
    ProductionOutput = 8,
    TransferOut = 9,
    TransferIn = 10,
    AdjustmentIn = 11,
    AdjustmentOut = 12,
    StocktakeVariance = 13
}
```

**Linking Logic**:
```csharp
// Get details for a retail sale
var details = await _context.TransactionDetails
    .Where(d => d.HeaderId == headerId && d.TransactionType == TransactionType.Sale)
    .Include(d => d.Product)
    .ToListAsync();

// Get header (cast to correct type based on TransactionType)
var header = await _context.RetailTransactionHeaders
    .FirstOrDefaultAsync(h => h.RetailTransactionHeaderId == headerId);
```

### 4.2 Option B: Separate Header/Detail Tables Per Module ❌ NOT RECOMMENDED

**Structure**:
```
RetailTransactionHeaders → RetailTransactionDetails
WholesaleTransactionHeaders → WholesaleTransactionDetails
PurchaseHeaders → PurchaseDetails
ProductionHeaders → ProductionDetails
TransferHeaders → TransferDetails
```

**Benefits**:
- ✅ **Simple for beginners**: Clear 1:1 relationship (easier to understand initially)
- ✅ **Strong FK integrity**: Database-enforced referential integrity

**Drawbacks**:
- ❌ **Schema duplication**: 5+ detail tables with 90% identical columns (maintenance nightmare)
- ❌ **Harder to report**: Must UNION ALL across 5+ tables to get all transaction details
- ❌ **More work to add new types**: Must create both header AND detail table for each new type
- ❌ **Movement generation duplicated**: Each module implements own movement generation logic
- ❌ **Poor for enterprise systems**: Doesn't scale as system grows
- ❌ **Inconsistent implementations**: Different modules might implement details differently over time

**Example Problem** (reporting across all transactions):
```sql
-- Option B: Must UNION across many tables
SELECT HeaderId, TransactionType, ProductId, Quantity FROM RetailTransactionDetails WHERE ProductId = 123
UNION ALL
SELECT HeaderId, TransactionType, ProductId, Quantity FROM WholesaleTransactionDetails WHERE ProductId = 123
UNION ALL
SELECT HeaderId, TransactionType, ProductId, Quantity FROM PurchaseDetails WHERE ProductId = 123
UNION ALL
SELECT HeaderId, TransactionType, ProductId, Quantity FROM ProductionDetails WHERE ProductId = 123
UNION ALL
SELECT HeaderId, TransactionType, ProductId, Quantity FROM TransferDetails WHERE ProductId = 123;

-- Option A: Single query
SELECT HeaderId, TransactionType, ProductId, Quantity FROM TransactionDetails WHERE ProductId = 123;
```

### 4.3 DECISION: Option A ✅

**Recommendation**: Use **Option A** (Specialized Headers + Unified Detail Table)

**Rationale**:
1. **Project420 is an enterprise system** with multiple transaction types (Retail, Wholesale, Purchasing, Production, Transfers, Adjustments)
2. **We need unified reporting** across all transaction types (inventory reports, sales analysis, traceability queries)
3. **Movement generation must be consistent** regardless of source (one `MovementService` for all)
4. **Future expansion likely** (export sales, inter-company transfers, consignment, drop-shipping)
5. **Option A is standard practice** in mature ERP systems (SAP, Dynamics, NetSuite all use this pattern)
6. **SAHPRA traceability requires** querying all movements across all transaction types (Option A makes this trivial)

**Implementation Plan** (Phase 7):
1. Create `TransactionDetails` unified table
2. Add `TransactionType` enum and discriminator column
3. Refactor existing headers to link to unified details
4. Implement `MovementService` with transaction-agnostic movement generation
5. Create database migration
6. Test movement generation from retail, purchasing, production sources
7. Validate SOH calculation from unified movement ledger

---

## 5. MOVEMENT TABLE STRUCTURE

### 5.1 Movement Entity (Conceptual)

```csharp
public class Movement : AuditableEntity
{
    public int MovementId { get; set; }

    // Product identification
    [Required]
    public int ProductId { get; set; }
    public virtual Product? Product { get; set; }

    // Movement classification
    [Required]
    [StringLength(50)]
    public string MovementType { get; set; } // GRV, Sale, ProductionInput, etc.

    [Required]
    [StringLength(10)]
    public string Direction { get; set; } // IN or OUT

    // Quantities
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; } // Count of units

    [Column(TypeName = "decimal(18,4)")]
    public decimal Mass { get; set; } // Weight in kg (for flower, trim, concentrates)

    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; } // Monetary value (for financial reporting)

    // Traceability
    [StringLength(100)]
    public string? BatchNumber { get; set; } // Link to source batch

    [StringLength(50)]
    public string? SerialNumber { get; set; } // Individual unit identifier

    // Source transaction
    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } // Sale, GRV, Production, etc.

    [Required]
    public int HeaderId { get; set; } // Link to transaction header

    [Required]
    public int DetailId { get; set; } // Link to transaction detail

    // Reason and audit
    [Required]
    [StringLength(500)]
    public string MovementReason { get; set; }

    public DateTime TransactionDate { get; set; }

    [StringLength(100)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string? SystemId { get; set; }

    // Location tracking (future enhancement)
    public int? LocationId { get; set; }
    public virtual Location? Location { get; set; }

    // Soft delete (POPIA compliance)
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

### 5.2 Movement Types

| MovementType | Direction | Source Transaction | Example |
|--------------|-----------|-------------------|---------|
| **GRV** | IN | Purchase (GRV) | Received 100 units from supplier |
| **RTS** | OUT | Purchase (RTS) | Returned 10 defective units to supplier |
| **Sale** | OUT | Retail (Sale) | Customer bought 5 units |
| **ReturnFromCustomer** | IN | Retail (Refund) | Customer returned 2 units |
| **WholesaleSale** | OUT | Wholesale (Invoice) | Sold 500 units to retailer |
| **ProductionInput** | OUT | Production (Input) | Consumed 10kg flower in pre-roll production |
| **ProductionOutput** | IN | Production (Output) | Produced 9,500 pre-rolls |
| **TransferOut** | OUT | Inventory (Transfer) | Transferred 50 units to Store B |
| **TransferIn** | IN | Inventory (Transfer) | Received 50 units from Store A |
| **AdjustmentIn** | IN | Inventory (Adjustment) | Stocktake found 5 extra units |
| **AdjustmentOut** | OUT | Inventory (Adjustment) | Stocktake found 3 units missing (shrinkage) |
| **Destruction** | OUT | Inventory (Destruction) | Destroyed 20 expired units (SAHPRA) |

### 5.3 Indexes (Performance Critical)

```sql
-- Primary key
CREATE INDEX IX_Movements_MovementId ON Movements(MovementId);

-- SOH calculation (most common query)
CREATE INDEX IX_Movements_ProductId_TransactionDate
    ON Movements(ProductId, TransactionDate, Direction)
    INCLUDE (Quantity, Mass, IsDeleted);

-- Batch traceability
CREATE INDEX IX_Movements_BatchNumber
    ON Movements(BatchNumber)
    WHERE BatchNumber IS NOT NULL;

-- Serial number lookup
CREATE INDEX IX_Movements_SerialNumber
    ON Movements(SerialNumber)
    WHERE SerialNumber IS NOT NULL;

-- Transaction lookup
CREATE INDEX IX_Movements_TransactionType_HeaderId
    ON Movements(TransactionType, HeaderId);

-- Audit queries
CREATE INDEX IX_Movements_TransactionDate_UserId
    ON Movements(TransactionDate, UserId);
```

---

## 6. MOVEMENT GENERATION PATTERNS

### 6.1 MovementService (Centralized)

```csharp
public interface IMovementService
{
    Task GenerateMovementsAsync(TransactionType transactionType, int headerId);
    Task ReverseMovementsAsync(TransactionType transactionType, int headerId, string reason);
    Task<decimal> CalculateSOHAsync(int productId, DateTime? asOfDate = null);
    Task<IEnumerable<Movement>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);
}

public class MovementService : IMovementService
{
    private readonly ApplicationDbContext _context;

    public async Task GenerateMovementsAsync(TransactionType transactionType, int headerId)
    {
        // Get transaction details
        var details = await _context.TransactionDetails
            .Where(d => d.HeaderId == headerId && d.TransactionType == transactionType)
            .Include(d => d.Product)
            .ToListAsync();

        // Determine movement type and direction based on transaction type
        var (movementType, direction) = GetMovementTypeAndDirection(transactionType);

        // Create movements
        var movements = new List<Movement>();

        foreach (var detail in details)
        {
            var movement = new Movement
            {
                ProductId = detail.ProductId,
                MovementType = movementType,
                Direction = direction,
                Quantity = detail.Quantity,
                Mass = detail.Product.NetWeight * detail.Quantity,
                Value = detail.LineTotal,
                BatchNumber = detail.BatchNumber,
                SerialNumber = detail.SerialNumber,
                TransactionType = transactionType.ToString(),
                HeaderId = headerId,
                DetailId = detail.TransactionDetailId,
                MovementReason = $"{transactionType} transaction {headerId}",
                TransactionDate = DateTime.UtcNow,
                UserId = GetCurrentUserId()
            };

            movements.Add(movement);
        }

        // Save movements
        _context.Movements.AddRange(movements);
        await _context.SaveChangesAsync();

        // Invalidate SOH cache for affected products
        await InvalidateSOHCacheAsync(movements.Select(m => m.ProductId).Distinct());
    }

    private (string movementType, string direction) GetMovementTypeAndDirection(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.GRV => ("GRV", "IN"),
            TransactionType.RTS => ("RTS", "OUT"),
            TransactionType.Sale => ("Sale", "OUT"),
            TransactionType.Refund => ("ReturnFromCustomer", "IN"),
            TransactionType.WholesaleSale => ("WholesaleSale", "OUT"),
            TransactionType.ProductionInput => ("ProductionInput", "OUT"),
            TransactionType.ProductionOutput => ("ProductionOutput", "IN"),
            TransactionType.TransferOut => ("TransferOut", "OUT"),
            TransactionType.TransferIn => ("TransferIn", "IN"),
            TransactionType.AdjustmentIn => ("AdjustmentIn", "IN"),
            TransactionType.AdjustmentOut => ("AdjustmentOut", "OUT"),
            _ => throw new ArgumentException($"Unknown transaction type: {transactionType}")
        };
    }
}
```

### 6.2 Example: Retail Sale Movement Generation

```csharp
// 1. User completes checkout
var checkoutRequest = new CheckoutRequestDto
{
    CustomerId = 123,
    Items = new List<CartItemDto>
    {
        new() { ProductId = 101, Quantity = 2, UnitPrice = 150.00m },
        new() { ProductId = 102, Quantity = 1, UnitPrice = 80.00m }
    },
    PaymentMethod = PaymentMethod.Cash,
    AmountTendered = 500.00m
};

// 2. TransactionService creates header + details
var transaction = await _transactionService.CheckoutAsync(checkoutRequest);
// Transaction saved:
// - RetailTransactionHeaders: SALE-20250101-001
// - TransactionDetails: 2 lines

// 3. TransactionService calls MovementService
await _movementService.GenerateMovementsAsync(TransactionType.Sale, transaction.TransactionHeaderId);

// 4. Movements created:
// - Movement 1: Product 101, OUT, Qty -2, Reason: "Sale transaction SALE-20250101-001"
// - Movement 2: Product 102, OUT, Qty -1, Reason: "Sale transaction SALE-20250101-001"

// 5. SOH automatically updated (via calculation)
// Product 101 SOH: 100 - 2 = 98
// Product 102 SOH: 50 - 1 = 49
```

### 6.3 Example: Production Movement Generation

```csharp
// Production batch: Pre-roll production
// Input: 10kg flower → Output: 9,500 pre-rolls

// 1. Create production batch header
var productionBatch = new ProductionBatchHeader
{
    BatchNumber = "BATCH-2025-003",
    ProductType = "Pre-Rolls",
    Status = ProductionStatus.InProgress
};
await _context.ProductionHeaders.AddAsync(productionBatch);
await _context.SaveChangesAsync();

// 2. Record input consumption (Step 1: Milling)
var inputDetail = new TransactionDetail
{
    HeaderId = productionBatch.ProductionBatchHeaderId,
    TransactionType = TransactionType.ProductionInput,
    ProductId = 101, // Flower product
    Quantity = 10, // 10kg
    BatchNumber = "BATCH-2025-001" // Source batch
};
await _context.TransactionDetails.AddAsync(inputDetail);
await _context.SaveChangesAsync();

// 3. Generate movement for input (OUT movement)
await _movementService.GenerateMovementsAsync(TransactionType.ProductionInput, productionBatch.ProductionBatchHeaderId);
// Movement: Product 101, OUT, Qty -10kg, Batch BATCH-2025-001, Reason: "Production input for BATCH-2025-003"

// 4. Record output creation (Step 4: Packaging)
var outputDetail = new TransactionDetail
{
    HeaderId = productionBatch.ProductionBatchHeaderId,
    TransactionType = TransactionType.ProductionOutput,
    ProductId = 201, // Pre-roll product
    Quantity = 9500, // 9,500 pre-rolls
    BatchNumber = "BATCH-2025-003" // Output batch
};
await _context.TransactionDetails.AddAsync(outputDetail);
await _context.SaveChangesAsync();

// 5. Generate movement for output (IN movement)
await _movementService.GenerateMovementsAsync(TransactionType.ProductionOutput, productionBatch.ProductionBatchHeaderId);
// Movement: Product 201, IN, Qty +9,500, Batch BATCH-2025-003, Reason: "Production output for BATCH-2025-003"

// 6. SOH updated:
// Product 101 (Flower): SOH decreased by 10kg
// Product 201 (Pre-rolls): SOH increased by 9,500 units
```

---

## 7. BATCH TRANSFORMATION MODEL

### 7.1 Single-Batch Multi-Step Manufacturing

**Key Principle**: A single batch flows through multiple steps without creating new batch numbers (unless combining batches or creating different product type).

```
BATCH-2025-003 (Pre-Roll Production)
    ↓
Step 1: Milling
├── Input: 10kg flower (from BATCH-2025-001)
├── Output: 9.8kg milled flower (WIP)
└── Loss: 0.2kg (dust, over-processing)
    ↓ Movements:
    ├── OUT: -10kg flower, Product 101, Batch BATCH-2025-001
    └── [WIP tracked internally, no movement yet]

Step 2: Pre-Roll Filling
├── Input: 9.8kg milled flower (WIP)
├── Output: 9,500 filled pre-rolls (WIP)
└── Loss: 0.3kg (spillage, QC rejects)
    ↓ Movements:
    └── [WIP tracked internally, no movement yet]

Step 3: Capping & QC
├── Input: 9,500 filled pre-rolls (WIP)
├── Output: 9,400 finished pre-rolls
└── Waste: 100 rejects (damaged)
    ↓ Movements:
    └── [WIP tracked internally, no movement yet]

Step 4: Packaging
├── Input: 9,400 pre-rolls
├── Output: 940 packs (10 per pack)
└── Each pack gets SN: SN-PR-20250101-001 to SN-PR-20250101-940
    ↓ Movements:
    └── IN: +940 packs, Product 201, Batch BATCH-2025-003, SNs assigned
```

**Key Points**:
- Same batch number throughout (`BATCH-2025-003`)
- Input movement created at start (OUT movement from source batch)
- WIP tracked in `ProcessingSteps` table (not movements)
- Output movement created at end (IN movement for finished goods)
- Serial numbers assigned to finished goods (linked to batch)
- Full traceability maintained (SN → Batch → Steps → Source Material)

### 7.2 ProcessingSteps Table (Conceptual)

```csharp
public class ProcessingStep : AuditableEntity
{
    public int ProcessingStepId { get; set; }

    [Required]
    public int ProductionBatchId { get; set; }
    public virtual ProductionBatch? ProductionBatch { get; set; }

    [Required]
    public int StepNumber { get; set; } // 1, 2, 3, 4

    [Required]
    [StringLength(100)]
    public string StepName { get; set; } // Milling, Filling, Capping, Packaging

    [Required]
    [StringLength(50)]
    public string StepType { get; set; } // Mechanical, Chemical, Formulation, Packaging

    // Input
    [Column(TypeName = "decimal(18,4)")]
    public decimal InputQuantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal InputMass { get; set; }

    // Output
    [Column(TypeName = "decimal(18,4)")]
    public decimal OutputQuantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal OutputMass { get; set; }

    // Loss
    [Column(TypeName = "decimal(18,4)")]
    public decimal LossMass { get; set; }

    [StringLength(200)]
    public string? LossReason { get; set; } // Spillage, QC rejects, evaporation

    // Potency (if applicable)
    [StringLength(10)]
    public string? InputPotencyTHC { get; set; }

    [StringLength(10)]
    public string? OutputPotencyTHC { get; set; }

    // Timing
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [StringLength(100)]
    public string? OperatorId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } // Pending, InProgress, Completed, Failed
}
```

---

## 8. PERFORMANCE CONSIDERATIONS

### 8.1 SOH Calculation Optimization

**Problem**: Calculating SOH from millions of movements is slow.

**Solution**: Cache SOH, refresh incrementally.

```csharp
public class StockLevelCache
{
    public int StockLevelCacheId { get; set; }
    public int ProductId { get; set; }
    public int? LocationId { get; set; }
    public string? BatchNumber { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal QuantityOnHand { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal MassOnHand { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValueOnHand { get; set; }

    public DateTime LastCalculated { get; set; }
    public int LastMovementId { get; set; } // Track which movement was last processed
}

// Incremental refresh
public async Task RefreshSOHCacheAsync(int productId)
{
    // Get current cache
    var cache = await _context.StockLevelCache
        .FirstOrDefaultAsync(c => c.ProductId == productId && c.LocationId == null);

    // Get new movements since last calculation
    int lastMovementId = cache?.LastMovementId ?? 0;

    var newMovements = await _context.Movements
        .Where(m => m.ProductId == productId && m.MovementId > lastMovementId && !m.IsDeleted)
        .ToListAsync();

    if (!newMovements.Any())
        return; // No new movements

    // Calculate delta
    decimal deltaQty = newMovements.Sum(m => m.Direction == "IN" ? m.Quantity : -m.Quantity);
    decimal deltaMass = newMovements.Sum(m => m.Direction == "IN" ? m.Mass : -m.Mass);
    decimal deltaValue = newMovements.Sum(m => m.Direction == "IN" ? m.Value : -m.Value);

    if (cache == null)
    {
        // Create new cache entry
        cache = new StockLevelCache
        {
            ProductId = productId,
            QuantityOnHand = deltaQty,
            MassOnHand = deltaMass,
            ValueOnHand = deltaValue,
            LastCalculated = DateTime.UtcNow,
            LastMovementId = newMovements.Max(m => m.MovementId)
        };
        _context.StockLevelCache.Add(cache);
    }
    else
    {
        // Update existing cache
        cache.QuantityOnHand += deltaQty;
        cache.MassOnHand += deltaMass;
        cache.ValueOnHand += deltaValue;
        cache.LastCalculated = DateTime.UtcNow;
        cache.LastMovementId = newMovements.Max(m => m.MovementId);
    }

    await _context.SaveChangesAsync();
}
```

### 8.2 Movement Table Partitioning

```sql
-- Partition movements table by year-month
-- Partition 1: 2025-01
-- Partition 2: 2025-02
-- ...
-- Partition 12: 2025-12

CREATE PARTITION FUNCTION PF_Movements_TransactionDate (DATETIME2)
AS RANGE RIGHT FOR VALUES
('2025-01-01', '2025-02-01', '2025-03-01', '2025-04-01', '2025-05-01', '2025-06-01',
 '2025-07-01', '2025-08-01', '2025-09-01', '2025-10-01', '2025-11-01', '2025-12-01');

CREATE PARTITION SCHEME PS_Movements_TransactionDate
AS PARTITION PF_Movements_TransactionDate
ALL TO ([PRIMARY]);

-- Create partitioned table
CREATE TABLE Movements (
    MovementId INT IDENTITY(1,1),
    ProductId INT NOT NULL,
    MovementType NVARCHAR(50) NOT NULL,
    Direction NVARCHAR(10) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    TransactionDate DATETIME2 NOT NULL,
    -- ... other columns
) ON PS_Movements_TransactionDate(TransactionDate);
```

**Benefits**:
- Queries on recent movements (last month) only scan one partition (faster)
- Archive old partitions (>2 years) to separate filegroup/database
- Maintain 7-year history while keeping active queries fast

---

## 9. IMPLEMENTATION EXAMPLES

### 9.1 Example: Refund with Movement Reversal

```csharp
// Original sale: SALE-20250101-001
// Customer bought 2x Product A (ProductId 101)

// User processes refund
var refundRequest = new RefundRequestDto
{
    OriginalTransactionId = 1,
    RefundItems = new List<RefundItemDto>
    {
        new() { DetailId = 1, Quantity = 2 } // Refund all 2 units
    },
    RefundReason = "Customer not satisfied",
    RefundMethod = PaymentMethod.Cash
};

// 1. Create refund transaction header
var refundHeader = new RetailTransactionHeader
{
    TransactionNumber = "RFND-20250101-001",
    CustomerId = 123,
    OriginalTransactionId = 1,
    TransactionType = TransactionType.Refund,
    Date = DateTime.UtcNow,
    Status = TransactionStatus.Completed,
    Total = -300.00m // Negative (refund)
};
await _context.RetailTransactionHeaders.AddAsync(refundHeader);
await _context.SaveChangesAsync();

// 2. Create refund transaction details
var refundDetail = new TransactionDetail
{
    HeaderId = refundHeader.RetailTransactionHeaderId,
    TransactionType = TransactionType.Refund,
    ProductId = 101,
    Quantity = 2,
    UnitPrice = 150.00m,
    LineTotal = -300.00m // Negative (refund)
};
await _context.TransactionDetails.AddAsync(refundDetail);
await _context.SaveChangesAsync();

// 3. Generate movements (IN movement - stock returning)
await _movementService.GenerateMovementsAsync(TransactionType.Refund, refundHeader.RetailTransactionHeaderId);

// Movement created:
// - ProductId: 101
// - MovementType: ReturnFromCustomer
// - Direction: IN
// - Quantity: +2
// - Reason: "Refund transaction RFND-20250101-001"

// 4. SOH updated automatically
// Product 101 SOH: 98 + 2 = 100 (back to original)
```

---

## ✅ IMPLEMENTATION CHECKLIST (Phase 7)

Movement Architecture Implementation:

- [ ] Create `TransactionDetails` unified table with `TransactionType` discriminator
- [ ] Create `Movements` table with all required columns (ProductId, MovementType, Direction, Quantity, Mass, BatchNumber, SerialNumber, TransactionType, HeaderId, DetailId)
- [ ] Implement `MovementService` with `GenerateMovementsAsync(transactionType, headerId)` method
- [ ] Refactor existing transaction headers to link to unified `TransactionDetails`
- [ ] Add indexes on `Movements` table (ProductId, BatchNumber, SerialNumber, TransactionType+HeaderId)
- [ ] Test movement generation from Retail POS (Sale, Refund)
- [ ] Test movement generation from Purchasing (GRV, RTS)
- [ ] Test movement generation from Production (Input, Output)
- [ ] Implement SOH calculation from movements (`CalculateSOHAsync`)
- [ ] Create `StockLevelCache` table and implement incremental refresh
- [ ] Test SOH accuracy across all transaction types
- [ ] Performance test: 100K movements, SOH query <200ms
- [ ] Implement movement archival strategy (7-year retention)

---

**Document Status**: ✅ COMPLETE
**Critical**: This architecture is the foundation of Project420. Review carefully before implementation.
**Next**: Read **04-RETAIL-POS-REQUIREMENTS.md** for detailed POS specifications.
