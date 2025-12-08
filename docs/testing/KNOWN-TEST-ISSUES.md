# Known Test Issues - Project420

**Last Updated**: 2025-12-07
**Test Run Status**: 310 total tests, 301 passing, 9 failing

---

## 🔴 Failing Tests (9)

### PaymentReconciliationService Tests (4 failures)

**Root Cause**: Service implementation relies on in-memory session storage and accesses navigation properties that aren't properly mocked.

**Failing Tests**:
1. `OpenCashDrawerAsync_WithValidRequest_CreatesSession`
2. `ReconcileCashDrawerAsync_WithAcceptableVariance_Succeeds`
3. `ReconcileCashDrawerAsync_WithLargeVariance_RequiresManagerApproval`
4. `ReconcileCashDrawerAsync_WithManagerApproval_Succeeds`

**Error**: `System.ArgumentNullException: Value cannot be null. (Parameter 'source')`
**Location**: `PaymentReconciliationService.cs:153` - `transactions.Where()` call

**Issue**:
- Service tries to access `transaction.Payments` navigation property
- Mocked transactions don't have navigation properties populated
- Service uses in-memory `_activeSessions` dictionary which breaks unit test isolation

**Recommendation**: Refactor service to:
- Accept session storage through dependency injection (testable abstraction)
- Avoid directly accessing navigation properties in LINQ queries
- Use DTOs instead of entity objects for calculations

---

### RefundService Tests (3 failures)

**Failing Tests**:
1. `ProcessRefundAsync_CreatesNegativeTransactionAmounts`
2. `ValidateRefundEligibilityAsync_FullyRefunded_ReturnsIneligible`
3. `ValidateRefundEligibilityAsync_VoidedTransaction_ReturnsIneligible`

**Root Cause**: String matching assertions too strict

**Error Examples**:
- Expected `"fully refunded"` but got `"Transaction has been fully refunded"`
- Expected `"voided/cancelled"` but got `"Transaction has been voided/cancelled"`

**Recommendation**: Update test assertions to use `.Contain()` instead of exact match

---

### TransactionService Tests (1 failure)

**Failing Test**:
1. `ProcessCheckoutAsync_CalculatesChangeDueForCashPayment`

**Error**: `Expected result.Success to be True, but found False`

**Recommendation**: Investigate change calculation logic in TransactionService

---

### TransactionSearchService Tests (1 failure)

**Failing Test**:
1. `GetTransactionStatisticsAsync_CalculatesCorrectly`

**Error**: `Expected 166.67M, but found 166.66666666666666666666666667M`

**Root Cause**: Decimal precision difference

**Recommendation**: Use `.BeApproximately()` with tolerance instead of exact decimal match

---

## ✅ Passing Tests (301)

### Management Module (223 tests - 100% pass rate)
- ✅ CustomerServiceTests (20 tests)
- ✅ VATCalculationServiceTests (44 tests)
- ✅ PricelistServiceTests (20+ tests)
- ✅ ProductServiceTests (11 tests)
- ✅ CreateProductValidatorTests (62 tests)
- ✅ UpdateProductValidatorTests (53 tests)
- ✅ CustomerRegistrationValidatorTests (18 tests)

### Retail POS Module (78 tests - 90% pass rate)
- ✅ RefundService (other tests passing)
- ✅ TransactionService (other tests passing)
- ✅ POSCalculationService (passing)

---

## 📊 Test Coverage Summary

**Overall**: 15.4% line coverage
**Target**: 90%+ line coverage

### Services with 0% Coverage (HIGH PRIORITY):
- TransactionNumberGeneratorService
- AuditLogService
- ProductCategoryService
- DebtorCategoryService
- PopiaDataMaskingService
- All DAL Repositories

### Services Needing Improvement (40-80%):
- ProductService: 54.8% → Target: 80%+
- PricelistService: 60.1% → Target: 80%+
- TransactionSearchService: 44% → Target: 80%+
- PaymentReconciliationService: 20.6% → Target: 80%+ (after refactoring)

### Services with Excellent Coverage (>80%):
- ✅ CustomerService: 100%
- ✅ TransactionService: 94.7%
- ✅ POSCalculationService: 93.9%
- ✅ RefundService: 89.4%
- ✅ VATCalculationService: 87.6%

---

## 🎯 Action Plan

### Phase 1: Add Missing Tests (HIGH IMPACT)
1. Write TransactionNumberGeneratorService tests (0% → 80%+)
2. Write AuditLogService tests (0% → 80%+)
3. Write ProductCategoryService tests (0% → 80%+)
4. Write DebtorCategoryService tests (0% → 80%+)

**Expected Impact**: +10% overall coverage

### Phase 2: Improve Existing Coverage
1. Improve ProductService tests (54.8% → 80%+)
2. Improve PricelistService tests (60.1% → 80%+)
3. Improve TransactionSearchService tests (44% → 80%+)

**Expected Impact**: +8% overall coverage

### Phase 3: Fix Known Issues
1. Fix RefundService assertion issues (3 tests)
2. Fix TransactionSearchService decimal precision (1 test)
3. Fix TransactionService change calculation (1 test)
4. Refactor PaymentReconciliationService for testability (4 tests)

**Expected Impact**: 100% pass rate

### Phase 4: Integration Tests
1. End-to-end workflow tests
2. Database integration tests
3. API integration tests (when API developed)

**Expected Impact**: +5% overall coverage + higher confidence

---

## 🚀 Target Metrics

- **Short Term**: 40% overall coverage, 305+ passing tests
- **Medium Term**: 70% overall coverage, 310+ passing tests
- **Long Term**: 90% overall coverage, 500+ tests (including integration)

---

**Last Test Run**: 2025-12-07 11:50:59
**Build Status**: ✅ 0 Errors, 15 Warnings
**Test Status**: ⚠️ 301/310 Passing (97.1% pass rate)
