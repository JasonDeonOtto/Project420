# 05 - PRODUCTION MODEL SPECIFICATION

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: PoC Specification - Retail Production Focus (60-75%)
**Related Documents**:
- [00-MAIN-SPECIFICATION.md](00-MAIN-SPECIFICATION.md) - Central specification index
- [01-MODULE-DESCRIPTIONS.md](01-MODULE-DESCRIPTIONS.md) - All module overviews
- [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md) - Movement system backbone
- [06-SERIAL-BATCH-GENERATION.md](06-SERIAL-BATCH-GENERATION.md) - Batch/SN generation
- [PRODUCTION-DAL-VALIDATION.md](PRODUCTION-DAL-VALIDATION.md) - DAL validation & implementation
- [10-GLOSSARY.md](10-GLOSSARY.md) - Terminology reference

---

## DOCUMENT PURPOSE

This document provides the **enterprise-grade specification** for the **Production Module**, focusing on **three production types**:

1. **Bucking Production** (35% stub) - Basic flower processing
2. **General Manufacturing** (45% stub) - Concentrates, edibles, oils
3. **Retail Production** (60-75% FOCUS) - Pre-roll assembly for retail sale

**PoC Completeness Targets**:
- **Bucking**: 35% (stub implementation, basic entities only)
- **General Manufacturing**: 45% (stub implementation, basic workflow)
- **Retail Production**: 60-75% (**PRIORITY** - full workflow, serial number generation)

**Critical Success Factors (Retail Production)**:
1. Single-batch multi-step manufacturing model (no batch explosion)
2. Automated serial number generation for pre-rolls
3. Accurate movement generation (raw materials OUT, finished goods IN)
4. Full batch traceability (flower batch → pre-roll batch → serial number)
5. Integration with Retail POS (serial numbers captured at sale)

---

## TABLE OF CONTENTS

1. [Executive Summary](#1-executive-summary)
2. [Module Architecture](#2-module-architecture)
3. [Production Types Overview](#3-production-types-overview)
4. [Single-Batch Multi-Step Model](#4-single-batch-multi-step-model)
5. [Core Entities](#5-core-entities)
6. [Bucking Production (35% Stub)](#6-bucking-production-35-stub)
7. [General Manufacturing (45% Stub)](#7-general-manufacturing-45-stub)
8. [Retail Production (60-75% FOCUS)](#8-retail-production-60-75-focus)
9. [Batch Number Generation](#9-batch-number-generation)
10. [Serial Number Generation](#10-serial-number-generation)
11. [Movement Integration](#11-movement-integration)
12. [Quality Control](#12-quality-control)
13. [Waste Tracking](#13-waste-tracking)
14. [Data Access Layer](#14-data-access-layer)
15. [Business Logic Layer](#15-business-logic-layer)
16. [API Endpoints](#16-api-endpoints)
17. [Validation Rules](#17-validation-rules)
18. [Testing Strategy](#18-testing-strategy)
19. [Implementation Roadmap](#19-implementation-roadmap)

---

## 1. EXECUTIVE SUMMARY

### 1.1 Module Overview

The **Production Module** transforms raw materials into finished goods through three distinct production types:

**1. Bucking Production (35% Stub)**
- **Purpose**: Process raw cannabis flower from cultivation
- **Input**: Whole plants from cultivation
- **Output**: Bucked flower (ready for drying/curing)
- **PoC Status**: Stub implementation (basic entities, no full workflow)

**2. General Manufacturing (45% Stub)**
- **Purpose**: Create concentrates, oils, edibles
- **Input**: Flower, trim, raw materials
- **Output**: Concentrates, oils, capsules, edibles
- **PoC Status**: Stub implementation (basic workflow, minimal features)

**3. Retail Production (60-75% FOCUS)**
- **Purpose**: Assemble pre-rolls for retail sale
- **Input**: Bucked flower, pre-roll cones, filters
- **Output**: Finished pre-rolls with serial numbers
- **PoC Status**: Full implementation (primary focus)

**Key Design Principle**: **Single-Batch Multi-Step Manufacturing**

```
TRADITIONAL (BATCH EXPLOSION):
Batch-001 → Step 1 → Batch-002 → Step 2 → Batch-003 → Step 3 → Batch-004
❌ Creates new batch at each step (traceability nightmare)

PROJECT420 (SINGLE-BATCH MULTI-STEP):
Batch-001 → Step 1 → Step 2 → Step 3 → Step 4 → Batch-001 (same batch!)
✅ ONE batch flows through ALL steps (simple traceability)
```

### 1.2 Why Retail Production is the Focus

**Market Entry Priority**:
- Pre-rolls are **high-margin, high-volume** products in cannabis retail
- Customers prefer pre-packaged convenience over loose flower
- Enables retail sales immediately (no need for full manufacturing capabilities)

**Technical Complexity**:
- Requires serial number generation (individual product tracking)
- Integrates with Retail POS (serial numbers scanned at sale)
- Full batch traceability (flower batch → pre-roll batch → serial → customer)

**Compliance Requirements**:
- SAHPRA requires serial number tracking for retail products
- Seed-to-sale traceability from cultivation → production → sale
- Batch recall capability (if quality issues arise)

### 1.3 Current Implementation Status

**Implemented (✅)**:
- Basic production batch entity
- Production step tracking
- Raw material consumption tracking
- Basic movement generation

**Partially Implemented (🟡)**:
- Single-batch multi-step model (needs refinement)
- Batch number generation (exists but needs enhancement)
- Serial number generation (basic, needs retail production integration)
- Movement generation (needs consistency with Option A)

**Not Yet Implemented (❌)**:
- Retail production workflow (pre-roll assembly)
- Serial number auto-generation during production
- Quality control checkpoints
- Waste tracking (trim, rejects, spillage)
- Production scheduling
- Work order management
- BOM (Bill of Materials) management

---

## 2. MODULE ARCHITECTURE

### 2.1 Architectural Principles

The Production Module follows **Project420's 3-tier architecture** with **Option A Movement Architecture**:

```
┌─────────────────────────────────────────────────────────────┐
│                        UI LAYER                              │
│       (Blazor Server - Production Management Interface)     │
│                                                              │
│  ┌────────────────┬──────────────────┬───────────────────┐ │
│  │ Batch View     │ Step Tracking    │ Quality Control   │ │
│  │ - Batch details│ - Step progress  │ - QC checkpoints  │ │
│  │ - Input/Output │ - Material usage │ - Reject tracking │ │
│  │ - Traceability │ - Labor tracking │ - Waste recording │ │
│  └────────────────┴──────────────────┴───────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LOGIC LAYER                       │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │ProductionService │BatchNumberService│SerialNumService │ │
│  │                  │                  │                 │ │
│  │- CreateBatch()   │- GenerateBatch() │- GenerateSerial│ │
│  │- AddStep()       │- ValidateBatch() │- ValidateSerial│ │
│  │- CompleteStep()  │- ParseBatch()    │- BulkGenerate()│ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │MovementService   │QualityService    │WasteService     │ │
│  │                  │                  │                 │ │
│  │- CreateMovement()│- RecordQC()      │- RecordWaste()  │ │
│  │- ReverseMovement()│- ApproveReject()│- CalculateYield│ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                         │
│                                                              │
│  ┌──────────────────┬──────────────────┬─────────────────┐ │
│  │ProductionBatchRepo│ProductionStepRepo│SerialNumberRepo│ │
│  │                  │                  │                 │ │
│  │IProductionBatchRepo│IProductionStepRepo│ISerialNumRepo│ │
│  └──────────────────┴──────────────────┴─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       MODEL LAYER                            │
│                                                              │
│  ProductionBatch, ProductionStep, ProductionInput,          │
│  ProductionOutput, SerialNumber, QualityCheck, WasteRecord  │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       DATABASE                               │
│                    PostgreSQL 17                             │
│              Database: project420_production                 │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Key Design Decisions

**Decision 1: Single-Batch Multi-Step Model**
- **Rationale**: Simplifies traceability, reduces complexity
- **Impact**: ONE batch number flows through all steps (no batch explosion)
- **Implementation**: `ProductionBatch` has many `ProductionSteps`, same batch number throughout

**Decision 2: Serial Number Generation During Production**
- **Rationale**: Pre-rolls need individual tracking for retail compliance
- **Impact**: Production service auto-generates serial numbers for each unit
- **Implementation**: `SerialNumberService` called during final production step

**Decision 3: Movement Generation on Step Completion**
- **Rationale**: Movements must be consistent across all modules (Option A)
- **Impact**: Each production step creates IN/OUT movements when completed
- **Implementation**: `MovementService` called after step validation

**Decision 4: Retail Production as Priority**
- **Rationale**: Market entry focus, high-margin product
- **Impact**: Bucking and General Manufacturing are stubs (basic entities only)
- **Implementation**: 60-75% effort on retail production, 25-40% on stubs

---

## 3. PRODUCTION TYPES OVERVIEW

### 3.1 Comparison Matrix

| Feature | Bucking (35%) | General Mfg (45%) | Retail Prod (75%) |
|---------|---------------|-------------------|-------------------|
| **PoC Completeness** | 35% (stub) | 45% (stub) | 60-75% (full) |
| **Priority** | Low | Medium | **HIGH** |
| **Entities** | Basic only | Basic only | Full implementation |
| **Workflows** | Stub | Stub | Complete |
| **Batch Tracking** | Basic | Basic | Full traceability |
| **Serial Tracking** | No | No | **YES** |
| **Movement Integration** | Basic | Basic | Full |
| **Quality Control** | No | No | YES |
| **Waste Tracking** | No | Basic | YES |
| **BOM Management** | No | No | Simple BOM |
| **Work Orders** | No | No | Manual entry |

### 3.2 Production Type Selection Logic

```csharp
public enum ProductionType
{
    Bucking,              // 35% - Stub
    GeneralManufacturing, // 45% - Stub
    RetailProduction      // 75% - FOCUS
}

public ProductionType DetermineProductionType(Product outputProduct)
{
    // Retail production: Pre-rolls, vapes, packaged flower
    if (outputProduct.ProductType == ProductType.PreRoll ||
        outputProduct.ProductType == ProductType.PreRollCannabis)
        return ProductionType.RetailProduction;

    // Bucking: Raw flower processing
    if (outputProduct.ProductCategory.CategoryName.Contains("Bucked Flower"))
        return ProductionType.Bucking;

    // General manufacturing: Everything else
    return ProductionType.GeneralManufacturing;
}
```

---

## 4. SINGLE-BATCH MULTI-STEP MODEL

### 4.1 Concept Overview

**Traditional Batch Explosion (NOT USED)**:
```
Raw Material Batch: BATCH-001 (1kg flower)
    ↓
Step 1: Bucking → Creates BATCH-002 (900g bucked flower)
    ↓
Step 2: Grinding → Creates BATCH-003 (850g ground flower)
    ↓
Step 3: Rolling → Creates BATCH-004 (800g pre-rolls, 100 units)
    ↓
Result: 4 different batch numbers for ONE production run
Problem: Traceability requires joining 4 batches
```

**Project420 Single-Batch Multi-Step (USED)**:
```
Production Batch: BATCH-001 (1kg flower)
    ↓
Step 1: Bucking (BATCH-001, 900g output)
    ↓
Step 2: Grinding (BATCH-001, 850g output)
    ↓
Step 3: Rolling (BATCH-001, 100 pre-rolls)
    ↓
Step 4: Packaging (BATCH-001, 100 packaged pre-rolls)
    ↓
Result: ONE batch number for entire production run
Benefit: Simple traceability (BATCH-001 from start to finish)
```

### 4.2 Step Tracking Model

**ProductionBatch** (header):
- BatchNumber: `0101202412110001`
- Status: `Pending`, `InProgress`, `Completed`, `Cancelled`
- CurrentStep: `1`, `2`, `3`, `4` (tracks progress)

**ProductionStep** (details):
- ProductionBatchId: Links to header
- StepNumber: `1`, `2`, `3`, `4`
- StepName: `"Bucking"`, `"Grinding"`, `"Rolling"`, `"Packaging"`
- Status: `NotStarted`, `InProgress`, `Completed`, `Failed`
- InputQuantity: `1000g` (from previous step or initial input)
- OutputQuantity: `900g` (actual output after step)
- WastageQuantity: `100g` (spillage, rejects, trim)

**Step Progression**:
```csharp
public async Task CompleteStepAsync(int stepId, decimal outputQuantity, string userId)
{
    var step = await _stepRepo.GetByIdAsync(stepId);
    var batch = await _batchRepo.GetByIdAsync(step.ProductionBatchId);

    // 1. Update step status
    step.Status = StepStatus.Completed;
    step.OutputQuantity = outputQuantity;
    step.WastageQuantity = step.InputQuantity - outputQuantity;
    step.CompletedDate = DateTime.UtcNow;
    step.CompletedBy = userId;

    // 2. Create movements (inputs OUT, outputs IN)
    await CreateStepMovementsAsync(step);

    // 3. Update batch current step
    batch.CurrentStep = step.StepNumber + 1;

    // 4. If final step, complete batch
    if (step.StepNumber == batch.TotalSteps)
    {
        batch.Status = BatchStatus.Completed;
        batch.CompletedDate = DateTime.UtcNow;
    }

    await _stepRepo.UpdateAsync(step);
    await _batchRepo.UpdateAsync(batch);
}
```

### 4.3 Traceability Flow

```
Flower Batch (Cultivation): CULT-BATCH-001
    ↓ (Used as input)
Production Batch (Pre-Rolls): PROD-BATCH-002
    ↓ Step 1: Bucking (Input: CULT-BATCH-001, Output: PROD-BATCH-002)
    ↓ Step 2: Grinding (Input: PROD-BATCH-002, Output: PROD-BATCH-002)
    ↓ Step 3: Rolling (Input: PROD-BATCH-002, Output: PROD-BATCH-002)
    ↓ Step 4: Packaging (Input: PROD-BATCH-002, Output: PROD-BATCH-002)
    ↓
Serial Numbers Generated: SN-001, SN-002, ..., SN-100
    ↓ (Linked to PROD-BATCH-002)
Retail Sale: Customer buys SN-042
    ↓
Traceability Query: SN-042 → PROD-BATCH-002 → CULT-BATCH-001 → Plant-ID-XYZ
```

---

## 5. CORE ENTITIES

### 5.1 ProductionBatch

**Purpose**: Header for every production run (one batch, multiple steps).

**Schema**:
```sql
CREATE TABLE ProductionBatches (
    ProductionBatchId INT IDENTITY(1,1) PRIMARY KEY,
    BatchNumber NVARCHAR(100) NOT NULL UNIQUE, -- e.g., "0101202412110001"

    -- Production details
    ProductionType NVARCHAR(50) NOT NULL, -- 'Bucking', 'GeneralManufacturing', 'RetailProduction'
    ProductionDate DATE NOT NULL,
    SiteId INT NOT NULL,

    -- Output product
    OutputProductId INT NOT NULL,
    PlannedQuantity DECIMAL(18,4) NOT NULL,
    ActualQuantity DECIMAL(18,4) NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Step tracking
    TotalSteps INT NOT NULL DEFAULT 1,
    CurrentStep INT NOT NULL DEFAULT 0,

    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'InProgress', 'Completed', 'Cancelled'

    -- Dates
    StartedDate DATETIME NULL,
    CompletedDate DATETIME NULL,

    -- Personnel
    ProductionManager NVARCHAR(100) NULL,

    -- Notes
    Notes NVARCHAR(1000) NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL,

    CONSTRAINT FK_ProdBatch_Site FOREIGN KEY (SiteId) REFERENCES Sites(SiteId),
    CONSTRAINT FK_ProdBatch_OutputProduct FOREIGN KEY (OutputProductId) REFERENCES Products(ProductId)
);

CREATE INDEX IX_ProdBatch_Number ON ProductionBatches(BatchNumber);
CREATE INDEX IX_ProdBatch_Status ON ProductionBatches(Status);
CREATE INDEX IX_ProdBatch_Date ON ProductionBatches(ProductionDate);
```

**C# Entity**:
```csharp
public class ProductionBatch : AuditableEntity
{
    public int ProductionBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    // Production details
    public ProductionType ProductionType { get; set; }
    public DateTime ProductionDate { get; set; }
    public int SiteId { get; set; }
    public Site Site { get; set; } = null!;

    // Output product
    public int OutputProductId { get; set; }
    public Product OutputProduct { get; set; } = null!;
    public decimal PlannedQuantity { get; set; }
    public decimal? ActualQuantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Step tracking
    public int TotalSteps { get; set; } = 1;
    public int CurrentStep { get; set; } = 0;

    // Status
    public BatchStatus Status { get; set; } = BatchStatus.Pending;

    // Dates
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Personnel
    public string? ProductionManager { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<ProductionStep> ProductionSteps { get; set; } = new List<ProductionStep>();
    public ICollection<ProductionInput> ProductionInputs { get; set; } = new List<ProductionInput>();
    public ICollection<ProductionOutput> ProductionOutputs { get; set; } = new List<ProductionOutput>();
}

public enum BatchStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}
```

### 5.2 ProductionStep

**Purpose**: Tracks each step in a multi-step production process.

**Schema**:
```sql
CREATE TABLE ProductionSteps (
    ProductionStepId INT IDENTITY(1,1) PRIMARY KEY,
    ProductionBatchId INT NOT NULL,

    -- Step details
    StepNumber INT NOT NULL,
    StepName NVARCHAR(200) NOT NULL, -- 'Bucking', 'Grinding', 'Rolling', 'Packaging'
    StepDescription NVARCHAR(500) NULL,

    -- Quantities
    InputQuantity DECIMAL(18,4) NULL,
    OutputQuantity DECIMAL(18,4) NULL,
    WastageQuantity DECIMAL(18,4) NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'NotStarted', -- 'NotStarted', 'InProgress', 'Completed', 'Failed'

    -- Dates
    StartedDate DATETIME NULL,
    CompletedDate DATETIME NULL,

    -- Personnel
    Operator NVARCHAR(100) NULL,
    CompletedBy NVARCHAR(100) NULL,

    -- Notes
    Notes NVARCHAR(1000) NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_ProdStep_Batch FOREIGN KEY (ProductionBatchId)
        REFERENCES ProductionBatches(ProductionBatchId)
);

CREATE INDEX IX_ProdStep_Batch ON ProductionSteps(ProductionBatchId);
CREATE INDEX IX_ProdStep_Number ON ProductionSteps(ProductionBatchId, StepNumber);
CREATE INDEX IX_ProdStep_Status ON ProductionSteps(Status);
```

**C# Entity**:
```csharp
public class ProductionStep : AuditableEntity
{
    public int ProductionStepId { get; set; }
    public int ProductionBatchId { get; set; }
    public ProductionBatch ProductionBatch { get; set; } = null!;

    // Step details
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string? StepDescription { get; set; }

    // Quantities
    public decimal? InputQuantity { get; set; }
    public decimal? OutputQuantity { get; set; }
    public decimal? WastageQuantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;

    // Status
    public StepStatus Status { get; set; } = StepStatus.NotStarted;

    // Dates
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Personnel
    public string? Operator { get; set; }
    public string? CompletedBy { get; set; }

    // Notes
    public string? Notes { get; set; }
}

public enum StepStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}
```

### 5.3 ProductionInput

**Purpose**: Tracks raw materials consumed in production.

**Schema**:
```sql
CREATE TABLE ProductionInputs (
    ProductionInputId INT IDENTITY(1,1) PRIMARY KEY,
    ProductionBatchId INT NOT NULL,
    ProductionStepId INT NULL, -- Optional: link to specific step

    -- Input product
    ProductId INT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Traceability
    BatchNumber NVARCHAR(100) NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_ProdInput_Batch FOREIGN KEY (ProductionBatchId)
        REFERENCES ProductionBatches(ProductionBatchId),
    CONSTRAINT FK_ProdInput_Step FOREIGN KEY (ProductionStepId)
        REFERENCES ProductionSteps(ProductionStepId),
    CONSTRAINT FK_ProdInput_Product FOREIGN KEY (ProductId)
        REFERENCES Products(ProductId)
);

CREATE INDEX IX_ProdInput_Batch ON ProductionInputs(ProductionBatchId);
CREATE INDEX IX_ProdInput_Product ON ProductionInputs(ProductId);
```

### 5.4 ProductionOutput

**Purpose**: Tracks finished goods produced (with serial numbers if applicable).

**Schema**:
```sql
CREATE TABLE ProductionOutputs (
    ProductionOutputId INT IDENTITY(1,1) PRIMARY KEY,
    ProductionBatchId INT NOT NULL,
    ProductionStepId INT NULL, -- Optional: link to specific step

    -- Output product
    ProductId INT NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitOfMeasure NVARCHAR(20) NOT NULL,

    -- Traceability
    BatchNumber NVARCHAR(100) NOT NULL, -- Same as ProductionBatch.BatchNumber
    SerialNumber NVARCHAR(50) NULL, -- If serial-tracked product

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_ProdOutput_Batch FOREIGN KEY (ProductionBatchId)
        REFERENCES ProductionBatches(ProductionBatchId),
    CONSTRAINT FK_ProdOutput_Step FOREIGN KEY (ProductionStepId)
        REFERENCES ProductionSteps(ProductionStepId),
    CONSTRAINT FK_ProdOutput_Product FOREIGN KEY (ProductId)
        REFERENCES Products(ProductId)
);

CREATE INDEX IX_ProdOutput_Batch ON ProductionOutputs(ProductionBatchId);
CREATE INDEX IX_ProdOutput_Product ON ProductionOutputs(ProductId);
CREATE INDEX IX_ProdOutput_Serial ON ProductionOutputs(SerialNumber) WHERE SerialNumber IS NOT NULL;
```

### 5.5 SerialNumber

**Purpose**: Individual tracking for retail products (pre-rolls, vapes).

**Schema**:
```sql
CREATE TABLE SerialNumbers (
    SerialNumberId INT IDENTITY(1,1) PRIMARY KEY,
    SerialNumber NVARCHAR(50) NOT NULL UNIQUE, -- e.g., "0124121100015" (13-digit short)

    -- Product linkage
    ProductId INT NOT NULL,
    BatchNumber NVARCHAR(100) NOT NULL,

    -- Production linkage
    ProductionBatchId INT NULL,
    ProductionOutputId INT NULL,

    -- Status
    Status NVARCHAR(50) NOT NULL DEFAULT 'Produced', -- 'Produced', 'InStock', 'Sold', 'Returned', 'Destroyed'

    -- Sale linkage (when sold)
    SoldDate DATETIME NULL,
    SoldTransactionId INT NULL,

    -- Audit
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,

    CONSTRAINT FK_Serial_Product FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT FK_Serial_ProdBatch FOREIGN KEY (ProductionBatchId)
        REFERENCES ProductionBatches(ProductionBatchId),
    CONSTRAINT FK_Serial_ProdOutput FOREIGN KEY (ProductionOutputId)
        REFERENCES ProductionOutputs(ProductionOutputId)
);

CREATE UNIQUE INDEX IX_Serial_Number ON SerialNumbers(SerialNumber);
CREATE INDEX IX_Serial_Product ON SerialNumbers(ProductId);
CREATE INDEX IX_Serial_Batch ON SerialNumbers(BatchNumber);
CREATE INDEX IX_Serial_Status ON SerialNumbers(Status);
```

---

## 6. BUCKING PRODUCTION (35% STUB)

### 6.1 Stub Implementation Scope

**Entities**: ✅ Basic entities only (ProductionBatch, ProductionStep)
**Workflows**: ❌ Not implemented (manual stub)
**Movements**: ❌ Not implemented
**Serial Numbers**: ❌ Not applicable
**Quality Control**: ❌ Not implemented

**Stub Purpose**: Placeholder for future implementation. Basic database schema exists, but no business logic.

### 6.2 Bucking Workflow (Future Implementation)

```
BUCKING PRODUCTION (NOT YET IMPLEMENTED)

Input: Whole cannabis plants from cultivation
    ↓
Step 1: De-stemming (remove stems, keep flower)
    ↓
Step 2: Trimming (remove excess leaves)
    ↓
Step 3: Sorting (separate by quality grade)
    ↓
Output: Bucked flower (ready for drying/curing)

Batch Traceability: Cultivation Batch → Bucking Batch (same number)
```

### 6.3 Stub Implementation

**Entities** (defined but not fully implemented):
```csharp
// Stub: Basic entity exists
var buckingBatch = new ProductionBatch
{
    BatchNumber = "0101202412110001",
    ProductionType = ProductionType.Bucking,
    OutputProductId = buckedFlowerProductId,
    PlannedQuantity = 1000m, // 1kg
    TotalSteps = 3,
    Status = BatchStatus.Pending
};

// No workflow implementation yet
```

---

## 7. GENERAL MANUFACTURING (45% STUB)

### 7.1 Stub Implementation Scope

**Entities**: ✅ Basic entities (ProductionBatch, ProductionStep, Inputs/Outputs)
**Workflows**: 🟡 Minimal implementation (simple create/complete)
**Movements**: 🟡 Basic implementation
**Serial Numbers**: ❌ Not applicable
**Quality Control**: ❌ Not implemented

**Stub Purpose**: Minimal viable implementation for concentrates, oils, edibles.

### 7.2 General Manufacturing Workflow (Minimal)

```
GENERAL MANUFACTURING (MINIMAL IMPLEMENTATION)

Input: Flower, trim, raw materials
    ↓
Step 1: Extraction / Processing
    ↓
Step 2: Refinement / Mixing
    ↓
Step 3: Packaging
    ↓
Output: Concentrates, oils, capsules, edibles

Batch Traceability: Input Batches → Manufacturing Batch
```

### 7.3 Stub Implementation

**Minimal Service**:
```csharp
public async Task<ProductionBatch> CreateManufacturingBatchAsync(CreateManufacturingBatchDto dto)
{
    // 1. Generate batch number
    var batchNumber = await _batchNumberService.GenerateBatchNumberAsync(
        dto.SiteId,
        BatchType.GeneralManufacturing
    );

    // 2. Create production batch
    var batch = new ProductionBatch
    {
        BatchNumber = batchNumber,
        ProductionType = ProductionType.GeneralManufacturing,
        ProductionDate = dto.ProductionDate,
        SiteId = dto.SiteId,
        OutputProductId = dto.OutputProductId,
        PlannedQuantity = dto.PlannedQuantity,
        TotalSteps = dto.Steps.Count,
        Status = BatchStatus.Pending,
        CreatedBy = dto.UserId
    };

    await _batchRepo.AddAsync(batch);

    // 3. Create steps (basic)
    foreach (var stepDto in dto.Steps)
    {
        var step = new ProductionStep
        {
            ProductionBatchId = batch.ProductionBatchId,
            StepNumber = stepDto.StepNumber,
            StepName = stepDto.StepName,
            UnitOfMeasure = batch.UnitOfMeasure,
            Status = StepStatus.NotStarted,
            CreatedBy = dto.UserId
        };

        await _stepRepo.AddAsync(step);
    }

    await _batchRepo.SaveChangesAsync();

    return batch;
}

// Minimal step completion (no full workflow)
public async Task CompleteManufacturingStepAsync(int stepId, decimal outputQuantity, string userId)
{
    var step = await _stepRepo.GetByIdAsync(stepId);
    step.Status = StepStatus.Completed;
    step.OutputQuantity = outputQuantity;
    step.CompletedDate = DateTime.UtcNow;
    step.CompletedBy = userId;

    await _stepRepo.UpdateAsync(step);
    await _stepRepo.SaveChangesAsync();

    // TODO: Create movements (not implemented yet)
}
```

---

## 8. RETAIL PRODUCTION (60-75% FOCUS)

### 8.1 Full Implementation Scope

**Entities**: ✅ Full implementation (all entities)
**Workflows**: ✅ Complete workflow (4-step pre-roll assembly)
**Movements**: ✅ Full integration with MovementService
**Serial Numbers**: ✅ Auto-generation for each pre-roll
**Quality Control**: ✅ Basic QC checkpoints
**Waste Tracking**: ✅ Trim, rejects, spillage

**Focus**: **Pre-Roll Assembly Workflow**

### 8.2 Pre-Roll Assembly Workflow

```
┌─────────────────────────────────────────────────────────────┐
│              PRE-ROLL ASSEMBLY WORKFLOW (4 STEPS)            │
└─────────────────────────────────────────────────────────────┘

INPUT MATERIALS:
- Bucked flower (from Bucking Production or Purchase)
- Pre-roll cones (empty cones)
- Filters (cardboard tips)
- Packaging materials (tubes, labels)

STEP 1: GRINDING
├── Input: 1000g bucked flower (Batch: CULT-001)
├── Process: Grind flower to consistent texture
├── Output: 950g ground flower (5% wastage)
├── Movements: OUT 1000g flower, IN 950g ground flower
└── Status: Completed

STEP 2: FILLING
├── Input: 950g ground flower, 100 empty cones, 100 filters
├── Process: Fill cones with ground flower (0.8g per cone)
├── Output: 100 filled pre-rolls (80g total), 870g excess ground flower
├── Wastage: 50g spillage
├── Movements: OUT 950g + 100 cones + 100 filters, IN 100 filled pre-rolls
└── Status: Completed

STEP 3: QUALITY CONTROL
├── Input: 100 filled pre-rolls
├── Process: Inspect weight, firmness, appearance
├── Output: 95 approved pre-rolls, 5 rejects
├── Movements: None (QC checkpoint only)
└── Status: Completed

STEP 4: PACKAGING & SERIALIZATION
├── Input: 95 approved pre-rolls, 95 tubes, 95 labels
├── Process: Package each pre-roll, generate serial number, apply label
├── Output: 95 packaged pre-rolls with serial numbers
├── Serial Numbers: SN-0124121100001 through SN-0124121100095
├── Movements: IN 95 packaged pre-rolls (retail-ready)
└── Status: Completed

FINAL OUTPUT:
- 95 packaged pre-rolls (Batch: PROD-002, Serials: SN-001 to SN-095)
- Ready for retail sale
- Full traceability: SN → PROD-002 → CULT-001 → Plant-ID
```

### 8.3 Retail Production Service Implementation

**Full Implementation**:
```csharp
public class RetailProductionService : IRetailProductionService
{
    private readonly IProductionBatchRepository _batchRepo;
    private readonly IProductionStepRepository _stepRepo;
    private readonly ISerialNumberService _serialNumberService;
    private readonly IMovementService _movementService;
    private readonly IBatchNumberService _batchNumberService;

    public async Task<ProductionBatch> CreatePreRollBatchAsync(CreatePreRollBatchDto dto, string userId)
    {
        // 1. Generate batch number
        var batchNumber = await _batchNumberService.GenerateBatchNumberAsync(
            dto.SiteId,
            BatchType.RetailProduction
        );

        // 2. Create production batch
        var batch = new ProductionBatch
        {
            BatchNumber = batchNumber,
            ProductionType = ProductionType.RetailProduction,
            ProductionDate = dto.ProductionDate,
            SiteId = dto.SiteId,
            OutputProductId = dto.PreRollProductId,
            PlannedQuantity = dto.PlannedQuantity, // e.g., 100 units
            UnitOfMeasure = "unit",
            TotalSteps = 4, // Grinding, Filling, QC, Packaging
            CurrentStep = 0,
            Status = BatchStatus.Pending,
            ProductionManager = userId,
            CreatedBy = userId
        };

        await _batchRepo.AddAsync(batch);

        // 3. Create production steps
        var steps = new[]
        {
            new ProductionStep { StepNumber = 1, StepName = "Grinding", UnitOfMeasure = "g" },
            new ProductionStep { StepNumber = 2, StepName = "Filling", UnitOfMeasure = "unit" },
            new ProductionStep { StepNumber = 3, StepName = "Quality Control", UnitOfMeasure = "unit" },
            new ProductionStep { StepNumber = 4, StepName = "Packaging & Serialization", UnitOfMeasure = "unit" }
        };

        foreach (var step in steps)
        {
            step.ProductionBatchId = batch.ProductionBatchId;
            step.Status = StepStatus.NotStarted;
            step.CreatedBy = userId;
            await _stepRepo.AddAsync(step);
        }

        // 4. Record input materials
        var inputs = new[]
        {
            new ProductionInput
            {
                ProductionBatchId = batch.ProductionBatchId,
                ProductId = dto.FlowerProductId,
                Quantity = dto.FlowerQuantity, // e.g., 1000g
                UnitOfMeasure = "g",
                BatchNumber = dto.FlowerBatchNumber,
                CreatedBy = userId
            },
            new ProductionInput
            {
                ProductionBatchId = batch.ProductionBatchId,
                ProductId = dto.ConeProductId,
                Quantity = dto.PlannedQuantity, // e.g., 100 cones
                UnitOfMeasure = "unit",
                CreatedBy = userId
            },
            new ProductionInput
            {
                ProductionBatchId = batch.ProductionBatchId,
                ProductId = dto.FilterProductId,
                Quantity = dto.PlannedQuantity, // e.g., 100 filters
                UnitOfMeasure = "unit",
                CreatedBy = userId
            }
        };

        foreach (var input in inputs)
        {
            await _productionInputRepo.AddAsync(input);
        }

        await _batchRepo.SaveChangesAsync();

        return batch;
    }

    public async Task CompleteStepAsync(int stepId, CompleteStepDto dto, string userId)
    {
        var step = await _stepRepo.GetByIdAsync(stepId);
        var batch = await _batchRepo.GetByIdAsync(step.ProductionBatchId);

        // Validate step can be completed
        if (step.Status == StepStatus.Completed)
            throw new ValidationException("Step already completed");

        if (batch.CurrentStep + 1 != step.StepNumber)
            throw new ValidationException($"Must complete steps in order. Current step: {batch.CurrentStep}");

        // Update step
        step.Status = StepStatus.Completed;
        step.OutputQuantity = dto.OutputQuantity;
        step.WastageQuantity = dto.WastageQuantity;
        step.CompletedDate = DateTime.UtcNow;
        step.CompletedBy = userId;
        step.Notes = dto.Notes;

        await _stepRepo.UpdateAsync(step);

        // Generate movements for this step
        await CreateStepMovementsAsync(batch, step, userId);

        // Update batch progress
        batch.CurrentStep = step.StepNumber;

        // If final step, generate serial numbers and complete batch
        if (step.StepNumber == batch.TotalSteps)
        {
            await GenerateSerialNumbersAsync(batch, dto.OutputQuantity, userId);
            batch.Status = BatchStatus.Completed;
            batch.ActualQuantity = dto.OutputQuantity;
            batch.CompletedDate = DateTime.UtcNow;
        }

        await _batchRepo.UpdateAsync(batch);
        await _batchRepo.SaveChangesAsync();
    }

    private async Task GenerateSerialNumbersAsync(ProductionBatch batch, decimal quantity, string userId)
    {
        var count = (int)quantity; // e.g., 95 units

        for (int i = 1; i <= count; i++)
        {
            // Generate 13-digit short serial number
            var serialNumber = await _serialNumberService.GenerateShortSerialNumberAsync(
                batch.SiteId,
                batch.OutputProductId,
                batch.BatchNumber
            );

            // Create serial number record
            var serial = new SerialNumber
            {
                SerialNumber = serialNumber,
                ProductId = batch.OutputProductId,
                BatchNumber = batch.BatchNumber,
                ProductionBatchId = batch.ProductionBatchId,
                Status = "Produced",
                CreatedBy = userId
            };

            await _serialNumberRepo.AddAsync(serial);

            // Create production output record
            var output = new ProductionOutput
            {
                ProductionBatchId = batch.ProductionBatchId,
                ProductId = batch.OutputProductId,
                Quantity = 1, // One unit
                UnitOfMeasure = "unit",
                BatchNumber = batch.BatchNumber,
                SerialNumber = serialNumber,
                CreatedBy = userId
            };

            await _productionOutputRepo.AddAsync(output);
        }

        await _serialNumberRepo.SaveChangesAsync();
    }

    private async Task CreateStepMovementsAsync(ProductionBatch batch, ProductionStep step, string userId)
    {
        // Step-specific movement logic
        switch (step.StepNumber)
        {
            case 1: // Grinding: OUT flower (raw), IN ground flower
                var inputs = await _productionInputRepo.GetByBatchAsync(batch.ProductionBatchId);
                var flowerInput = inputs.FirstOrDefault(i => i.Product.ProductType == ProductType.RawMaterial);

                if (flowerInput != null)
                {
                    // OUT movement: Raw flower consumed
                    await _movementService.CreateMovementAsync(new CreateMovementDto
                    {
                        ProductId = flowerInput.ProductId,
                        SiteId = batch.SiteId,
                        MovementType = MovementType.ProductionInput,
                        MovementDirection = MovementDirection.OUT,
                        Quantity = flowerInput.Quantity,
                        UnitOfMeasure = flowerInput.UnitOfMeasure,
                        BatchNumber = flowerInput.BatchNumber,
                        TransactionType = "Production",
                        TransactionId = batch.ProductionBatchId,
                        ReferenceNumber = batch.BatchNumber,
                        Notes = $"Step {step.StepNumber}: {step.StepName}",
                        CreatedBy = userId
                    });
                }
                break;

            case 2: // Filling: OUT cones/filters, IN filled pre-rolls (WIP)
                // Similar movement logic
                break;

            case 4: // Packaging: IN finished goods (retail-ready)
                // Final IN movement: Finished pre-rolls
                await _movementService.CreateMovementAsync(new CreateMovementDto
                {
                    ProductId = batch.OutputProductId,
                    SiteId = batch.SiteId,
                    MovementType = MovementType.ProductionOutput,
                    MovementDirection = MovementDirection.IN,
                    Quantity = step.OutputQuantity.Value,
                    UnitOfMeasure = step.UnitOfMeasure,
                    BatchNumber = batch.BatchNumber,
                    TransactionType = "Production",
                    TransactionId = batch.ProductionBatchId,
                    ReferenceNumber = batch.BatchNumber,
                    Notes = $"Production completed: {step.OutputQuantity} units",
                    CreatedBy = userId
                });
                break;
        }
    }
}
```

---

## 9. BATCH NUMBER GENERATION

**See [06-SERIAL-BATCH-GENERATION.md](06-SERIAL-BATCH-GENERATION.md) for full specification.**

**Quick Summary**:
- Format: 16 digits `SSTTYYYYMMDDNNNN`
- Example: `0101202412110001` = Site 01, Type 01 (Retail Prod), 2024-12-11, Seq 0001

---

## 10. SERIAL NUMBER GENERATION

**See [06-SERIAL-BATCH-GENERATION.md](06-SERIAL-BATCH-GENERATION.md) for full specification.**

**Quick Summary**:
- Short format (13 digits): `SSYYMMDDNNNNN` (EAN-13 compatible)
- Example: `0124121100015` = Site 01, 2024-12-11, Seq 00015
- Generated during final production step (Packaging & Serialization)
- Linked to batch number for traceability

---

## 11. MOVEMENT INTEGRATION

### 11.1 Movement Generation on Step Completion

**Principle**: Every production step creates movements when completed.

**Step 1 (Grinding)**: OUT movements for raw flower
**Step 2 (Filling)**: OUT movements for cones/filters
**Step 4 (Packaging)**: IN movements for finished goods

**Example**:
```csharp
// Step 1: Grinding completed
// OUT movement: 1000g flower (raw material consumed)
Movement: ProductionInput, OUT, 1000g, Batch: CULT-001

// Step 4: Packaging completed
// IN movement: 95 units packaged pre-rolls (finished goods produced)
Movement: ProductionOutput, IN, 95 units, Batch: PROD-002
```

### 11.2 SOH Impact

**Before Production**:
- Flower SOH: 1000g (Site 1, Batch: CULT-001)
- Pre-roll SOH: 0 units

**After Production**:
- Flower SOH: 0g (consumed in production)
- Pre-roll SOH: 95 units (Site 1, Batch: PROD-002, Serials: SN-001 to SN-095)

---

## 12. QUALITY CONTROL

### 12.1 QC Checkpoints

**Step 3: Quality Control** (Pre-Roll Assembly)

**QC Criteria**:
- Weight: 0.8g ± 0.1g per pre-roll
- Firmness: Not too tight, not too loose
- Appearance: No tears in cone, filter secure

**QC Outcome**:
- Approved: Continue to packaging
- Rejected: Record as wastage, do not serialize

**Implementation**:
```csharp
public async Task RecordQualityCheckAsync(int stepId, QualityCheckDto dto, string userId)
{
    var step = await _stepRepo.GetByIdAsync(stepId);

    // Record QC results
    var qc = new QualityCheck
    {
        ProductionStepId = stepId,
        InspectedQuantity = dto.InspectedQuantity,
        ApprovedQuantity = dto.ApprovedQuantity,
        RejectedQuantity = dto.RejectedQuantity,
        RejectionReason = dto.RejectionReason,
        Inspector = userId,
        CreatedBy = userId
    };

    await _qcRepo.AddAsync(qc);

    // Update step output (only approved units)
    step.OutputQuantity = dto.ApprovedQuantity;
    step.WastageQuantity = dto.RejectedQuantity;

    await _stepRepo.UpdateAsync(step);
    await _stepRepo.SaveChangesAsync();
}
```

---

## 13. WASTE TRACKING

### 13.1 Waste Types

| Waste Type | Example | Tracking |
|------------|---------|----------|
| Spillage | Ground flower spilled during filling | ProductionStep.WastageQuantity |
| Rejects | Pre-rolls failing QC | QualityCheck.RejectedQuantity |
| Trim | Stems, leaves removed during bucking | ProductionStep.WastageQuantity |

### 13.2 Waste Movement

**Option 1**: Record as wastage movement (OUT)
```csharp
// Wastage movement: 50g spillage
Movement: Wastage, OUT, 50g, Batch: PROD-002
```

**Option 2**: Calculate as difference between input and output
```csharp
Wastage = InputQuantity - OutputQuantity
// No explicit movement, implied from step quantities
```

**Project420 Implementation**: **Option 2** (simpler, no extra movements)

---

## 14. DATA ACCESS LAYER

### 14.1 IProductionBatchRepository

```csharp
public interface IProductionBatchRepository
{
    // CRUD
    Task<ProductionBatch?> GetByIdAsync(int batchId);
    Task<ProductionBatch?> GetByNumberAsync(string batchNumber);
    Task<ProductionBatch?> GetByIdWithDetailsAsync(int batchId);
    Task<IEnumerable<ProductionBatch>> GetAllAsync();
    Task AddAsync(ProductionBatch batch);
    Task UpdateAsync(ProductionBatch batch);
    Task DeleteAsync(int batchId); // Soft delete
    Task SaveChangesAsync();

    // Queries
    Task<IEnumerable<ProductionBatch>> GetByStatusAsync(BatchStatus status);
    Task<IEnumerable<ProductionBatch>> GetBySiteAsync(int siteId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<ProductionBatch>> GetByProductionTypeAsync(ProductionType type);
}
```

---

## 15. BUSINESS LOGIC LAYER

### 15.1 IRetailProductionService

```csharp
public interface IRetailProductionService
{
    // Batch management
    Task<ProductionBatchDto> CreatePreRollBatchAsync(CreatePreRollBatchDto dto, string userId);
    Task<ProductionBatchDto> GetBatchByIdAsync(int batchId);
    Task<ProductionBatchDto> GetBatchByNumberAsync(string batchNumber);

    // Step management
    Task StartStepAsync(int stepId, string userId);
    Task CompleteStepAsync(int stepId, CompleteStepDto dto, string userId);

    // Quality control
    Task RecordQualityCheckAsync(int stepId, QualityCheckDto dto, string userId);

    // Serial numbers
    Task<IEnumerable<SerialNumberDto>> GetSerialNumbersByBatchAsync(string batchNumber);
}
```

---

## 16. API ENDPOINTS

### 16.1 Production Controller

```csharp
[ApiController]
[Route("api/production")]
public class ProductionController : ControllerBase
{
    // Create pre-roll batch
    [HttpPost("retail/prerolls")]
    public async Task<IActionResult> CreatePreRollBatch([FromBody] CreatePreRollBatchDto dto)
    {
        var batch = await _retailProductionService.CreatePreRollBatchAsync(dto, User.Identity.Name);
        return CreatedAtAction(nameof(GetBatch), new { id = batch.ProductionBatchId }, batch);
    }

    // Get batch
    [HttpGet("batches/{id}")]
    public async Task<IActionResult> GetBatch(int id)
    {
        var batch = await _retailProductionService.GetBatchByIdAsync(id);
        if (batch == null) return NotFound();
        return Ok(batch);
    }

    // Complete step
    [HttpPost("steps/{id}/complete")]
    public async Task<IActionResult> CompleteStep(int id, [FromBody] CompleteStepDto dto)
    {
        await _retailProductionService.CompleteStepAsync(id, dto, User.Identity.Name);
        return NoContent();
    }
}
```

---

## 17. VALIDATION RULES

### 17.1 Batch Validation

```csharp
public class CreatePreRollBatchValidator : AbstractValidator<CreatePreRollBatchDto>
{
    public CreatePreRollBatchValidator(ISOHService sohService)
    {
        RuleFor(x => x.SiteId)
            .GreaterThan(0).WithMessage("Site ID is required");

        RuleFor(x => x.PlannedQuantity)
            .GreaterThan(0).WithMessage("Planned quantity must be greater than 0");

        RuleFor(x => x.FlowerQuantity)
            .GreaterThan(0).WithMessage("Flower quantity is required");

        // Check sufficient flower stock
        RuleFor(x => x)
            .MustAsync(async (dto, cancellation) =>
            {
                var soh = await sohService.GetSOHAsync(dto.FlowerProductId, dto.SiteId, dto.FlowerBatchNumber);
                return soh >= dto.FlowerQuantity;
            })
            .WithMessage("Insufficient flower stock");
    }
}
```

---

## 18. TESTING STRATEGY

### 18.1 Unit Tests

```csharp
public class RetailProductionServiceTests
{
    [Fact]
    public async Task CreatePreRollBatch_ValidData_CreatesSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IProductionBatchRepository>();
        var service = new RetailProductionService(mockRepo.Object, null, null, null, null);

        var dto = new CreatePreRollBatchDto
        {
            SiteId = 1,
            PreRollProductId = 10,
            FlowerProductId = 5,
            FlowerQuantity = 1000m,
            PlannedQuantity = 100
        };

        // Act
        var result = await service.CreatePreRollBatchAsync(dto, "testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalSteps);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<ProductionBatch>()), Times.Once);
    }
}
```

---

## 19. IMPLEMENTATION ROADMAP

### 19.1 Phase 10: Retail Production (Week 4)

**Tasks**:
- [ ] Implement `RetailProductionService` with 4-step workflow
- [ ] Integrate serial number generation
- [ ] Implement movement generation for each step
- [ ] Create QC checkpoint functionality
- [ ] Unit test production workflow (20+ tests)

**Deliverable**: Retail production at 60-75% completeness.

### 19.2 Phase 11-12: Stubs & Final Testing (Weeks 5-6)

**Tasks**:
- [ ] Create minimal Bucking stub (35%)
- [ ] Create minimal General Manufacturing stub (45%)
- [ ] Integration test retail production
- [ ] End-to-end traceability testing

**Deliverable**: Production module complete.

---

**END OF PRODUCTION MODEL SPECIFICATION**
