# Header-Detail Pattern Implementation Plan
## Transaction & Batch Detail Line Items for Full Traceability

**Created**: 2025-12-08
**Status**: ACTIVE - Design Phase
**Purpose**: Implement Header-Detail pattern for Production, Transactions, and other key business processes

---

## 🎯 Overview

**Pattern**: Header-Detail (also known as Master-Detail or Parent-Child)

**Benefits**:
- ✅ Full traceability of all inputs/outputs
- ✅ Flexible transaction types (Invoice, Credit, Return, etc.)
- ✅ Recipe management (many ingredients → 1 product)
- ✅ Waste tracking (1 product → many outputs + waste)
- ✅ SAHPRA compliance (detailed audit trails)
- ✅ SARS compliance (detailed tax breakdowns)

---

## 📋 Current vs. Proposed Structure

### **1. Production Batches - CRITICAL CHANGE**

#### Current Structure (Phase 5):
```csharp
ProductionBatch (Header)
├── BatchNumber
├── StrainName
├── StartingWeightGrams
├── FinalWeightGrams
└── WasteWeightGrams
```

**Problem**: Cannot track individual inputs (harvest batches) or outputs (products + waste)

---

#### Proposed Structure (Header-Detail):

```csharp
// Header
ProductionBatch (Header)
├── Id
├── BatchNumber
├── ProductionType (enum)
│   ├── 1 = Recipe Production (many inputs → 1 output) [e.g., edibles, oils]
│   └── 2 = Processing (1 input → many outputs + waste) [e.g., trimming, curing]
├── Status (Open, InProgress, Completed, Closed)
├── StartDate
├── CompletionDate
└── Notes

// Detail Lines
ProductionBatchDetail (Detail)
├── Id
├── ProductionBatchId (FK)
├── LineType (enum)
│   ├── 1 = Input (raw material consumed)
│   └── 2 = Output (finished product created)
│   └── 3 = Waste (waste/byproduct)
├── ProductSKU
├── ProductName
├── StrainName
├── BatchNumber (source batch if input, new batch if output)
├── Quantity
├── WeightGrams
├── UnitCost
├── TotalCost
└── Notes
```

**Example Use Cases:**

**Type 1 - Recipe Production (many → 1):**
```
ProductionBatch #1 - Recipe Production
Header:
  - BatchNumber: PR-251208-A7K3
  - ProductionType: 1 (Recipe)
  - Status: Completed

Details:
  Line 1: Input  | Dried Flower (Durban Poison) | 100g | Batch: HB-251201-X9
  Line 2: Input  | Coconut Oil                   | 500ml | Batch: RM-251205-K4
  Line 3: Input  | Sunflower Lecithin           | 50g  | Batch: RM-251203-P2
  Line 4: Output | Cannabis Oil (500ml bottle)  | 1 unit | Batch: PR-251208-A7K3
  Line 5: Waste  | Plant Material Waste         | 5g   | Batch: WS-251208-Q1
```

**Type 2 - Processing (1 → many + waste):**
```
ProductionBatch #2 - Processing (Trimming)
Header:
  - BatchNumber: PR-251210-B2K9
  - ProductionType: 2 (Processing)
  - Status: Completed

Details:
  Line 1: Input  | Wet Flower (untrimmed)       | 1000g | Batch: HB-251205-M3
  Line 2: Output | Premium Flower (trimmed)     | 800g  | Batch: PR-251210-B2K9-A
  Line 3: Output | Shake (small buds/sugar leaf)| 150g  | Batch: PR-251210-B2K9-B
  Line 4: Waste  | Stems/Fan Leaves             | 50g   | Batch: WS-251210-T7
```

---

### **2. Transaction Headers/Details - ALREADY EXISTS (Enhance)**

#### Current Structure (Phase 4):
```csharp
TransactionHeader
├── TransactionNumber
├── TransactionType (enum: Sale, Return, etc.)
├── DebtorId
├── TotalAmount
├── VATAmount
└── ...

TransactionDetail
├── TransactionHeaderId (FK)
├── ProductSKU
├── Quantity
├── UnitPrice
└── LineTotal
```

**Status**: ✅ Already implemented correctly!

**Enhancement Needed**: Expand TransactionType enum

---

#### Proposed Enhancement:

```csharp
public enum TransactionType
{
    // Sales (Debit Customer)
    WholesaleInvoice = 1,      // Bulk sale to dispensary
    RetailInvoice = 2,         // Individual sale to customer

    // Credits (Credit Customer)
    WholesaleCredit = 3,       // Wholesale return/credit note
    RetailCredit = 4,          // Retail return/credit note

    // Internal
    StockAdjustment = 5,       // Inventory correction
    StockTransfer = 6,         // Between locations

    // Future
    ProFormaInvoice = 7,       // Quote/estimate
    DeliveryNote = 8           // Delivery without invoice
}
```

**Usage Example:**
```csharp
// Wholesale Invoice
TransactionHeader
  - TransactionNumber: WINV-251208-001
  - TransactionType: 1 (WholesaleInvoice)
  - DebtorId: 5 (Green Leaf Dispensary)
  - TotalAmount: R15,000.00

TransactionDetails:
  Line 1: Durban Poison (100g) | R100/g | R10,000
  Line 2: Pineapple Express (50g) | R100/g | R5,000

// Retail Credit Note (Return)
TransactionHeader
  - TransactionNumber: RCRD-251209-012
  - TransactionType: 4 (RetailCredit)
  - DebtorId: 23 (John Smith)
  - TotalAmount: -R500.00 (negative = credit)

TransactionDetails:
  Line 1: Cannabis Oil (defective) | -R500 | Qty: -1
```

---

### **3. Stock Movements - ADD DETAIL STRUCTURE**

#### Current Structure (Phase 5):
```csharp
StockMovement (Flat structure)
├── MovementNumber
├── MovementType
├── ProductSKU
├── Quantity
├── BatchNumber
└── ...
```

**Problem**: Only tracks one product per movement. Cannot track multi-product transfers.

---

#### Proposed Structure:

```csharp
// Header
StockMovementHeader
├── Id
├── MovementNumber
├── MovementType (Receipt, Issue, Transfer, Adjustment)
├── MovementDate
├── FromLocation
├── ToLocation
├── Status (Pending, Approved, Completed)
├── RequestedBy
├── ApprovedBy
└── Notes

// Detail
StockMovementDetail
├── Id
├── StockMovementHeaderId (FK)
├── LineNumber
├── ProductSKU
├── ProductName
├── BatchNumber
├── Quantity
├── WeightGrams
├── FromBin (optional: warehouse bin location)
├── ToBin
└── Notes
```

**Example:**
```
StockMovementHeader
  - MovementNumber: STM-251208-045
  - MovementType: Transfer
  - FromLocation: Grow Room A
  - ToLocation: Curing Room B
  - Status: Completed

StockMovementDetails:
  Line 1: Durban Poison (Batch: HB-251205-M3) | 500g
  Line 2: Pineapple Express (Batch: HB-251206-K1) | 300g
  Line 3: Wedding Cake (Batch: HB-251207-P9) | 450g
```

---

### **4. Stock Transfers - REPLACE WITH StockMovementHeader**

**Current**: Separate `StockTransfer` entity
**Proposed**: Use `StockMovementHeader` with `MovementType = Transfer`

**Reasoning**:
- ✅ Eliminates duplicate logic
- ✅ Consistent API (all stock movements through one system)
- ✅ Simpler reporting and queries

---

### **5. Harvest Batches - ADD DETAIL FOR PLANT TRACKING**

#### Current Structure:
```csharp
HarvestBatch
├── BatchNumber
├── GrowCycleId
├── PlantCount
└── TotalWetWeightGrams
```

**Problem**: Cannot track which specific plants contributed to the batch.

---

#### Proposed Structure:

```csharp
// Header
HarvestBatch (Header)
├── Id
├── BatchNumber
├── GrowCycleId
├── HarvestDate
├── TotalPlantCount
├── TotalWetWeightGrams
├── TotalDryWeightGrams
└── Status

// Detail
HarvestBatchDetail (Detail)
├── Id
├── HarvestBatchId (FK)
├── PlantId (FK to Plant table)
├── PlantTag
├── StrainName
├── WetWeightGrams
├── DryWeightGrams
└── Notes
```

**Example:**
```
HarvestBatch
  - BatchNumber: HB-251208-X9
  - GrowCycleId: 12
  - TotalPlantCount: 10
  - TotalWetWeightGrams: 5000g

HarvestBatchDetails:
  Line 1: Plant#001 (Durban Poison) | Wet: 520g | Dry: 104g
  Line 2: Plant#002 (Durban Poison) | Wet: 490g | Dry: 98g
  Line 3: Plant#003 (Durban Poison) | Wet: 510g | Dry: 102g
  ...
  Line 10: Plant#010 (Durban Poison) | Wet: 500g | Dry: 100g
```

**SAHPRA Compliance**: Individual plant tracking as required by Section 22C

---

## 🏗️ Implementation Plan

### **Phase 7.1: Production Batch Header-Detail**

#### Step 1: Create ProductionBatchDetail Entity

```csharp
// Production/Models/Entities/ProductionBatchDetail.cs
public class ProductionBatchDetail : AuditableEntity
{
    public int Id { get; set; }

    // Link to header
    public int ProductionBatchId { get; set; }
    public ProductionBatch ProductionBatch { get; set; }

    public int LineNumber { get; set; }

    // Line type
    public ProductionLineType LineType { get; set; }  // Input, Output, Waste

    // Product info
    [Required, MaxLength(50)]
    public string ProductSKU { get; set; }

    [Required, MaxLength(200)]
    public string ProductName { get; set; }

    [MaxLength(100)]
    public string? StrainName { get; set; }

    [MaxLength(100)]
    public string? BatchNumber { get; set; }  // Source batch for inputs, new batch for outputs

    // Quantities
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? WeightGrams { get; set; }

    // Costing
    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalCost { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
```

#### Step 2: Add ProductionType Enum

```csharp
// Production/Models/Enums/ProductionType.cs
public enum ProductionType
{
    RecipeProduction = 1,    // Many inputs → 1 output (e.g., edibles, oils, concentrates)
    Processing = 2           // 1 input → many outputs + waste (e.g., trimming, curing, bucking)
}

// Production/Models/Enums/ProductionLineType.cs
public enum ProductionLineType
{
    Input = 1,      // Raw material consumed
    Output = 2,     // Finished product created
    Waste = 3       // Waste/byproduct
}
```

#### Step 3: Update ProductionBatch Entity

```csharp
// Add to existing ProductionBatch
public class ProductionBatch : AuditableEntity
{
    // ... existing properties ...

    // NEW: Production type
    public ProductionType ProductionType { get; set; }

    // NEW: Navigation to details
    public ICollection<ProductionBatchDetail> Details { get; set; }
}
```

#### Step 4: Create Migration

```bash
dotnet ef migrations add AddProductionBatchDetails \
  --project src/Modules/Production/Project420.Production.DAL \
  --startup-project src/API/Project420.API.WebApi \
  --context ProductionDbContext
```

#### Step 5: Update ProductionBatchService

```csharp
public interface IProductionBatchService
{
    Task<ProductionBatchDto> CreateRecipeProductionAsync(CreateRecipeProductionDto dto);
    Task<ProductionBatchDto> CreateProcessingBatchAsync(CreateProcessingBatchDto dto);
    Task<ProductionBatchDto> GetWithDetailsAsync(int id);
}

// DTO Examples
public class CreateRecipeProductionDto
{
    public string Name { get; set; }
    public string RecipeCode { get; set; }
    public List<ProductionInputDto> Inputs { get; set; }  // Ingredients
    public ProductionOutputDto Output { get; set; }       // Finished product
}

public class ProductionInputDto
{
    public string ProductSKU { get; set; }
    public string BatchNumber { get; set; }
    public decimal Quantity { get; set; }
    public decimal WeightGrams { get; set; }
}
```

---

### **Phase 7.2: Stock Movement Header-Detail**

#### Step 1: Rename StockMovement → StockMovementHeader

```csharp
// Inventory/Models/Entities/StockMovementHeader.cs
public class StockMovementHeader : AuditableEntity
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string MovementNumber { get; set; }

    public StockMovementType MovementType { get; set; }

    public DateTime MovementDate { get; set; }

    [MaxLength(200)]
    public string? FromLocation { get; set; }

    [MaxLength(200)]
    public string? ToLocation { get; set; }

    public MovementStatus Status { get; set; }  // Pending, Approved, Completed, Cancelled

    [MaxLength(100)]
    public string? RequestedBy { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation
    public ICollection<StockMovementDetail> Details { get; set; }
}
```

#### Step 2: Create StockMovementDetail Entity

```csharp
// Inventory/Models/Entities/StockMovementDetail.cs
public class StockMovementDetail : AuditableEntity
{
    public int Id { get; set; }

    public int StockMovementHeaderId { get; set; }
    public StockMovementHeader StockMovementHeader { get; set; }

    public int LineNumber { get; set; }

    [Required, MaxLength(50)]
    public string ProductSKU { get; set; }

    [Required, MaxLength(200)]
    public string ProductName { get; set; }

    [MaxLength(100)]
    public string? BatchNumber { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? WeightGrams { get; set; }

    [MaxLength(100)]
    public string? FromBin { get; set; }  // Warehouse bin location

    [MaxLength(100)]
    public string? ToBin { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalValue { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
```

#### Step 3: Add MovementStatus Enum

```csharp
public enum MovementStatus
{
    Pending = 1,      // Awaiting approval
    Approved = 2,     // Approved, ready to execute
    InProgress = 3,   // Being processed
    Completed = 4,    // Finished
    Cancelled = 5     // Cancelled
}
```

---

### **Phase 7.3: Harvest Batch Detail**

```csharp
// Cultivation/Models/Entities/HarvestBatchDetail.cs
public class HarvestBatchDetail : AuditableEntity
{
    public int Id { get; set; }

    public int HarvestBatchId { get; set; }
    public HarvestBatch HarvestBatch { get; set; }

    public int LineNumber { get; set; }

    public int PlantId { get; set; }
    public Plant Plant { get; set; }

    [Required, MaxLength(100)]
    public string PlantTag { get; set; }

    [MaxLength(100)]
    public string? StrainName { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? WetWeightGrams { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DryWeightGrams { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
```

---

## 📊 Database Schema Changes Summary

### New Tables to Create:

1. **ProductionBatchDetails**
   - ProductionBatchId (FK)
   - LineType (Input, Output, Waste)
   - ProductSKU, Quantity, WeightGrams, UnitCost

2. **StockMovementDetails**
   - StockMovementHeaderId (FK)
   - ProductSKU, Quantity, BatchNumber, FromBin, ToBin

3. **HarvestBatchDetails**
   - HarvestBatchId (FK)
   - PlantId (FK)
   - WetWeightGrams, DryWeightGrams

### Tables to Rename:

1. **StockMovements** → **StockMovementHeaders**
   - Add Status column
   - Remove product-specific columns (moved to Details)

### Tables to Enhance:

1. **TransactionHeaders**
   - ✅ Already correct (no changes)
   - Enhance TransactionType enum

2. **ProductionBatches**
   - Add ProductionType column
   - Keep existing columns (aggregate totals)

---

## 🔄 Migration Strategy

### Option A: Big Bang (All at Once)
- Create all detail tables in one migration
- Requires data migration from existing flat structures
- Higher risk, faster completion

### Option B: Incremental (One Module at a Time)
- Migrate ProductionBatch first (most critical)
- Then StockMovement
- Then HarvestBatch
- Lower risk, slower completion

**Recommended**: **Option B** (Incremental)

---

## 📋 Implementation Checklist

### Phase 7.1: Production Batch Details
- [ ] Create ProductionType enum (Recipe, Processing)
- [ ] Create ProductionLineType enum (Input, Output, Waste)
- [ ] Create ProductionBatchDetail entity
- [ ] Update ProductionBatch to include ProductionType
- [ ] Add navigation property (Details collection)
- [ ] Create migration for ProductionBatchDetails table
- [ ] Apply migration
- [ ] Create ProductionBatchDetailRepository
- [ ] Update ProductionBatchService (CreateRecipe, CreateProcessing methods)
- [ ] Create DTOs (CreateRecipeProductionDto, CreateProcessingBatchDto)
- [ ] Create validators for detail lines
- [ ] Write unit tests (recipe with 5 inputs → 1 output)
- [ ] Write unit tests (processing 1 input → 3 outputs + waste)

### Phase 7.2: Stock Movement Details
- [ ] Create MovementStatus enum
- [ ] Rename StockMovement → StockMovementHeader
- [ ] Create StockMovementDetail entity
- [ ] Create migration (rename table, create details table)
- [ ] Migrate existing data (1 header row → 1 detail row)
- [ ] Apply migration
- [ ] Update StockMovementRepository
- [ ] Update StockMovementService
- [ ] Create DTOs for header-detail creation
- [ ] Update validators
- [ ] Write unit tests (multi-product transfer)

### Phase 7.3: Harvest Batch Details
- [ ] Create HarvestBatchDetail entity
- [ ] Update HarvestBatch (add Details navigation)
- [ ] Create migration for HarvestBatchDetails table
- [ ] Apply migration
- [ ] Update HarvestBatchRepository
- [ ] Update HarvestBatchService
- [ ] Create DTOs
- [ ] Update validators
- [ ] Write unit tests (harvest 10 plants → 1 batch)

### Phase 7.4: Transaction Type Enhancement
- [ ] Expand TransactionType enum (Wholesale, Retail, Credits)
- [ ] Update TransactionService to handle all types
- [ ] Create specific methods (CreateWholesaleInvoice, CreateRetailCredit, etc.)
- [ ] Update UI to show different forms based on type
- [ ] Write unit tests for each transaction type

---

## 🎯 Benefits of Header-Detail Pattern

### Traceability Benefits:
✅ **Full Input Tracking**: Know exactly which harvest batches went into each production run
✅ **Waste Accounting**: Track every gram of waste (SAHPRA requirement)
✅ **Recipe Management**: Store recipes as production templates
✅ **Multi-Product Transfers**: Move 20 products in one stock transfer
✅ **SAHPRA Compliance**: Individual plant tracking in harvest batches

### Financial Benefits:
✅ **Cost Tracking**: Calculate exact cost per batch (inputs + labor)
✅ **Yield Analysis**: Compare input weight vs output weight
✅ **Waste Costing**: Know the cost of waste for each batch
✅ **SARS Compliance**: Detailed tax records with line-by-line breakdown

### Operational Benefits:
✅ **Batch Splitting**: Create multiple output batches from one production run
✅ **Quality Control**: Track which inputs affected product quality
✅ **Recall Management**: Trace bad batch back to source plants
✅ **Inventory Accuracy**: Multi-product movements reduce transaction count

---

## 🚦 Success Criteria

**Before considering Phase 7 complete:**

1. ✅ ProductionBatch can create recipes (5 inputs → 1 output)
2. ✅ ProductionBatch can process batches (1 input → multiple outputs + waste)
3. ✅ StockMovement can transfer 20+ products in one movement
4. ✅ HarvestBatch tracks all individual plants (SAHPRA compliant)
5. ✅ TransactionHeaders support all 8 transaction types
6. ✅ Full traceability: SN → Batch → Production Detail → Harvest Detail → Plant
7. ✅ All unit tests passing (100+ new tests)
8. ✅ Data migration successful (no data loss)

---

## 📊 Estimated Effort

**Phase 7.1 - Production Batch Details**: 2-3 days
**Phase 7.2 - Stock Movement Details**: 1-2 days
**Phase 7.3 - Harvest Batch Details**: 1 day
**Phase 7.4 - Transaction Type Enhancement**: 0.5 days
**Testing & Documentation**: 1 day

**Total**: **5-7 days** for complete Header-Detail implementation

---

**STATUS**: ✅ **DESIGN COMPLETE - READY FOR APPROVAL**
**RECOMMENDED START**: After Batch/SN system (Phase 7.0) OR concurrently
**PRIORITY**: HIGH (Critical for SAHPRA compliance and traceability)

