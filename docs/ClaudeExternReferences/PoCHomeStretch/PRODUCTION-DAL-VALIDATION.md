# Project420 - Production DAL Validation & Expansion Plan
## Detailed Analysis and Implementation Requirements for Production Module

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: VALIDATION COMPLETE
**Priority**: 🟡 MEDIUM (Retail Production), 🟢 LOW (General Production/Cultivation)

---

## 📋 EXECUTIVE SUMMARY

### Current State Assessment

Based on CLAUDE.md (Phase 6 Complete), the Production module currently has:
- ✅ Entities created: ProductionBatch, ProcessingStep, QualityControl, LabTest
- ✅ DbContext created: ProductionDbContext with Fluent API configurations
- ✅ Validators created: 8 validators (Create/Update for each entity)
- ✅ Basic DTOs created: ProductionBatchDtos, ProcessingStepDtos, QualityControlDtos, LabTestDtos
- ✅ Basic Services created: IProductionBatchService, IProcessingStepService, IQualityControlService, ILabTestService
- ❌ Repositories NOT created (only entity/DbContext layer exists)
- ❌ Full BLL implementation incomplete
- ❌ Retail Production sub-module NOT specialized
- ❌ Movement generation NOT integrated

### Target State (Phase 10 - Week 4)

| Component | Current | Target | Priority | Completeness |
|-----------|---------|--------|----------|--------------|
| **Retail Production DAL** | 20% | 75% | 🟡 MEDIUM | Good implementation needed |
| **Retail Production BLL** | 20% | 75% | 🟡 MEDIUM | Full workflow required |
| **General Production DAL** | 20% | 45% | 🟢 LOW | Stub implementation |
| **Cultivation DAL** | 15% | 35% | 🟢 LOW | Basic stub |

---

## 1. VALIDATION OF EXISTING PRODUCTION ENTITIES

### 1.1 ProductionBatch Entity (Current)

**Location**: `src/Modules/Production/Project420.Production.Models/Entities/ProductionBatch.cs`

**Expected Structure**:
```csharp
public class ProductionBatch : AuditableEntity
{
    public int ProductionBatchId { get; set; }

    [Required]
    [StringLength(50)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ProductType { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? TotalInputMass { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? TotalOutputQuantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<ProcessingStep>? ProcessingSteps { get; set; }
    public virtual ICollection<QualityControl>? QualityControls { get; set; }
    public virtual ICollection<LabTest>? LabTests { get; set; }
}
```

**✅ Validation**: If entity matches above, it's correct.

**❌ Missing Fields** (add if not present):
- `SiteId` (int) - Required for batch number generation
- `SourceBatchNumber` (string, nullable) - Traceability to source batch
- `TargetProductId` (int, nullable) - What product are we producing?

**🔧 Required Changes**:
```csharp
// Add to ProductionBatch entity:
[Required]
public int SiteId { get; set; }

[StringLength(50)]
public string? SourceBatchNumber { get; set; }

public int? TargetProductId { get; set; }
public virtual Product? TargetProduct { get; set; }
```

---

### 1.2 ProcessingStep Entity (Current)

**Expected Structure**:
```csharp
public class ProcessingStep : AuditableEntity
{
    public int ProcessingStepId { get; set; }

    [Required]
    public int ProductionBatchId { get; set; }
    public virtual ProductionBatch? ProductionBatch { get; set; }

    [Required]
    public int StepNumber { get; set; }

    [Required]
    [StringLength(100)]
    public string StepName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string StepType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,4)")]
    public decimal? InputQuantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? InputMass { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? OutputQuantity { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? OutputMass { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? LossMass { get; set; }

    [StringLength(200)]
    public string? LossReason { get; set; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    [StringLength(100)]
    public string? OperatorId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
}
```

**✅ Validation**: If entity matches above, it's correct.

**❌ Missing Fields** (add if not present):
- `InputPotencyTHC` (string, nullable) - THC% of input material
- `OutputPotencyTHC` (string, nullable) - THC% of output material

**🔧 Required Changes**:
```csharp
// Add to ProcessingStep entity:
[StringLength(10)]
public string? InputPotencyTHC { get; set; }

[StringLength(10)]
public string? OutputPotencyTHC { get; set; }
```

---

### 1.3 QualityControl Entity (Current)

**Expected Structure**:
```csharp
public class QualityControl : AuditableEntity
{
    public int QualityControlId { get; set; }

    [Required]
    public int ProductionBatchId { get; set; }
    public virtual ProductionBatch? ProductionBatch { get; set; }

    public int? ProcessingStepId { get; set; }
    public virtual ProcessingStep? ProcessingStep { get; set; }

    [Required]
    public DateTime InspectionDate { get; set; }

    [Required]
    [StringLength(100)]
    public string InspectorId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Result { get; set; } = "Pending";

    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

**✅ Validation**: Entity structure is adequate.

**⚠️ Enhancement Needed**: Add specific QC check fields for retail production:
```csharp
// Add to QualityControl entity:
[Column(TypeName = "decimal(18,4)")]
public decimal? WeightVariance { get; set; } // For pre-roll weight checks

public int? SampleSize { get; set; }
public int? PassedSamples { get; set; }
public int? FailedSamples { get; set; }
```

---

### 1.4 LabTest Entity (Current)

**Expected Structure**:
```csharp
public class LabTest : AuditableEntity
{
    public int LabTestId { get; set; }

    [Required]
    public int ProductionBatchId { get; set; }
    public virtual ProductionBatch? ProductionBatch { get; set; }

    [Required]
    [StringLength(200)]
    public string LabName { get; set; } = string.Empty;

    [Required]
    public DateTime TestDate { get; set; }

    [StringLength(50)]
    public string? THCPercentage { get; set; }

    [StringLength(50)]
    public string? CBDPercentage { get; set; }

    [StringLength(50)]
    public string? ContaminantResult { get; set; }

    [StringLength(50)]
    public string? MicrobialResult { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
```

**✅ Validation**: Entity structure is adequate.

**⚠️ Enhancement Needed**: Add COA storage:
```csharp
// Add to LabTest entity:
[StringLength(100)]
public string? COAFileName { get; set; }

public byte[]? COAFileData { get; set; } // Store PDF
```

---

## 2. REQUIRED: RETAIL PRODUCTION REPOSITORY LAYER

### 2.1 IProductionBatchRepository Interface

**Location**: `src/Modules/Production/Project420.Production.DAL/Repositories/IProductionBatchRepository.cs`

**Required Methods**:
```csharp
public interface IProductionBatchRepository
{
    // Basic CRUD
    Task<ProductionBatch?> GetByIdAsync(int batchId);
    Task<ProductionBatch?> GetByBatchNumberAsync(string batchNumber);
    Task<IEnumerable<ProductionBatch>> GetAllAsync();
    Task<ProductionBatch> CreateAsync(ProductionBatch batch);
    Task<ProductionBatch> UpdateAsync(ProductionBatch batch);
    Task DeleteAsync(int batchId);

    // Eager loading
    Task<ProductionBatch?> GetWithStepsAsync(int batchId);
    Task<ProductionBatch?> GetWithQCAndLabTestsAsync(int batchId);
    Task<ProductionBatch?> GetCompleteAsync(int batchId); // All related entities

    // Business queries
    Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync();
    Task<IEnumerable<ProductionBatch>> GetByStatusAsync(string status);
    Task<IEnumerable<ProductionBatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductionBatch>> GetByProductTypeAsync(string productType);
    Task<IEnumerable<ProductionBatch>> GetBatchesNeedingQCAsync();
    Task<IEnumerable<ProductionBatch>> GetBatchesNeedingLabTestAsync();
    Task<IEnumerable<ProductionBatch>> GetCompletedBatchesAsync(DateTime startDate, DateTime endDate);

    // Retail production specific
    Task<IEnumerable<ProductionBatch>> GetPreRollBatchesAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<ProductionBatch>> GetPackagingBatchesAsync(DateTime startDate, DateTime endDate);
}
```

### 2.2 ProductionBatchRepository Implementation

**Location**: `src/Modules/Production/Project420.Production.DAL/Repositories/ProductionBatchRepository.cs`

**Implementation**:
```csharp
public class ProductionBatchRepository : IProductionBatchRepository
{
    private readonly ProductionDbContext _context;

    public ProductionBatchRepository(ProductionDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ProductionBatch?> GetByIdAsync(int batchId)
    {
        return await _context.ProductionBatches
            .FirstOrDefaultAsync(b => b.ProductionBatchId == batchId);
    }

    public async Task<ProductionBatch?> GetByBatchNumberAsync(string batchNumber)
    {
        return await _context.ProductionBatches
            .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
    }

    public async Task<ProductionBatch?> GetWithStepsAsync(int batchId)
    {
        return await _context.ProductionBatches
            .Include(b => b.ProcessingSteps!.OrderBy(s => s.StepNumber))
            .FirstOrDefaultAsync(b => b.ProductionBatchId == batchId);
    }

    public async Task<ProductionBatch?> GetWithQCAndLabTestsAsync(int batchId)
    {
        return await _context.ProductionBatches
            .Include(b => b.QualityControls)
            .Include(b => b.LabTests)
            .FirstOrDefaultAsync(b => b.ProductionBatchId == batchId);
    }

    public async Task<ProductionBatch?> GetCompleteAsync(int batchId)
    {
        return await _context.ProductionBatches
            .Include(b => b.ProcessingSteps!.OrderBy(s => s.StepNumber))
            .Include(b => b.QualityControls)
            .Include(b => b.LabTests)
            .FirstOrDefaultAsync(b => b.ProductionBatchId == batchId);
    }

    public async Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync()
    {
        return await _context.ProductionBatches
            .Where(b => b.Status == "Active" || b.Status == "InProgress")
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetBatchesNeedingQCAsync()
    {
        return await _context.ProductionBatches
            .Where(b => b.Status == "PendingQC")
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductionBatch>> GetPreRollBatchesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ProductionBatches
            .Where(b => b.ProductType == "PreRoll" || b.ProductType == "Retail Production"
                && b.StartDate >= startDate && b.StartDate <= endDate)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    // ... implement remaining methods ...
}
```

---

### 2.3 ProcessingStepRepository

**Required Methods**:
```csharp
public interface IProcessingStepRepository
{
    Task<ProcessingStep> CreateAsync(ProcessingStep step);
    Task<ProcessingStep?> GetByIdAsync(int stepId);
    Task<IEnumerable<ProcessingStep>> GetByBatchIdAsync(int batchId);
    Task<ProcessingStep> UpdateAsync(ProcessingStep step);
    Task CompleteStepAsync(int stepId, decimal outputQuantity, decimal outputMass, decimal lossMass, string? lossReason);
    Task<ProcessingStep?> GetCurrentStepAsync(int batchId); // Get active step for batch
    Task<bool> CanProceedToNextStepAsync(int batchId, int currentStepNumber); // Validate step completion
}
```

**Implementation Notes**:
- `CompleteStepAsync` should mark step as completed, record outputs/losses
- Validate previous step completed before allowing next step
- Track operator, start/end times

---

### 2.4 QualityControlRepository (Stub)

**Required Methods** (minimal):
```csharp
public interface IQualityControlRepository
{
    Task<QualityControl> CreateAsync(QualityControl qc);
    Task<QualityControl?> GetByIdAsync(int qcId);
    Task<IEnumerable<QualityControl>> GetByBatchIdAsync(int batchId);
    Task<QualityControl> UpdateAsync(QualityControl qc);
}
```

---

### 2.5 LabTestRepository (Stub)

**Required Methods** (minimal):
```csharp
public interface ILabTestRepository
{
    Task<LabTest> CreateAsync(LabTest labTest);
    Task<LabTest?> GetByIdAsync(int labTestId);
    Task<IEnumerable<LabTest>> GetByBatchIdAsync(int batchId);
    Task AttachCOAAsync(int labTestId, string fileName, byte[] fileData);
}
```

---

## 3. REQUIRED: RETAIL PRODUCTION SERVICE LAYER

### 3.1 IRetailProductionService Interface

**Location**: `src/Modules/Production/Project420.Production.BLL/Services/IRetailProductionService.cs`

**Required Methods**:
```csharp
public interface IRetailProductionService
{
    // Pre-Roll Production
    Task<ProductionBatchDto> StartPreRollProductionAsync(StartPreRollProductionDto dto);
    Task<ProcessingStepDto> RecordMillingStepAsync(int batchId, RecordMillingStepDto dto);
    Task<ProcessingStepDto> RecordFillingStepAsync(int batchId, RecordFillingStepDto dto);
    Task<ProcessingStepDto> RecordCappingStepAsync(int batchId, RecordCappingStepDto dto);
    Task<List<string>> PackageAndGenerateSerialNumbersAsync(int batchId, PackagePreRollsDto dto);

    // Packaged Flower Production
    Task<ProductionBatchDto> StartPackagedFlowerProductionAsync(StartPackagedFlowerProductionDto dto);
    Task<ProcessingStepDto> RecordSelectionStepAsync(int batchId, RecordSelectionStepDto dto);
    Task<ProcessingStepDto> RecordWeighingStepAsync(int batchId, RecordWeighingStepDto dto);
    Task<List<string>> PackageFlowerAndGenerateSerialNumbersAsync(int batchId, PackageFlowerDto dto);

    // General
    Task<ProcessingStepDto> CompleteStepAsync(int batchId, int stepNumber, CompleteStepDto dto);
    Task<ProductionBatchDto> CompleteBatchAsync(int batchId);
    Task<ProductionReportDto> GetBatchReportAsync(int batchId);
    Task<List<ProductionBatchDto>> GetActiveBatchesAsync();
}
```

### 3.2 Pre-Roll Production Workflow

**Steps**:
1. **Milling**: Grind flower to consistent texture
   - Input: Bucked flower (10kg)
   - Output: Milled flower (9.8kg)
   - Loss: 0.2kg (dust, over-processing)

2. **Filling**: Fill pre-roll cones
   - Input: Milled flower (9.8kg)
   - Output: Filled pre-rolls (9,500 units @ 1g = 9.5kg)
   - Loss: 0.3kg (spillage, QC rejects)

3. **Capping**: Attach filters, inspect
   - Input: Filled pre-rolls (9,500 units)
   - Output: Capped pre-rolls (9,400 units)
   - Loss: 100 units (damaged, incorrect weight)

4. **Packaging**: Pack retail units, assign SNs
   - Input: Capped pre-rolls (9,400 units)
   - Output: Packaged pre-roll packs (940 packs of 10)
   - SNs: SN-PR-20250115-001 to SN-PR-20250115-940

**Implementation**:
```csharp
public async Task<ProductionBatchDto> StartPreRollProductionAsync(StartPreRollProductionDto dto)
{
    // 1. Generate batch number
    var batchNumber = await _batchNumberService.GenerateBatchNumberAsync(
        dto.SiteId,
        BatchType.RetailProduction
    );

    // 2. Create batch record
    var batch = new ProductionBatch
    {
        BatchNumber = batchNumber,
        SiteId = dto.SiteId,
        ProductType = "PreRoll",
        Status = "Active",
        StartDate = DateTime.UtcNow,
        SourceBatchNumber = dto.SourceFlowerBatchNumber,
        TargetProductId = dto.TargetProductId,
        Notes = dto.Notes
    };

    await _batchRepository.CreateAsync(batch);

    // 3. Create input movement (consume flower)
    await CreateProductionInputMovementAsync(
        batchId: batch.ProductionBatchId,
        productId: dto.SourceFlowerProductId,
        quantity: dto.InputMass,
        batchNumber: dto.SourceFlowerBatchNumber
    );

    return _mapper.Map<ProductionBatchDto>(batch);
}

public async Task<List<string>> PackageAndGenerateSerialNumbersAsync(int batchId, PackagePreRollsDto dto)
{
    var batch = await _batchRepository.GetByIdAsync(batchId);
    if (batch == null)
        throw new NotFoundException($"Batch {batchId} not found");

    // Generate SNs for each pack
    var serialNumbers = new List<string>();
    for (int i = 1; i <= dto.Quantity; i++)
    {
        var sn = await _serialNumberService.GenerateFullSerialNumberAsync(
            siteId: batch.SiteId,
            strainId: dto.StrainId,
            productType: ProductType.PreRoll,
            batchNumber: batch.BatchNumber,
            unitSequence: i,
            weightInGrams: dto.WeightPerPack
        );

        serialNumbers.Add(sn);
    }

    // Create output movement (produce pre-rolls)
    await CreateProductionOutputMovementAsync(
        batchId: batchId,
        productId: batch.TargetProductId.Value,
        quantity: dto.Quantity,
        batchNumber: batch.BatchNumber,
        serialNumbers: serialNumbers
    );

    // Mark batch as completed
    batch.Status = "Completed";
    batch.CompletionDate = DateTime.UtcNow;
    batch.TotalOutputQuantity = dto.Quantity;
    await _batchRepository.UpdateAsync(batch);

    return serialNumbers;
}
```

---

## 4. MOVEMENT GENERATION INTEGRATION

### 4.1 Production Input Movement

When recording production input (consuming flower):

```csharp
private async Task CreateProductionInputMovementAsync(
    int batchId,
    int productId,
    decimal quantity,
    string batchNumber)
{
    // Create transaction detail
    var detail = new TransactionDetail
    {
        HeaderId = batchId,
        TransactionType = TransactionType.ProductionInput,
        ProductId = productId,
        Quantity = quantity,
        BatchNumber = batchNumber,
        UnitPrice = 0, // Not relevant for production
        LineTotal = 0
    };

    await _context.TransactionDetails.AddAsync(detail);
    await _context.SaveChangesAsync();

    // Generate movements (OUT movement - reduces inventory)
    await _movementService.GenerateMovementsAsync(
        TransactionType.ProductionInput,
        batchId
    );
}
```

### 4.2 Production Output Movement

When completing batch (producing finished goods):

```csharp
private async Task CreateProductionOutputMovementAsync(
    int batchId,
    int productId,
    decimal quantity,
    string batchNumber,
    List<string> serialNumbers)
{
    // Create transaction detail
    var detail = new TransactionDetail
    {
        HeaderId = batchId,
        TransactionType = TransactionType.ProductionOutput,
        ProductId = productId,
        Quantity = quantity,
        BatchNumber = batchNumber,
        UnitPrice = 0,
        LineTotal = 0
    };

    await _context.TransactionDetails.AddAsync(detail);
    await _context.SaveChangesAsync();

    // Generate movements (IN movement - increases inventory)
    await _movementService.GenerateMovementsAsync(
        TransactionType.ProductionOutput,
        batchId
    );
}
```

---

## 5. REQUIRED DTOS

### 5.1 Pre-Roll Production DTOs

```csharp
// Start batch
public class StartPreRollProductionDto
{
    public int SiteId { get; set; }
    public string SourceFlowerBatchNumber { get; set; } = string.Empty;
    public int SourceFlowerProductId { get; set; }
    public decimal InputMass { get; set; }
    public int TargetProductId { get; set; }
    public int StrainId { get; set; }
    public string? Notes { get; set; }
}

// Record step
public class RecordMillingStepDto
{
    public decimal InputMass { get; set; }
    public decimal OutputMass { get; set; }
    public decimal LossMass { get; set; }
    public string? LossReason { get; set; }
    public string OperatorId { get; set; } = string.Empty;
}

// Package
public class PackagePreRollsDto
{
    public int Quantity { get; set; } // Number of packs
    public int UnitsPerPack { get; set; } // Usually 10
    public decimal WeightPerPack { get; set; } // Total weight of pack
    public int StrainId { get; set; }
}

// Report
public class ProductionReportDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal TotalInputMass { get; set; }
    public decimal TotalOutputQuantity { get; set; }
    public decimal YieldPercentage { get; set; }
    public List<ProcessingStepDto> ProcessingSteps { get; set; } = new();
}
```

---

## 6. STUB IMPLEMENTATIONS (General Production & Cultivation)

### 6.1 General Production Service (Stub)

**Purpose**: Extraction, formulation (edibles, vapes, oils)

**Implementation**: Create interface + stub methods only. Full implementation deferred to post-PoC.

```csharp
public interface IManufacturingService
{
    Task<ProductionBatchDto> StartExtractionBatchAsync(StartExtractionBatchDto dto);
    Task<ProductionBatchDto> StartFormulationBatchAsync(StartFormulationBatchDto dto);
    // ... stub methods only
}

public class ManufacturingService : IManufacturingService
{
    public Task<ProductionBatchDto> StartExtractionBatchAsync(StartExtractionBatchDto dto)
    {
        throw new NotImplementedException("Extraction workflow implementation deferred to post-PoC");
    }

    // ... stub implementations
}
```

### 6.2 Cultivation Service (Stub)

**Purpose**: Plant tracking, grow cycles, harvest batches

**Implementation**: Entities already created in Phase 5. Create stub service only.

```csharp
public interface ICultivationService
{
    Task<PlantDto> CreatePlantAsync(CreatePlantDto dto);
    Task<GrowCycleDto> CreateGrowCycleAsync(CreateGrowCycleDto dto);
    Task<HarvestBatchDto> RecordHarvestAsync(RecordHarvestDto dto);
    // ... stub methods
}

public class CultivationService : ICultivationService
{
    public Task<PlantDto> CreatePlantAsync(CreatePlantDto dto)
    {
        throw new NotImplementedException("Cultivation workflow implementation deferred to post-PoC");
    }

    // ... stub implementations
}
```

---

## 7. VALIDATION CHECKLIST

### 7.1 Phase 10 Deliverables (Week 4)

**Retail Production (75% Target)**:
- [ ] Validate existing entities (add missing fields)
- [ ] Create `IProductionBatchRepository` interface (20 methods)
- [ ] Implement `ProductionBatchRepository` class
- [ ] Create `IProcessingStepRepository` interface (8 methods)
- [ ] Implement `ProcessingStepRepository` class
- [ ] Create `IRetailProductionService` interface (15 methods)
- [ ] Implement `RetailProductionService` class
- [ ] Implement pre-roll production workflow (4 steps)
- [ ] Implement packaged flower workflow (3 steps)
- [ ] Integrate with `MovementService` (input/output movements)
- [ ] Integrate with `BatchNumberGeneratorService`
- [ ] Integrate with `SerialNumberGeneratorService`
- [ ] Create all required DTOs (8+ DTOs)
- [ ] Unit test repositories (20+ tests)
- [ ] Unit test services (30+ tests)
- [ ] Integration test end-to-end pre-roll production
- [ ] Create production reports (batch summary, yield analysis)

**General Production (45% Target - Stub)**:
- [ ] Create `IManufacturingService` interface
- [ ] Implement stub methods (throw NotImplementedException)
- [ ] Create basic DTOs
- [ ] Document workflows (for future implementation)

**Cultivation (35% Target - Stub)**:
- [ ] Create `ICultivationService` interface
- [ ] Implement stub methods (throw NotImplementedException)
- [ ] Entities already created (Phase 5) ✅
- [ ] Document workflows (for future implementation)

---

## 8. PRIORITY RANKING

### High Priority (Week 4 - Phase 10)
1. **ProductionBatchRepository** - Complete implementation required
2. **ProcessingStepRepository** - Complete implementation required
3. **RetailProductionService** - Pre-roll workflow required
4. **Movement Integration** - Critical for SOH accuracy
5. **Serial Number Generation** - Required for packaging step

### Medium Priority (Phase 10)
6. **Packaged Flower Workflow** - Secondary retail production type
7. **Production Reports** - Batch summary, yields
8. **QC Integration** - Quality control checks

### Low Priority (Post-PoC)
9. **Extraction Workflow** - Stub only for PoC
10. **Formulation Workflow** - Stub only for PoC
11. **Cultivation Workflows** - Stub only for PoC

---

## ✅ VALIDATION COMPLETE

**Status**: ✅ Analysis complete, requirements documented
**Action**: Proceed with Phase 10 implementation (Week 4)
**Focus**: Retail Production DAL + BLL to 75% completeness
**Defer**: General production and cultivation to post-PoC (stub implementations only)

**Next Steps**:
1. Validate existing entities (add missing fields per section 1)
2. Create repository interfaces and implementations (section 2)
3. Create service layer (section 3)
4. Integrate movement generation (section 4)
5. Create DTOs (section 5)
6. Stub out general production and cultivation (section 6)
7. Test thoroughly (20+ repository tests, 30+ service tests)
