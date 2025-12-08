# Phase 5: Production-Ready Prototype - Implementation Plan

**Plan Created**: 2025-12-07
**Target Start**: Ready to begin
**Estimated Duration**: 10-14 days
**Status**: READY TO EXECUTE

---

## Executive Summary

Phase 5 transforms Project420 from a retail POS proof-of-concept into a production-ready cannabis cultivation, production, and inventory management system. This phase implements the essential modules required for legal compliance with South African cannabis regulations (SAHPRA, DALRRD, Cannabis Act 2024).

**Goal**: Create fully functional seed-to-sale traceability for cannabis operations.

---

## Prerequisites (COMPLETED ✅)

- [x] Phase 4 Retail vertical slice operational
- [x] CODING-STANDARDS.md established
- [x] MODULE-TEMPLATE.md created
- [x] Both compliance guides reviewed
- [x] 224+ tests passing
- [x] 0 errors, 0 warnings build
- [x] Shared services operational (VAT, Audit Log, Transaction Numbers)

---

## Implementation Strategy

### Approach: Template-Driven Development

Each module will follow the established MODULE-TEMPLATE.md pattern:

1. **Plan** → Complete Module Planning Checklist
2. **Models** → Create entities and enums
3. **DAL** → Create DbContext and repositories
4. **BLL** → Create services, DTOs, validators
5. **UI** → Create Blazor pages (basic)
6. **Tests** → Write unit tests (target 70%+ coverage)
7. **Integration** → Connect to Shared services
8. **Documentation** → Update module docs

---

## Module 1: Cultivation Module

**Priority**: ⚠️ CRITICAL
**Duration**: 3 days
**Compliance**: SAHPRA Section 22C, DALRRD Hemp Permits

### Day 1: Models & DAL

#### Entities to Create

**File**: `src/Modules/Cultivation/Project420.Cultivation.Models/Entities/`

1. **GrowCycle.cs**
   ```csharp
   - Id, Name, Description
   - StartDate, EndDate, Status (enum: Planning, Active, Harvesting, Complete)
   - TargetYield, ActualYield
   - GrowRoomId (FK)
   - BatchNumber (SAHPRA traceability)
   - Notes
   - AuditableEntity fields
   ```

2. **GrowRoom.cs**
   ```csharp
   - Id, Name, Code, Description
   - Capacity (plant count)
   - EnvironmentalSettings (JSON: temp, humidity, CO2, light schedule)
   - LocationDescription
   - IsActive
   - AuditableEntity fields
   ```

3. **Plant.cs**
   ```csharp
   - Id, PlantTag (unique barcode/RFID)
   - GrowCycleId (FK)
   - MotherPlantId (FK - nullable, for clones)
   - StrainName, GeneticLineage
   - PlantDate, TransplantDate
   - CurrentStage (enum: Seed, Clone, Vegetative, Flowering, Harvested, Destroyed)
   - Location (grow room + specific position)
   - HealthStatus (enum: Healthy, Stressed, Diseased, Dead)
   - Notes
   - HarvestDate, WetWeight, DryWeight
   - DestructionDate, DestructionReason
   - AuditableEntity fields
   ```

4. **PlantStage.cs** (History table)
   ```csharp
   - Id, PlantId (FK)
   - Stage (enum: Seed, Clone, Vegetative, Flowering, Harvested, Destroyed)
   - StartDate, EndDate
   - Notes
   - AuditableEntity fields
   ```

5. **HarvestBatch.cs**
   ```csharp
   - Id, BatchNumber (SAHPRA compliance)
   - GrowCycleId (FK)
   - HarvestDate
   - WetWeight, DryWeight (after drying)
   - PlantCount
   - StrainName
   - LabTestDate, THCPercentage, CBDPercentage
   - QualityGrade (enum: A, B, C, Reject)
   - Notes
   - AuditableEntity fields
   ```

6. **CultivationTask.cs**
   ```csharp
   - Id, TaskName, Description
   - GrowCycleId (FK), GrowRoomId (FK)
   - TaskType (enum: Watering, Feeding, Pruning, Pest Control, Transplant, Harvest)
   - ScheduledDate, CompletedDate
   - AssignedTo (user ID)
   - Status (enum: Pending, InProgress, Completed, Cancelled)
   - Notes
   - AuditableEntity fields
   ```

**Enums**: `PlantStage`, `PlantHealthStatus`, `TaskType`, `TaskStatus`, `GrowCycleStatus`, `QualityGrade`

#### DAL Implementation

**File**: `src/Modules/Cultivation/Project420.Cultivation.DAL/CultivationDbContext.cs`

- DbSets: GrowCycles, GrowRooms, Plants, PlantStages, HarvestBatches, CultivationTasks
- Fluent API relationships
- Soft delete query filters
- SaveChangesAsync override for audit trails

**Repositories**: 6 interfaces + implementations
- IGrowCycleRepository / GrowCycleRepository
- IGrowRoomRepository / GrowRoomRepository
- IPlantRepository / PlantRepository (special methods: GetByPlantTag, GetByStage, GetByGrowRoom)
- IPlantStageRepository / PlantStageRepository
- IHarvestBatchRepository / HarvestBatchRepository
- ICultivationTaskRepository / CultivationTaskRepository

**Migrations**:
```bash
cd src/Modules/Cultivation/Project420.Cultivation.DAL
dotnet ef migrations add InitialCultivationModule
dotnet ef database update
```

---

### Day 2: BLL Implementation

#### Services to Create

**File**: `src/Modules/Cultivation/Project420.Cultivation.BLL/Services/`

1. **IGrowCycleService / GrowCycleService**
   - CRUD operations
   - Business methods: StartCycle, CompleteCycle, CalculateYield

2. **IGrowRoomService / GrowRoomService**
   - CRUD operations
   - Business methods: GetAvailableCapacity, GetCurrentOccupancy

3. **IPlantService / PlantService**
   - CRUD operations
   - Business methods: RegisterPlant, TransitionStage, RecordHarvest, DestroyPlant
   - Compliance: ValidatePlantTag, TrackMotherPlant

4. **IPlantStageService / PlantStageService**
   - Track stage transitions
   - Calculate days in each stage

5. **IHarvestBatchService / HarvestBatchService**
   - CRUD operations
   - Business methods: CreateBatchFromHarvest, RecordDryWeight, LinkLabResults

6. **ICultivationTaskService / CultivationTaskService**
   - CRUD operations
   - Business methods: ScheduleTask, CompleteTask, GetUpcomingTasks

#### DTOs to Create

**File**: `src/Modules/Cultivation/Project420.Cultivation.BLL/DTOs/`

- CreatePlantDto, UpdatePlantDto, PlantDetailsDto
- CreateGrowCycleDto, UpdateGrowCycleDto, GrowCycleDetailsDto
- CreateHarvestBatchDto, HarvestBatchDetailsDto
- CreateCultivationTaskDto, CultivationTaskDetailsDto

#### Validators

**File**: `src/Modules/Cultivation/Project420.Cultivation.BLL/Validators/`

- CreatePlantValidator (plant tag unique, strain required, grow cycle valid)
- CreateGrowCycleValidator (dates valid, grow room capacity)
- CreateHarvestBatchValidator (batch number unique, weights positive, THC/CBD format)
- CreateCultivationTaskValidator (dates valid, task type valid)

#### AutoMapper Profile

**File**: `src/Modules/Cultivation/Project420.Cultivation.BLL/Mappings/CultivationMappingProfile.cs`

---

### Day 3: Basic UI & Testing

#### Blazor Pages

**File**: `src/Modules/Cultivation/Project420.Cultivation.UI.Blazor/Components/Pages/`

1. **Plants/**
   - PlantList.razor - List all plants with filters (stage, grow room, health status)
   - PlantCreate.razor - Register new plant (clone or seed)
   - PlantEdit.razor - Update plant details
   - PlantDetails.razor - View plant history (stage transitions, tasks)

2. **GrowCycles/**
   - GrowCycleList.razor - List all grow cycles
   - GrowCycleCreate.razor - Start new grow cycle
   - GrowCycleDashboard.razor - View cycle progress, plant count, tasks

3. **HarvestBatches/**
   - HarvestBatchList.razor - List all harvest batches
   - HarvestBatchCreate.razor - Create batch from harvest
   - HarvestBatchDetails.razor - View batch details, lab results

#### Unit Tests

**File**: `tests/Project420.Cultivation.Tests/Services/`

- PlantServiceTests.cs (20+ tests)
- GrowCycleServiceTests.cs (15+ tests)
- HarvestBatchServiceTests.cs (10+ tests)

**Target**: 70%+ code coverage

---

## Module 2: Production/Manufacturing Module

**Priority**: ⚠️ CRITICAL
**Duration**: 3 days
**Compliance**: SAHPRA GMP, SARS Yield Tracking

### Day 4: Models & DAL

#### Entities to Create

**File**: `src/Modules/Production/Project420.Production.Models/Entities/`

1. **ProductionBatch.cs**
   ```csharp
   - Id, BatchNumber (unique, SAHPRA)
   - HarvestBatchId (FK - from Cultivation module)
   - ProductionType (enum: Flower, PreRoll, Extract, Edible, Topical)
   - StartDate, EndDate, Status (enum: Planning, InProgress, QC, Complete, Rejected)
   - InputWeight (from harvest), OutputWeight (final packaged)
   - WasteWeight, WasteReason
   - YieldPercentage (calculated)
   - Notes
   - AuditableEntity fields
   ```

2. **ProcessingStep.cs**
   ```csharp
   - Id, ProductionBatchId (FK)
   - StepType (enum: Drying, Curing, Trimming, Grinding, Extraction, Packaging)
   - StartDate, EndDate, Duration (calculated)
   - InputWeight, OutputWeight
   - Status (enum: Pending, InProgress, Complete, Failed)
   - QualityCheckRequired, QualityCheckPassed
   - PerformedBy (user ID)
   - Equipment (optional: machine/tool used)
   - Notes
   - AuditableEntity fields
   ```

3. **QualityControl.cs**
   ```csharp
   - Id, ProductionBatchId (FK), ProcessingStepId (FK - nullable)
   - InspectionDate, InspectorName
   - QCType (enum: Visual, Weight, Moisture, Potency, Contamination)
   - Result (enum: Pass, Fail, Conditional)
   - Findings, CorrectiveAction
   - PassedToNextStage
   - AuditableEntity fields
   ```

4. **PackagingRun.cs**
   ```csharp
   - Id, ProductionBatchId (FK)
   - PackageDate
   - PackageSize (e.g., 1g, 3.5g, 7g, 14g, 28g)
   - UnitsPackaged, TotalWeight
   - PackageType (enum: Jar, Bag, Bottle, Box)
   - LabelBatchNumber, LabelPrintDate
   - ExpiryDate
   - AuditableEntity fields
   ```

5. **LabTest.cs**
   ```csharp
   - Id, ProductionBatchId (FK)
   - LabName, LabAccreditationNumber (ISO/IEC 17025)
   - SampleDate, TestDate, ResultDate
   - TestType (enum: Potency, Contaminants, Terpenes, Moisture)
   - THCPercentage, CBDPercentage, TerpeneProfile (JSON)
   - Pesticides, HeavyMetals, Microbials (JSON)
   - OverallResult (enum: Pass, Fail)
   - COANumber (Certificate of Analysis)
   - COAFileUrl (link to uploaded PDF)
   - Notes
   - AuditableEntity fields
   ```

**Enums**: `ProductionType`, `ProcessingStepType`, `ProductionStatus`, `QCType`, `QCResult`, `PackageType`, `LabTestType`, `LabTestResult`

#### DAL Implementation

- ProductionDbContext with 5 DbSets
- 5 repository interfaces + implementations
- Migration: InitialProductionModule

---

### Day 5: BLL Implementation

#### Services to Create

1. **IProductionBatchService / ProductionBatchService**
   - CreateBatchFromHarvest, AdvanceStep, CompleteProduction, CalculateYield

2. **IProcessingStepService / ProcessingStepService**
   - RecordStep, CompleteStep, CalculateWeightLoss

3. **IQualityControlService / QualityControlService**
   - PerformInspection, RecordResult, ApplyCorrectiveAction

4. **IPackagingService / PackagingService**
   - CreatePackagingRun, GenerateLabels, RecordUnits

5. **ILabTestService / LabTestService**
   - SubmitSample, RecordResults, UploadCOA, ValidatePotency

#### DTOs & Validators

- 5 sets of DTOs (Create, Update, Details)
- 5 validators with compliance checks
- ProductionMappingProfile

---

### Day 6: Basic UI & Testing

#### Blazor Pages

1. **ProductionBatches/**
   - ProductionBatchList.razor
   - ProductionBatchCreate.razor (select harvest batch)
   - ProductionWorkflow.razor (track steps: dry → cure → trim → package)

2. **QualityControl/**
   - QCInspectionList.razor
   - QCInspectionCreate.razor (record inspection results)

3. **LabTests/**
   - LabTestList.razor
   - LabTestCreate.razor (submit sample)
   - LabTestUploadCOA.razor (upload PDF certificate)

#### Unit Tests

- ProductionBatchServiceTests.cs (20+ tests)
- QualityControlServiceTests.cs (15+ tests)
- LabTestServiceTests.cs (10+ tests)

---

## Module 3: Inventory/Stock Management Module

**Priority**: ⚠️ CRITICAL
**Duration**: 4 days
**Compliance**: Cannabis Act Seed-to-Sale, SARS Inventory Tracking

### Day 7-8: Models & DAL

#### Entities to Create

**File**: `src/Modules/Inventory/Project420.Inventory.Models/Entities/`

1. **StockMovement.cs**
   ```csharp
   - Id, TransactionType (enum: Receipt, Issue, Transfer, Adjustment, Sale, Return, Waste)
   - ProductId (FK), ProductSKU, ProductName (denormalized)
   - BatchNumber, LabTestDate
   - Quantity, UnitOfMeasure (enum: Grams, Units, Kilograms)
   - FromLocationId (FK), ToLocationId (FK)
   - MovementDate, ProcessedBy
   - ReferenceNumber (GRN, SalesInvoice, TransferNote, AdjustmentNote)
   - Reason (for adjustments)
   - CostPrice, TotalValue
   - AuditableEntity fields
   ```

2. **StockTransfer.cs**
   ```csharp
   - Id, TransferNumber (unique)
   - FromWarehouseId (FK), ToWarehouseId (FK)
   - TransferDate, ReceivedDate
   - Status (enum: Pending, InTransit, Received, Cancelled)
   - RequestedBy, ApprovedBy, ReceivedBy
   - Notes
   - AuditableEntity fields
   ```

3. **StockTransferDetail.cs**
   ```csharp
   - Id, StockTransferId (FK)
   - ProductId (FK), ProductSKU, BatchNumber
   - QuantityRequested, QuantityReceived
   - Variance, VarianceReason
   - AuditableEntity fields
   ```

4. **StockAdjustment.cs**
   ```csharp
   - Id, AdjustmentNumber (unique)
   - AdjustmentDate, AdjustmentType (enum: Shrinkage, Damage, Theft, Expiry, CountCorrection)
   - WarehouseId (FK), LocationId (FK)
   - ApprovedBy, Reason
   - TotalValue (financial impact)
   - AuditableEntity fields
   ```

5. **StockAdjustmentDetail.cs**
   ```csharp
   - Id, StockAdjustmentId (FK)
   - ProductId (FK), ProductSKU, BatchNumber
   - QuantityBefore, QuantityAfter, Adjustment
   - Reason
   - AuditableEntity fields
   ```

6. **StockCount.cs**
   ```csharp
   - Id, CountNumber (unique)
   - CountDate, CountType (enum: CycleCount, FullCount, SpotCheck)
   - WarehouseId (FK), LocationId (FK - nullable for full counts)
   - Status (enum: Scheduled, InProgress, Complete, Reconciled)
   - CountedBy, ApprovedBy
   - VarianceValue (total value of discrepancies)
   - AuditableEntity fields
   ```

7. **StockCountDetail.cs**
   ```csharp
   - Id, StockCountId (FK)
   - ProductId (FK), ProductSKU, BatchNumber
   - SystemQuantity, CountedQuantity, Variance
   - VarianceReason
   - AuditableEntity fields
   ```

8. **Warehouse.cs**
   ```csharp
   - Id, Code, Name, Description
   - Address, PhoneNumber
   - IsActive, IsPrimary
   - AuditableEntity fields
   ```

9. **Location.cs**
   ```csharp
   - Id, WarehouseId (FK)
   - Code (e.g., A01-01-01), Name, Description
   - Aisle, Bay, Shelf, Bin
   - Capacity, CurrentOccupancy
   - IsActive
   - AuditableEntity fields
   ```

**Enums**: `TransactionType`, `TransferStatus`, `AdjustmentType`, `CountType`, `CountStatus`, `UnitOfMeasure`

#### DAL Implementation

- InventoryDbContext with 9 DbSets
- 9 repository interfaces + implementations
- Migration: InitialInventoryModule
- Complex queries: GetCurrentStock, GetStockByLocation, GetMovementHistory

---

### Day 9: BLL Implementation

#### Services to Create

1. **IStockMovementService / StockMovementService**
   - RecordReceipt, RecordIssue, RecordSale, RecordWaste
   - GetMovementHistory, GetCurrentStock

2. **IStockTransferService / StockTransferService**
   - CreateTransfer, ReceiveTransfer, CancelTransfer
   - Business logic: Update stock levels, validate quantities

3. **IStockAdjustmentService / StockAdjustmentService**
   - CreateAdjustment, ApproveAdjustment
   - Business logic: Validate reason, update stock levels, create audit log

4. **IStockCountService / StockCountService**
   - ScheduleCount, RecordCount, ReconcileCount
   - Business logic: Calculate variances, generate adjustments

5. **IWarehouseService / WarehouseService**
   - CRUD operations, GetStockByWarehouse

6. **ILocationService / LocationService**
   - CRUD operations, GetAvailableLocations, GetLocationCapacity

#### DTOs & Validators

- 9 sets of DTOs
- Validators with cannabis compliance checks (batch tracking, waste reasons)
- InventoryMappingProfile

---

### Day 10: Basic UI & Testing

#### Blazor Pages

1. **StockMovements/**
   - StockMovementList.razor (filter by type, date, product)
   - GoodsReceived.razor (receive stock from cultivation/production)
   - StockIssue.razor (issue stock to retail)

2. **StockTransfers/**
   - StockTransferList.razor
   - StockTransferCreate.razor (create transfer request)
   - StockTransferReceive.razor (receive and reconcile transfer)

3. **StockCounts/**
   - StockCountList.razor
   - StockCountCreate.razor (schedule count)
   - StockCountRecord.razor (enter counted quantities)
   - StockCountReconcile.razor (review variances, create adjustments)

4. **Warehouses/**
   - WarehouseList.razor
   - StockByWarehouse.razor (view current stock levels by warehouse)

#### Unit Tests

- StockMovementServiceTests.cs (25+ tests)
- StockTransferServiceTests.cs (20+ tests)
- StockCountServiceTests.cs (20+ tests)

---

## Module 4: Background Jobs (Hangfire)

**Priority**: 🔧 MEDIUM
**Duration**: 2 days
**Purpose**: Automated Compliance & Reporting

### Day 11: Hangfire Setup

#### Installation

```bash
cd src/Shared
dotnet new classlib -n Project420.Shared.BackgroundJobs
cd Project420.Shared.BackgroundJobs
dotnet add package Hangfire.Core
dotnet add package Hangfire.SqlServer
dotnet add package Hangfire.AspNetCore
```

#### Configuration

**File**: `src/Shared/Project420.Shared.BackgroundJobs/Configuration/HangfireConfiguration.cs`

```csharp
public static class HangfireConfiguration
{
    public static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireDb")));

        services.AddHangfireServer();

        return services;
    }
}
```

#### Jobs to Create

**File**: `src/Shared/Project420.Shared.BackgroundJobs/Jobs/`

1. **DailyStockReconciliationJob.cs**
   - Run daily at 2 AM
   - Compare system stock vs physical counts
   - Generate variance report
   - Alert if variances > threshold

2. **ExpiryDateAlertJob.cs**
   - Run daily at 8 AM
   - Check products expiring in next 30/14/7 days
   - Send email/SMS notifications to managers

3. **MonthlyComplianceReportJob.cs**
   - Run on 1st of month at 6 AM
   - Generate SAHPRA/DALRRD compliance reports
   - Email to compliance officer

4. **AuditLogArchivalJob.cs**
   - Run monthly
   - Archive audit logs older than 1 year to separate table
   - Maintain 7-year retention

5. **LabTestReminderJob.cs**
   - Run daily
   - Check batches pending lab tests > 7 days
   - Send reminders

#### Registration

**File**: `Program.cs`

```csharp
builder.Services.AddHangfireJobs(builder.Configuration);

// Schedule recurring jobs
RecurringJob.AddOrUpdate<DailyStockReconciliationJob>(
    "daily-stock-reconciliation",
    job => job.Execute(),
    Cron.Daily(2)); // 2 AM

RecurringJob.AddOrUpdate<ExpiryDateAlertJob>(
    "expiry-date-alert",
    job => job.Execute(),
    Cron.Daily(8)); // 8 AM
```

#### Hangfire Dashboard

```csharp
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

Access: `https://localhost:5001/hangfire`

---

### Day 12: Job Implementation & Testing

- Implement all 5 jobs
- Write unit tests for job logic
- Test scheduling and execution
- Configure email/SMS notifications

---

## Module 5: Document Management

**Priority**: 🔧 MEDIUM
**Duration**: 1 day
**Purpose**: Store Compliance Documents

### Day 13: Document Storage Implementation

#### Entities

**File**: `src/Shared/Project420.Shared.Documents/Models/`

1. **Document.cs**
   ```csharp
   - Id, FileName, FileExtension, ContentType
   - FileSize, FileUrl (blob storage path)
   - DocumentCategoryId (FK)
   - RelatedEntityType (e.g., "HarvestBatch", "ProductionBatch")
   - RelatedEntityId
   - UploadDate, UploadedBy
   - Description, Tags
   - AuditableEntity fields
   ```

2. **DocumentCategory.cs**
   ```csharp
   - Id, Name, Description
   - Required (boolean: is this category mandatory?)
   - RetentionPeriodYears
   - AuditableEntity fields
   ```

#### File Storage Service

**File**: `src/Shared/Project420.Shared.Documents/Services/`

```csharp
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> DownloadFileAsync(string fileUrl);
    Task DeleteFileAsync(string fileUrl);
}

public class LocalFileStorageService : IFileStorageService
{
    // Implementation for local filesystem
    // Store in: ~/wwwroot/uploads/ or ~/App_Data/uploads/
}

public class AzureBlobStorageService : IFileStorageService
{
    // Implementation for Azure Blob Storage (future)
}
```

#### API Controller

**File**: `src/API/Project420.API.WebApi/Controllers/DocumentsController.cs`

- POST /api/documents - Upload file
- GET /api/documents/{id} - Download file
- DELETE /api/documents/{id} - Delete file
- GET /api/documents/category/{categoryId} - List by category

#### Blazor UI

- DocumentUpload.razor - Upload component
- DocumentList.razor - List documents by entity
- DocumentViewer.razor - View/download documents

#### Seed Document Categories

```
- Lab Test Certificates (COA)
- Cannabis Licenses (SAHPRA, DALRRD)
- Supplier Licenses
- Employee Training Certificates
- Medical Prescriptions (Section 21)
```

---

## Module 6: Notification System

**Priority**: 🔧 MEDIUM
**Duration**: 1 day
**Purpose**: Automated Notifications

### Day 14: Email Notification Implementation

#### Email Service

**File**: `src/Shared/Project420.Shared.Notifications/Services/`

```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendTemplatedEmailAsync(string to, string templateName, object model);
}

public class SmtpEmailService : IEmailService
{
    // Implementation using System.Net.Mail.SmtpClient
}
```

#### Configuration

**appsettings.json**:
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@project420.com",
    "FromName": "Project420 System"
  }
}
```

#### Email Templates

**File**: `src/Shared/Project420.Shared.Notifications/Templates/`

- OrderConfirmation.cshtml
- OrderReadyForPickup.cshtml
- LowStockAlert.cshtml
- ExpiryWarning.cshtml
- LabTestReminder.cshtml

#### Integration

- Call from background jobs
- Call from services (e.g., when order placed)
- Queue-based (future: use RabbitMQ or Azure Service Bus)

---

## Week 3-4: Integration & Testing

### Day 15-17: Module Integration

1. **Verify Module Communication**
   - Cultivation → Production (harvest batch handoff)
   - Production → Inventory (finished goods receipt)
   - Inventory → POS (stock availability)

2. **Test Seed-to-Sale Flow**
   - Register plant → Grow → Harvest → Process → Package → Receive into inventory → Sell

3. **Compliance Verification**
   - SAHPRA: Batch tracking complete?
   - DALRRD: Hemp permit tracking?
   - Cannabis Act: Age verification enforced?
   - POPIA: Audit trails working?
   - SARS: VAT calculations accurate?

4. **Shared Service Integration**
   - VAT calculations used in all transactions
   - Audit logs created for all data changes
   - Transaction numbers generated consistently

### Day 18-20: Testing & Documentation

1. **Integration Tests**
   - End-to-end workflows
   - Multi-module scenarios
   - Database transaction testing

2. **UI/UX Refinement**
   - Navigation improvements
   - Consistent styling (cannabis theme)
   - Form validation improvements
   - Loading states and error handling

3. **Documentation Updates**
   - Update PROJECT-STATUS.md
   - Create module-specific architecture docs
   - Update README.md
   - API documentation (Swagger)

4. **Performance Testing**
   - Load testing with large datasets
   - Query optimization
   - Indexing review

---

## Phase 5 Success Criteria (Checklist)

### Cultivation Module
- [ ] Plants can be registered and tracked
- [ ] Growth stages transition correctly
- [ ] Harvest batches created from plants
- [ ] Mother plant lineage tracked
- [ ] Grow cycles managed end-to-end

### Production Module
- [ ] Production batches created from harvest
- [ ] Processing steps tracked (dry, cure, trim, package)
- [ ] Quality control inspections recorded
- [ ] Lab tests submitted and COA uploaded
- [ ] Yield percentages calculated

### Inventory Module
- [ ] Stock movements recorded (receipt, issue, transfer)
- [ ] Stock transfers between warehouses
- [ ] Stock adjustments for shrinkage/damage
- [ ] Stock counts performed and reconciled
- [ ] Current stock levels accurate

### Background Jobs
- [ ] Daily stock reconciliation running
- [ ] Expiry date alerts sending
- [ ] Compliance reports generating
- [ ] Hangfire dashboard accessible

### Document Management
- [ ] Documents uploaded successfully
- [ ] COA files stored and retrievable
- [ ] Document categories managed

### Notification System
- [ ] Email notifications sending
- [ ] Low stock alerts working
- [ ] Expiry warnings delivered

### Integration
- [ ] Full seed-to-sale traceability demonstrated
- [ ] All modules use Shared services (VAT, Audit, Transaction Numbers)
- [ ] SAHPRA GMP compliance verified
- [ ] DALRRD compliance verified
- [ ] POPIA compliance verified

### Code Quality
- [ ] All modules build with 0 errors, 0 warnings
- [ ] 70%+ unit test coverage across all modules
- [ ] All tests passing
- [ ] CODING-STANDARDS.md followed
- [ ] Documentation complete

---

## Dependency Injection Registration

**Add to Program.cs**:

```csharp
// ========================================
// CULTIVATION MODULE
// ========================================
builder.Services.AddDbContext<CultivationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CultivationDb")));

builder.Services.AddScoped<IGrowCycleRepository, GrowCycleRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<IHarvestBatchRepository, HarvestBatchRepository>();

builder.Services.AddScoped<IGrowCycleService, GrowCycleService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IHarvestBatchService, HarvestBatchService>();

// ========================================
// PRODUCTION MODULE
// ========================================
builder.Services.AddDbContext<ProductionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductionDb")));

builder.Services.AddScoped<IProductionBatchRepository, ProductionBatchRepository>();
builder.Services.AddScoped<ILabTestRepository, LabTestRepository>();

builder.Services.AddScoped<IProductionBatchService, ProductionBatchService>();
builder.Services.AddScoped<ILabTestService, LabTestService>();

// ========================================
// INVENTORY MODULE
// ========================================
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDb")));

builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

// ========================================
// BACKGROUND JOBS (HANGFIRE)
// ========================================
builder.Services.AddHangfireJobs(builder.Configuration);

// ========================================
// DOCUMENT MANAGEMENT
// ========================================
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// ========================================
// NOTIFICATIONS
// ========================================
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ========================================
// VALIDATORS (ALL MODULES)
// ========================================
builder.Services.AddValidatorsFromAssemblyContaining<CreatePlantValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductionBatchValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStockMovementValidator>();

// ========================================
// AUTOMAPPER (ALL PROFILES)
// ========================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
```

---

## Risk Management

### Potential Risks

1. **Scope Creep**: Too many features added beyond MVP
   - **Mitigation**: Strict adherence to Phase 5 plan, defer advanced features to Phase 6

2. **Integration Complexity**: Modules don't integrate smoothly
   - **Mitigation**: Follow MODULE-TEMPLATE.md pattern consistently, use Shared services

3. **Performance Issues**: Large datasets slow down queries
   - **Mitigation**: Database indexing, query optimization, pagination

4. **Compliance Gaps**: Missing regulatory requirements
   - **Mitigation**: Regular review of SA_Cannabis_Software_Guide.md and cultivation laws guide

5. **Testing Time**: Insufficient time for proper testing
   - **Mitigation**: Write tests alongside development, target 70% coverage minimum

---

## Next Steps After Phase 5

### Phase 6: Reporting & Analytics
- Compliance reports (SAHPRA, DALRRD, SARS)
- Sales analytics dashboard
- Inventory analytics
- Yield analysis reports

### Phase 7: MAUI Mobile App
- Mobile barcode scanning
- Mobile stock counts
- Offline mode support
- Field cultivation tracking

### Phase 8: Advanced Features
- Multi-tenant support
- Advanced RBAC with granular permissions
- IoT sensor integration (grow rooms)
- Machine learning (yield prediction)

---

## Reference Documents

- **Compliance**: SA_Cannabis_Software_Guide.md, south-africa-cannabis-cultivation-production-laws-guide.md
- **Coding Standards**: CODING-STANDARDS.md
- **Module Template**: MODULE-TEMPLATE.md
- **Project Status**: PROJECT-STATUS.md
- **Main Config**: CLAUDE.md

---

**PHASE 5 PLAN**: ✅ COMPLETE AND READY TO EXECUTE
**ESTIMATED DURATION**: 10-14 days
**COMPLIANCE**: SAHPRA + DALRRD + POPIA + Cannabis Act 2024
**DELIVERABLE**: Production-ready seed-to-sale cannabis management system
