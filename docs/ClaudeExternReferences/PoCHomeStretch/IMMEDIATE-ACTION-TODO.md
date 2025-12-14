# Project420 - IMMEDIATE ACTION TODO LIST
## Movement Architecture Implementation Status

**Created**: 2025-12-11
**Last Updated**: 2025-12-13 (Session 2)
**Status**: 🟢 PHASE 7C COMPLETE - Migration Ready to Apply
**Timeline**: Phase 7C complete, migration created, Phase 8 ready to continue
**Goal**: Establish solid foundation for remaining PoC work

---

## 🚨 PHASE 7C: ARCHITECTURAL CORRECTION ✅ COMPLETE

### Issue Identified (2025-12-13)
**Problem**: Business data tables were incorrectly placed in Project420_Shared database
- TransactionDetails, Movements, SerialNumbers, BatchNumberSequences were in SharedDbContext
- These tables need FK relationships with Products (which is in Project420_Dev)
- Cross-database FK constraints are not possible
- Cross-database transactions require distributed transaction handling

### Correct Architecture
| Database | Purpose | Tables |
|----------|---------|--------|
| **Project420_Dev** (Business) | All transactional/business data | Products, RetailTransactionHeaders, **TransactionDetails**, **Movements**, **SerialNumbers**, **BatchNumberSequences**, etc. |
| **Project420_Shared** (Infrastructure) | Cross-cutting services & setup | AuditLogs, ErrorLogs, StationConnections, TransactionNumberSequences, Config tables |

### Solution Implemented ✅ COMPLETE
1. **Created IBusinessDbContext interface** (`Shared.Core/Abstractions/IBusinessDbContext.cs`)
   - Defines DbSets for business data: TransactionDetails, Movements, SerialNumbers, BatchNumberSequences, SerialNumberSequences
   - Allows shared services to access business data without circular dependency

2. **PosDbContext now implements IBusinessDbContext**
   - Contains all business data DbSets
   - Single database (Project420_Dev), single transaction scope
   - FK relationships with Products work properly

3. **Shared services updated to use IBusinessDbContext**
   - MovementService → uses IBusinessDbContext (not SharedDbContext)
   - BatchNumberGeneratorService → uses IBusinessDbContext
   - SerialNumberGeneratorService → uses IBusinessDbContext

4. **DI registration updated**
   - `IBusinessDbContext` → `PosDbContext` in Program.cs
   - Services inject interface, DI resolves to PosDbContext

5. **TransactionRepository simplified**
   - Uses single context (PosDbContext) for all operations
   - Atomic transactions (no cross-database concerns)

### Benefits of Correction
- ✅ FK constraints work properly (TransactionDetail → Product)
- ✅ Single transaction scope (atomic checkout operations)
- ✅ Simpler queries (no cross-database joins needed)
- ✅ Better data integrity
- ✅ Services remain in Shared.Database (shared logic)
- ✅ Data resides in correct location (business database)

### Files Changed
| File | Change |
|------|--------|
| `Shared.Core/Abstractions/IBusinessDbContext.cs` | NEW - Interface for business DbContext |
| `Shared.Core/Project420.Shared.Core.csproj` | Added EF Core reference for interface |
| `POS.DAL/PosDbContext.cs` | Implements IBusinessDbContext, added business DbSets |
| `Shared.Database/SharedDbContext.cs` | Removed business DbSets (TransactionDetails, Movements, etc.) |
| `Shared.Database/Services/MovementService.cs` | Uses IBusinessDbContext |
| `Shared.Database/Services/BatchNumberGeneratorService.cs` | Uses IBusinessDbContext |
| `Shared.Database/Services/SerialNumberGeneratorService.cs` | Uses IBusinessDbContext |
| `Shared.Database/Extensions/ServiceCollectionExtensions.cs` | Simplified, removed obsolete registrations |
| `POS.UI.Blazor/Program.cs` | Added IBusinessDbContext registration, service registrations |
| `POS.DAL/Repositories/TransactionRepository.cs` | Uses single context (PosDbContext) |

### Build Status
- ✅ Main projects build successfully (0 errors)
- ✅ Unit tests updated (Session 2)
- ✅ Database migration created (Session 2)

### Session 2 Progress (2025-12-13)
**Unit Tests Fixed:**
- Created `TestBusinessDbContext` class implementing `IBusinessDbContext` for in-memory testing
- Updated `MovementServiceTests.cs` to use `TestBusinessDbContext`
- Updated `BatchNumberGeneratorServiceTests.cs` to use `TestBusinessDbContext`
- Updated `SerialNumberGeneratorServiceTests.cs` to use `TestBusinessDbContext`
- All 200+ tests pass

**Migration Created:**
- Migration: `20251213082925_BusinessDataTables_Phase7C`
- Location: `POS.DAL/Migrations/`
- Changes:
  - Drops old `POSTransactionHeaders` and `POSTransactionDetails` tables
  - Creates `RetailTransactionHeaders` (renamed from POSTransactionHeaders)
  - Creates `TransactionDetails` (unified for all transaction types)
  - Creates `Movements` (for SOH calculation with all indexes)
  - Creates `BatchNumberSequences` (Phase 8)
  - Creates `SerialNumberSequences` (Phase 8)
  - Creates `SerialNumbers` (Phase 8)
  - All FK constraints and indexes properly configured

**Files Created/Modified (Session 2):**
| File | Change |
|------|--------|
| `tests/Project420.Shared.Tests/TestBusinessDbContext.cs` | NEW - Test implementation of IBusinessDbContext |
| `tests/Project420.Shared.Tests/Services/MovementServiceTests.cs` | Uses TestBusinessDbContext |
| `tests/Project420.Shared.Tests/Services/BatchNumberGeneratorServiceTests.cs` | Uses TestBusinessDbContext |
| `tests/Project420.Shared.Tests/Services/SerialNumberGeneratorServiceTests.cs` | Uses TestBusinessDbContext |
| `POS.DAL/Migrations/20251213082925_BusinessDataTables_Phase7C.cs` | NEW - Database migration |

---

## 🎯 PHASE 7 STATUS OVERVIEW

### Phase 7A: Movement Architecture Foundation ✅ COMPLETE
- ✅ TransactionType enum with all movement types
- ✅ TransactionDetail entity (unified)
- ✅ Movement entity (aligned with spec)
- ✅ SharedDbContext updated with new entities
- ✅ Database migrations generated and applied
- ✅ IMovementService interface created
- ✅ MovementService fully implemented
- ✅ MovementService registered in DI
- ✅ 51 unit tests created and passing
- ✅ Database configuration corrected (Project420_Shared created)

### Phase 7B: Unified Transaction Architecture 🟡 IN PROGRESS (~80% Complete)
- ✅ POSTransactionHeader → RetailTransactionHeader (file renamed)
- ✅ POSTransactionDetails DbSet removed from PosDbContext
- ✅ POSTransactionDetail.cs file deleted (orphaned)
- ✅ TransactionRepository updated to use SharedDbContext.TransactionDetails
- ✅ MovementService integrated into TransactionService
- ✅ MovementService integrated into RefundService
- ✅ GenerateMovementsAsync called after sales
- ✅ GenerateMovementsAsync called after refunds
- ✅ ReverseMovementsAsync called on transaction void
- ✅ IPOSCalculationService updated to use TransactionDetail
- ✅ POSCalculationService updated to use TransactionDetail
- ✅ TransactionSearchService updated to use TransactionDetail
- ✅ BarcodeScanDemo.razor updated to use TransactionDetail
- ✅ Product.cs navigation property updated to TransactionDetail
- ✅ Payment.cs navigation property updated to RetailTransactionHeader
- ✅ Debtor.cs navigation property updated to RetailTransactionHeader
- ✅ Project420.Retail.POS.BLL.csproj reference added for Shared.Database
- ✅ Full solution build passes (0 errors)
- 📋 Database migration for RetailTransactionHeaders table rename (pending)
- 📋 Integration testing with database (pending)

**Detailed Plan**: See `docs/roadmap/PHASE-UNIFIED-TRANSACTION-ARCHITECTURE.md`

---

## ✅ COMPLETED: DAY 1-3 (Database Schema & MovementService)

### Task 1.1: Create TransactionDetails Unified Table ✅ COMPLETE
**Status**: ✅ DONE
**Database**: Project420_Shared

Table created with all specified columns:
- TransactionDetailId (PK)
- HeaderId + TransactionType (discriminator pattern)
- ProductId, ProductSKU, ProductName (denormalized)
- Quantity, UnitPrice, DiscountAmount, VATAmount, LineTotal
- BatchNumber, SerialNumber, WeightGrams
- Full audit fields (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, IsDeleted, DeletedAt, DeletedBy)

**Validation**:
- [x] Table created successfully
- [x] All indexes applied
- [x] No build errors

---

### Task 1.2: Create/Update Movements Table ✅ COMPLETE
**Status**: ✅ DONE
**Database**: Project420_Shared

Movement table created with:
- MovementId (PK)
- ProductId, ProductSKU, ProductName (denormalized)
- MovementType, Direction (IN/OUT)
- Quantity, Mass, Value
- BatchNumber, SerialNumber
- TransactionType, HeaderId, DetailId (source linking)
- MovementReason, TransactionDate, UserId, LocationId
- Full audit fields

**Validation**:
- [x] Table created successfully
- [x] All indexes applied (including covering index for SOH calculation)
- [x] No build errors

---

### Task 1.3: Add TransactionType Enum ✅ COMPLETE
**Status**: ✅ DONE
**File**: `src/Shared/Project420.Shared.Core/Enums/TransactionType.cs`

All transaction types included:
- Sale, Refund, AccountPayment, Layby, Quote (Retail)
- GRV, RTS (Purchasing)
- WholesaleSale, WholesaleRefund (Wholesale)
- ProductionInput, ProductionOutput (Production)
- TransferOut, TransferIn (Transfers)
- AdjustmentIn, AdjustmentOut, StocktakeVariance (Adjustments)

**Validation**:
- [x] Enum created
- [x] Namespace correct
- [x] Build passes

---

### Task 1.4: Generate & Apply Migration ✅ COMPLETE
**Status**: ✅ DONE
**Migration**: `20251211185247_MovementArchitecture_TransactionDetails_And_Movements`

**Validation**:
- [x] Migration generated
- [x] Migration reviewed (DDL correct)
- [x] Migration applied successfully
- [x] Database schema updated
- [x] No errors in application startup

---

### Task 2.1-2.3: Entity & DbContext Updates ✅ COMPLETE
**Status**: ✅ DONE

**TransactionDetail Entity** (`src/Shared/Project420.Shared.Core/Entities/TransactionDetail.cs`):
- Inherits from AuditableEntity
- All fields with correct data annotations
- Product navigation property

**Movement Entity** (`src/Shared/Project420.Shared.Core/Entities/Movement.cs`):
- Inherits from AuditableEntity
- Direction as MovementDirection enum
- TransactionType as TransactionType enum
- All linking fields (HeaderId, DetailId)

**SharedDbContext** (`src/Shared/Project420.Shared.Database/SharedDbContext.cs`):
- DbSet<TransactionDetail> TransactionDetails
- DbSet<Movement> Movements
- Fluent API configurations
- Indexes configured
- Global query filters applied (soft delete)

**Validation**:
- [x] Entities created
- [x] DbContext updated
- [x] Build passes

---

### Task 3.1-3.3: MovementService Implementation ✅ COMPLETE
**Status**: ✅ DONE

**IMovementService Interface** (`src/Shared/Project420.Shared.Database/Services/IMovementService.cs`):
- GenerateMovementsAsync(transactionType, headerId)
- CreateMovementAsync(movement)
- ReverseMovementsAsync(transactionType, headerId, reason)
- CalculateSOHAsync(productId, asOfDate?, locationId?)
- CalculateBatchSOHAsync(productId, batchNumber, asOfDate?)
- CalculateSOHBatchAsync(productIds, asOfDate?)
- GetMovementHistoryAsync(productId, startDate, endDate)
- GetMovementsByBatchAsync(batchNumber)
- GetMovementsBySerialNumberAsync(serialNumber)
- GetMovementsByTransactionAsync(transactionType, headerId)
- GetMovementDirection(transactionType)
- GetMovementTypeName(transactionType)
- IsStockAffectingTransaction(transactionType)

**MovementService Implementation** (`src/Shared/Project420.Shared.Database/Services/MovementService.cs`):
- Full implementation of Movement Architecture (Option A)
- SOH = SUM(IN) - SUM(OUT) from Movement ledger
- Movement generation from TransactionDetails
- Movement reversal (soft delete with reason)
- Batch SOH calculation
- Historical SOH (as-of-date)
- Comprehensive logging

**DI Registration**:
- ServiceCollectionExtensions.cs (both overloads)
- Program.cs (Blazor POS)

**Validation**:
- [x] Interface created with XML documentation
- [x] Service fully implemented
- [x] DI registration complete
- [x] Build passes

---

### Task 6.1: Unit Tests for MovementService ✅ COMPLETE
**Status**: ✅ DONE - 51 TESTS PASSING
**File**: `tests/Project420.Shared.Tests/Services/MovementServiceTests.cs`

**Test Coverage**:

Movement Generation Tests:
- [x] GenerateMovementsAsync_Sale_Should_Create_OUT_Movements
- [x] GenerateMovementsAsync_GRV_Should_Create_IN_Movements
- [x] GenerateMovementsAsync_Refund_Should_Create_IN_Movements
- [x] GenerateMovementsAsync_ProductionInput_Should_Create_OUT_Movements
- [x] GenerateMovementsAsync_ProductionOutput_Should_Create_IN_Movements
- [x] GenerateMovementsAsync_NoDetails_Should_Return_Zero
- [x] GenerateMovementsAsync_AccountPayment_Should_Skip_NonStockTransaction
- [x] GenerateMovementsAsync_WithBatchNumber_Should_Link_Correctly
- [x] GenerateMovementsAsync_WithSerialNumber_Should_Link_Correctly

Movement Reversal Tests:
- [x] ReverseMovementsAsync_Should_Soft_Delete_Movements
- [x] ReverseMovementsAsync_NoMovements_Should_Return_Zero
- [x] ReverseMovementsAsync_EmptyReason_Should_Throw

SOH Calculation Tests:
- [x] CalculateSOHAsync_Should_Return_Correct_SOH
- [x] CalculateSOHAsync_WithAsOfDate_Should_Return_Historical_SOH
- [x] CalculateSOHAsync_Should_Exclude_Deleted_Movements
- [x] CalculateSOHAsync_NoMovements_Should_Return_Zero
- [x] CalculateBatchSOHAsync_Should_Return_Batch_Specific_SOH
- [x] CalculateSOHBatchAsync_Should_Return_Dictionary_Of_SOH
- [x] CalculateSOHBatchAsync_EmptyList_Should_Return_Empty_Dictionary

Movement Query Tests:
- [x] GetMovementHistoryAsync_Should_Return_Movements_In_Date_Range
- [x] GetMovementsByBatchAsync_Should_Return_All_Batch_Movements
- [x] GetMovementsBySerialNumberAsync_Should_Return_Serial_Movements
- [x] GetMovementsByTransactionAsync_Should_Return_Transaction_Movements

Utility Method Tests:
- [x] GetMovementDirection_Should_Map_All_Transaction_Types (Theory with 12 cases)
- [x] GetMovementTypeName_Should_Return_Descriptive_Names (Theory)
- [x] IsStockAffectingTransaction_Should_Correctly_Identify_Stock_Transactions (Theory)

Direct Movement Creation Tests:
- [x] CreateMovementAsync_Should_Create_Movement_Successfully
- [x] CreateMovementAsync_NullMovement_Should_Throw
- [x] CreateMovementAsync_InvalidProductId_Should_Throw
- [x] CreateMovementAsync_ZeroQuantity_Should_Throw
- [x] CreateMovementAsync_EmptyReason_Should_Throw

**Validation**:
- [x] All 51 tests created
- [x] All tests passing ✅
- [x] Test coverage comprehensive

---

## ✅ COMPLETED: Database Configuration Fix

### Issue Identified
- `Project420_Shared` database did not exist
- SharedDbContext migrations were created but never applied
- Web API used different connection string than Blazor POS

### Resolution ✅ COMPLETE
1. Created `Project420_Shared` database
2. Applied all SharedDbContext migrations
3. Updated Web API appsettings.json with correct connection strings
4. Removed orphan `Project420` database

### Current Database Structure
| Database | Purpose | Tables |
|----------|---------|--------|
| Project420_Dev | Business data (POS, Products, Management, etc.) | 29 |
| Project420_Shared | Shared services (Movements, Audit, Errors, etc.) | 7 |

### Project420_Shared Tables
- AuditLogs
- ErrorLogs
- Movements ✅ NEW
- StationConnections
- TransactionDetails ✅ NEW
- TransactionNumberSequences
- __EFMigrationsHistory

---

## 📋 NEXT PHASE: Phase 8 - Batch & Serial Number System

### Architecture Summary (Post Phase 7C Correction)
```
┌─────────────────────────────────────────────────────────────────┐
│                      Project420_Dev (Business Database)          │
│                                                                  │
│  ┌─────────────────────┐    ┌─────────────────────────────────┐ │
│  │ Products            │◄───│ TransactionDetails               │ │
│  │ (Master Data)       │    │ (HeaderId + TransactionType)     │ │
│  └─────────────────────┘    └─────────────────────────────────┘ │
│           ▲                             │                        │
│           │                             ▼                        │
│  ┌─────────────────────┐    ┌─────────────────────────────────┐ │
│  │ RetailTransactionHeaders │    │ Movements                   │ │
│  │ (Sales, Refunds)        │    │ (SOH = IN - OUT)             │ │
│  └─────────────────────┘    └─────────────────────────────────┘ │
│                                                                  │
│  ┌─────────────────────┐    ┌─────────────────────────────────┐ │
│  │ SerialNumbers       │    │ BatchNumberSequences             │ │
│  │ (Unit tracking)     │    │ (Batch generation)               │ │
│  └─────────────────────┘    └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                  Project420_Shared (Infrastructure Database)     │
│                                                                  │
│  ┌─────────────────────┐    ┌─────────────────────────────────┐ │
│  │ AuditLogs           │    │ ErrorLogs                        │ │
│  │ (POPIA compliance)  │    │ (Error tracking)                 │ │
│  └─────────────────────┘    └─────────────────────────────────┘ │
│                                                                  │
│  ┌─────────────────────┐    ┌─────────────────────────────────┐ │
│  │ StationConnections  │    │ TransactionNumberSequences       │ │
│  │ (Multi-tenant)      │    │ (Number generation)              │ │
│  └─────────────────────┘    └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### Key Architecture Principle
- **Shared.Database project** contains service LOGIC (MovementService, BatchNumberGeneratorService, etc.)
- **Services inject IBusinessDbContext** (resolved to PosDbContext at runtime)
- **Business DATA** resides in Project420_Dev (single database for transactional consistency)
- **Infrastructure DATA** resides in Project420_Shared (cross-cutting concerns)

### Implementation Steps (Phase 7B)

**Step 1: Entity Renames**
- [ ] Rename `POSTransactionHeader` → `RetailTransactionHeader`
- [ ] Rename table `TransactionHeaders` → `RetailTransactionHeaders`
- [ ] Update navigation properties in Payment.cs, Debtor.cs

**Step 2: Integrate Unified TransactionDetails**
- [ ] Remove `POSTransactionDetail` entity
- [ ] Update PosDbContext (remove POSTransactionDetails DbSet)
- [ ] Update repositories to write to SharedDbContext.TransactionDetails
- [ ] Update repositories to read from SharedDbContext.TransactionDetails

**Step 3: Integrate MovementService**
- [ ] Inject `IMovementService` into `TransactionService`
- [ ] Call `GenerateMovementsAsync()` after transaction completes
- [ ] Call `ReverseMovementsAsync()` when transaction voided/refunded

**Step 4: Database Migration**
- [ ] Create migration for table rename
- [ ] Drop old TransactionDetails from Project420_Dev (if empty)
- [ ] Apply migrations

**Step 5: Testing**
- [ ] Update existing unit tests
- [ ] Create integration tests for unified flow
- [ ] Verify SOH calculations
- [ ] Test refund flow with movement reversal

---

## 📊 PROGRESS TRACKING

### Phase 7A: Movement Architecture Foundation
| Task | Status | Notes |
|------|--------|-------|
| TransactionType enum | ✅ Complete | All 16 types |
| TransactionDetail entity | ✅ Complete | Unified schema |
| Movement entity | ✅ Complete | Option A architecture |
| SharedDbContext update | ✅ Complete | DbSets + Fluent API |
| Database migrations | ✅ Complete | Applied to Project420_Shared |
| IMovementService interface | ✅ Complete | 13 methods |
| MovementService implementation | ✅ Complete | Full implementation |
| DI registration | ✅ Complete | Both extension + Program.cs |
| Unit tests | ✅ Complete | 51 tests passing |
| Database configuration | ✅ Complete | Project420_Shared created |

### Phase 7B: Unified Transaction Architecture (2025-12-12)
| Task | Status | Notes |
|------|--------|-------|
| Entity renames | ✅ Complete | POSTransactionHeader → RetailTransactionHeader |
| PosDbContext update | ✅ Complete | POSTransactionDetails DbSet removed |
| POSTransactionDetail.cs cleanup | ✅ Complete | Orphaned file deleted |
| Repository updates | ✅ Complete | Using SharedDbContext for details |
| MovementService integration | ✅ Complete | TransactionService + RefundService |
| GenerateMovementsAsync integration | ✅ Complete | Called after sales/refunds |
| ReverseMovementsAsync integration | ✅ Complete | Called on transaction void |
| IPOSCalculationService update | ✅ Complete | Uses TransactionDetail |
| POSCalculationService update | ✅ Complete | Uses TransactionDetail |
| TransactionSearchService update | ✅ Complete | Uses TransactionDetail |
| BarcodeScanDemo.razor update | ✅ Complete | Uses TransactionDetail |
| Navigation property updates | ✅ Complete | Product, Payment, Debtor |
| BLL project reference | ✅ Complete | Added Shared.Database reference |
| Build verification | ✅ Complete | 0 errors |
| Database migrations | ✅ Complete | 20251213082925_BusinessDataTables_Phase7C created |
| Migration applied | 📋 Pending | Run `dotnet ef database update` to apply |
| Integration testing | 📋 Pending | Full flow testing with database |

---

## 🚀 AFTER PHASE 7B: PROCEED TO PHASE 8

**Phase 8 Focus**: Batch & Serial Number System
**Prerequisites**: Phase 7B MUST be complete

**Phase 8 Immediate Tasks** (Top 5):
1. Define batch number format (16 digits)
2. Define serial number formats (28 full, 13 short)
3. Implement `BatchNumberGeneratorService`
4. Implement `SerialNumberGeneratorService`
5. Integrate with Production module (batch assignment on GRV, SN assignment on packaging)

---

## 🚨 KEY ACHIEVEMENTS

### Movement Architecture (Option A) Implemented
- SOH is NEVER stored directly
- SOH = SUM(Quantity WHERE Direction = IN) - SUM(Quantity WHERE Direction = OUT)
- Movements are immutable (soft delete only)
- Full audit trail for cannabis compliance (SAHPRA/SARS)

### Technical Highlights
- 51 comprehensive unit tests
- In-memory database testing with EF Core
- Proper async/await patterns
- Comprehensive logging
- Soft delete support with query filters

### Compliance Features
- Batch number tracking for seed-to-sale traceability
- Serial number tracking for individual items
- Weight tracking for cannabis reconciliation
- Movement reasons documented for compliance reporting
- Full audit trail via AuditableEntity

---

**Document Status**: 🟢 PHASE 9 IN PROGRESS - Retail POS Completion
**Last Updated**: 2025-12-13 (Session 4)
**Completed**: Phase 7 (A/B/C) + Phase 8 + Phase 9.1 (Barcode Scanning)
**Current Phase**: Phase 9 - Retail POS Completion (9.1 COMPLETE)
**Build Status**: ✅ 0 Errors
**Test Status**: ✅ 204 Tests Passing
**Database Status**: ✅ Project420_Dev (34 tables) + Project420_Shared (5 tables) - FULLY APPLIED

---

## ✅ PHASE 7 & 8 COMPLETE SUMMARY

### Phase 7A: Movement Architecture Foundation ✅
- TransactionType enum with 16 transaction types
- Movement entity with Option A architecture (SOH = SUM(IN) - SUM(OUT))
- MovementService with 13 methods fully implemented
- 51 unit tests passing

### Phase 7B: Unified Transaction Architecture ✅
- POSTransactionHeader → RetailTransactionHeader renamed
- Unified TransactionDetails table
- MovementService integrated into TransactionService/RefundService
- All navigation properties updated

### Phase 7C: Architectural Correction ✅
- IBusinessDbContext interface created
- Business data tables moved to Project420_Dev
- Shared services use IBusinessDbContext (resolved to PosDbContext)
- Single database transaction scope for atomic operations

### Phase 8: Batch & Serial Number System ✅
- **BatchNumberGeneratorService**: 16-digit format (SSTTYYYYMMDDNNNN)
  - Site ID, Batch Type, Date, Sequence embedded
  - Thread-safe with database transactions
  - 47 unit tests passing
- **SerialNumberGeneratorService**: Full (30-digit) + Short (13-digit) formats
  - Full SN: Site, Strain, Type, Date, Batch, Unit, Weight, Pack, Check digit
  - Short SN: EAN-13 compatible for barcodes
  - Luhn check digit validation
  - 51 unit tests passing
- **Database Tables**: BatchNumberSequences, SerialNumbers, SerialNumberSequences
- **DI Registration**: Both services registered in Program.cs

### Database Schema (Applied)
**Project420_Dev (34 tables)**:
- BatchNumberSequences, SerialNumbers, SerialNumberSequences (Phase 8)
- Movements, TransactionDetails, RetailTransactionHeaders (Phase 7)
- Products, Debtors, Payments, ProductCategories, etc.
- GrowRooms, GrowCycles, Plants, HarvestBatches (Cultivation)
- ProductionBatches, ProcessingSteps, QualityControls, LabTests (Production)
- StockMovements, StockTransfers, StockAdjustments, StockCounts (Inventory)

**Project420_Shared (5 tables)**:
- AuditLogs, ErrorLogs, StationConnections, TransactionNumberSequences

---

## 📋 PHASE 9: RETAIL POS COMPLETION (NEXT)

**Priority**: 🔴 CRITICAL
**Target**: 85-90% POS feature completeness
**Prerequisites**: Phase 7 & 8 ✅ COMPLETE

### Phase 9 Key Tasks:
1. **Barcode & Serial Number Scanning** (9.1)
   - EAN-13, Code128, QR code support
   - SN lookup via SerialNumberGeneratorService
   - Product quick search fallback

2. **Line-Level & Header-Level Discounts** (9.2)
   - Discount UI components
   - VAT recalculation after discounts
   - Receipt updates

3. **Multi-Tender Checkout** (9.3)
   - Cash, Card, EFT, Mobile Payment
   - Split tender support
   - Change calculation

4. **Refund Workflow** (9.4)
   - Full and partial refunds
   - Movement reversal (IN movements)
   - Credit note generation

5. **Cash Drop & Cash Out** (9.5)
   - End-of-shift reconciliation
   - Variance tracking

6. **Transaction Cancellation** (9.6)
   - Before/after payment handling
   - Manager override requirement

7. **Age Verification Enhancement** (9.7)
   - SA ID card scanning (13-digit)
   - DOB extraction and age calculation

8. **Compliant Receipt Generation** (9.8)
   - Batch/SN on receipts
   - VAT breakdown
   - Legal disclaimers

9. **Movement Generation Optimization** (9.9)
   - Batch insert performance
   - Async generation

---

## 📊 PHASE 9 PROGRESS TRACKING

### Phase 9.1: Barcode & Serial Number Scanning ✅ COMPLETE (2025-12-13)
**Files Created**:
| File | Purpose |
|------|---------|
| `POS.BLL/DTOs/BarcodeScanDtos.cs` | DTOs for scan results, product search, SN validation |
| `POS.BLL/Services/IBarcodeService.cs` | Interface for barcode scanning operations |
| `POS.BLL/Services/BarcodeService.cs` | Full implementation with EAN-13, Code128, SN support |

**Files Modified**:
| File | Change |
|------|--------|
| `POS.BLL/DTOs/CartItemDto.cs` | Added SerialNumber and IsSerializedItem properties |
| `POS.UI.Blazor/Program.cs` | Registered IBarcodeService in DI |
| `POS.UI.Blazor/Components/Pages/POSCheckout.razor` | Full barcode scanning integration |

**Features Implemented**:
- ✅ Barcode type detection (EAN-13, UPC, Full SN, Short SN, SKU)
- ✅ EAN-13 check digit validation
- ✅ Luhn check digit validation for serial numbers
- ✅ Full serial number lookup (30 digits)
- ✅ Short serial number lookup (13 digits - EAN-13 compatible)
- ✅ Standard barcode lookup (ProductBarcodes table)
- ✅ SKU lookup fallback
- ✅ Product search by name/SKU/strain
- ✅ Serialized item detection (can't modify qty, can't add twice)
- ✅ Stock warning messages (low stock, out of stock)
- ✅ Integration with POSCheckout.razor UI

**API Summary (IBarcodeService)**:
```csharp
Task<BarcodeScanResultDto> ProcessScanAsync(string scannedValue);
Task<SerialNumberValidationDto> ValidateSerialNumberAsync(string serialNumber);
Task<ProductSearchResultDto> SearchProductsAsync(string searchTerm, int pageSize = 20);
Task<CartItemDto?> GetProductForCartAsync(int productId);
Task<List<SerialNumberValidationDto>> GetAvailableSerialNumbersAsync(int productId, int limit = 50);
string DetectBarcodeType(string value);
bool ValidateEAN13CheckDigit(string ean13);
bool ValidateLuhnCheckDigit(string value);
```

### Phase 9.2: Line-Level & Header-Level Discounts ✅ COMPLETE (2025-12-14)
**Files Modified**:
| File | Change |
|------|--------|
| `POS.BLL/DTOs/CartItemDto.cs` | Added DiscountAmount, DiscountPercentage, DiscountReason, OriginalLineTotal properties |
| `POS.BLL/DTOs/CheckoutResultDto.cs` | Added SerialNumbers property |
| `POS.BLL/Services/IPOSCalculationService.cs` | Added 4 new discount methods with VAT recalculation |
| `POS.BLL/Services/POSCalculationService.cs` | Implemented discount methods with SA VAT compliance |
| `POS.BLL/Services/TransactionService.cs` | Updated to handle line-level discounts and VAT recalculation |
| `POS.UI.Blazor/Components/Pages/POSCheckout.razor` | Full discount UI (line + header), modal, preview |
| `tests/Project420.Retail.POS.Tests/Infrastructure/ServiceTestBase.cs` | Added MockPOSCalculationService |
| `tests/Project420.Retail.POS.Tests/Services/TransactionServiceTests.cs` | Updated for new service signature |

**Features Implemented**:
- ✅ Line-level discount input (percentage or fixed amount)
- ✅ Header-level (transaction) discount input
- ✅ VAT recalculation after discounts (SA compliance: discount total then recalculate VAT)
- ✅ Discount modal with preview (shows original, discount, new total)
- ✅ Quick percentage buttons (5%, 10%, 15%, 20%, 25%)
- ✅ Discount reason selection (Staff, Loyalty, Damaged, Price Match, Promotion, Manager Override)
- ✅ Cart display with discount column (strikethrough original, show discounted)
- ✅ Order summary with discount breakdown
- ✅ Receipt display with line-level discounts

**API Summary (IPOSCalculationService - New Methods)**:
```csharp
(decimal subtotal, decimal vatAmount, decimal total) CalculateLineWithDiscount(decimal originalTotal, decimal discountAmount);
decimal CalculateVATAfterDiscount(decimal discountedTotal);
Dictionary<int, decimal> CalculateHeaderDiscountProration(decimal headerDiscount, List<(int lineId, decimal lineTotal)> lineItems);
decimal CalculateDiscountAmount(decimal originalAmount, decimal discountPercentage);
```

**SA VAT Compliance Note**:
- Discounts are applied to the total (VAT-inclusive)
- VAT is then recalculated on the discounted total
- Formula: New VAT = DiscountedTotal - (DiscountedTotal / 1.15)

### Phase 9.3: Multi-Tender Checkout ✅ COMPLETE (2025-12-14)
**Files Created**:
| File | Purpose |
|------|---------|
| `POS.BLL/DTOs/PaymentTenderDto.cs` | DTO for individual tender lines + PaymentBreakdownDto |

**Files Modified**:
| File | Change |
|------|--------|
| `POS.BLL/DTOs/CheckoutRequestDto.cs` | Added Tenders list and IsMultiTender property |
| `POS.BLL/DTOs/CheckoutResultDto.cs` | Added PaymentBreakdown property |
| `POS.DAL/Repositories/TransactionRepository.cs` | Added multi-payment overload CreateSaleAsync(header, details, List<Payment>) |
| `POS.BLL/Services/TransactionService.cs` | Creates multiple Payment records, calculates change from cash only |
| `POS.UI.Blazor/Components/Pages/POSCheckout.razor` | Full multi-tender UI with split payment toggle |

**Features Implemented**:
- ✅ Split payment toggle to enable multi-tender mode
- ✅ Add/remove individual tenders (Cash, Card, EFT, Mobile)
- ✅ Amount input per tender with reference field for non-cash
- ✅ Running balance display (Transaction Total, Tendered, Remaining, Change)
- ✅ "Add Remaining as Cash" quick button
- ✅ Change calculation from cash tenders only
- ✅ Validation: total tendered >= transaction total
- ✅ Receipt shows payment breakdown for multi-tender
- ✅ FIC Act compliance notes for cash > R25,000
- ✅ Backwards compatible with single payment mode

**API Summary (PaymentTenderDto)**:
```csharp
public class PaymentTenderDto
{
    public PaymentMethod Method { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? BankOrProvider { get; set; }
    public string? MaskedCardNumber { get; set; }
    public string? CardType { get; set; }
    public string? AuthorizationCode { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? Notes { get; set; }
}
```

### Phase 9.4: Refund Workflow ✅ ALREADY IMPLEMENTED
- RefundProcessing.razor with full workflow
- RefundService with validation and approval
- Movement reversal (IN movements) on refund

### Phase 9.5: Cash Drop & Cash Out ✅ ALREADY IMPLEMENTED
- CashDrawerManagement.razor
- PaymentReconciliationService
- Denomination counting
- Variance tracking

### Phase 9.6: Transaction Cancellation ✅ COMPLETE (2025-12-14)
**Files Created**:
| File | Purpose |
|------|---------|
| `TransactionCancellation.razor` | Full cancellation UI with pre/post payment workflows |

**Files Modified**:
| File | Change |
|------|--------|
| `TransactionService.cs` | Added ValidateCancellationEligibilityAsync, ProcessCancellationAsync, ValidateManagerPinAsync |

**Implementation Details**:
- [x] Pre-payment cancellation (cart clear) - no DB changes
- [x] Post-payment cancellation (void) - movement reversal
- [x] Manager override requirement for completed transactions
- [x] Cancellation eligibility validation (status, age, amount thresholds)
- [x] 11 cancellation reasons (CancellationReason enum)
- [x] Manager PIN validation (PoC: 4+ digit numeric or "1234")
- [x] Audit trail with reason, notes, user, manager
- [x] Payment reversal notification for completed transactions

**Business Rules**:
- Completed transactions require manager approval
- Transactions > 30 minutes old require manager approval
- Transactions > R1,000 require manager approval
- Transactions > 24 hours cannot be cancelled (use refund instead)
- Already cancelled/refunded transactions cannot be re-cancelled

**Routes**:
- `/pos/cancel` - Selection screen
- `/pos/cancel/{transactionNumber}` - Direct lookup

### Phase 9.7: Age Verification Enhancement ✅ COMPLETE (2025-12-14)
**Files Created**:
| File | Purpose |
|------|---------|
| `ISAIdValidationService.cs` | Interface for SA ID validation |
| `SAIdValidationService.cs` | Full SA ID validation with Luhn algorithm |
| `AgeVerificationDtos.cs` | DTOs for age verification workflow |
| `SAIdValidationServiceTests.cs` | 44 unit tests for SA ID validation |

**Files Modified**:
| File | Change |
|------|--------|
| `POSCheckout.razor` | Enhanced age verification UI with ID scan, DOB entry, quick verify tabs |
| `Program.cs` | Registered ISAIdValidationService in DI |

**Implementation Details**:
- [x] SA ID format validation (YYMMDDGSSSCAZ - 13 digits)
- [x] Luhn mod-10 check digit validation
- [x] DOB extraction from ID (first 6 digits)
- [x] Century detection (1900s vs 2000s based on current year)
- [x] Age calculation from DOB
- [x] Gender extraction (digit 7: 0-4=Female, 5-9=Male)
- [x] Citizenship extraction (digit 11: 0=SA Citizen, 1=PR)
- [x] Manual DOB entry with validation
- [x] ID masking for privacy (850101*****86)
- [x] 44 comprehensive unit tests

**UI Features**:
- Tabbed interface: Scan ID | Enter DOB | Quick Verify
- Real-time validation on ID scan
- Verification result display (method, age, masked ID)
- Clear & re-verify option
- Warning for customers who just turned 18

### Phase 9.8: Compliant Receipt Generation 📋 PENDING
- [ ] Batch/SN on receipt line items
- [ ] VAT breakdown
- [ ] Legal disclaimers

### Phase 9.9: Movement Generation Optimization 📋 PENDING
- [ ] Batch insert performance
- [ ] Async generation patterns

---

**Document Status**: 🟢 PHASE 9 IN PROGRESS - Retail POS Completion
**Last Updated**: 2025-12-14 (Session 8 - Phase 9.7 Complete)
**Completed**: Phase 7 (A/B/C) + Phase 8 + Phase 9.1 (Barcode) + Phase 9.2 (Discounts) + Phase 9.3 (Multi-Tender) + Phase 9.6 (Transaction Cancellation) + Phase 9.7 (Age Verification)
**Current Phase**: Phase 9 - Retail POS Completion (9.1 ✅ 9.2 ✅ 9.3 ✅ 9.4 ✅ 9.5 ✅ 9.6 ✅ 9.7 ✅ → Next: 9.8)
**Build Status**: ✅ 0 Errors
**Test Status**: ✅ 140 POS Tests Passing (96 + 44 SA ID)

*Phase 9.7 COMPLETE! SA ID validation with Luhn algorithm, DOB extraction, age verification. Next: Phase 9.8 (Compliant Receipt Generation)* 🚀
