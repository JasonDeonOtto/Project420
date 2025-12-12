# 02 - INVENTORY MODULE SPECIFICATION

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: PoC Specification - Enterprise Grade Target (90%)
**Related Documents**:
- [00-MAIN-SPECIFICATION.md](00-MAIN-SPECIFICATION.md) - Central specification index
- [01-MODULE-DESCRIPTIONS.md](01-MODULE-DESCRIPTIONS.md) - All module overviews
- [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md) - Movement system backbone
- [06-SERIAL-BATCH-GENERATION.md](06-SERIAL-BATCH-GENERATION.md) - Batch/SN generation
- [10-GLOSSARY.md](10-GLOSSARY.md) - Terminology reference

---

## DOCUMENT PURPOSE

This document provides the **enterprise-grade specification** for the **Inventory Module** - the most critical component of Project420. The Inventory Module is the foundation upon which all other modules depend, providing:

- **Movement-based SOH calculation** (never stored directly)
- **Batch and serial number tracking** (seed-to-sale traceability)
- **Multi-location inventory management** (sites, zones, bins)
- **Real-time stock visibility** (sub-second query performance)
- **Cannabis-specific compliance** (SAHPRA, DALRRD, Cannabis Act 2024)
- **Integration hub** for all transaction modules (Retail, Purchasing, Production, Cultivation)

**PoC Completeness Target**: 90% (Enterprise Grade)

**Critical Success Factors**:
1. SOH calculation must be 100% accurate at all times
2. Movement generation must be consistent across all modules
3. Batch/serial number tracking must provide full traceability
4. Performance must support 50 concurrent users
5. System must handle 10,000+ movements per day

---

## TABLE OF CONTENTS

1. [Executive Summary](#1-executive-summary)
2. [Module Architecture](#2-module-architecture)
3. [Core Entities](#3-core-entities)
4. [Movement System](#4-movement-system)
5. [SOH Calculation Engine](#5-soh-calculation-engine)
6. [Batch & Serial Number Tracking](#6-batch--serial-number-tracking)
7. [Multi-Location Management](#7-multi-location-management)
8. [Stock Adjustments](#8-stock-adjustments)
9. [Stock Transfers](#9-stock-transfers)
10. [Stock Takes (Cycle Counting)](#10-stock-takes-cycle-counting)
11. [Reporting & Queries](#11-reporting--queries)
12. [Performance Optimization](#12-performance-optimization)
13. [Data Access Layer](#13-data-access-layer)
14. [Business Logic Layer](#14-business-logic-layer)
15. [API Endpoints](#15-api-endpoints)
16. [Validation Rules](#16-validation-rules)
17. [Testing Strategy](#17-testing-strategy)
18. [Compliance & Audit](#18-compliance--audit)
19. [Implementation Roadmap](#19-implementation-roadmap)

---

## 1. EXECUTIVE SUMMARY

### 1.1 Module Overview

The **Inventory Module** is the **central nervous system** of Project420. Every business transaction in the system ultimately results in an inventory movement:

- **Retail Sale** → OUT movement (reduces SOH)
- **Purchase Receipt (GRV)** → IN movement (increases SOH)
- **Production Output** → IN movement (finished goods)
- **Production Input** → OUT movement (raw materials consumed)
- **Stock Transfer** → OUT (source location) + IN (destination location)
- **Stock Adjustment** → IN or OUT (corrections, wastage, theft)

**Key Principle**: **SOH is NEVER stored in the database**. It is always calculated in real-time from movements:

```sql
SOH = SUM(IN movements) - SUM(OUT movements)
```

This ensures:
- **100% accuracy** - No possibility of SOH drift
- **Full audit trail** - Every stock change is traceable
- **Compliance** - Seed-to-sale tracking for regulators
- **Tamper-proof** - Cannot manipulate SOH without creating movement

### 1.2 Current Implementation Status (Phase 6 Complete)

**Implemented (✅)**:
- `Product` entity with full validation
- `ProductCategory` hierarchy with cannabis types
- `Site` entity (multi-location foundation)
- `Movements` table (basic structure)
- `ProductRepository` with CRUD operations
- `ProductService` with business logic
- Unit tests (224 passing)

**Partially Implemented (🟡)**:
- Movement generation (basic, needs consistency)
- Batch number tracking (exists but incomplete)
- SOH calculation (implemented but not optimized)
- Multi-location support (entities exist, logic incomplete)

**Not Yet Implemented (❌)**:
- Serial number tracking (critical for retail production)
- Stock transfers between locations
- Cycle counting / stock takes
- Movement reversal handling
- Performance optimization (indexes, caching)
- Advanced reporting (stock aging, movement history)

### 1.3 PoC Completion Target (90%)

To achieve 90% completeness by Phase 12 (Week 6), the Inventory Module must deliver:

**Must Have (90% Target)**:
- ✅ Movement-based SOH calculation (100% accurate)
- ✅ Batch number generation and tracking
- ✅ Serial number generation and tracking (retail production)
- ✅ Multi-site inventory management
- ✅ Stock adjustments with approval workflow
- ✅ Stock transfers between sites/zones
- ✅ Basic cycle counting (stock takes)
- ✅ Movement reversal handling
- ✅ Real-time SOH queries (optimized)
- ✅ Movement history reporting
- ✅ Low stock alerts
- ✅ Negative stock prevention
- ✅ Cannabis compliance (batch traceability)

**Nice to Have (Beyond 90%)**:
- ⚪ Stock aging reports
- ⚪ ABC analysis (fast/slow movers)
- ⚪ Demand forecasting
- ⚪ Automatic reorder points
- ⚪ Inter-company transfers
- ⚪ Consignment stock tracking

---

## 2. MODULE ARCHITECTURE

### 2.1 Architectural Principles

The Inventory Module follows **Project420's 3-tier architecture**:

```
┌─────────────────────────────────────────────────────────────┐
│                        UI LAYER                              │
│  (Blazor Server - Future, API Integration for PoC)         │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LOGIC LAYER                       │
│                                                              │
│  ┌──────────────┬──────────────┬──────────────────────────┐│
│  │ProductService│MovementService│StockTransferService     ││
│  │              │               │                          ││
│  │- GetSOH()    │- CreateMovement│- ValidateTransfer()    ││
│  │- GetByBatch()│- ReverseMovement│- ExecuteTransfer()   ││
│  │- LowStockCheck│- ValidateMovement│- GetTransferHistory││
│  └──────────────┴──────────────┴──────────────────────────┘│
│                                                              │
│  ┌──────────────┬──────────────┬──────────────────────────┐│
│  │StockAdjustService│CycleCountService│SerialBatchService││
│  │                  │                  │                  ││
│  │- CreateAdjustment│- StartCount()    │- GenerateBatch() ││
│  │- ApproveAdjustment│- RecordCount()  │- GenerateSerial()││
│  │- RejectAdjustment│- FinalizeCount() │- ValidateNumber()││
│  └──────────────┴──────────────┴──────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                         │
│                                                              │
│  ┌──────────────┬──────────────┬──────────────────────────┐│
│  │ProductRepo   │MovementRepo  │TransactionDetailRepo     ││
│  │              │              │                          ││
│  │IProductRepo  │IMovementRepo │ITransactionDetailRepo    ││
│  └──────────────┴──────────────┴──────────────────────────┘│
│                                                              │
│  ┌──────────────┬──────────────┬──────────────────────────┐│
│  │StockAdjustRepo│CycleCountRepo│SerialBatchRepo          ││
│  │               │              │                          ││
│  │IStockAdjustRepo│ICycleCountRepo│ISerialBatchRepo       ││
│  └──────────────┴──────────────┴──────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       MODEL LAYER                            │
│                                                              │
│  Product, ProductCategory, Site, Zone, Bin, Movements,      │
│  TransactionDetails, StockAdjustments, CycleCounts,         │
│  SerialNumbers, BatchNumbers                                 │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       DATABASE                               │
│                    PostgreSQL 17                             │
│              Database: project420_inventory                  │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Key Design Decisions

**Decision 1: Movement-Based SOH**
- **Rationale**: Ensures 100% accuracy, full audit trail, compliance
- **Impact**: SOH queries require aggregation (performance optimization needed)
- **Implementation**: Indexed views, caching strategies

**Decision 2: Option A Movement Architecture**
- **Rationale**: Unified `TransactionDetails` table with specialized headers
- **Impact**: Consistent movement generation across all modules
- **Implementation**: See [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md)

**Decision 3: Batch-First, Serial-Optional**
- **Rationale**: All cannabis products tracked by batch; serials only for high-value retail items
- **Impact**: Batch number mandatory on all movements; serial number optional
- **Implementation**: See [06-SERIAL-BATCH-GENERATION.md](06-SERIAL-BATCH-GENERATION.md)

**Decision 4: Multi-Location Hierarchy**
- **Rationale**: Support complex warehouse layouts (Site → Zone → Bin)
- **Impact**: SOH calculated per location, transfers between locations
- **Implementation**: LocationId on Movements table (composite: Site + Zone + Bin)

**Decision 5: Soft Delete + 7-Year Retention**
- **Rationale**: POPIA compliance (retain financial records 7 years)
- **Impact**: All entities have `IsDeleted` flag; queries filter `IsDeleted = 0`
- **Implementation**: Global query filter in DbContext

---

## 3. CORE ENTITIES

### 3.1 Product

**Purpose**: Master data for all inventory items (raw materials, work-in-progress, finished goods).

**Schema**:
```sql
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(50) NOT NULL UNIQUE,
    ProductName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,

    -- Categorization
    ProductCategoryId INT NOT NULL,
    ProductType NVARCHAR(50) NOT NULL, -- 'RawMaterial', 'FinishedGood', 'PreRoll', etc.

    -- Cannabis-specific
    IsCannabisProduct BIT NOT NULL DEFAULT 0,
    THCPercentage DECIMAL(5,2) NULL,
    CBDPercentage DECIMAL(5,2) NULL,
    StrainName NVARCHAR(100) NULL,

    -- Inventory settings
    UnitOfMeasure NVARCHAR(20) NOT NULL, -- 'g', 'kg', 'unit', 'ea'
    IsSerialTracked BIT NOT NULL DEFAULT 0,
    IsBatchTracked BIT NOT NULL DEFAULT 1, -- All cannabis = batch tracked
    AllowNegativeStock BIT NOT NULL DEFAULT 0,

    -- Pricing
    StandardCostPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    StandardSellPrice DECIMAL(18,2) NOT NULL DEFAULT 0,

    -- Reorder settings
    ReorderLevel DECIMAL(18,4) NULL,
    ReorderQuantity DECIMAL(18,4) NULL,

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME NULL,
    DeletedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_Products_ProductCategory FOREIGN KEY (ProductCategoryId)
        REFERENCES ProductCategories(ProductCategoryId)
);

CREATE INDEX IX_Products_ProductCode ON Products(ProductCode);
CREATE INDEX IX_Products_ProductCategory ON Products(ProductCategoryId);
CREATE INDEX IX_Products_Active ON Products(IsActive, IsDeleted);
CREATE INDEX IX_Products_Cannabis ON Products(IsCannabisProduct) WHERE IsCannabisProduct = 1;
```

**C# Entity**:
```csharp
public class Product : AuditableEntity
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Categorization
    public int ProductCategoryId { get; set; }
    public ProductCategory ProductCategory { get; set; } = null!;
    public ProductType ProductType { get; set; }

    // Cannabis-specific
    public bool IsCannabisProduct { get; set; }
    public decimal? THCPercentage { get; set; }
    public decimal? CBDPercentage { get; set; }
    public string? StrainName { get; set; }

    // Inventory settings
    public string UnitOfMeasure { get; set; } = "g";
    public bool IsSerialTracked { get; set; }
    public bool IsBatchTracked { get; set; } = true;
    public bool AllowNegativeStock { get; set; }

    // Pricing
    public decimal StandardCostPrice { get; set; }
    public decimal StandardSellPrice { get; set; }

    // Reorder settings
    public decimal? ReorderLevel { get; set; }
    public decimal? ReorderQuantity { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
    public ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
}

public enum ProductType
{
    RawMaterial,
    WorkInProgress,
    FinishedGood,
    PreRoll,
    PreRollCannabis,
    PreRollEmpty,
    PackagingMaterial,
    Consumable
}
```

**Validation Rules** (FluentValidation):
```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Product code is required")
            .MaximumLength(50)
            .Matches("^[A-Z0-9-]+$").WithMessage("Product code must be alphanumeric with hyphens");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200);

        RuleFor(x => x.ProductCategoryId)
            .GreaterThan(0).WithMessage("Product category is required");

        RuleFor(x => x.UnitOfMeasure)
            .NotEmpty()
            .Must(BeValidUOM).WithMessage("Invalid unit of measure");

        // Cannabis validation
        When(x => x.IsCannabisProduct, () =>
        {
            RuleFor(x => x.IsBatchTracked)
                .Equal(true).WithMessage("Cannabis products must be batch tracked");

            RuleFor(x => x.THCPercentage)
                .InclusiveBetween(0, 100).When(x => x.THCPercentage.HasValue);

            RuleFor(x => x.CBDPercentage)
                .InclusiveBetween(0, 100).When(x => x.CBDPercentage.HasValue);
        });

        // Pricing validation
        RuleFor(x => x.StandardCostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative");

        RuleFor(x => x.StandardSellPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sell price cannot be negative");

        // Reorder validation
        When(x => x.ReorderLevel.HasValue, () =>
        {
            RuleFor(x => x.ReorderLevel)
                .GreaterThan(0).WithMessage("Reorder level must be positive");
        });
    }

    private bool BeValidUOM(string uom)
    {
        var validUOMs = new[] { "g", "kg", "unit", "ea", "ml", "l" };
        return validUOMs.Contains(uom.ToLower());
    }
}
```

### 3.2 ProductCategory

**Purpose**: Hierarchical categorization of products (supports unlimited depth).

**Schema**:
```sql
CREATE TABLE ProductCategories (
    ProductCategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryCode NVARCHAR(50) NOT NULL UNIQUE,
    CategoryName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,

    -- Hierarchy
    ParentCategoryId INT NULL,
    CategoryLevel INT NOT NULL DEFAULT 0,
    CategoryPath NVARCHAR(500) NOT NULL, -- e.g., "/1/5/12/" for breadcrumb navigation

    -- Cannabis-specific
    IsCannabisCategory BIT NOT NULL DEFAULT 0,

    -- Display
    DisplayOrder INT NOT NULL DEFAULT 0,

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_ProductCategories_Parent FOREIGN KEY (ParentCategoryId)
        REFERENCES ProductCategories(ProductCategoryId)
);

CREATE INDEX IX_ProductCategories_Parent ON ProductCategories(ParentCategoryId);
CREATE INDEX IX_ProductCategories_Path ON ProductCategories(CategoryPath);
```

**Example Hierarchy**:
```
1. Cannabis Products (IsCannabisCategory=1)
   ├── 1.1 Raw Flower
   │   ├── 1.1.1 Indica
   │   ├── 1.1.2 Sativa
   │   └── 1.1.3 Hybrid
   ├── 1.2 Pre-Rolls
   │   ├── 1.2.1 Indica Pre-Rolls
   │   └── 1.2.2 Sativa Pre-Rolls
   └── 1.3 Concentrates

2. Packaging Materials
   ├── 2.1 Pre-Roll Cones
   ├── 2.2 Jars & Containers
   └── 2.3 Labels

3. Consumables
   └── 3.1 Office Supplies
```

### 3.3 Movement

**Purpose**: **THE CORE TABLE**. Records every stock change in the system.

**Schema**:
```sql
CREATE TABLE Movements (
    MovementId INT IDENTITY(1,1) PRIMARY KEY,

    -- Product & Location
    ProductId INT NOT NULL,
    SiteId INT NOT NULL,
    ZoneId INT NULL,
    BinId INT NULL,

    -- Movement details
    MovementType NVARCHAR(50) NOT NULL, -- 'GRV', 'Sale', 'ProductionInput', 'Adjustment', etc.
    MovementDirection NVARCHAR(10) NOT NULL, -- 'IN' or 'OUT'
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Traceability
    BatchNumber NVARCHAR(100) NULL,
    SerialNumber NVARCHAR(50) NULL,

    -- Transaction linkage
    TransactionType NVARCHAR(50) NULL, -- 'Retail', 'Purchase', 'Production', 'StockAdjustment'
    TransactionId INT NULL, -- HeaderId from RetailTransactionHeaders, PurchaseHeaders, etc.
    TransactionDetailId INT NULL, -- DetailId from TransactionDetails

    -- Reference
    ReferenceNumber NVARCHAR(100) NULL, -- e.g., "GRV-2024-001", "SALE-2024-12345"
    Notes NVARCHAR(500) NULL,

    -- Reversal handling
    IsReversed BIT NOT NULL DEFAULT 0,
    ReversedByMovementId INT NULL,
    ReversalReason NVARCHAR(500) NULL,

    -- Audit
    MovementDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_Movements_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Movements_Site FOREIGN KEY (SiteId) REFERENCES Sites(SiteId),
    CONSTRAINT FK_Movements_ReversedBy FOREIGN KEY (ReversedByMovementId)
        REFERENCES Movements(MovementId)
);

-- CRITICAL INDEXES FOR PERFORMANCE
CREATE INDEX IX_Movements_Product_Site ON Movements(ProductId, SiteId) INCLUDE (Quantity, MovementDirection, IsDeleted);
CREATE INDEX IX_Movements_Batch ON Movements(BatchNumber) WHERE BatchNumber IS NOT NULL;
CREATE INDEX IX_Movements_Serial ON Movements(SerialNumber) WHERE SerialNumber IS NOT NULL;
CREATE INDEX IX_Movements_Transaction ON Movements(TransactionType, TransactionId);
CREATE INDEX IX_Movements_Date ON Movements(MovementDate);
CREATE INDEX IX_Movements_Type ON Movements(MovementType);
```

**C# Entity**:
```csharp
public class Movement : AuditableEntity
{
    public int MovementId { get; set; }

    // Product & Location
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int SiteId { get; set; }
    public Site Site { get; set; } = null!;
    public int? ZoneId { get; set; }
    public Zone? Zone { get; set; }
    public int? BinId { get; set; }
    public Bin? Bin { get; set; }

    // Movement details
    public MovementType MovementType { get; set; }
    public MovementDirection MovementDirection { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Traceability
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Transaction linkage
    public string? TransactionType { get; set; }
    public int? TransactionId { get; set; }
    public int? TransactionDetailId { get; set; }

    // Reference
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    // Reversal handling
    public bool IsReversed { get; set; }
    public int? ReversedByMovementId { get; set; }
    public Movement? ReversedByMovement { get; set; }
    public string? ReversalReason { get; set; }

    // Audit
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}

public enum MovementType
{
    GRV,                    // Goods Received (Purchase)
    GRVReturn,              // Return to Supplier
    Sale,                   // Retail/Wholesale Sale
    SaleReturn,             // Customer Return
    ProductionInput,        // Raw material consumed
    ProductionOutput,       // Finished goods produced
    TransferOut,            // Stock leaving location
    TransferIn,             // Stock arriving at location
    AdjustmentIn,           // Correction (increase)
    AdjustmentOut,          // Correction (decrease)
    CycleCount,             // Stock take adjustment
    Wastage,                // Spoilage, damage
    Theft                   // Loss
}

public enum MovementDirection
{
    IN,     // Increases SOH
    OUT     // Decreases SOH
}
```

### 3.4 Site (Location Hierarchy - Level 1)

**Purpose**: Physical locations (warehouses, retail stores, production facilities).

**Schema**:
```sql
CREATE TABLE Sites (
    SiteId INT IDENTITY(1,1) PRIMARY KEY,
    SiteCode NVARCHAR(50) NOT NULL UNIQUE,
    SiteName NVARCHAR(200) NOT NULL,
    SiteType NVARCHAR(50) NOT NULL, -- 'Warehouse', 'RetailStore', 'ProductionFacility'

    -- Address
    AddressLine1 NVARCHAR(200) NULL,
    AddressLine2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Country NVARCHAR(100) NOT NULL DEFAULT 'South Africa',

    -- License (cannabis compliance)
    LicenseNumber NVARCHAR(100) NULL,
    LicenseExpiry DATE NULL,

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL
);
```

### 3.5 Zone (Location Hierarchy - Level 2)

**Purpose**: Sections within a site (e.g., "Receiving", "Picking", "Quarantine").

**Schema**:
```sql
CREATE TABLE Zones (
    ZoneId INT IDENTITY(1,1) PRIMARY KEY,
    SiteId INT NOT NULL,
    ZoneCode NVARCHAR(50) NOT NULL,
    ZoneName NVARCHAR(200) NOT NULL,
    ZoneType NVARCHAR(50) NULL, -- 'Receiving', 'Storage', 'Picking', 'Quarantine', 'Production'

    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_Zones_Site FOREIGN KEY (SiteId) REFERENCES Sites(SiteId),
    CONSTRAINT UK_Zones_Code UNIQUE (SiteId, ZoneCode)
);
```

### 3.6 Bin (Location Hierarchy - Level 3)

**Purpose**: Specific storage locations within a zone (e.g., "A1-01-05" = Aisle 1, Rack 01, Shelf 05).

**Schema**:
```sql
CREATE TABLE Bins (
    BinId INT IDENTITY(1,1) PRIMARY KEY,
    ZoneId INT NOT NULL,
    BinCode NVARCHAR(50) NOT NULL,
    BinName NVARCHAR(200) NOT NULL,

    -- Physical attributes
    Aisle NVARCHAR(10) NULL,
    Rack NVARCHAR(10) NULL,
    Shelf NVARCHAR(10) NULL,

    -- Capacity
    MaxWeight DECIMAL(18,4) NULL,
    MaxVolume DECIMAL(18,4) NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_Bins_Zone FOREIGN KEY (ZoneId) REFERENCES Zones(ZoneId),
    CONSTRAINT UK_Bins_Code UNIQUE (ZoneId, BinCode)
);
```

**Example Location Hierarchy**:
```
Site: JHB-WAREHOUSE-01 (Johannesburg Main Warehouse)
  ├── Zone: RECEIVING (Goods-in area)
  │   ├── Bin: RECV-01
  │   └── Bin: RECV-02
  ├── Zone: BULK-STORAGE (High-volume storage)
  │   ├── Bin: A1-01-01 (Aisle 1, Rack 01, Shelf 01)
  │   ├── Bin: A1-01-02
  │   └── Bin: A1-02-01
  ├── Zone: PICKING (Fast-moving items)
  │   └── Bin: PICK-ZONE-A
  └── Zone: QUARANTINE (Quality hold)
      └── Bin: QUAR-01
```

---

## 4. MOVEMENT SYSTEM

### 4.1 Movement Generation Principles

**CRITICAL RULE**: Every inventory change MUST create a Movement record.

**Consistency Requirements**:
1. **All modules use the same pattern** (see [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md))
2. **MovementService is the ONLY way to create movements** (no direct repository writes)
3. **Batch/serial numbers propagated from transaction details**
4. **Movement direction matches transaction type**

**Movement Generation Pattern**:
```csharp
// In RetailTransactionService (after creating transaction)
foreach (var detail in transactionDetails)
{
    await _movementService.CreateMovementAsync(new CreateMovementDto
    {
        ProductId = detail.ProductId,
        SiteId = transaction.SiteId,
        MovementType = MovementType.Sale,
        MovementDirection = MovementDirection.OUT,
        Quantity = detail.Quantity,
        UnitOfMeasure = detail.UnitOfMeasure,
        BatchNumber = detail.BatchNumber,
        SerialNumber = detail.SerialNumber,
        TransactionType = "Retail",
        TransactionId = transaction.TransactionId,
        TransactionDetailId = detail.TransactionDetailId,
        ReferenceNumber = transaction.TransactionNumber,
        MovementDate = transaction.TransactionDate,
        CreatedBy = transaction.CreatedBy
    });
}
```

### 4.2 Movement Types & Directions

| Movement Type | Direction | Source Module | Trigger Event |
|--------------|-----------|---------------|---------------|
| `GRV` | IN | Purchasing | Goods Received Note created |
| `GRVReturn` | OUT | Purchasing | Return to Supplier processed |
| `Sale` | OUT | Retail/Wholesale | Transaction completed |
| `SaleReturn` | IN | Retail/Wholesale | Customer return accepted |
| `ProductionInput` | OUT | Production | Raw materials consumed |
| `ProductionOutput` | IN | Production | Finished goods completed |
| `TransferOut` | OUT | Inventory | Stock leaves source location |
| `TransferIn` | IN | Inventory | Stock arrives at destination |
| `AdjustmentIn` | IN | Inventory | Correction (add stock) |
| `AdjustmentOut` | OUT | Inventory | Correction (remove stock) |
| `CycleCount` | IN/OUT | Inventory | Stock take variance |
| `Wastage` | OUT | Production/Inventory | Spoilage, damage, spillage |
| `Theft` | OUT | Inventory | Loss, shrinkage |

### 4.3 MovementService Implementation

**Interface**:
```csharp
public interface IMovementService
{
    // Core operations
    Task<MovementDto> CreateMovementAsync(CreateMovementDto dto);
    Task<MovementDto> ReverseMovementAsync(int movementId, string reason, string userId);
    Task<IEnumerable<MovementDto>> CreateBulkMovementsAsync(IEnumerable<CreateMovementDto> dtos);

    // Queries
    Task<IEnumerable<MovementDto>> GetMovementsByProductAsync(int productId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<MovementDto>> GetMovementsByBatchAsync(string batchNumber);
    Task<IEnumerable<MovementDto>> GetMovementsBySerialAsync(string serialNumber);
    Task<IEnumerable<MovementDto>> GetMovementsBySiteAsync(int siteId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<MovementDto>> GetMovementsByTransactionAsync(string transactionType, int transactionId);

    // Validation
    Task<bool> ValidateMovementAsync(CreateMovementDto dto);
    Task<bool> CanReverseMovementAsync(int movementId);
}
```

**Implementation Highlights**:
```csharp
public class MovementService : IMovementService
{
    private readonly IMovementRepository _movementRepo;
    private readonly IProductRepository _productRepo;
    private readonly ILogger<MovementService> _logger;

    public async Task<MovementDto> CreateMovementAsync(CreateMovementDto dto)
    {
        // 1. Validate movement
        await ValidateMovementAsync(dto);

        // 2. Get product details (for UOM, batch/serial requirements)
        var product = await _productRepo.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new NotFoundException($"Product {dto.ProductId} not found");

        // 3. Validate batch/serial requirements
        if (product.IsBatchTracked && string.IsNullOrEmpty(dto.BatchNumber))
            throw new ValidationException("Batch number required for batch-tracked products");

        if (product.IsSerialTracked && string.IsNullOrEmpty(dto.SerialNumber))
            throw new ValidationException("Serial number required for serial-tracked products");

        // 4. Check for negative stock (if product doesn't allow it)
        if (!product.AllowNegativeStock && dto.MovementDirection == MovementDirection.OUT)
        {
            var currentSOH = await GetSOHAsync(dto.ProductId, dto.SiteId, dto.BatchNumber);
            if (currentSOH < dto.Quantity)
                throw new ValidationException($"Insufficient stock. SOH: {currentSOH}, Requested: {dto.Quantity}");
        }

        // 5. Create movement entity
        var movement = new Movement
        {
            ProductId = dto.ProductId,
            SiteId = dto.SiteId,
            ZoneId = dto.ZoneId,
            BinId = dto.BinId,
            MovementType = dto.MovementType,
            MovementDirection = dto.MovementDirection,
            Quantity = dto.Quantity,
            UnitOfMeasure = dto.UnitOfMeasure ?? product.UnitOfMeasure,
            BatchNumber = dto.BatchNumber,
            SerialNumber = dto.SerialNumber,
            TransactionType = dto.TransactionType,
            TransactionId = dto.TransactionId,
            TransactionDetailId = dto.TransactionDetailId,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            MovementDate = dto.MovementDate ?? DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };

        // 6. Save to database
        await _movementRepo.AddAsync(movement);
        await _movementRepo.SaveChangesAsync();

        // 7. Log movement
        _logger.LogInformation(
            "Movement created: {MovementType} {Direction} {Quantity} {UOM} of Product {ProductId} at Site {SiteId}",
            movement.MovementType, movement.MovementDirection, movement.Quantity, movement.UnitOfMeasure,
            movement.ProductId, movement.SiteId);

        // 8. Return DTO
        return MapToDto(movement);
    }

    public async Task<MovementDto> ReverseMovementAsync(int movementId, string reason, string userId)
    {
        // 1. Get original movement
        var original = await _movementRepo.GetByIdAsync(movementId);
        if (original == null)
            throw new NotFoundException($"Movement {movementId} not found");

        // 2. Check if already reversed
        if (original.IsReversed)
            throw new ValidationException("Movement already reversed");

        // 3. Check if can be reversed (business rules)
        await CanReverseMovementAsync(movementId);

        // 4. Create reversal movement (opposite direction, same quantity)
        var reversal = new Movement
        {
            ProductId = original.ProductId,
            SiteId = original.SiteId,
            ZoneId = original.ZoneId,
            BinId = original.BinId,
            MovementType = original.MovementType,
            MovementDirection = original.MovementDirection == MovementDirection.IN
                ? MovementDirection.OUT
                : MovementDirection.IN,
            Quantity = original.Quantity,
            UnitOfMeasure = original.UnitOfMeasure,
            BatchNumber = original.BatchNumber,
            SerialNumber = original.SerialNumber,
            TransactionType = original.TransactionType,
            TransactionId = original.TransactionId,
            ReferenceNumber = $"REV-{original.ReferenceNumber}",
            Notes = $"Reversal of Movement {movementId}: {reason}",
            MovementDate = DateTime.UtcNow,
            CreatedBy = userId
        };

        // 5. Mark original as reversed
        original.IsReversed = true;
        original.ReversedByMovementId = reversal.MovementId;
        original.ReversalReason = reason;

        // 6. Save both
        await _movementRepo.AddAsync(reversal);
        await _movementRepo.UpdateAsync(original);
        await _movementRepo.SaveChangesAsync();

        _logger.LogWarning("Movement {MovementId} reversed by {UserId}. Reason: {Reason}",
            movementId, userId, reason);

        return MapToDto(reversal);
    }

    private async Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null)
    {
        // This is a simplified version - see Section 5 for optimized implementation
        var movements = await _movementRepo.GetMovementsByProductAsync(productId, siteId, batchNumber);

        var soh = movements
            .Where(m => !m.IsDeleted && !m.IsReversed)
            .Sum(m => m.MovementDirection == MovementDirection.IN ? m.Quantity : -m.Quantity);

        return soh;
    }
}
```

---

## 5. SOH CALCULATION ENGINE

### 5.1 Core Principle

**CRITICAL**: SOH is **NEVER stored** in the `Products` table or any other table. It is always calculated in real-time from the `Movements` table:

```sql
SOH = SUM(IN movements) - SUM(OUT movements)
```

**Why?**
- **100% Accuracy**: No possibility of SOH drift or inconsistency
- **Full Audit Trail**: Every stock change is traceable
- **Compliance**: Regulators can verify every movement
- **Tamper-Proof**: Cannot manipulate SOH without creating a movement

**Trade-Off**: Requires aggregation queries (performance optimization critical).

### 5.2 SOH Query Patterns

**Basic SOH (Product + Site)**:
```sql
SELECT
    ProductId,
    SiteId,
    SUM(CASE
        WHEN MovementDirection = 'IN' THEN Quantity
        ELSE -Quantity
    END) AS StockOnHand
FROM Movements
WHERE ProductId = @ProductId
  AND SiteId = @SiteId
  AND IsDeleted = 0
  AND IsReversed = 0
GROUP BY ProductId, SiteId;
```

**SOH by Batch**:
```sql
SELECT
    ProductId,
    SiteId,
    BatchNumber,
    SUM(CASE
        WHEN MovementDirection = 'IN' THEN Quantity
        ELSE -Quantity
    END) AS StockOnHand
FROM Movements
WHERE ProductId = @ProductId
  AND SiteId = @SiteId
  AND BatchNumber = @BatchNumber
  AND IsDeleted = 0
  AND IsReversed = 0
GROUP BY ProductId, SiteId, BatchNumber;
```

**SOH by Location (Site + Zone + Bin)**:
```sql
SELECT
    ProductId,
    SiteId,
    ZoneId,
    BinId,
    BatchNumber,
    SUM(CASE
        WHEN MovementDirection = 'IN' THEN Quantity
        ELSE -Quantity
    END) AS StockOnHand
FROM Movements
WHERE ProductId = @ProductId
  AND IsDeleted = 0
  AND IsReversed = 0
GROUP BY ProductId, SiteId, ZoneId, BinId, BatchNumber
HAVING SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) > 0;
```

**SOH for All Products (Dashboard)**:
```sql
SELECT
    p.ProductId,
    p.ProductCode,
    p.ProductName,
    s.SiteId,
    s.SiteName,
    SUM(CASE
        WHEN m.MovementDirection = 'IN' THEN m.Quantity
        ELSE -m.Quantity
    END) AS StockOnHand,
    p.UnitOfMeasure
FROM Products p
CROSS JOIN Sites s
LEFT JOIN Movements m ON m.ProductId = p.ProductId AND m.SiteId = s.SiteId
WHERE p.IsActive = 1
  AND p.IsDeleted = 0
  AND s.IsActive = 1
  AND s.IsDeleted = 0
  AND (m.IsDeleted = 0 OR m.IsDeleted IS NULL)
  AND (m.IsReversed = 0 OR m.IsReversed IS NULL)
GROUP BY p.ProductId, p.ProductCode, p.ProductName, s.SiteId, s.SiteName, p.UnitOfMeasure
HAVING SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) > 0
ORDER BY p.ProductCode, s.SiteName;
```

### 5.3 Performance Optimization Strategies

**Problem**: SOH queries require full table scans of `Movements` (expensive for large datasets).

**Solutions**:

**1. Indexed Views (SQL Server / PostgreSQL Materialized Views)**

Create a pre-aggregated view that SQL Server maintains automatically:

```sql
-- SQL Server Indexed View
CREATE VIEW vw_StockOnHand WITH SCHEMABINDING
AS
SELECT
    m.ProductId,
    m.SiteId,
    m.BatchNumber,
    SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) AS StockOnHand,
    COUNT_BIG(*) AS MovementCount
FROM dbo.Movements m
WHERE m.IsDeleted = 0 AND m.IsReversed = 0
GROUP BY m.ProductId, m.SiteId, m.BatchNumber;

CREATE UNIQUE CLUSTERED INDEX IX_vw_SOH ON vw_StockOnHand(ProductId, SiteId, BatchNumber);
```

**PostgreSQL Materialized View**:
```sql
CREATE MATERIALIZED VIEW mv_stock_on_hand AS
SELECT
    ProductId,
    SiteId,
    BatchNumber,
    SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS StockOnHand,
    COUNT(*) AS MovementCount
FROM Movements
WHERE IsDeleted = FALSE AND IsReversed = FALSE
GROUP BY ProductId, SiteId, BatchNumber;

CREATE UNIQUE INDEX idx_mv_soh ON mv_stock_on_hand(ProductId, SiteId, BatchNumber);

-- Refresh strategy (after each movement batch)
REFRESH MATERIALIZED VIEW CONCURRENTLY mv_stock_on_hand;
```

**2. Application-Level Caching (Redis)**

Cache SOH values with TTL (Time-To-Live):

```csharp
public class CachedSOHService : ISOHService
{
    private readonly IMovementRepository _movementRepo;
    private readonly IDistributedCache _cache; // Redis
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null)
    {
        var cacheKey = $"SOH:{productId}:{siteId}:{batchNumber ?? "ALL"}";

        // Try get from cache
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            return decimal.Parse(cached);
        }

        // Calculate from database
        var soh = await CalculateSOHFromMovementsAsync(productId, siteId, batchNumber);

        // Cache result
        await _cache.SetStringAsync(cacheKey, soh.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheDuration
        });

        return soh;
    }

    public async Task InvalidateSOHCacheAsync(int productId, int siteId, string? batchNumber = null)
    {
        var cacheKey = $"SOH:{productId}:{siteId}:{batchNumber ?? "ALL"}";
        await _cache.RemoveAsync(cacheKey);
    }
}

// In MovementService.CreateMovementAsync() - invalidate cache after creating movement
await _cachedSOHService.InvalidateSOHCacheAsync(dto.ProductId, dto.SiteId, dto.BatchNumber);
```

**3. Composite Indexes on Movements Table**

Ensure queries use covering indexes:

```sql
-- Primary SOH calculation index
CREATE INDEX IX_Movements_SOH_Calculation
ON Movements(ProductId, SiteId, BatchNumber)
INCLUDE (Quantity, MovementDirection, IsDeleted, IsReversed);

-- By site only
CREATE INDEX IX_Movements_Site_SOH
ON Movements(SiteId, ProductId)
INCLUDE (Quantity, MovementDirection, IsDeleted, IsReversed);

-- By date range (for historical SOH)
CREATE INDEX IX_Movements_Date_SOH
ON Movements(ProductId, SiteId, MovementDate)
INCLUDE (Quantity, MovementDirection, IsDeleted, IsReversed);
```

**4. Partitioning (Future - High Volume)**

For datasets > 10 million movements, partition by date:

```sql
-- Partition by year/month
CREATE PARTITION FUNCTION pf_Movements_ByMonth (DATETIME)
AS RANGE RIGHT FOR VALUES (
    '2024-01-01', '2024-02-01', '2024-03-01', -- ... monthly partitions
);

CREATE PARTITION SCHEME ps_Movements_ByMonth
AS PARTITION pf_Movements_ByMonth
ALL TO ([PRIMARY]);

-- Create table on partition scheme
CREATE TABLE Movements (
    MovementId INT IDENTITY(1,1),
    MovementDate DATETIME NOT NULL,
    -- other columns...
) ON ps_Movements_ByMonth(MovementDate);
```

### 5.4 SOH Service Implementation

**Interface**:
```csharp
public interface ISOHService
{
    // Basic SOH queries
    Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null);
    Task<decimal> GetSOHAtDateAsync(int productId, int siteId, DateTime asOfDate, string? batchNumber = null);
    Task<IEnumerable<SOHDto>> GetSOHByProductAsync(int productId);
    Task<IEnumerable<SOHDto>> GetSOHBySiteAsync(int siteId);
    Task<IEnumerable<SOHDto>> GetSOHByBatchAsync(string batchNumber);

    // Advanced queries
    Task<IEnumerable<SOHLocationDto>> GetSOHByLocationAsync(int productId, int siteId);
    Task<IEnumerable<LowStockDto>> GetLowStockItemsAsync(int siteId);
    Task<IEnumerable<NegativeStockDto>> GetNegativeStockItemsAsync(int siteId);
    Task<IEnumerable<SOHDto>> GetAllSOHAsync(int? siteId = null);

    // Validation
    Task<bool> HasSufficientStockAsync(int productId, int siteId, decimal quantity, string? batchNumber = null);
}
```

**Implementation**:
```csharp
public class SOHService : ISOHService
{
    private readonly IMovementRepository _movementRepo;
    private readonly IProductRepository _productRepo;
    private readonly IDistributedCache _cache;

    public async Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null)
    {
        // Try cache first
        var cacheKey = $"SOH:{productId}:{siteId}:{batchNumber ?? "ALL"}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return decimal.Parse(cached);

        // Query from database (use indexed view if available, else calculate)
        var soh = await _movementRepo.GetSOHAsync(productId, siteId, batchNumber);

        // Cache for 5 minutes
        await _cache.SetStringAsync(cacheKey, soh.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return soh;
    }

    public async Task<decimal> GetSOHAtDateAsync(int productId, int siteId, DateTime asOfDate, string? batchNumber = null)
    {
        // Historical SOH (cannot use cache - date varies)
        return await _movementRepo.GetSOHAtDateAsync(productId, siteId, asOfDate, batchNumber);
    }

    public async Task<IEnumerable<LowStockDto>> GetLowStockItemsAsync(int siteId)
    {
        // Get all products with reorder levels
        var products = await _productRepo.GetProductsWithReorderLevelsAsync();

        var lowStockItems = new List<LowStockDto>();

        foreach (var product in products)
        {
            var soh = await GetSOHAsync(product.ProductId, siteId);

            if (product.ReorderLevel.HasValue && soh <= product.ReorderLevel.Value)
            {
                lowStockItems.Add(new LowStockDto
                {
                    ProductId = product.ProductId,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    SiteId = siteId,
                    CurrentSOH = soh,
                    ReorderLevel = product.ReorderLevel.Value,
                    ReorderQuantity = product.ReorderQuantity,
                    UnitOfMeasure = product.UnitOfMeasure
                });
            }
        }

        return lowStockItems;
    }

    public async Task<bool> HasSufficientStockAsync(int productId, int siteId, decimal quantity, string? batchNumber = null)
    {
        var soh = await GetSOHAsync(productId, siteId, batchNumber);
        return soh >= quantity;
    }
}
```

---

## 6. BATCH & SERIAL NUMBER TRACKING

**See [06-SERIAL-BATCH-GENERATION.md](06-SERIAL-BATCH-GENERATION.md) for full specification.**

### 6.1 Quick Summary

**Batch Numbers (16 digits)**: `SSTTYYYYMMDDNNNN`
- SS = Site ID (2 digits)
- TT = Batch Type (2 digits)
- YYYYMMDD = Date (8 digits)
- NNNN = Sequence number (4 digits)
- Example: `0101202412110001` = Site 01, Type 01 (Cultivation), 2024-12-11, Sequence 0001

**Serial Numbers (28 digits - Full)**: `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC`
- Embedded metadata for full traceability
- Example: `0000110120241211000010000100010`

**Serial Numbers (13 digits - Short)**: `SSYYMMDDNNNNN`
- EAN-13 compatible (barcode)
- Luhn check digit
- Example: `0124121100015`

**Key Rules**:
- All cannabis products = batch tracked (mandatory)
- Only high-value retail items = serial tracked (pre-rolls, vapes)
- Batch numbers generated at:
  - Cultivation (harvest batch)
  - Production (manufacturing batch for pre-rolls, concentrates)
  - Purchasing (supplier batch for raw materials)
- Serial numbers generated at:
  - Retail production (individual pre-rolls, vapes)
  - Product registration (for imported finished goods)

---

## 7. MULTI-LOCATION MANAGEMENT

### 7.1 Location Hierarchy

Project420 supports a **3-level location hierarchy**:

```
Site (Level 1) → Zone (Level 2) → Bin (Level 3)
```

**Example**:
```
Site: JHB-WAREHOUSE-01
  ├── Zone: RECEIVING
  │   ├── Bin: RECV-01
  │   └── Bin: RECV-02
  ├── Zone: BULK-STORAGE
  │   ├── Bin: A1-01-01
  │   ├── Bin: A1-01-02
  │   └── Bin: A1-02-01
  └── Zone: PICKING
      └── Bin: PICK-ZONE-A
```

### 7.2 SOH by Location

SOH can be calculated at any level of the hierarchy:

**By Site Only**:
```sql
SELECT SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS SOH
FROM Movements
WHERE ProductId = @ProductId AND SiteId = @SiteId AND IsDeleted = 0 AND IsReversed = 0;
```

**By Zone**:
```sql
SELECT SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS SOH
FROM Movements
WHERE ProductId = @ProductId AND SiteId = @SiteId AND ZoneId = @ZoneId
  AND IsDeleted = 0 AND IsReversed = 0;
```

**By Bin (Most Granular)**:
```sql
SELECT SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS SOH
FROM Movements
WHERE ProductId = @ProductId AND SiteId = @SiteId AND ZoneId = @ZoneId AND BinId = @BinId
  AND IsDeleted = 0 AND IsReversed = 0;
```

### 7.3 Location Assignment Rules

**Receiving Zone**:
- All GRVs default to RECEIVING zone
- Stock must be transferred to storage zones after quality check

**Bulk Storage Zone**:
- High-volume, slow-moving items
- Large batch quantities

**Picking Zone**:
- Fast-moving items
- Small batch quantities
- Close to dispatch area

**Quarantine Zone**:
- Failed quality checks
- Awaiting regulatory approval
- Cannot be sold or transferred (without approval)

**Production Zone**:
- Work-in-progress
- Raw materials allocated to production
- Finished goods (before transfer to storage)

---

## 8. STOCK ADJUSTMENTS

### 8.1 Purpose

Stock adjustments correct discrepancies between physical stock and system SOH. Common causes:
- **Wastage**: Spoilage, damage, spillage
- **Theft**: Shrinkage, loss
- **Data Entry Errors**: Incorrect GRV quantity, incorrect sale quantity
- **Cycle Count Variance**: Physical count doesn't match SOH

### 8.2 Adjustment Types

| Adjustment Type | Direction | Requires Approval | Examples |
|----------------|-----------|-------------------|----------|
| `AdjustmentIn` | IN | Yes | Found stock, correction of under-count |
| `AdjustmentOut` | OUT | Yes | Wastage, theft, breakage |
| `CycleCount` | IN/OUT | Yes | Stock take variance |
| `Wastage` | OUT | No (if < threshold) | Spoilage (cannabis past expiry) |
| `Theft` | OUT | Yes (always) | Shrinkage, loss |

### 8.3 Stock Adjustment Entity

**Schema**:
```sql
CREATE TABLE StockAdjustments (
    StockAdjustmentId INT IDENTITY(1,1) PRIMARY KEY,
    AdjustmentNumber NVARCHAR(100) NOT NULL UNIQUE, -- e.g., "ADJ-2024-001"

    -- Product & Location
    ProductId INT NOT NULL,
    SiteId INT NOT NULL,
    ZoneId INT NULL,
    BinId INT NULL,

    -- Adjustment details
    AdjustmentType NVARCHAR(50) NOT NULL, -- 'AdjustmentIn', 'AdjustmentOut', 'Wastage', 'Theft'
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Traceability
    BatchNumber NVARCHAR(100) NULL,
    SerialNumber NVARCHAR(50) NULL,

    -- Reason & Justification
    Reason NVARCHAR(500) NOT NULL,
    Notes NVARCHAR(1000) NULL,

    -- Approval workflow
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected'
    RequestedBy NVARCHAR(100) NOT NULL,
    RequestedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ApprovedBy NVARCHAR(100) NULL,
    ApprovedDate DATETIME NULL,
    RejectedBy NVARCHAR(100) NULL,
    RejectedDate DATETIME NULL,
    RejectionReason NVARCHAR(500) NULL,

    -- Movement linkage (created after approval)
    MovementId INT NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_StockAdj_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_StockAdj_Site FOREIGN KEY (SiteId) REFERENCES Sites(SiteId),
    CONSTRAINT FK_StockAdj_Movement FOREIGN KEY (MovementId) REFERENCES Movements(MovementId)
);

CREATE INDEX IX_StockAdj_Product ON StockAdjustments(ProductId);
CREATE INDEX IX_StockAdj_Status ON StockAdjustments(Status);
CREATE INDEX IX_StockAdj_Number ON StockAdjustments(AdjustmentNumber);
```

### 8.4 Adjustment Workflow

**1. Create Adjustment Request**:
```csharp
var adjustment = new StockAdjustment
{
    AdjustmentNumber = await _numberService.GenerateAdjustmentNumberAsync(),
    ProductId = dto.ProductId,
    SiteId = dto.SiteId,
    AdjustmentType = dto.AdjustmentType,
    Quantity = dto.Quantity,
    BatchNumber = dto.BatchNumber,
    Reason = dto.Reason,
    Status = "Pending",
    RequestedBy = dto.UserId,
    RequestedDate = DateTime.UtcNow
};

await _adjustmentRepo.AddAsync(adjustment);
```

**2. Approve Adjustment** (creates movement):
```csharp
public async Task ApproveAdjustmentAsync(int adjustmentId, string approvedBy)
{
    var adjustment = await _adjustmentRepo.GetByIdAsync(adjustmentId);
    if (adjustment.Status != "Pending")
        throw new ValidationException("Only pending adjustments can be approved");

    // Update adjustment status
    adjustment.Status = "Approved";
    adjustment.ApprovedBy = approvedBy;
    adjustment.ApprovedDate = DateTime.UtcNow;

    // Create movement
    var movement = await _movementService.CreateMovementAsync(new CreateMovementDto
    {
        ProductId = adjustment.ProductId,
        SiteId = adjustment.SiteId,
        ZoneId = adjustment.ZoneId,
        BinId = adjustment.BinId,
        MovementType = adjustment.AdjustmentType == "AdjustmentIn"
            ? MovementType.AdjustmentIn
            : MovementType.AdjustmentOut,
        MovementDirection = adjustment.AdjustmentType == "AdjustmentIn"
            ? MovementDirection.IN
            : MovementDirection.OUT,
        Quantity = adjustment.Quantity,
        BatchNumber = adjustment.BatchNumber,
        SerialNumber = adjustment.SerialNumber,
        ReferenceNumber = adjustment.AdjustmentNumber,
        Notes = $"Adjustment approved: {adjustment.Reason}",
        CreatedBy = approvedBy
    });

    // Link movement to adjustment
    adjustment.MovementId = movement.MovementId;

    await _adjustmentRepo.UpdateAsync(adjustment);
}
```

**3. Reject Adjustment**:
```csharp
public async Task RejectAdjustmentAsync(int adjustmentId, string rejectedBy, string reason)
{
    var adjustment = await _adjustmentRepo.GetByIdAsync(adjustmentId);
    if (adjustment.Status != "Pending")
        throw new ValidationException("Only pending adjustments can be rejected");

    adjustment.Status = "Rejected";
    adjustment.RejectedBy = rejectedBy;
    adjustment.RejectedDate = DateTime.UtcNow;
    adjustment.RejectionReason = reason;

    await _adjustmentRepo.UpdateAsync(adjustment);
}
```

---

## 9. STOCK TRANSFERS

### 9.1 Purpose

Stock transfers move inventory between locations:
- **Inter-Site**: Warehouse → Retail Store
- **Inter-Zone**: Receiving → Bulk Storage
- **Inter-Bin**: A1-01-01 → A1-02-05

### 9.2 Transfer Entity

**Schema**:
```sql
CREATE TABLE StockTransfers (
    StockTransferId INT IDENTITY(1,1) PRIMARY KEY,
    TransferNumber NVARCHAR(100) NOT NULL UNIQUE, -- e.g., "TRF-2024-001"

    -- Product
    ProductId INT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Source location
    SourceSiteId INT NOT NULL,
    SourceZoneId INT NULL,
    SourceBinId INT NULL,

    -- Destination location
    DestinationSiteId INT NOT NULL,
    DestinationZoneId INT NULL,
    DestinationBinId INT NULL,

    -- Traceability
    BatchNumber NVARCHAR(100) NULL,
    SerialNumber NVARCHAR(50) NULL,

    -- Transfer details
    TransferDate DATETIME NOT NULL DEFAULT GETDATE(),
    Reason NVARCHAR(500) NULL,
    Notes NVARCHAR(1000) NULL,

    -- Status tracking
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'InTransit', 'Completed', 'Cancelled'
    RequestedBy NVARCHAR(100) NOT NULL,
    CompletedBy NVARCHAR(100) NULL,
    CompletedDate DATETIME NULL,

    -- Movement linkage
    MovementOutId INT NULL, -- Movement at source (OUT)
    MovementInId INT NULL,  -- Movement at destination (IN)

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_StockTransfer_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_StockTransfer_SourceSite FOREIGN KEY (SourceSiteId) REFERENCES Sites(SiteId),
    CONSTRAINT FK_StockTransfer_DestSite FOREIGN KEY (DestinationSiteId) REFERENCES Sites(SiteId),
    CONSTRAINT FK_StockTransfer_MovementOut FOREIGN KEY (MovementOutId) REFERENCES Movements(MovementId),
    CONSTRAINT FK_StockTransfer_MovementIn FOREIGN KEY (MovementInId) REFERENCES Movements(MovementId)
);

CREATE INDEX IX_StockTransfer_Product ON StockTransfers(ProductId);
CREATE INDEX IX_StockTransfer_Status ON StockTransfers(Status);
CREATE INDEX IX_StockTransfer_SourceSite ON StockTransfers(SourceSiteId);
CREATE INDEX IX_StockTransfer_DestSite ON StockTransfers(DestinationSiteId);
```

### 9.3 Transfer Workflow

**1. Create Transfer Request**:
```csharp
var transfer = new StockTransfer
{
    TransferNumber = await _numberService.GenerateTransferNumberAsync(),
    ProductId = dto.ProductId,
    Quantity = dto.Quantity,
    SourceSiteId = dto.SourceSiteId,
    SourceZoneId = dto.SourceZoneId,
    SourceBinId = dto.SourceBinId,
    DestinationSiteId = dto.DestinationSiteId,
    DestinationZoneId = dto.DestinationZoneId,
    DestinationBinId = dto.DestinationBinId,
    BatchNumber = dto.BatchNumber,
    Reason = dto.Reason,
    Status = "Pending",
    RequestedBy = dto.UserId
};

await _transferRepo.AddAsync(transfer);
```

**2. Execute Transfer** (creates 2 movements):
```csharp
public async Task ExecuteTransferAsync(int transferId, string userId)
{
    var transfer = await _transferRepo.GetByIdAsync(transferId);
    if (transfer.Status != "Pending")
        throw new ValidationException("Only pending transfers can be executed");

    // Validate sufficient stock at source
    var sourceSOH = await _sohService.GetSOHAsync(
        transfer.ProductId, transfer.SourceSiteId, transfer.BatchNumber);

    if (sourceSOH < transfer.Quantity)
        throw new ValidationException($"Insufficient stock at source. SOH: {sourceSOH}, Required: {transfer.Quantity}");

    // Create OUT movement at source
    var movementOut = await _movementService.CreateMovementAsync(new CreateMovementDto
    {
        ProductId = transfer.ProductId,
        SiteId = transfer.SourceSiteId,
        ZoneId = transfer.SourceZoneId,
        BinId = transfer.SourceBinId,
        MovementType = MovementType.TransferOut,
        MovementDirection = MovementDirection.OUT,
        Quantity = transfer.Quantity,
        BatchNumber = transfer.BatchNumber,
        SerialNumber = transfer.SerialNumber,
        ReferenceNumber = transfer.TransferNumber,
        Notes = $"Transfer to Site {transfer.DestinationSiteId}",
        CreatedBy = userId
    });

    // Create IN movement at destination
    var movementIn = await _movementService.CreateMovementAsync(new CreateMovementDto
    {
        ProductId = transfer.ProductId,
        SiteId = transfer.DestinationSiteId,
        ZoneId = transfer.DestinationZoneId,
        BinId = transfer.DestinationBinId,
        MovementType = MovementType.TransferIn,
        MovementDirection = MovementDirection.IN,
        Quantity = transfer.Quantity,
        BatchNumber = transfer.BatchNumber,
        SerialNumber = transfer.SerialNumber,
        ReferenceNumber = transfer.TransferNumber,
        Notes = $"Transfer from Site {transfer.SourceSiteId}",
        CreatedBy = userId
    });

    // Update transfer status
    transfer.Status = "Completed";
    transfer.CompletedBy = userId;
    transfer.CompletedDate = DateTime.UtcNow;
    transfer.MovementOutId = movementOut.MovementId;
    transfer.MovementInId = movementIn.MovementId;

    await _transferRepo.UpdateAsync(transfer);
}
```

---

## 10. STOCK TAKES (CYCLE COUNTING)

### 10.1 Purpose

Stock takes (cycle counting) verify physical stock matches system SOH. Performed:
- **Periodically**: Monthly, quarterly, annually
- **Ad-hoc**: After suspected theft, damage, or discrepancies
- **Regulatory**: Cannabis compliance (SAHPRA, DALRRD)

### 10.2 Cycle Count Entity

**Schema**:
```sql
CREATE TABLE CycleCounts (
    CycleCountId INT IDENTITY(1,1) PRIMARY KEY,
    CountNumber NVARCHAR(100) NOT NULL UNIQUE, -- e.g., "CC-2024-001"

    -- Scope
    SiteId INT NOT NULL,
    ZoneId INT NULL,
    BinId INT NULL,
    CountType NVARCHAR(50) NOT NULL, -- 'Full', 'Partial', 'Batch', 'Product'

    -- Schedule
    ScheduledDate DATE NOT NULL,
    CountDate DATETIME NULL,

    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Scheduled', -- 'Scheduled', 'InProgress', 'Completed', 'Cancelled'

    -- Participants
    AssignedTo NVARCHAR(100) NOT NULL,
    CountedBy NVARCHAR(100) NULL,
    VerifiedBy NVARCHAR(100) NULL,

    -- Results
    ItemsExpected INT NULL,
    ItemsCounted INT NULL,
    VariancesFound INT NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_CycleCount_Site FOREIGN KEY (SiteId) REFERENCES Sites(SiteId)
);

CREATE TABLE CycleCountLines (
    CycleCountLineId INT IDENTITY(1,1) PRIMARY KEY,
    CycleCountId INT NOT NULL,

    -- Product
    ProductId INT NOT NULL,
    BatchNumber NVARCHAR(100) NULL,

    -- Count details
    SystemSOH DECIMAL(18,4) NOT NULL,
    PhysicalCount DECIMAL(18,4) NULL,
    Variance DECIMAL(18,4) NULL, -- PhysicalCount - SystemSOH
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Adjustment (if variance accepted)
    AdjustmentId INT NULL,

    -- Notes
    Notes NVARCHAR(500) NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_CycleCountLine_CycleCount FOREIGN KEY (CycleCountId) REFERENCES CycleCounts(CycleCountId),
    CONSTRAINT FK_CycleCountLine_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_CycleCountLine_Adjustment FOREIGN KEY (AdjustmentId) REFERENCES StockAdjustments(StockAdjustmentId)
);
```

### 10.3 Cycle Count Workflow

**1. Schedule Cycle Count**:
```csharp
var cycleCount = new CycleCount
{
    CountNumber = await _numberService.GenerateCycleCountNumberAsync(),
    SiteId = dto.SiteId,
    ZoneId = dto.ZoneId,
    CountType = dto.CountType,
    ScheduledDate = dto.ScheduledDate,
    AssignedTo = dto.AssignedTo,
    Status = "Scheduled",
    CreatedBy = dto.UserId
};

await _cycleCountRepo.AddAsync(cycleCount);
```

**2. Start Cycle Count** (generates count lines):
```csharp
public async Task StartCycleCountAsync(int cycleCountId, string userId)
{
    var cycleCount = await _cycleCountRepo.GetByIdAsync(cycleCountId);
    if (cycleCount.Status != "Scheduled")
        throw new ValidationException("Only scheduled counts can be started");

    // Get products in scope
    var productsInScope = await GetProductsInScopeAsync(cycleCount);

    // Create count lines
    foreach (var product in productsInScope)
    {
        var systemSOH = await _sohService.GetSOHAsync(product.ProductId, cycleCount.SiteId);

        var line = new CycleCountLine
        {
            CycleCountId = cycleCountId,
            ProductId = product.ProductId,
            BatchNumber = product.BatchNumber,
            SystemSOH = systemSOH,
            UnitOfMeasure = product.UnitOfMeasure,
            CreatedBy = userId
        };

        await _cycleCountLineRepo.AddAsync(line);
    }

    // Update status
    cycleCount.Status = "InProgress";
    cycleCount.CountDate = DateTime.UtcNow;
    cycleCount.CountedBy = userId;
    cycleCount.ItemsExpected = productsInScope.Count;

    await _cycleCountRepo.UpdateAsync(cycleCount);
}
```

**3. Record Physical Counts**:
```csharp
public async Task RecordPhysicalCountAsync(int lineId, decimal physicalCount, string userId)
{
    var line = await _cycleCountLineRepo.GetByIdAsync(lineId);

    line.PhysicalCount = physicalCount;
    line.Variance = physicalCount - line.SystemSOH;

    await _cycleCountLineRepo.UpdateAsync(line);
}
```

**4. Finalize Cycle Count** (creates adjustments for variances):
```csharp
public async Task FinalizeCycleCountAsync(int cycleCountId, string userId)
{
    var cycleCount = await _cycleCountRepo.GetByIdAsync(cycleCountId);
    if (cycleCount.Status != "InProgress")
        throw new ValidationException("Only in-progress counts can be finalized");

    var lines = await _cycleCountLineRepo.GetByCycleCountAsync(cycleCountId);

    int variancesFound = 0;

    foreach (var line in lines.Where(l => l.Variance != 0))
    {
        // Create stock adjustment for variance
        var adjustment = await _adjustmentService.CreateAdjustmentAsync(new CreateAdjustmentDto
        {
            ProductId = line.ProductId,
            SiteId = cycleCount.SiteId,
            AdjustmentType = line.Variance > 0 ? "AdjustmentIn" : "AdjustmentOut",
            Quantity = Math.Abs(line.Variance.Value),
            BatchNumber = line.BatchNumber,
            Reason = $"Cycle count variance: {cycleCount.CountNumber}",
            UserId = userId
        });

        // Auto-approve cycle count adjustments (or require manual approval)
        await _adjustmentService.ApproveAdjustmentAsync(adjustment.AdjustmentId, userId);

        // Link adjustment to count line
        line.AdjustmentId = adjustment.AdjustmentId;
        await _cycleCountLineRepo.UpdateAsync(line);

        variancesFound++;
    }

    // Update cycle count status
    cycleCount.Status = "Completed";
    cycleCount.VerifiedBy = userId;
    cycleCount.ItemsCounted = lines.Count();
    cycleCount.VariancesFound = variancesFound;

    await _cycleCountRepo.UpdateAsync(cycleCount);
}
```

---

## 11. REPORTING & QUERIES

### 11.1 Standard Reports

**1. Stock on Hand Report**:
```sql
-- All products, all sites, current SOH
SELECT
    p.ProductCode,
    p.ProductName,
    s.SiteName,
    SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) AS SOH,
    p.UnitOfMeasure,
    p.ReorderLevel,
    CASE
        WHEN SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) <= p.ReorderLevel
        THEN 'Low Stock'
        ELSE 'OK'
    END AS Status
FROM Products p
CROSS JOIN Sites s
LEFT JOIN Movements m ON m.ProductId = p.ProductId AND m.SiteId = s.SiteId
WHERE p.IsActive = 1 AND p.IsDeleted = 0
  AND s.IsActive = 1 AND s.IsDeleted = 0
  AND (m.IsDeleted = 0 OR m.IsDeleted IS NULL)
  AND (m.IsReversed = 0 OR m.IsReversed IS NULL)
GROUP BY p.ProductCode, p.ProductName, s.SiteName, p.UnitOfMeasure, p.ReorderLevel
HAVING SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) > 0
ORDER BY p.ProductCode, s.SiteName;
```

**2. Movement History Report**:
```sql
-- All movements for a product in date range
SELECT
    m.MovementDate,
    m.MovementType,
    m.MovementDirection,
    m.Quantity,
    m.UnitOfMeasure,
    m.BatchNumber,
    m.SerialNumber,
    s.SiteName,
    m.ReferenceNumber,
    m.Notes,
    m.CreatedBy
FROM Movements m
JOIN Sites s ON s.SiteId = m.SiteId
WHERE m.ProductId = @ProductId
  AND m.MovementDate BETWEEN @FromDate AND @ToDate
  AND m.IsDeleted = 0
ORDER BY m.MovementDate DESC, m.CreatedDate DESC;
```

**3. Batch Traceability Report**:
```sql
-- Full traceability for a batch number
SELECT
    m.MovementDate,
    m.MovementType,
    m.MovementDirection,
    m.Quantity,
    p.ProductCode,
    p.ProductName,
    s.SiteName,
    m.ReferenceNumber,
    m.CreatedBy
FROM Movements m
JOIN Products p ON p.ProductId = m.ProductId
JOIN Sites s ON s.SiteId = m.SiteId
WHERE m.BatchNumber = @BatchNumber
  AND m.IsDeleted = 0
ORDER BY m.MovementDate, m.CreatedDate;
```

**4. Low Stock Report**:
```sql
-- Products below reorder level
SELECT
    p.ProductCode,
    p.ProductName,
    s.SiteName,
    SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) AS SOH,
    p.ReorderLevel,
    p.ReorderQuantity,
    p.UnitOfMeasure
FROM Products p
CROSS JOIN Sites s
LEFT JOIN Movements m ON m.ProductId = p.ProductId AND m.SiteId = s.SiteId
WHERE p.IsActive = 1 AND p.IsDeleted = 0
  AND p.ReorderLevel IS NOT NULL
  AND s.IsActive = 1 AND s.IsDeleted = 0
  AND (m.IsDeleted = 0 OR m.IsDeleted IS NULL)
  AND (m.IsReversed = 0 OR m.IsReversed IS NULL)
GROUP BY p.ProductCode, p.ProductName, s.SiteName, p.ReorderLevel, p.ReorderQuantity, p.UnitOfMeasure
HAVING SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) <= p.ReorderLevel
ORDER BY p.ProductCode;
```

**5. Negative Stock Report** (should be empty!):
```sql
-- Products with negative SOH (data integrity issue)
SELECT
    p.ProductCode,
    p.ProductName,
    s.SiteName,
    SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) AS SOH,
    p.UnitOfMeasure
FROM Products p
CROSS JOIN Sites s
LEFT JOIN Movements m ON m.ProductId = p.ProductId AND m.SiteId = s.SiteId
WHERE (m.IsDeleted = 0 OR m.IsDeleted IS NULL)
  AND (m.IsReversed = 0 OR m.IsReversed IS NULL)
GROUP BY p.ProductCode, p.ProductName, s.SiteName, p.UnitOfMeasure
HAVING SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) < 0
ORDER BY SOH ASC;
```

### 11.2 Advanced Reports (Beyond PoC)

**Stock Aging Report**:
```sql
-- Age of batches (days since first movement)
SELECT
    p.ProductCode,
    p.ProductName,
    m.BatchNumber,
    MIN(m.MovementDate) AS FirstMovement,
    DATEDIFF(DAY, MIN(m.MovementDate), GETDATE()) AS AgeInDays,
    SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) AS SOH
FROM Movements m
JOIN Products p ON p.ProductId = m.ProductId
WHERE m.IsDeleted = 0 AND m.IsReversed = 0
GROUP BY p.ProductCode, p.ProductName, m.BatchNumber
HAVING SUM(CASE WHEN m.MovementDirection = 'IN' THEN m.Quantity ELSE -m.Quantity END) > 0
ORDER BY AgeInDays DESC;
```

**ABC Analysis** (fast/slow movers):
```sql
-- Classify products by movement frequency
WITH MovementCounts AS (
    SELECT
        ProductId,
        COUNT(*) AS MovementCount,
        SUM(Quantity) AS TotalQuantityMoved
    FROM Movements
    WHERE MovementDate >= DATEADD(MONTH, -3, GETDATE())
      AND IsDeleted = 0
    GROUP BY ProductId
)
SELECT
    p.ProductCode,
    p.ProductName,
    mc.MovementCount,
    mc.TotalQuantityMoved,
    CASE
        WHEN mc.MovementCount >= 100 THEN 'A (Fast Mover)'
        WHEN mc.MovementCount >= 20 THEN 'B (Medium Mover)'
        ELSE 'C (Slow Mover)'
    END AS ABCClass
FROM Products p
LEFT JOIN MovementCounts mc ON mc.ProductId = p.ProductId
WHERE p.IsActive = 1 AND p.IsDeleted = 0
ORDER BY mc.MovementCount DESC;
```

---

## 12. PERFORMANCE OPTIMIZATION

### 12.1 Database Indexes

**Critical Indexes** (already defined in entity schemas):
```sql
-- Movements table (most critical)
CREATE INDEX IX_Movements_SOH_Calculation
ON Movements(ProductId, SiteId, BatchNumber)
INCLUDE (Quantity, MovementDirection, IsDeleted, IsReversed);

CREATE INDEX IX_Movements_Date ON Movements(MovementDate);
CREATE INDEX IX_Movements_Transaction ON Movements(TransactionType, TransactionId);
CREATE INDEX IX_Movements_Batch ON Movements(BatchNumber) WHERE BatchNumber IS NOT NULL;
CREATE INDEX IX_Movements_Serial ON Movements(SerialNumber) WHERE SerialNumber IS NOT NULL;

-- Products table
CREATE INDEX IX_Products_ProductCode ON Products(ProductCode);
CREATE INDEX IX_Products_Active ON Products(IsActive, IsDeleted);
CREATE INDEX IX_Products_Cannabis ON Products(IsCannabisProduct) WHERE IsCannabisProduct = 1;

-- Sites table
CREATE INDEX IX_Sites_Active ON Sites(IsActive, IsDeleted);
```

### 12.2 Query Optimization Strategies

**1. Covering Indexes**:
Ensure SOH queries don't need to access base table:
```sql
-- Query uses INCLUDE columns (no table lookup needed)
CREATE INDEX IX_Movements_SOH_Covering
ON Movements(ProductId, SiteId, IsDeleted, IsReversed)
INCLUDE (Quantity, MovementDirection, BatchNumber);
```

**2. Filtered Indexes**:
Exclude soft-deleted rows from index:
```sql
CREATE INDEX IX_Movements_Active
ON Movements(ProductId, SiteId)
WHERE IsDeleted = 0 AND IsReversed = 0;
```

**3. Indexed Views (SQL Server)**:
Pre-aggregate SOH calculations:
```sql
CREATE VIEW vw_SOH WITH SCHEMABINDING AS
SELECT
    ProductId,
    SiteId,
    BatchNumber,
    SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS SOH,
    COUNT_BIG(*) AS MovementCount
FROM dbo.Movements
WHERE IsDeleted = 0 AND IsReversed = 0
GROUP BY ProductId, SiteId, BatchNumber;

CREATE UNIQUE CLUSTERED INDEX IX_vw_SOH ON vw_SOH(ProductId, SiteId, BatchNumber);
```

**4. Partitioning (Future - High Volume)**:
Partition `Movements` table by month:
```sql
-- Partition function (monthly)
CREATE PARTITION FUNCTION pf_Movements_ByMonth (DATETIME)
AS RANGE RIGHT FOR VALUES (
    '2024-01-01', '2024-02-01', '2024-03-01', '2024-04-01', '2024-05-01',
    '2024-06-01', '2024-07-01', '2024-08-01', '2024-09-01', '2024-10-01',
    '2024-11-01', '2024-12-01', '2025-01-01' -- extend monthly
);

-- Partition scheme
CREATE PARTITION SCHEME ps_Movements_ByMonth
AS PARTITION pf_Movements_ByMonth
ALL TO ([PRIMARY]);

-- Apply to table
CREATE TABLE Movements (
    MovementId INT IDENTITY(1,1),
    MovementDate DATETIME NOT NULL,
    -- other columns...
    PRIMARY KEY (MovementId, MovementDate)
) ON ps_Movements_ByMonth(MovementDate);
```

### 12.3 Application-Level Optimization

**1. Redis Caching**:
```csharp
public class CachedSOHService : ISOHService
{
    private readonly IMovementRepository _movementRepo;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public async Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null)
    {
        var cacheKey = $"SOH:{productId}:{siteId}:{batchNumber ?? "ALL"}";

        // Try cache
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return decimal.Parse(cached);

        // Calculate from DB
        var soh = await _movementRepo.GetSOHAsync(productId, siteId, batchNumber);

        // Cache result (5 minutes)
        await _cache.SetStringAsync(cacheKey, soh.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _cacheDuration
        });

        return soh;
    }

    // Invalidate cache after creating movement
    public async Task InvalidateCacheAsync(int productId, int siteId, string? batchNumber = null)
    {
        var cacheKey = $"SOH:{productId}:{siteId}:{batchNumber ?? "ALL"}";
        await _cache.RemoveAsync(cacheKey);
    }
}
```

**2. Bulk Movement Creation**:
```csharp
public async Task<IEnumerable<MovementDto>> CreateBulkMovementsAsync(IEnumerable<CreateMovementDto> dtos)
{
    var movements = new List<Movement>();

    foreach (var dto in dtos)
    {
        // Validate (but don't save yet)
        await ValidateMovementAsync(dto);

        var movement = new Movement
        {
            ProductId = dto.ProductId,
            SiteId = dto.SiteId,
            // ... map all properties
        };

        movements.Add(movement);
    }

    // Bulk insert (single transaction)
    await _movementRepo.AddRangeAsync(movements);
    await _movementRepo.SaveChangesAsync();

    return movements.Select(MapToDto);
}
```

**3. Asynchronous Queries**:
Ensure all repository queries use `async` variants:
```csharp
// Good
var soh = await _movementRepo.GetSOHAsync(productId, siteId);

// Bad (blocks thread)
var soh = _movementRepo.GetSOH(productId, siteId);
```

### 12.4 PostgreSQL-Specific Optimization

**Materialized Views**:
```sql
CREATE MATERIALIZED VIEW mv_stock_on_hand AS
SELECT
    ProductId,
    SiteId,
    BatchNumber,
    SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS SOH,
    MAX(MovementDate) AS LastMovement
FROM Movements
WHERE IsDeleted = FALSE AND IsReversed = FALSE
GROUP BY ProductId, SiteId, BatchNumber;

CREATE UNIQUE INDEX idx_mv_soh ON mv_stock_on_hand(ProductId, SiteId, BatchNumber);

-- Refresh after movement batch
REFRESH MATERIALIZED VIEW CONCURRENTLY mv_stock_on_hand;
```

**Vacuum & Analyze** (maintenance):
```sql
-- Regular vacuum (reclaim space)
VACUUM ANALYZE Movements;

-- Full vacuum (monthly)
VACUUM FULL Movements;

-- Update statistics (after bulk inserts)
ANALYZE Movements;
```

---

## 13. DATA ACCESS LAYER

### 13.1 IMovementRepository

```csharp
public interface IMovementRepository
{
    // CRUD
    Task<Movement?> GetByIdAsync(int movementId);
    Task<IEnumerable<Movement>> GetAllAsync();
    Task AddAsync(Movement movement);
    Task AddRangeAsync(IEnumerable<Movement> movements);
    Task UpdateAsync(Movement movement);
    Task DeleteAsync(int movementId); // Soft delete
    Task SaveChangesAsync();

    // SOH calculations
    Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null);
    Task<decimal> GetSOHAtDateAsync(int productId, int siteId, DateTime asOfDate, string? batchNumber = null);
    Task<IEnumerable<SOHDto>> GetAllSOHBySiteAsync(int siteId);
    Task<IEnumerable<SOHDto>> GetAllSOHByProductAsync(int productId);

    // Queries
    Task<IEnumerable<Movement>> GetMovementsByProductAsync(int productId, int? siteId = null, string? batchNumber = null);
    Task<IEnumerable<Movement>> GetMovementsByBatchAsync(string batchNumber);
    Task<IEnumerable<Movement>> GetMovementsBySerialAsync(string serialNumber);
    Task<IEnumerable<Movement>> GetMovementsByDateRangeAsync(DateTime fromDate, DateTime toDate, int? siteId = null);
    Task<IEnumerable<Movement>> GetMovementsByTransactionAsync(string transactionType, int transactionId);
}
```

### 13.2 MovementRepository Implementation (Dapper - Performance)

```csharp
public class MovementRepository : IMovementRepository
{
    private readonly IDbConnection _db;
    private readonly ILogger<MovementRepository> _logger;

    public MovementRepository(IConfiguration config, ILogger<MovementRepository> logger)
    {
        _db = new NpgsqlConnection(config.GetConnectionString("DefaultConnection"));
        _logger = logger;
    }

    public async Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null)
    {
        var sql = @"
            SELECT COALESCE(SUM(
                CASE
                    WHEN MovementDirection = 'IN' THEN Quantity
                    ELSE -Quantity
                END
            ), 0) AS SOH
            FROM Movements
            WHERE ProductId = @ProductId
              AND SiteId = @SiteId
              AND (@BatchNumber IS NULL OR BatchNumber = @BatchNumber)
              AND IsDeleted = FALSE
              AND IsReversed = FALSE";

        var soh = await _db.QuerySingleAsync<decimal>(sql, new { ProductId = productId, SiteId = siteId, BatchNumber = batchNumber });
        return soh;
    }

    public async Task<IEnumerable<Movement>> GetMovementsByProductAsync(int productId, int? siteId = null, string? batchNumber = null)
    {
        var sql = @"
            SELECT *
            FROM Movements
            WHERE ProductId = @ProductId
              AND (@SiteId IS NULL OR SiteId = @SiteId)
              AND (@BatchNumber IS NULL OR BatchNumber = @BatchNumber)
              AND IsDeleted = FALSE
            ORDER BY MovementDate DESC, CreatedDate DESC";

        var movements = await _db.QueryAsync<Movement>(sql, new { ProductId = productId, SiteId = siteId, BatchNumber = batchNumber });
        return movements;
    }

    public async Task AddAsync(Movement movement)
    {
        var sql = @"
            INSERT INTO Movements (
                ProductId, SiteId, ZoneId, BinId,
                MovementType, MovementDirection, Quantity, UnitOfMeasure,
                BatchNumber, SerialNumber,
                TransactionType, TransactionId, TransactionDetailId,
                ReferenceNumber, Notes,
                IsReversed, ReversedByMovementId, ReversalReason,
                MovementDate, CreatedDate, CreatedBy, IsDeleted
            ) VALUES (
                @ProductId, @SiteId, @ZoneId, @BinId,
                @MovementType, @MovementDirection, @Quantity, @UnitOfMeasure,
                @BatchNumber, @SerialNumber,
                @TransactionType, @TransactionId, @TransactionDetailId,
                @ReferenceNumber, @Notes,
                @IsReversed, @ReversedByMovementId, @ReversalReason,
                @MovementDate, @CreatedDate, @CreatedBy, @IsDeleted
            )
            RETURNING MovementId";

        var movementId = await _db.ExecuteScalarAsync<int>(sql, movement);
        movement.MovementId = movementId;
    }

    public async Task AddRangeAsync(IEnumerable<Movement> movements)
    {
        var sql = @"
            INSERT INTO Movements (
                ProductId, SiteId, MovementType, MovementDirection, Quantity, UnitOfMeasure,
                BatchNumber, TransactionType, TransactionId, ReferenceNumber,
                MovementDate, CreatedDate, CreatedBy, IsDeleted
            ) VALUES (
                @ProductId, @SiteId, @MovementType, @MovementDirection, @Quantity, @UnitOfMeasure,
                @BatchNumber, @TransactionType, @TransactionId, @ReferenceNumber,
                @MovementDate, @CreatedDate, @CreatedBy, @IsDeleted
            )";

        await _db.ExecuteAsync(sql, movements);
    }
}
```

---

## 14. BUSINESS LOGIC LAYER

### 14.1 IInventoryService

```csharp
public interface IInventoryService
{
    // Product operations
    Task<ProductDto> GetProductByIdAsync(int productId);
    Task<ProductDto> GetProductByCodeAsync(string productCode);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool includeInactive = false);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto> UpdateProductAsync(int productId, UpdateProductDto dto);
    Task DeleteProductAsync(int productId, string userId);

    // SOH queries
    Task<decimal> GetSOHAsync(int productId, int siteId, string? batchNumber = null);
    Task<IEnumerable<SOHDto>> GetSOHByProductAsync(int productId);
    Task<IEnumerable<SOHDto>> GetSOHBySiteAsync(int siteId);
    Task<IEnumerable<LowStockDto>> GetLowStockItemsAsync(int siteId);

    // Inventory status
    Task<bool> HasSufficientStockAsync(int productId, int siteId, decimal quantity, string? batchNumber = null);
    Task<IEnumerable<NegativeStockDto>> GetNegativeStockItemsAsync(int siteId);
}
```

### 14.2 InventoryService Implementation

```csharp
public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepo;
    private readonly IMovementRepository _movementRepo;
    private readonly ISOHService _sohService;
    private readonly ILogger<InventoryService> _logger;

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // Validate product code uniqueness
        var existing = await _productRepo.GetByCodeAsync(dto.ProductCode);
        if (existing != null)
            throw new ValidationException($"Product code '{dto.ProductCode}' already exists");

        // Create product
        var product = new Product
        {
            ProductCode = dto.ProductCode,
            ProductName = dto.ProductName,
            Description = dto.Description,
            ProductCategoryId = dto.ProductCategoryId,
            ProductType = dto.ProductType,
            IsCannabisProduct = dto.IsCannabisProduct,
            THCPercentage = dto.THCPercentage,
            CBDPercentage = dto.CBDPercentage,
            UnitOfMeasure = dto.UnitOfMeasure,
            IsSerialTracked = dto.IsSerialTracked,
            IsBatchTracked = dto.IsBatchTracked,
            StandardCostPrice = dto.StandardCostPrice,
            StandardSellPrice = dto.StandardSellPrice,
            ReorderLevel = dto.ReorderLevel,
            ReorderQuantity = dto.ReorderQuantity,
            IsActive = true,
            CreatedBy = dto.UserId
        };

        // Validate entity
        var validator = new ProductValidator();
        var validationResult = await validator.ValidateAsync(product);
        if (!validationResult.IsValid)
            throw new ValidationException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        // Save
        await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync();

        _logger.LogInformation("Product created: {ProductCode} ({ProductId})", product.ProductCode, product.ProductId);

        return MapToDto(product);
    }

    public async Task<IEnumerable<LowStockDto>> GetLowStockItemsAsync(int siteId)
    {
        var products = await _productRepo.GetProductsWithReorderLevelsAsync();
        var lowStockItems = new List<LowStockDto>();

        foreach (var product in products)
        {
            var soh = await _sohService.GetSOHAsync(product.ProductId, siteId);

            if (product.ReorderLevel.HasValue && soh <= product.ReorderLevel.Value)
            {
                lowStockItems.Add(new LowStockDto
                {
                    ProductId = product.ProductId,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    SiteId = siteId,
                    CurrentSOH = soh,
                    ReorderLevel = product.ReorderLevel.Value,
                    ReorderQuantity = product.ReorderQuantity,
                    UnitOfMeasure = product.UnitOfMeasure
                });
            }
        }

        return lowStockItems;
    }
}
```

---

## 15. API ENDPOINTS

### 15.1 Inventory Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ISOHService _sohService;

    // Products
    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts([FromQuery] bool includeInactive = false)
    {
        var products = await _inventoryService.GetAllProductsAsync(includeInactive);
        return Ok(products);
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _inventoryService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = await _inventoryService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
    }

    // SOH
    [HttpGet("soh/product/{productId}/site/{siteId}")]
    public async Task<IActionResult> GetSOH(int productId, int siteId, [FromQuery] string? batchNumber = null)
    {
        var soh = await _sohService.GetSOHAsync(productId, siteId, batchNumber);
        return Ok(new { ProductId = productId, SiteId = siteId, BatchNumber = batchNumber, SOH = soh });
    }

    [HttpGet("soh/site/{siteId}")]
    public async Task<IActionResult> GetSOHBySite(int siteId)
    {
        var sohList = await _sohService.GetSOHBySiteAsync(siteId);
        return Ok(sohList);
    }

    // Low stock
    [HttpGet("low-stock/site/{siteId}")]
    public async Task<IActionResult> GetLowStock(int siteId)
    {
        var lowStock = await _inventoryService.GetLowStockItemsAsync(siteId);
        return Ok(lowStock);
    }
}
```

---

## 16. VALIDATION RULES

### 16.1 Movement Validation

```csharp
public class CreateMovementValidator : AbstractValidator<CreateMovementDto>
{
    public CreateMovementValidator(IProductRepository productRepo)
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.SiteId)
            .GreaterThan(0).WithMessage("Site ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.MovementType)
            .IsInEnum().WithMessage("Invalid movement type");

        RuleFor(x => x.MovementDirection)
            .IsInEnum().WithMessage("Invalid movement direction");

        // Batch number required for batch-tracked products
        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                var product = await productRepo.GetByIdAsync(dto.ProductId);
                if (product == null) return false;

                if (product.IsBatchTracked && string.IsNullOrEmpty(dto.BatchNumber))
                    return false;

                return true;
            })
            .WithMessage("Batch number required for batch-tracked products");

        // Serial number required for serial-tracked products
        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                var product = await productRepo.GetByIdAsync(dto.ProductId);
                if (product == null) return false;

                if (product.IsSerialTracked && string.IsNullOrEmpty(dto.SerialNumber))
                    return false;

                return true;
            })
            .WithMessage("Serial number required for serial-tracked products");
    }
}
```

---

## 17. TESTING STRATEGY

### 17.1 Unit Tests

**MovementService Tests**:
```csharp
public class MovementServiceTests
{
    [Fact]
    public async Task CreateMovementAsync_ValidMovement_CreatesSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IMovementRepository>();
        var mockProductRepo = new Mock<IProductRepository>();
        var service = new MovementService(mockRepo.Object, mockProductRepo.Object, null);

        var product = new Product { ProductId = 1, IsBatchTracked = true, AllowNegativeStock = false };
        mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        mockRepo.Setup(r => r.GetSOHAsync(1, 1, "BATCH001")).ReturnsAsync(100m);

        var dto = new CreateMovementDto
        {
            ProductId = 1,
            SiteId = 1,
            MovementType = MovementType.Sale,
            MovementDirection = MovementDirection.OUT,
            Quantity = 10m,
            BatchNumber = "BATCH001",
            CreatedBy = "testuser"
        };

        // Act
        var result = await service.CreateMovementAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProductId);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Movement>()), Times.Once);
    }

    [Fact]
    public async Task CreateMovementAsync_InsufficientStock_ThrowsException()
    {
        // Arrange
        var mockRepo = new Mock<IMovementRepository>();
        var mockProductRepo = new Mock<IProductRepository>();
        var service = new MovementService(mockRepo.Object, mockProductRepo.Object, null);

        var product = new Product { ProductId = 1, AllowNegativeStock = false };
        mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        mockRepo.Setup(r => r.GetSOHAsync(1, 1, null)).ReturnsAsync(5m); // Only 5 in stock

        var dto = new CreateMovementDto
        {
            ProductId = 1,
            SiteId = 1,
            MovementDirection = MovementDirection.OUT,
            Quantity = 10m // Trying to take 10
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateMovementAsync(dto));
    }
}
```

**SOHService Tests**:
```csharp
public class SOHServiceTests
{
    [Fact]
    public async Task GetSOHAsync_WithMovements_ReturnsCorrectSOH()
    {
        // Arrange
        var movements = new List<Movement>
        {
            new Movement { ProductId = 1, SiteId = 1, MovementDirection = MovementDirection.IN, Quantity = 100 },
            new Movement { ProductId = 1, SiteId = 1, MovementDirection = MovementDirection.OUT, Quantity = 30 },
            new Movement { ProductId = 1, SiteId = 1, MovementDirection = MovementDirection.OUT, Quantity = 20 }
        };

        var mockRepo = new Mock<IMovementRepository>();
        mockRepo.Setup(r => r.GetMovementsByProductAsync(1, 1, null)).ReturnsAsync(movements);
        mockRepo.Setup(r => r.GetSOHAsync(1, 1, null)).ReturnsAsync(50m); // 100 - 30 - 20 = 50

        var service = new SOHService(mockRepo.Object, null, null);

        // Act
        var soh = await service.GetSOHAsync(1, 1);

        // Assert
        Assert.Equal(50m, soh);
    }
}
```

### 17.2 Integration Tests

**Movement to SOH Integration**:
```csharp
public class MovementIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    [Fact]
    public async Task CreateMovements_UpdatesSOHCorrectly()
    {
        // Arrange
        var movementService = _fixture.GetService<IMovementService>();
        var sohService = _fixture.GetService<ISOHService>();

        var productId = 1;
        var siteId = 1;

        // Act - Create IN movement
        await movementService.CreateMovementAsync(new CreateMovementDto
        {
            ProductId = productId,
            SiteId = siteId,
            MovementType = MovementType.GRV,
            MovementDirection = MovementDirection.IN,
            Quantity = 100,
            CreatedBy = "test"
        });

        var sohAfterIn = await sohService.GetSOHAsync(productId, siteId);

        // Act - Create OUT movement
        await movementService.CreateMovementAsync(new CreateMovementDto
        {
            ProductId = productId,
            SiteId = siteId,
            MovementType = MovementType.Sale,
            MovementDirection = MovementDirection.OUT,
            Quantity = 30,
            CreatedBy = "test"
        });

        var sohAfterOut = await sohService.GetSOHAsync(productId, siteId);

        // Assert
        Assert.Equal(100m, sohAfterIn);
        Assert.Equal(70m, sohAfterOut);
    }
}
```

---

## 18. COMPLIANCE & AUDIT

### 18.1 POPIA Compliance (South Africa)

**Data Retention**:
- All financial records: **7 years** (SARS requirement)
- Soft delete only (never hard delete movements)
- `IsDeleted` flag + `DeletedDate` + `DeletedBy`

**Audit Trail**:
- Every table has: `CreatedDate`, `CreatedBy`, `ModifiedDate`, `ModifiedBy`
- Movements are immutable (cannot edit, only reverse)
- Full traceability: Who did what, when, where

### 18.2 Cannabis Compliance (SAHPRA, DALRRD)

**Seed-to-Sale Traceability**:
- Every batch tracked from cultivation → production → sale
- Batch number on every movement
- Serial number for high-value items

**Regulatory Reporting**:
- Stock on hand report (by batch, by date)
- Movement history (full audit trail)
- Wastage report (spoilage, theft)
- Batch traceability report (customer → batch → plant)

**License Management**:
- Site licenses tracked in `Sites` table
- License expiry dates monitored
- Alerts for expiring licenses

---

## 19. IMPLEMENTATION ROADMAP

### 19.1 Phase 7: Movement Architecture (Week 1) ✅ IN PROGRESS

**Status**: Currently implementing (see [IMMEDIATE-ACTION-TODO.md](IMMEDIATE-ACTION-TODO.md))

**Tasks**:
- [ ] Create unified `TransactionDetails` table
- [ ] Implement `MovementService` with consistent movement generation
- [ ] Refactor existing transactions (Retail, Purchasing) to use new architecture
- [ ] Unit test movement generation (15+ tests)
- [ ] Integration test SOH calculations

**Deliverable**: Movement architecture fully implemented and tested.

### 19.2 Phase 8: Batch & Serial Number Tracking (Week 2)

**Tasks**:
- [ ] Implement `BatchNumberService` (generation, validation)
- [ ] Implement `SerialNumberService` (full 28-digit and short 13-digit)
- [ ] Create `SerialNumbers` table with full traceability
- [ ] Integrate batch/serial generation with Production module
- [ ] Unit test number generation (20+ tests)
- [ ] Test Luhn check digit algorithm

**Deliverable**: Batch and serial number system fully operational.

### 19.3 Phase 9: Stock Adjustments & Transfers (Week 3)

**Tasks**:
- [ ] Implement `StockAdjustmentService` with approval workflow
- [ ] Implement `StockTransferService` (inter-site, inter-zone)
- [ ] Create UI for stock adjustments (Blazor or API)
- [ ] Create UI for stock transfers
- [ ] Unit test adjustment workflow (15+ tests)
- [ ] Integration test transfers (verify movements created correctly)

**Deliverable**: Stock adjustments and transfers operational.

### 19.4 Phase 10: Cycle Counting (Week 4)

**Tasks**:
- [ ] Implement `CycleCountService`
- [ ] Create cycle count scheduling
- [ ] Implement variance detection and adjustment creation
- [ ] Create cycle count UI
- [ ] Unit test cycle count workflow (15+ tests)
- [ ] Test variance handling

**Deliverable**: Cycle counting system operational.

### 19.5 Phase 11: Performance Optimization (Week 5)

**Tasks**:
- [ ] Create indexed views for SOH (SQL Server) or materialized views (PostgreSQL)
- [ ] Implement Redis caching for SOH queries
- [ ] Optimize database indexes (covering indexes, filtered indexes)
- [ ] Load test with 10,000+ movements
- [ ] Benchmark SOH queries (target: <100ms)
- [ ] Tune cache invalidation strategy

**Deliverable**: SOH queries respond in <100ms (50 concurrent users).

### 19.6 Phase 12: Reporting & Final Testing (Week 6)

**Tasks**:
- [ ] Implement standard reports (SOH, Movement History, Low Stock)
- [ ] Implement batch traceability report (compliance)
- [ ] Create reporting UI (dashboard)
- [ ] Full integration testing (all modules)
- [ ] Compliance testing (SAHPRA, DALRRD requirements)
- [ ] User acceptance testing (UAT)

**Deliverable**: Inventory module at 90% completeness, production-ready.

---

## APPENDIX A: KEY QUERIES REFERENCE

### SOH Calculation (Basic)
```sql
SELECT SUM(CASE WHEN MovementDirection = 'IN' THEN Quantity ELSE -Quantity END) AS SOH
FROM Movements
WHERE ProductId = @ProductId AND SiteId = @SiteId AND IsDeleted = 0 AND IsReversed = 0;
```

### Batch Traceability
```sql
SELECT * FROM Movements WHERE BatchNumber = @BatchNumber AND IsDeleted = 0 ORDER BY MovementDate;
```

### Low Stock Items
```sql
SELECT p.ProductCode, p.ProductName, p.ReorderLevel, /* calculated SOH */
FROM Products p /* join movements, calculate SOH */
WHERE SOH <= p.ReorderLevel;
```

---

## APPENDIX B: ENTITY RELATIONSHIP DIAGRAM

```
Products ──────┬──── Movements ──────┬──── Sites
               │                     │
               │                     ├──── Zones
               │                     │
               │                     └──── Bins
               │
               ├──── StockAdjustments
               │
               ├──── StockTransfers
               │
               ├──── CycleCountLines
               │
               └──── SerialNumbers
```

---

## DOCUMENT REVISION HISTORY

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-11 | Claude Code | Initial comprehensive specification |

---

**END OF INVENTORY MODULE SPECIFICATION**
