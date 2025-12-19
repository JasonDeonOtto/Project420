# Project420 - Cannabis Management System
## Project Status & Work Tracking

**Project Name**: Project420
**Type**: Cannabis Industry Management System (Proof of Concept)
**Target Market**: South African Cannabis Industry
**Created**: 2025-12-01
**Last Updated**: 2025-12-19
**Status**: 🟢 Phase 7C Complete | PoC Hostile Demo Tests COMPLETE | Credibility Criteria MET ✅

---

## 🎯 Project Vision

Create cost-effective, scalable, secure, and user-friendly software that aligns with South African cannabis regulations, supporting the full lifecycle from Plantation → Plant Production → Retail.

### Key Principles
- ✅ **Data-First Approach**: EF Core with database-first design
- ✅ **3-Tier Architecture**: Presentation → Business Logic → Data Access
- ✅ **Modular Design**: Each module as independent executable
- ✅ **Modern Stack**: Blazor + MAUI for UI, SQL/PostgreSQL/SQLite for data
- ✅ **Standards Compliant**: Following all coding standards and best practices

---

## 📊 Current Phase: Phase 7C - PoC Hostile Demo Tests COMPLETE ✅

### 🎯 PoC Credibility Status

**PoC Hostile Demo Tests (Phase 7C)** - COMPLETE ✅ (2025-12-19)
- All 11 hostile demo tests passing
- 7/7 Core Hostile Demos: PASS ✅
- 3/3 Secondary Demos: PASS ✅
- Bonus Audit Trail Demo: PASS ✅
- Tests location: `tests/Project420.Shared.Tests/Proof/HostileDemoTests.cs`

**Movement Architecture (Phase 7A-7B)** - COMPLETE ✅
- Option A: SOH = SUM(IN) - SUM(OUT) from movement ledger
- MovementService: 51 unit tests passing
- All transaction types integrated

**Batch & Serial Number System (Phase 8)** - 80% Complete
- BatchNumberGeneratorService: Implemented ✅
- SerialNumberGeneratorService: Implemented ✅
- POS scanning integration: Pending

### ✅ Completed Work

#### Project Setup (2025-12-01)
- [x] Created solution structure with 3-tier architecture
- [x] Set up folder hierarchy for modular design
- [x] Created Shared libraries (Core, Infrastructure, Database)
- [x] Initialized Management module projects
- [x] Initialized Retail.POS module projects
- [x] Set up test projects (xUnit)
- [x] Created documentation structure

#### Phase 1: Data Models - COMPLETE (2025-12-02) ✅

**Foundation Entities:**
- [x] Created `AuditableEntity` base class in Shared.Core
  - POPIA compliance (R10M penalty protection)
  - Audit trails (CreatedBy, CreatedAt, ModifiedBy, ModifiedAt)
  - Soft delete capability (IsDeleted, DeletedAt, DeletedBy)
  - 7-year retention compliance

**Core Entities (Retail.POS.Models):**
- [x] `Product` entity - Cannabis compliance complete
  - THC/CBD percentage tracking
  - Batch/lot number for seed-to-sale traceability
  - Strain name tracking
  - Lab test date tracking
  - Expiry date management
  - Pricing (selling + cost)
  - Inventory (stock on hand, reorder level)
  - Cannabis-specific fields for SAHPRA reporting

- [x] `Debtor` (Customer) entity - POPIA & age verification complete
  - Age verification fields (DateOfBirth, AgeVerified, AgeVerificationDate)
  - Medical cannabis license tracking (Section 21 permits)
  - PII protection notes (Email, Phone, Address, IdNumber)
  - Credit management (CreditLimit, CurrentBalance, PaymentTerms)
  - Individual vs Business customer types
  - Customer account numbers

- [x] `Pricelist` entity - Flexible pricing system
  - Multiple pricing strategies support
  - Date-based activation (EffectiveFrom, EffectiveTo)
  - Priority system for overlapping pricelists
  - Default pricelist designation
  - Percentage-based or fixed pricing
  - Support for VIP, medical, wholesale, promotional pricing

- [x] `PricelistItem` entity - Many-to-many join table
  - Links Products to Pricelists with specific prices
  - Tiered pricing support (MinimumQuantity, MaximumQuantity)
  - Historical price tracking via AuditableEntity

- [x] `TransactionHeader` entity - Invoice/receipt
  - Transaction types via enum (Sale, Refund, AccountPayment, Layby, Quote)
  - Transaction status via enum (Pending, Completed, Cancelled, OnHold, Refunded)
  - Financial totals (Subtotal, TaxAmount, TotalAmount, DiscountAmount)
  - Optional customer linking (walk-in vs account)
  - Pricelist tracking (which prices were used)
  - Refund tracking (OriginalTransactionId)
  - Cashier/processor tracking

- [x] `TransactionDetail` entity - Invoice line items
  - Denormalized product info (ProductSKU, ProductName, BatchNumber)
  - Historical pricing (UnitPrice at time of sale)
  - Quantity tracking (positive for sales, negative for refunds)
  - Line-level discounts
  - Cost price tracking for profit analysis
  - Cannabis batch tracking per line item

- [x] `Payment` entity - HOW customers pay (separate from Transaction)
  - Multiple payment methods via enum (Cash, Card, EFT, MobilePayment, OnAccount, Voucher)
  - Split payment support (multiple payments per transaction)
  - Account payments without transactions
  - Payment success/failure tracking
  - PCI-DSS compliance (masked card numbers only)
  - FIC Act compliance (cash > R25,000 reporting)
  - External reference tracking for reconciliation

**Enumerations (Type Safety):**
- [x] `TransactionStatus` enum - Pending, Completed, Cancelled, OnHold, Refunded
- [x] `TransactionType` enum - Sale, Refund, AccountPayment, Layby, Quote
- [x] `PaymentMethod` enum - Cash, Card, EFT, MobilePayment, OnAccount, Voucher

**Best Practices Applied:**
- ✅ All entities inherit from AuditableEntity (POPIA compliance)
- ✅ Comprehensive XML documentation on all entities and properties
- ✅ Data annotations for validation and database configuration
- ✅ Proper navigation properties for EF Core relationships
- ✅ Cannabis compliance fields throughout (THC/CBD, batch, age verification)
- ✅ Security considerations (PII encryption notes, PCI-DSS compliance)
- ✅ Denormalization for historical accuracy (prices, product names)
- ✅ Null safety with nullable reference types
- ✅ Default values to prevent null reference errors
- ✅ Clear separation of concerns (Transaction vs Payment)

**Build Status:**
- ✅ Solution builds successfully with 0 warnings, 0 errors
- ✅ All project references correctly configured
- ✅ Ready for EF Core DbContext and migrations

#### Projects Created
**Shared Layer:**
- Project420.Shared.Core
- Project420.Shared.Infrastructure
- Project420.Shared.Database

**Management Module:**
- Project420.Management.Models
- Project420.Management.DAL
- Project420.Management.BLL
- Project420.Management.UI.Blazor
- Project420.Management.UI.Maui (placeholder)

**Retail.POS Module:**
- Project420.Retail.POS.Models
- Project420.Retail.POS.DAL
- Project420.Retail.POS.BLL
- Project420.Retail.POS.UI.Blazor
- Project420.Retail.POS.UI.Maui (placeholder)

**Testing:**
- Project420.Management.Tests
- Project420.Retail.POS.Tests

---

## 🔄 Next Steps (Prioritized)

### Phase 1: Core Data Models - ✅ COMPLETE

### Phase 2: DbContext & Database Setup (CURRENT - 80% COMPLETE)

**Last Updated**: 2025-12-04
**Status**: 🟡 In Progress - Ready for Migrations

#### ✅ COMPLETED (Steps 1-4):

**Step 1: Install EF Core Packages** ✅
- [x] Microsoft.EntityFrameworkCore 9.0.0
- [x] Microsoft.EntityFrameworkCore.SqlServer 9.0.0
- [x] Microsoft.EntityFrameworkCore.Tools 10.0.0
- [x] Microsoft.EntityFrameworkCore.Design 9.0.0
- [x] Installed in: Retail.POS.DAL, Management.DAL, Shared.Database

**Step 2: Create DbContexts** ✅
- [x] `PosDbContext` (Retail.POS.DAL) - 370 lines
  - Operational POS data (Transactions, Payments, Details)
  - 7 DbSets configured
  - Fluent API relationships, indexes, soft delete filters
  - SaveChangesAsync override for audit trails

- [x] `ManagementDbContext` (Management.DAL) - 280 lines
  - Master data (Debtors, Products, Pricelists, UserProfiles)
  - 4 DbSets configured
  - Enterprise-grade indexes and relationships
  - POPIA compliance (soft deletes, audit trails)

- [x] `SharedDbContext` (Shared.Database) - 150 lines
  - Gatekeeping infrastructure (ErrorLogs, AuditLogs)
  - 2 DbSets configured
  - Centralized error/audit tracking across all modules
  - Immutable logs (compliance requirement)

**Step 2b: Error Handling Entities** ✅
- [x] `ErrorLog` entity (Shared.Core) - 170 lines
  - Centralized error tracking with severity levels
  - Resolution workflow tracking
  - POPIA-compliant (no PII in logs)

- [x] `AuditLog` entity (Shared.Core) - 150 lines
  - Compliance audit trail (POPIA, Cannabis Act, SARS)
  - Before/after value tracking
  - Security event logging
  - 7-year immutable retention

**Step 3: Connection Strings** ✅
- [x] Created appsettings.json with connection strings
- [x] Created appsettings.Development.json
- [x] Configured SQL Server Authentication
  - Server: JASON\SQLDEVED
  - User: sa
  - Two databases configured:
    - **Project420_Dev** (BusinessConnection) - POS + Management
    - **Project420_Shared** (SharedConnection) - Gatekeeping/Infrastructure

**Step 4: Register DbContexts with DI** ✅
- [x] Updated Program.cs in Retail.POS.UI.Blazor
- [x] Registered all 3 DbContexts with dependency injection
- [x] Connected to appsettings.json connection strings
- [x] Solution builds: 0 Errors, 0 Warnings

**Security** ✅
- [x] Created .gitignore to protect passwords
- [x] Excluded appsettings.Development.json from source control

#### ✅ COMPLETED (Steps 5-6):

**Step 5: Generate Migrations** ✅
- [x] Generate PosDbContext migration
- [x] Generate ManagementDbContext migration
- [x] Generate SharedDbContext migration
- [x] Migration files created: 20251204 InitialCreate for all 3 contexts

**Step 6: Apply Migrations (Create Databases)** ✅
- [x] Apply PosDbContext migration → Created tables in Project420_Dev
- [x] Apply ManagementDbContext migration → Created tables in Project420_Dev
- [x] Apply SharedDbContext migration → Created Project420_Shared database
- [x] Verified tables created in SQL Server

#### 🔄 IN PROGRESS (Steps 7-8):

**Step 7: Create Seed Data** ✅ **COMPLETE**
- [x] Created DatabaseSeeder class (Shared.Database)
- [x] Sample products defined (10 cannabis products: 6 strains, 1 pre-roll, 3 accessories)
- [x] Default pricelist defined ("Standard Retail" with prices)
- [x] Test customers defined (4 debtors: walk-in, regular, medical, business)
- [x] Test user profile defined (admin user) - Deferred: Requires User table in Shared DB
- [x] **SEEDING STRATEGY DOCUMENTED**: All seeding happens through ManagementDbContext only
  - Management and POS share the same database tables (Project420_Dev)
  - Seed once through Management → POS automatically sees the data
  - No duplicate seeding needed for POS
- [x] Run seeder to populate databases
- [x] Verify seed data in SQL Server - ✅ 10 products, 4 customers, 1 pricelist created

**Step 8: Test & Verify**
- [ ] Query products from database
- [ ] Create a customer record
- [ ] Test soft delete functionality
- [ ] Verify audit fields auto-populate
- [ ] Test CRUD operations

#### ✅ COMPLETED (Phase 2.5: BLL Reorganization):

**Architecture Refinement** ✅ **COMPLETE** (2025-12-05)
- [x] Reorganized Management.BLL from flat structure to domain-driven organization
- [x] Created domain-based folders matching DAL/Models structure:
  - `Sales/Retail/` - Pricelist services, DTOs, validators (12 files)
  - `Sales/SalesCommon/` - Customer services, DTOs, validators (3 files)
  - `StockManagement/` - Product services, DTOs, validators (7 files)
- [x] Updated all namespaces to reflect new structure:
  - `Project420.Management.BLL.Sales.Retail.*`
  - `Project420.Management.BLL.Sales.SalesCommon.*`
  - `Project420.Management.BLL.StockManagement.*`
- [x] Updated UI.Blazor Program.cs with correct namespace references
- [x] Verified solution builds: 0 Errors, 0 Warnings
- [x] Updated documentation (FOLDER-STRUCTURE-EXPLAINED.md)

**Benefits of Domain-Driven BLL Structure:**
- ✅ **Consistency**: BLL now matches DAL and Models organization
- ✅ **Maintainability**: Related files grouped by business domain
- ✅ **Scalability**: Easy to add new domains (Wholesale, Inventory, etc.)
- ✅ **Clarity**: Clear separation between Retail, Sales, and Stock management
- ✅ **Professional**: Enterprise-grade organization pattern

#### ✅ COMPLETED (Phase 3 Part A: Shared Services & BLL Gap Closure):

**Shared Services Implementation** ✅ **COMPLETE** (2025-12-06)

**Last Updated**: 2025-12-06
**Status**: 🟢 Complete - All core services implemented and registered

**1. VATCalculationService** ✅ **CRITICAL - FULLY TESTED**
- [x] Created `VATBreakdown.cs` DTO with validation
- [x] Created `IVATCalculationService.cs` interface (12 methods)
- [x] Implemented `VATCalculationService.cs` (287 lines)
- [x] **Universal transaction logic** - Used by POS, GRV, RTS, Invoices, Credits, Stock Adjustments
- [x] **SA VAT compliance** - 15% VAT rate with VAT-inclusive pricing
- [x] **Detail-first calculation** - Build details, aggregate to header
- [x] **Automatic rounding adjustment** - Handles rounding variance (< 1 cent tolerance)
- [x] **Discount support** - Percentage and fixed amount discounts
- [x] **Comprehensive tests** - 44 unit tests written and passing ✅
- [x] **Test coverage**: 100% of VATCalculationService methods

**Key Features:**
```csharp
// Line item calculation
var breakdown = _vatService.CalculateLineItem(unitPriceInclVAT: 10.00m, quantity: 1);
// Result: Subtotal=R8.70, VAT=R1.30, Total=R10.00

// Header aggregation
var headerTotals = _vatService.CalculateHeaderTotals(lineItems);
// Handles 100+ line items with accurate rounding
```

**2. TransactionNumberGeneratorService** ✅ **COMPLETE**
- [x] Created `TransactionTypeCode.cs` enum (10 transaction types)
- [x] Created `ITransactionNumberGeneratorService.cs` interface (5 methods)
- [x] Implemented `TransactionNumberGeneratorService.cs` (220 lines)
- [x] **Auto-generates unique numbers** - Format: `TYPE-YYYYMMDD-XXX`
- [x] **Thread-safe** - ConcurrentDictionary + SemaphoreSlim
- [x] **Daily sequence reset** - Sequences reset per type per day
- [x] **Transaction types supported**:
  - SALE (POS Sales)
  - GRV (Goods Received Notes)
  - RTS (Return to Supplier)
  - INV (Sales Invoices)
  - CRN (Credit Notes)
  - ADJ (Stock Adjustments)
  - PAY (Payments)
  - QTE (Quotations)
  - LAY (Layby)
  - TRF (Stock Transfers)

**Example Usage:**
```csharp
var saleNumber = await _transactionNumberService.GenerateAsync(TransactionTypeCode.SALE);
// Result: "SALE-20251206-001"

var grvNumber = await _transactionNumberService.GenerateAsync(TransactionTypeCode.GRV);
// Result: "GRV-20251206-001"
```

**3. AuditLogService** ✅ **INTERFACE COMPLETE**
- [x] Created `AuditLogDto.cs`
- [x] Created `IAuditLogService.cs` interface (10 methods)
- [x] Implemented `AuditLogService.cs` (140 lines - placeholder)
- [x] **POPIA compliance** - 7-year audit trail tracking
- [x] **Cannabis Act compliance** - Seed-to-sale traceability
- [x] **SARS compliance** - Financial transaction tracking
- [x] **Security event logging** - Login/logout, failed attempts
- [x] **Data change tracking** - Before/after values
- [x] **Query capabilities** - By entity, user, severity, date range

**Audit Methods:**
```csharp
// General audit log
await _auditService.LogAsync(auditDto);

// Data change with before/after
await _auditService.LogDataChangeAsync(
    entityType: "Product",
    entityId: "123",
    action: "PriceChanged",
    oldValue: "R250.00",
    newValue: "R300.00"
);

// Security event
await _auditService.LogSecurityEventAsync(
    action: "Login",
    userId: 1,
    username: "admin",
    ipAddress: "192.168.1.100",
    success: true
);

// Business transaction
await _auditService.LogTransactionAsync(
    transactionType: "Sale",
    transactionId: "SALE-20251206-001",
    amount: 500.00m,
    userId: 1,
    username: "cashier01"
);
```

**Note**: Database persistence pending SharedDbContext repository implementation.

**4. BLL Gap Closure** ✅ **VERIFIED COMPLETE**
- [x] **ProductCategoryService** - Verified existing implementation
  - CRUD operations, search, filter, validation
  - Product count aggregation
  - FluentValidation integration
  - Status: 100% Complete

- [x] **DebtorCategoryService** - Verified existing implementation
  - Customer segmentation
  - CRUD operations, search, filter
  - Cannabis Act compliance (medical permits)
  - POPIA compliance
  - Status: 100% Complete

**5. Dependency Injection Registration** ✅ **COMPLETE**
- [x] Registered `IVATCalculationService` (Scoped)
- [x] Registered `ITransactionNumberGeneratorService` (Singleton)
- [x] Registered `IAuditLogService` (Scoped)
- [x] Added project reference to Shared.Infrastructure in POS UI
- [x] Verified build: 0 Errors, 0 Warnings

**Build Status:**
- ✅ Solution builds successfully: 0 Errors, 0 Warnings
- ✅ All new services registered and accessible
- ✅ Test suite runs successfully: 44/44 VATCalculationService tests passing
- ✅ Ready for Phase 3 Part B (Testing & QA)

**Statistics:**
- **Total Services Created/Verified**: 6
- **New Services Built**: 3 (VAT, TransactionNumber, AuditLog)
- **Existing Services Verified**: 3 (ProductCategory, DebtorCategory, DI)
- **Files Created**: 9 new files
- **Lines of Code**: ~1,500+ lines
- **Unit Tests Written**: 44 tests (all passing)
- **Test Coverage**: 100% for VATCalculationService

**Impact:**
- ✅ **Universal VAT logic** ready for ALL transaction types
- ✅ **Consistent transaction numbering** across all modules
- ✅ **Audit framework** for POPIA/Cannabis Act/SARS compliance
- ✅ **Category services** ready for product and customer organization
- ✅ **All services DI-registered** and ready for UI development

### ✅ Phase 3 Part B: Testing & Quality Assurance - COMPLETE (2025-12-06)
- [x] Created test infrastructure (ServiceTestBase, RepositoryTestBase, TestDbContextFactory)
- [x] Wrote 224 unit tests (100% pass rate)
- [x] Achieved excellent code coverage:
  - CustomerService: 100% coverage (20 tests)
  - VATCalculationService: 87.6% coverage (44 tests)
  - PricelistService: 60.1% coverage (20+ tests)
  - ProductService: 54.8% coverage (11 tests)
- [x] Implemented 133 validator tests
- [x] Generated code coverage reports
- [x] Verified compliance scenarios (age verification, POPIA, VAT)

### ✅ Phase 4: Retail Vertical Slice - COMPLETE (2025-12-06)
- [x] Created TransactionRepository (DAL layer)
- [x] Created TransactionService (BLL layer)
- [x] Implemented DTOs (CartItemDto, CheckoutRequestDto, CheckoutResultDto)
- [x] Built POSCheckout.razor (UI layer)
- [x] Registered all services in DI
- [x] Verified full DAL → BLL → UI vertical slice
- [x] Cannabis compliance features (age verification, batch tracking)
- [x] SA VAT compliance (15% calculation)
- [x] Payment methods support
- [x] Transaction numbering system
- [x] Professional receipt display

**Statistics:**
- **Files Created**: 8 new files
- **Lines of Code**: ~1,800 lines
- **Methods Implemented**: 11 repository methods, 3 service methods
- **Pattern**: Complete vertical slice demonstrates full stack

### ✅ Phase 5: MVP Production Modules Complete (2025-12-08)
- [x] **Cultivation Module** - Plant tracking, grow cycles, harvest batches (17 entities)
- [x] **Production Module** - Processing workflow, QC, lab testing
- [x] **Inventory Module** - Stock movements, transfers, adjustments, counts
- [x] **3 DbContexts Created** - Cultivation, Production, Inventory
- [x] **Build Status**: 0 Errors

### ✅ Phase 6: Validators & Database Migrations Complete (2025-12-08)
- [x] **FluentValidation** - 32 validators across all modules
- [x] **Database Migrations** - All 3 module DbContexts migrated
- [x] **12 Database Tables** - Across Cultivation, Production, Inventory

### ✅ Phase 7A: Movement Architecture Foundation Complete (2025-12-12)
- [x] **TransactionType enum** - All 16 transaction types
- [x] **Movement entity** - Option A architecture (SOH from movements)
- [x] **MovementService** - 13 methods, 51 unit tests passing
- [x] **SOH Calculation**: SUM(IN) - SUM(OUT) from movement ledger

### ✅ Phase 7C: PoC Hostile Demo Tests Complete (2025-12-19)
- [x] **11 Hostile Demo Tests** - All passing
- [x] **PoC Credibility Criteria MET**
- [x] **Tests Prove**:
  - Movement immutability (no update/delete)
  - Compensating movements for corrections
  - Historical SOH reconstruction
  - Full traceability (WHO/WHAT/WHEN/WHY)
  - Retail cannot mutate stock directly
  - No silent corrections

---

## 🚀 Phase 8: Batch & Serial Number System - 80% Complete

**Goal**: Implement essential modules for a production-ready cannabis cultivation and retail system.

**Duration Estimate**: 10-14 days of scaffolding + implementation
**Target**: Create fully functional cultivation, production, and inventory tracking

### Module 1: Cultivation Module ⚠️ HIGH PRIORITY

**Purpose**: Track cannabis plant growth from seed to harvest (SAHPRA/DALRRD compliance)

**Entities**:
- GrowCycle - Overall cultivation cycle management
- GrowRoom - Physical growing locations
- Plant - Individual plant tracking (Cannabis Act requirement)
- PlantStage - Growth stage transitions (seed → veg → flower → harvest)
- HarvestBatch - Batch tracking from harvest
- CultivationTask - Scheduled tasks and maintenance

**Key Features**:
```
- Individual plant tagging/barcoding (SAHPRA GMP)
- Growth stage tracking (seed → vegetative → flowering → harvest)
- Mother plant management and genetic lineage
- Environmental condition logging (temp, humidity, light)
- Nutrient/feeding schedules
- Pest/disease tracking
- Harvest weight recording
- Compliance: Track EVERY plant from seed to sale
```

**Implementation Pattern**: Follow MODULE-TEMPLATE.md
- Models: 6 entities + enums
- DAL: CultivationDbContext + 6 repositories
- BLL: 6 services + DTOs + validators
- UI: Blazor pages for plant tracking
- API: CultivationController (optional)

**Compliance**: SAHPRA Section 22C, DALRRD hemp permits

---

### Module 2: Production/Manufacturing Module ⚠️ HIGH PRIORITY

**Purpose**: Bridge cultivation → retail with processing and quality control

**Entities**:
- ProductionBatch - Processing batch management
- ProcessingStep - Workflow step tracking (drying, curing, trimming, packaging)
- QualityControl - QC checkpoints and inspections
- PackagingRun - Packaging operations
- LabTest - Laboratory testing integration

**Key Features**:
```
- Production workflow: harvest → dry → cure → trim → package
- Batch tracking: harvest batch → production batch → retail SKU
- Weight reconciliation: wet weight → dry weight → packaged weight
- Quality control checkpoints
- Lab testing integration (THC/CBD COA management)
- Packaging and labeling
- Waste tracking (stems, trim, rejected product)
```

**Implementation Pattern**: Follow MODULE-TEMPLATE.md
- Models: 5 entities + enums
- DAL: ProductionDbContext + 5 repositories
- BLL: 5 services + DTOs + validators
- UI: Blazor pages for production workflow
- API: ProductionController

**Compliance**: SAHPRA GMP, SARS yield rates, waste tracking

---

### Module 3: Inventory/Stock Management Module ⚠️ HIGH PRIORITY

**Purpose**: Comprehensive stock movement tracking across all locations

**Entities**:
- StockMovement - All stock ins/outs
- StockTransfer - Location-to-location transfers
- StockAdjustment - Adjustments (shrinkage, damage, theft)
- StockCount - Cycle counting and physical inventory
- Warehouse - Storage locations
- Location - Specific storage bins/areas

**Key Features**:
```
- Goods Received (from cultivation/suppliers)
- Stock Transfers (warehouse → store)
- Stock Adjustments (shrinkage, damage, theft)
- Stock Counts (cycle counting, year-end)
- Batch/serial number tracking
- FIFO/FEFO enforcement (First Expired First Out)
- Multi-location stock visibility
- Track EVERY gram from growth to sale
```

**Implementation Pattern**: Follow MODULE-TEMPLATE.md
- Models: 6 entities + enums
- DAL: InventoryDbContext + 6 repositories
- BLL: 6 services + DTOs + validators
- UI: Blazor pages for stock management
- API: InventoryController

**Compliance**: Cannabis Act seed-to-sale, SARS inventory tracking

---

### Module 4: Background Jobs / Task Scheduler 🔧 MEDIUM PRIORITY

**Purpose**: Automated compliance, reporting, and data cleanup

**Technology**: Hangfire or Quartz.NET

**Scheduled Jobs**:
- DailyStockReconciliationJob - Verify stock levels daily
- ExpiryDateAlertJob - Alert on expiring products
- MonthlyComplianceReportJob - Generate regulatory reports
- AuditLogArchivalJob - Archive old audit logs
- LabTestReminderJob - Remind of pending lab tests

**Implementation**:
- Install Hangfire NuGet package
- Create Jobs/ folder structure
- Configure Hangfire dashboard
- Schedule recurring jobs
- Integration with email notifications

---

### Module 5: Document Management / File Storage 🔧 MEDIUM PRIORITY

**Purpose**: Store lab test certificates, licenses, compliance documents

**Entities**:
- Document - Document metadata
- DocumentCategory - Document types
- DocumentMetadata - Additional metadata

**Key Features**:
```
- Lab test certificates (COA - Certificate of Analysis)
- Cannabis licenses (cultivation, processing, retail)
- Supplier licenses
- Employee training certificates
- Medical cannabis prescriptions (Section 21 permits)
- File storage (Azure Blob or local filesystem)
```

**Implementation Pattern**: Follow MODULE-TEMPLATE.md
- Models: 3 entities
- Service: IFileStorageService (Azure Blob / local)
- API: DocumentsController (upload/download)
- UI: Document management pages

---

### Module 6: Notification System 🔧 MEDIUM PRIORITY

**Purpose**: Order confirmations, compliance alerts, low stock warnings

**Services**:
- IEmailService - SMTP email notifications
- ISmsService - SMS notifications (Twilio)
- INotificationService - Unified notification interface

**Notification Templates**:
- OrderConfirmationEmail
- OrderReadyForPickupEmail
- LowStockAlert
- ExpiryDateWarning
- LabTestDueReminder
- LicenseExpiryNotification

**Implementation**:
- Create Shared.Notifications project
- Implement email service (SMTP)
- Create Razor templates for emails
- Configuration in appsettings.json

---

### Phase 5 Implementation Order

**Week 1-2: Essential Modules**
1. Day 1-3: Cultivation Module (models, DAL, BLL, basic UI)
2. Day 4-6: Production Module (models, DAL, BLL, basic UI)
3. Day 7-10: Inventory Module (models, DAL, BLL, basic UI)

**Week 3: Infrastructure**
4. Day 11-12: Background Jobs (Hangfire setup + core jobs)
5. Day 13: Document Management (basic file storage)
6. Day 14: Notification System (email notifications)

**Week 3-4: Integration & Testing**
7. Integration testing across modules
8. Compliance verification
9. UI refinement
10. Documentation updates

---

### Phase 5 Success Criteria

- [ ] Plant tracking from seed to harvest
- [ ] Production batch tracking operational
- [ ] Stock movements fully tracked
- [ ] Background jobs running (daily reconciliation)
- [ ] Document storage working (COA uploads)
- [ ] Email notifications sending
- [ ] Full seed-to-sale traceability demonstrated
- [ ] SAHPRA GMP compliance verified
- [ ] DALRRD hemp permit tracking working
- [ ] All modules integrated via Shared services (VAT, Audit, Transaction Numbers)

---

### Phase 6: MAUI Mobile App (Future)
- [ ] Install MAUI workload
- [ ] Create MAUI projects
- [ ] Port Blazor components to MAUI
- [ ] Add mobile-specific features (barcode scanning, offline mode)

### Phase 7: Advanced Features (Future)
- [ ] Reporting module (compliance reports)
- [ ] Analytics dashboard
- [ ] Advanced RBAC (role-based access control)
- [ ] Multi-tenant support
- [ ] API rate limiting and security hardening

---

## 📋 Module Roadmap

### POC Focus (Current)
1. **Management Module** - Customer/Debtor, Product, Pricelist management
2. **Retail.POS Module** - Point of Sale transactions and inventory

### Future Modules (Post-POC)
3. **Plantation Module** - Plant cultivation tracking
4. **Production Module** - Processing and manufacturing
5. **Compliance Module** - Regulatory reporting and tracking
6. **Analytics Module** - Business intelligence and reporting
7. **Integration Module** - Third-party integrations (accounting, etc.)

---

## 🚧 Known Issues & Blockers

### Current Blockers
- **MAUI Projects**: MAUI workload not installed - need to run `dotnet workload install maui`
- MAUI projects are placeholders until workload is installed

### Technical Debt
- None yet (new project)

---

## 📈 Success Metrics

### POC Goals
- [ ] Successfully create and retrieve customer records
- [ ] Manage product catalog with pricing
- [ ] Process POS transactions
- [ ] Generate basic sales reports
- [ ] Demonstrate multi-database support (SQL Server + SQLite)
- [ ] Achieve <200ms response time for standard CRUD operations

### Long-term Goals
- Compliance with South African Cannabis Control Board regulations
- Support for 10,000+ products
- Handle 100+ concurrent POS transactions
- 99.9% uptime
- Mobile-first responsive design

---

## 📝 Recent Updates

### 2025-12-19 - Phase 7C PoC Hostile Demo Tests Complete! 🎯
- ✅ Created 11 hostile demo tests (HostileDemoTests.cs)
- ✅ All tests passing - PoC credibility criteria MET
- ✅ Tests demonstrate (not explain) system capabilities:
  - Movement immutability proven
  - Compensating movement pattern working
  - Historical SOH reconstruction functional
  - Full traceability (WHO/WHAT/WHEN/WHY) operational
- 🎓 **Achievement**: PoC meets "Hostile Demo Law" - can defend against skeptical reviewers
- 🚀 **Status**: Ready for database migration & integration testing

### 2025-12-15 - Phase 7-8 Integration Complete
- ✅ Movement Architecture (Phase 7) at 95%
- ✅ Batch & Serial Number System (Phase 8) at 80%
- ✅ All services integrated with MovementService

### 2025-12-12 - Phase 7A Movement Architecture Complete
- ✅ MovementService implemented with 51 tests
- ✅ Option A: SOH = SUM(IN) - SUM(OUT)
- ✅ All 16 transaction types supported

### 2025-12-08 - Phases 5-6 Complete
- ✅ Cultivation, Production, Inventory modules created
- ✅ 32 FluentValidation validators
- ✅ Database migrations applied

### 2025-12-06 - Phase 3 Part A Complete! 🎉
- ✅ Built VATCalculationService (universal transaction logic for POS, GRV, RTS, Invoices, Credits)
- ✅ Built TransactionNumberGeneratorService (auto-generate transaction numbers)
- ✅ Built AuditLogService (POPIA/Cannabis Act/SARS compliance audit trails)
- ✅ Verified ProductCategoryService and DebtorCategoryService complete
- ✅ Registered all services in dependency injection
- ✅ Created comprehensive test suite (44 tests - all passing)
- ✅ Solution builds successfully: 0 Errors, 0 Warnings
- 🎓 **Learning Achievement**: Built reusable shared services with enterprise patterns
- 🚀 **Benefit**: Universal VAT logic, consistent numbering, and audit framework ready for all modules
- 📊 **Impact**: ~1,500 lines of production code + tests, 100% test coverage for VAT service
- 🟢 **Ready for**: Phase 3 Part B (Testing & QA) or Phase 4 (UI Development)

### 2025-12-05 - BLL Architecture Refinement! 🎯
- ✅ Reorganized Management.BLL into domain-driven structure
- ✅ Implemented consistent folder organization across all layers (Models, DAL, BLL)
- ✅ Updated 22 files with new namespaces
- ✅ Updated documentation to reflect architectural improvements
- ✅ Solution builds successfully with 0 errors
- 🎓 **Learning Achievement**: Applied domain-driven design principles to business logic layer
- 🚀 **Benefit**: Improved maintainability and scalability for future development

### 2025-12-02 - Phase 1 Complete! 🎉
- ✅ Created comprehensive data model with 9 entities
- ✅ Implemented POPIA compliance via AuditableEntity base class
- ✅ Added cannabis-specific compliance fields (THC/CBD, batch tracking, age verification)
- ✅ Built flexible pricelist system for multiple pricing strategies
- ✅ Separated Transaction and Payment concepts correctly
- ✅ Applied enterprise-grade best practices throughout
- ✅ Added comprehensive XML documentation (200+ documentation blocks)
- ✅ Solution builds successfully with 0 errors
- 🎓 **Learning Achievement**: Understood base entity pattern, enums, denormalization, navigation properties
- 🚀 **Ready for Phase 2**: DbContext setup and database migrations

### 2025-12-01
- Initial project structure created
- Solution and 13 projects initialized
- Documentation framework established

---

## 👥 Team & Resources

**Development**: Claude AI + Jason
**Architecture**: 3-Tier Modular Design
**Documentation**: `/docs` folder
**Code Location**: `C:\Users\Jason\Documents\Mine\projects\Personal\Project420`

---

**Last Updated**: 2025-12-19
**Next Review**: After Database Migration & Integration Testing
