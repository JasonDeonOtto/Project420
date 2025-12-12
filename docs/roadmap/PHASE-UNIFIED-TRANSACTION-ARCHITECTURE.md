# Phase: Unified Transaction Architecture

**Status**: PLANNED
**Priority**: HIGH
**Estimated Effort**: 2-3 development sessions
**Dependencies**: MovementService (COMPLETED)

---

## Overview

Refactor the transaction system to use a unified architecture where:
- **Module-specific headers** stay in their respective databases (e.g., `RetailTransactionHeaders` in Project420_Dev)
- **Unified TransactionDetails** live in Project420_Shared (linked via `HeaderId` + `TransactionType`)
- **Movements** are generated from TransactionDetails for SOH calculation

This enables:
- Single source of truth for all transaction line items
- Consistent Movement generation across all transaction types
- SOH calculated from Movements (Option A architecture)
- Cross-module reporting capabilities

---

## Current State

### Database Structure
| Database | Tables |
|----------|--------|
| Project420_Dev | TransactionHeaders, TransactionDetails, Payments, Products, Debtors, etc. (29 tables) |
| Project420_Shared | TransactionDetails (unified), Movements, AuditLogs, ErrorLogs, etc. (7 tables) |

### Entity Structure
- `POSTransactionHeader` → maps to `TransactionHeaders` table
- `POSTransactionDetail` → maps to `TransactionDetails` table (POS-specific)
- Unified `TransactionDetail` in Shared (not yet integrated)
- `Movement` in Shared (created, MovementService implemented)

---

## Target State

### Headers (Module-Specific in Project420_Dev)
```
RetailTransactionHeaders  → Sales, Refunds, Quotes, Laybys, AccountPayments
GRVHeaders               → Goods Received Vouchers (future)
RTSHeaders               → Return to Supplier (future)
TransferHeaders          → Stock Transfers (future)
```

### Details (Unified in Project420_Shared)
```
TransactionDetails
├── HeaderId (int)           → Links to ANY header table
├── TransactionType (enum)   → Sale, Refund, GRV, RTS, Transfer, etc.
├── ProductId, ProductSKU, ProductName
├── Quantity, UnitPrice, VATAmount, LineTotal
├── BatchNumber, SerialNumber, WeightGrams
└── Audit fields (CreatedAt, CreatedBy, etc.)
```

### Movements (Generated in Project420_Shared)
```
Movements
├── Generated from TransactionDetails
├── Direction: IN or OUT
├── SOH = SUM(IN) - SUM(OUT)
└── Full audit trail for compliance
```

---

## Implementation Plan

### Step 1: Entity Renames (Low Risk)
- [ ] Rename `POSTransactionHeader` → `RetailTransactionHeader`
- [ ] Rename table `TransactionHeaders` → `RetailTransactionHeaders`
- [ ] Update all references in POS module

**Files Affected:**
- `POSTransactionHeader.cs` → `RetailTransactionHeader.cs`
- `PosDbContext.cs`
- `ITransactionRepository.cs`
- `TransactionRepository.cs`
- `IPaymentRepository.cs`
- `PaymentRepository.cs`
- `Payment.cs` (navigation property)
- `Debtor.cs` (navigation property)

### Step 2: Integrate Unified TransactionDetails (Medium Risk)
- [ ] Remove `POSTransactionDetail` entity and `POSTransactionDetails` DbSet
- [ ] Update PosDbContext to NOT manage TransactionDetails
- [ ] Update repositories to write TransactionDetails to SharedDbContext
- [ ] Update repositories to read TransactionDetails from SharedDbContext

**Key Changes:**
```csharp
// Before (POS-specific)
_context.POSTransactionDetails.Add(detail);

// After (Unified via SharedDbContext)
_sharedContext.TransactionDetails.Add(new TransactionDetail {
    HeaderId = header.Id,
    TransactionType = TransactionType.Sale,
    // ... other fields
});
```

### Step 3: Integrate MovementService (Medium Risk)
- [ ] Inject `IMovementService` into `TransactionService`
- [ ] Call `GenerateMovementsAsync()` when transaction completes
- [ ] Call `ReverseMovementsAsync()` when transaction is voided/refunded

**Integration Point:**
```csharp
// In TransactionService.ProcessCheckoutAsync()
await _transactionRepository.CreateSaleAsync(header, details, payment);
await _movementService.GenerateMovementsAsync(TransactionType.Sale, header.Id);
```

### Step 4: Database Migration
- [ ] Create migration to rename `TransactionHeaders` → `RetailTransactionHeaders`
- [ ] Create migration to drop old `TransactionDetails` from Project420_Dev
- [ ] Migrate existing data from old TransactionDetails to unified (if needed)
- [ ] Apply migrations to Project420_Dev

### Step 5: Testing & Verification
- [ ] Update existing unit tests
- [ ] Create integration tests for unified flow
- [ ] Verify SOH calculations work correctly
- [ ] Test refund flow with movement reversal
- [ ] Verify audit trail is complete

---

## Files to Modify

### POS Module - Models
| File | Change |
|------|--------|
| `POSTransactionHeader.cs` | Rename to `RetailTransactionHeader.cs` |
| `POSTransactionDetail.cs` | DELETE (use unified TransactionDetail) |
| `Payment.cs` | Update navigation property |
| `Debtor.cs` | Update navigation property |

### POS Module - DAL
| File | Change |
|------|--------|
| `PosDbContext.cs` | Rename DbSet, remove POSTransactionDetails |
| `ITransactionRepository.cs` | Update return types and parameters |
| `TransactionRepository.cs` | Major refactor - use RetailTransactionHeader, SharedDbContext for details |
| `IPaymentRepository.cs` | Update references |
| `PaymentRepository.cs` | Update references |

### POS Module - BLL
| File | Change |
|------|--------|
| `ITransactionService.cs` | Update types |
| `TransactionService.cs` | Inject IMovementService, update to use unified details |
| `IRefundService.cs` | Update types |
| `RefundService.cs` | Update to call MovementService for reversals |

### Shared Module
| File | Change |
|------|--------|
| `TransactionDetail.cs` | Already exists - verify schema matches |
| `IMovementService.cs` | Already exists - DONE |
| `MovementService.cs` | Already exists - DONE |

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Breaking existing POS functionality | Medium | High | Comprehensive testing, feature flag |
| Data migration issues | Low | High | Backup before migration, rollback plan |
| Performance impact (cross-DB queries) | Low | Medium | Use proper indexing, batch operations |
| Missing edge cases | Medium | Medium | Review all transaction flows |

---

## Rollback Plan

1. Keep old entity files until fully tested
2. Database migration includes DOWN script
3. Feature flag to switch between old/new implementations
4. Full database backup before migration

---

## Success Criteria

- [ ] All POS transactions create unified TransactionDetails in Project420_Shared
- [ ] All POS transactions generate Movements via MovementService
- [ ] SOH can be calculated from Movements table
- [ ] Refunds properly reverse Movements
- [ ] All existing tests pass
- [ ] No data loss during migration
- [ ] Performance is acceptable (<500ms for transaction creation)

---

## Notes

### Completed Prerequisites
- [x] MovementService implemented and tested (51 unit tests passing)
- [x] Unified TransactionDetail entity created
- [x] Movement entity created
- [x] Project420_Shared database created with all tables
- [x] Database connection strings aligned across projects

### Architecture Decision
Chose **Option A (Movement Architecture)** where:
- SOH is NEVER stored directly
- SOH = SUM(Quantity WHERE Direction = IN) - SUM(Quantity WHERE Direction = OUT)
- Movements are immutable (soft delete only)
- Full audit trail for cannabis compliance (SAHPRA/SARS)
