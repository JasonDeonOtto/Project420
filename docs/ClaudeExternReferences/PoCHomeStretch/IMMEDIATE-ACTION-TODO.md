# Project420 - IMMEDIATE ACTION TODO LIST
## PoC Hostile Demo Implementation

**Created**: 2025-12-11
**Last Updated**: 2025-12-19
**Status**: 🟢 ALL HOSTILE DEMOS COMPLETE → POC READY FOR FREEZE
**Timeline**: Must complete before PoC freeze
**Goal**: Demonstrate credibility through hostile scenario testing

---

## 🚨 SESSION UPDATE (2025-12-19)

### ✅ COMPLETED THIS SESSION

**All Hostile Demo Tests Built and Passing** (11 tests total):

**Core Hostile Demos (7/7 PASSING):**
- ✅ 2.1a: UPDATE Movement FAILS LOUDLY - API has no update method
- ✅ 2.1b: DELETE Movement FAILS LOUDLY - Only soft delete via ReverseMovementsAsync
- ✅ 2.2: Compensating Movement - Mistake + correction both visible in ledger
- ✅ 2.3: GetStockAsOf Reconstruction - Different dates return different SOH
- ✅ 2.4: CorrelationId Traceability - WHO/WHAT/WHEN/WHY all documented
- ✅ 2.5: Retail Cannot Mutate Stock - No SetStock/UpdateStock methods exist
- ✅ 2.6: No Silent Corrections - All adjustments are visible movements

**Secondary Hostile Demos (3/3 PASSING):**
- ✅ 3.1: Atomicity - No partial movements on failure
- ✅ 3.2: Batch Lineage - End-to-end batch tracking visible
- ✅ 3.3: Invalid Action Rejection - Clear error messages on invalid input

**Test File Location:**
- `tests/Project420.Shared.Tests/Proof/HostileDemoTests.cs`

**Run Command:**
```bash
dotnet test --filter "HostileDemoTests" --logger "console;verbosity=detailed"
```

---

### ✅ PREVIOUSLY COMPLETED

**Batch Number Format Updated** (12 digits, week-based):
- Format: `SSTTYYYWWNNNN`
- Example: `011025510001` = Site 01, Production (10), 2025, Week 51, Batch #1
- Visual identification: Type + week immediately visible
- All tests updated and passing

**Serial Number Format Updated** (16 digits, batch-linked):
- Format: `TTYYYWWBBBBSSSSSS`
- Example: `1025510001000001` = Production (10), 2025, Week 51, from Batch 0001, Serial #1
- Embeds parent batch reference (YYWW + BBBB)
- Full traceability: Serial → Batch → Origin

**Key Benefits**:
- 100% numeric (barcode/scanner friendly)
- Visually identifiable (type codes, week-based)
- Week-based (aligns with production cycles)
- Batch linkage in serial enables recall tracing

---

## 🚨 POC HOSTILE DEMO LAW - NEXT SESSION PRIORITY

> **Reference**: `docs/PoCEvolution/project_420_po_c_hostile_demo_law.md`
> **Rule**: If it can be explained but not demonstrated, it is not done.
> **Standard**: "Watch what happens when I do X" — NOT "The code is designed to..."
> **Proof is EVIDENTIARY**: Tests must ATTEMPT the action and show FAILURE

---

### CORE HOSTILE DEMOS (Mandatory - All 7 Required)

| # | Hostile Question | Test to Build | Status |
|---|------------------|---------------|--------|
| 2.1a | "What stops editing history?" | `UPDATE movement → must FAIL` | ✅ **PASSING** |
| 2.1b | "What stops deleting history?" | `DELETE movement → must FAIL` | ✅ **PASSING** |
| 2.2 | "What happens on mistake?" | Compensating movement → both visible | ✅ **PASSING** |
| 2.3 | "Stock on past date?" | `GetStockAsOf(date)` → different dates = different results | ✅ **PASSING** |
| 2.4 | "Why did this happen?" | CorrelationId → links action to movements | ✅ **PASSING** |
| 2.5 | "Can Retail cheat stock?" | Direct mutation from Retail → must FAIL | ✅ **PASSING** |
| 2.6 | "Can someone fix quietly?" | Correction → is a visible movement | ✅ **PASSING** |

### SECONDARY HOSTILE DEMOS (1+ Required)

| # | Hostile Question | Test to Build | Status |
|---|------------------|---------------|--------|
| 3.1 | "What if system fails mid-op?" | Simulated failure → no partial movement | ✅ **PASSING** |
| 3.2 | "Where did batch go?" | Batch movements → end-to-end visible | ✅ **PASSING** |
| 3.3 | "Invalid action?" | Invalid transition → rejected with explanation | ✅ **PASSING** |

---

### Acceptance Criteria for Each Test

**2.1 Immutability of Stock History** ✅
- [x] No UI path exists for UPDATE/DELETE
- [x] No service method succeeds for UPDATE/DELETE
- [x] Failure is explicit (throws exception or returns error)

**2.2 Correction Without Rewriting History** ✅
- [x] Perform incorrect movement (visible in ledger)
- [x] Apply compensating movement (new record, not edit)
- [x] Both movements visible in history
- [x] Net stock resolves correctly

**2.3 Stock State Reconstruction (As-Of)** ✅
- [x] Execute `CalculateSOHAsync(productId, asOfDate)`
- [x] Show different results for different dates
- [x] Output derived from movements ONLY (no snapshot)

**2.4 Action Traceability (Correlation)** ✅
- [x] CorrelationId present on movements
- [x] Single ID links origin action → all resulting movements
- [x] Traceable in logs or admin view

**2.5 Retail Cannot Mutate Stock** ✅
- [x] Attempt direct stock mutation from Retail layer
- [x] Operation fails (MovementService is the ONLY authority)
- [x] Retail can only REQUEST movements, not CREATE them directly

**2.6 No Silent Corrections** ✅
- [x] Apply any correction
- [x] Correction appears as a visible movement record
- [x] No hidden state changes exist

**3.1 Atomicity of Movements** ✅
- [x] Simulate failure mid-operation
- [x] Verify no partial movement was persisted
- [x] Transaction rollback confirmed

**3.2 Batch Lineage Visibility** ✅
- [x] Track batch from GRV → Sale (or full chain)
- [x] All movements show BatchNumber
- [x] End-to-end query works

**3.3 Invalid Action Rejection** ✅
- [x] Attempt invalid transition (e.g., sell negative stock)
- [x] System rejects with clear explanation
- [x] No movement created

---

### PoC Freeze Criteria

✅ **PoC is CREDIBLE when:**
- ✅ All 7 Core Hostile Demos pass (7/7 PASSING)
- ✅ At least 1 Secondary Hostile Demo passes (3/3 PASSING)

🔒 **CRITERIA MET: POC IS READY FOR FREEZE → Move to Prototype**

---

## 🏗️ INFRASTRUCTURE STATUS (Pre-Requisites for Demos)

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

**Document Status**: 🟢 PHASE 7C COMPLETE - Migration Ready to Apply
**Completed**: Entity renames, MovementService integration, all code updates, unit tests fixed, migration created
**Pending**: Apply database migration, integration testing
**Build Status**: ✅ 0 Errors (excluding Android SDK)
**Test Status**: ✅ 200+ Tests Passing (MovementService, BatchNumberGenerator, SerialNumberGenerator, Luhn)
**Database Status**: ✅ Project420_Dev + Project420_Shared (migration pending)

### Key Phase 7B/7C Achievements:
- **Unified TransactionDetail**: All POS code now uses Shared.Core.TransactionDetail
- **MovementService Integration**: Sales, refunds, and voids generate/reverse movements
- **Entity Rename**: POSTransactionHeader → RetailTransactionHeader
- **Navigation Updates**: Payment, Debtor, Product all point to correct entities
- **IBusinessDbContext**: Interface for shared services to access business data
- **Unit Tests Fixed**: TestBusinessDbContext created for in-memory testing
- **Migration Created**: All business tables ready to deploy to Project420_Dev
- **Property Updates**: All code uses VATAmount/LineTotal instead of TaxAmount/Total/Subtotal
- **Build Clean**: Full solution compiles with 0 errors

### Remaining Work:
1. **Apply Migration**: Run `dotnet ef database update` in POS.DAL project (⚠️ Will drop existing transaction data!)
2. **Integration Testing**: Test full checkout flow with database
3. **Verify SOH Calculation**: Confirm movements are being generated correctly
4. **Continue Phase 8**: BatchNumberGeneratorService and SerialNumberGeneratorService are already implemented and tested

### Next Steps (Session 3):
```bash
# Apply migration to Project420_Dev database
cd src/Modules/Retail/POS/Project420.Retail.POS.DAL
dotnet ef database update --startup-project ../Project420.Retail.POS.UI.Blazor
```

*Phase 7C COMPLETE - Migration ready to apply! Phase 8 implementation already done!* 🚀
