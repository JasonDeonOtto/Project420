# Project420 - PoC Home Stretch Todo Roadmap
## Comprehensive Task List for 85-90% PoC Completion

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: ACTIVE - PoC Final Phase
**Target**: 85-90% completeness across mandatory + high-priority modules
**Timeline**: 6 weeks (Phases 7-12)

---

## 📊 CURRENT STATUS SUMMARY

### Phase 6 Complete ✅ (as of 2025-12-08)
- ✅ All MVP modules created (Management, POS, OnlineOrders, Cultivation, Production, Inventory)
- ✅ Database schema migrated (12 tables across 3 DbContexts)
- ✅ FluentValidation integrated (32 validators)
- ✅ Shared services operational (VAT, Transaction Numbers, Audit Logs)
- ✅ Testing framework established (224 tests, 100% pass rate)
- ✅ Retail POS vertical slice complete (DAL → BLL → UI)

### Module Completeness Matrix

| Module | Current | Target | Gap | Priority | Notes |
|--------|---------|--------|-----|----------|-------|
| **Inventory** | 35% | 90% | 55% | 🔴 CRITICAL | Core module, SOH engine, movement ledger |
| **Retail POS** | 60% | 85-90% | 25-30% | 🔴 CRITICAL | Market entry module, full checkout workflow |
| **Purchasing** | 25% | 75% | 50% | 🟠 HIGH | GRV, RTS, required for inventory flow |
| **Production (Retail)** | 60% | 75% | 15% | 🟡 MEDIUM | Pre-rolls, packaging, retail production |
| **Production (General)** | 20% | 45% | 25% | 🟢 LOW | Basic batch tracking, step recording (stub) |
| **Cultivation** | 15% | 35% | 20% | 🟢 LOW | Basic plant tracking, harvest batches (stub) |
| **Wholesale** | 0% | 25% | 25% | 🟢 LOW | Basic order/invoice structure (stub) |

---

## 🎯 6-WEEK ROADMAP

###PHASE 7: Movement Architecture Implementation
**Duration**: Week 1
**Goal**: Implement Option A (specialized headers + unified details)
**Priority**: 🔴 CRITICAL
**Dependencies**: None
**Completion Criteria**: All transactions write to unified TransactionDetails, movements generated consistently

### PHASE 8: Batch & Serial Number System
**Duration**: Week 2
**Goal**: Enterprise-grade batch and serial number generation
**Priority**: 🔴 CRITICAL
**Dependencies**: Phase 7 (movement architecture)
**Completion Criteria**: Full traceability (plant → batch → step → SN → sale)

### PHASE 9: Retail POS Completion
**Duration**: Week 3
**Goal**: 85-90% POS feature completeness
**Priority**: 🔴 CRITICAL
**Dependencies**: Phase 7 (movements), Phase 8 (batch/SN)
**Completion Criteria**: Full checkout workflow with scanning, VAT, refunds, discounts

### PHASE 10: Production DAL Expansion
**Duration**: Week 4
**Goal**: Complete Retail Production DAL + stub general production
**Priority**: 🟡 MEDIUM
**Dependencies**: Phase 8 (batch numbers)
**Completion Criteria**: Pre-roll/packaging workflow complete with movement generation

### PHASE 11: Inventory SOH Engine
**Duration**: Week 5
**Goal**: Performant multi-table SOH calculation engine
**Priority**: 🔴 CRITICAL
**Dependencies**: Phase 7 (movements)
**Completion Criteria**: SOH calculated from movements, cache operational, <200ms queries

### PHASE 12: Purchasing Workflow
**Duration**: Week 6
**Goal**: Complete GRV, RTS workflows
**Priority**: 🟠 HIGH
**Dependencies**: Phase 7 (movements), Phase 8 (batch/SN)
**Completion Criteria**: GRV creates IN movements, RTS creates OUT movements, three-way matching

---

## PHASE 7: MOVEMENT ARCHITECTURE IMPLEMENTATION

**Week**: 1
**Priority**: 🔴 CRITICAL
**Target**: Unified transaction detail table + consistent movement generation

### 7.1 Database Schema Changes

**Tasks**:
- [ ] **7.1.1** Create `TransactionDetails` unified table
  - Columns: `DetailId` (PK), `HeaderId`, `TransactionType` (discriminator), `ProductId`, `Quantity`, `UnitPrice`, `DiscountAmount`, `VATAmount`, `LineTotal`, `BatchNumber`, `SerialNumber`, `Notes`
  - Indexes: `IX_TransactionDetails_HeaderId_TransactionType`, `IX_TransactionDetails_ProductId`
- [ ] **7.1.2** Add `TransactionType` enum to codebase
  - Values: `Sale`, `Refund`, `GRV`, `RTS`, `WholesaleSale`, `WholesaleRefund`, `ProductionInput`, `ProductionOutput`, `TransferOut`, `TransferIn`, `AdjustmentIn`, `AdjustmentOut`, `StocktakeVariance`
- [ ] **7.1.3** Create `Movements` table (if not exists)
  - Columns: `MovementId` (PK), `ProductId`, `MovementType`, `Direction`, `Quantity`, `Mass`, `Value`, `BatchNumber`, `SerialNumber`, `TransactionType`, `HeaderId`, `DetailId`, `MovementReason`, `TransactionDate`, `UserId`, `LocationId`, `IsDeleted`, `DeletedAt`, `DeletedBy`
  - Indexes: `IX_Movements_ProductId_TransactionDate`, `IX_Movements_BatchNumber`, `IX_Movements_SerialNumber`, `IX_Movements_TransactionType_HeaderId`
- [ ] **7.1.4** Generate and apply migration

**Validation**:
- [ ] All tables created successfully
- [ ] Indexes applied (verify with `sp_helpindex`)
- [ ] Foreign keys referential integrity enforced
- [ ] Build passes (0 errors, <5 warnings)

---

### 7.2 Refactor Existing Transactions

**Tasks**:
- [ ] **7.2.1** Refactor `RetailTransactionHeaders` to link to unified `TransactionDetails`
  - Remove `RetailTransactionDetails` table (if exists)
  - Update `RetailTransactionHeaders` to use `TransactionDetails` via `HeaderId` + `TransactionType = Sale/Refund`
- [ ] **7.2.2** Refactor `PurchaseHeaders` to link to unified `TransactionDetails`
  - Remove `PurchaseDetails` table (if exists)
  - Update `PurchaseHeaders` to use `TransactionDetails` via `HeaderId` + `TransactionType = GRV/RTS`
- [ ] **7.2.3** Refactor `ProductionHeaders` to link to unified `TransactionDetails`
  - Remove `ProductionDetails` table (if exists)
  - Update `ProductionHeaders` to use `TransactionDetails` via `HeaderId` + `TransactionType = ProductionInput/ProductionOutput`
- [ ] **7.2.4** Update all repositories to query unified `TransactionDetails`
  - Example: `_context.TransactionDetails.Where(d => d.HeaderId == headerId && d.TransactionType == TransactionType.Sale)`
- [ ] **7.2.5** Update all services to write to unified `TransactionDetails`

**Validation**:
- [ ] All existing transactions migrated to unified details table
- [ ] No orphaned detail records
- [ ] All queries return correct results
- [ ] Unit tests updated and passing (100% pass rate)

---

### 7.3 Implement MovementService

**Tasks**:
- [ ] **7.3.1** Create `IMovementService` interface
  ```csharp
  Task GenerateMovementsAsync(TransactionType transactionType, int headerId);
  Task ReverseMovementsAsync(TransactionType transactionType, int headerId, string reason);
  Task<decimal> CalculateSOHAsync(int productId, DateTime? asOfDate = null);
  Task<IEnumerable<Movement>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);
  ```
- [ ] **7.3.2** Implement `MovementService` class
  - `GenerateMovementsAsync`: Read details, determine movement type/direction, create movements
  - `ReverseMovementsAsync`: Soft-delete movements for cancelled transactions
  - `CalculateSOHAsync`: SUM movements (IN - OUT) for product
  - `GetMovementHistoryAsync`: Query movements by product and date range
- [ ] **7.3.3** Implement movement type/direction mapping
  ```csharp
  private (string movementType, string direction) GetMovementTypeAndDirection(TransactionType transactionType)
  {
      return transactionType switch
      {
          TransactionType.GRV => ("GRV", "IN"),
          TransactionType.Sale => ("Sale", "OUT"),
          TransactionType.ProductionInput => ("ProductionInput", "OUT"),
          TransactionType.ProductionOutput => ("ProductionOutput", "IN"),
          // ... etc
      };
  }
  ```
- [ ] **7.3.4** Register `MovementService` in DI container (`Program.cs`)
  - `builder.Services.AddScoped<IMovementService, MovementService>();`

**Validation**:
- [ ] `MovementService` compiles without errors
- [ ] All methods unit tested (10+ tests covering all transaction types)
- [ ] Test coverage >80%

---

### 7.4 Integrate Movement Generation

**Tasks**:
- [ ] **7.4.1** Update `TransactionService.CheckoutAsync` to call `MovementService.GenerateMovementsAsync` after saving transaction
- [ ] **7.4.2** Update `PurchaseService` (GRV/RTS) to call `MovementService.GenerateMovementsAsync`
- [ ] **7.4.3** Update `ProductionService` (batch input/output) to call `MovementService.GenerateMovementsAsync`
- [ ] **7.4.4** Update `InventoryService` (transfers, adjustments) to call `MovementService.GenerateMovementsAsync`
- [ ] **7.4.5** Add error handling (transaction rollback if movement generation fails)

**Validation**:
- [ ] Retail sale creates OUT movements (test with sample transaction)
- [ ] GRV creates IN movements (test with sample GRV)
- [ ] Production creates IN/OUT movements (test with sample batch)
- [ ] Movements link correctly to source transactions (HeaderId, DetailId populated)
- [ ] All integration tests passing

---

### 7.5 Test Movement Architecture

**Tasks**:
- [ ] **7.5.1** Create test: Retail sale generates correct movements
- [ ] **7.5.2** Create test: Retail refund reverses movements (creates IN movements)
- [ ] **7.5.3** Create test: GRV generates IN movements with batch assignment
- [ ] **7.5.4** Create test: RTS generates OUT movements
- [ ] **7.5.5** Create test: Production input generates OUT movements
- [ ] **7.5.6** Create test: Production output generates IN movements
- [ ] **7.5.7** Create test: SOH calculation accurate across all movement types
- [ ] **7.5.8** Create test: Movement soft delete (transaction cancellation)
- [ ] **7.5.9** Performance test: Generate 10,000 movements, verify <5 seconds
- [ ] **7.5.10** Performance test: Calculate SOH with 100,000 movements, verify <500ms

**Validation**:
- [ ] All tests passing (100% pass rate)
- [ ] Test coverage for movement generation >90%
- [ ] Performance acceptable

---

### PHASE 7 SUCCESS CRITERIA ✅

- [ ] Unified `TransactionDetails` table operational
- [ ] All transaction types write to unified details
- [ ] `MovementService` generates movements consistently (all transaction types)
- [ ] SOH calculated accurately from movement ledger
- [ ] Performance acceptable (<500ms SOH query with 100K movements)
- [ ] All unit + integration tests passing (100%)
- [ ] Build status: 0 errors, <5 warnings
- [ ] Documentation updated (architecture diagrams, code comments)

---

## PHASE 8: BATCH & SERIAL NUMBER SYSTEM

**Week**: 2
**Priority**: 🔴 CRITICAL
**Target**: Enterprise-grade batch/SN generation with embedded traceability

### 8.1 Batch Number System

**Tasks**:
- [ ] **8.1.1** Define batch number format
  - Format: `SSTTYYYYMMDDNNNN` (16 digits)
  - `SS` = Site ID (01-99)
  - `TT` = Batch Type (01=Cultivation, 02=Production, 03=Retail Production)
  - `YYYYMMDD` = Date
  - `NNNN` = Sequence number (0001-9999)
  - Example: `0102202501010001` = Site 01, Production batch, 2025-01-01, sequence 0001
- [ ] **8.1.2** Create `Batch` entity
  - Columns: `BatchId` (PK), `BatchNumber` (unique), `BatchType`, `SiteId`, `CreatedDate`, `Status`, `SourceBatchNumber` (FK for traceability), `Notes`
- [ ] **8.1.3** Create `IBatchNumberGeneratorService` interface
  ```csharp
  string GenerateBatchNumber(int siteId, BatchType batchType);
  Task<Batch> CreateBatchAsync(int siteId, BatchType batchType, string? sourceBatchNumber = null);
  Task<Batch?> GetBatchAsync(string batchNumber);
  ```
- [ ] **8.1.4** Implement `BatchNumberGeneratorService`
  - Generate batch number with embedded metadata
  - Ensure uniqueness (check database before returning)
  - Thread-safe (use database sequence or lock)
- [ ] **8.1.5** Create `BatchRepository` (DAL)
  - CRUD operations
  - `GetByBatchNumberAsync`, `GetActiveByTypeAsync`, `GetBatchHistoryAsync`
- [ ] **8.1.6** Register services in DI container

**Validation**:
- [ ] Batch numbers generated with correct format
- [ ] Batch numbers unique (no duplicates)
- [ ] Thread-safe (test with concurrent requests)
- [ ] Unit tests passing (10+ tests)

---

### 8.2 Serial Number System

**Tasks**:
- [ ] **8.2.1** Define serial number formats
  - **Full SN** (28 digits): `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC`
    - `SSSSS` = Site + Strain (5 digits)
    - `TT` = Product Type (01=Flower, 02=Pre-roll, 03=Edible, 04=Vape)
    - `YYYYMMDD` = Date
    - `BBBBB` = Batch sequence (5 digits)
    - `UUUUU` = Unit sequence (5 digits)
    - `WWWW` = Weight in grams (4 digits, e.g., 0100 = 1.00g)
    - `QC` = Check digit (2 digits, Luhn algorithm)
  - **Short SN** (13 digits): `SSYYMMDDNNNNN`
    - For barcodes (EAN-13 compatible)
    - `SS` = Site ID
    - `YYMMDD` = Date
    - `NNNNN` = Sequence
- [ ] **8.2.2** Create `SerialNumber` entity
  - Columns: `SerialNumberId` (PK), `SerialNumber` (unique), `ProductId`, `BatchNumber`, `SiteId`, `CreatedDate`, `Status`, `SoldDate`, `CustomerId`
- [ ] **8.2.3** Create `ISerialNumberGeneratorService` interface
  ```csharp
  string GenerateFullSerialNumber(int siteId, int strainId, ProductType productType, string batchNumber, int unitSequence, decimal weight);
  string GenerateShortSerialNumber(int siteId);
  Task<SerialNumber> CreateSerialNumberAsync(int productId, string batchNumber, decimal weight);
  Task<SerialNumber?> GetBySerialNumberAsync(string serialNumber);
  Task MarkAsSoldAsync(string serialNumber, int customerId, DateTime soldDate);
  ```
- [ ] **8.2.4** Implement `SerialNumberGeneratorService`
  - Generate full SN with embedded metadata
  - Generate short SN for barcodes
  - Calculate check digit (Luhn algorithm)
  - Ensure uniqueness
- [ ] **8.2.5** Create `SerialNumberRepository` (DAL)
  - CRUD operations
  - `GetBySerialNumberAsync`, `GetAvailableForSaleAsync`, `GetSoldByCustomerAsync`
- [ ] **8.2.6** Register services in DI container

**Validation**:
- [ ] Full SNs generated with correct format (28 digits)
- [ ] Short SNs generated with correct format (13 digits)
- [ ] Check digit calculation correct (Luhn algorithm)
- [ ] SNs unique (no duplicates)
- [ ] Unit tests passing (15+ tests)

---

### 8.3 Integration with Production Module

**Tasks**:
- [ ] **8.3.1** Update `ProductionBatchService` to generate batch numbers on batch creation
  - Call `BatchNumberGeneratorService.GenerateBatchNumber` when creating production batch
  - Store batch number in `ProductionBatch.BatchNumber`
- [ ] **8.3.2** Update `ProcessingStepService` to link steps to parent batch
  - Each step records `BatchNumber` (link to parent batch)
  - Track inputs/outputs per step
- [ ] **8.3.3** Update `RetailProductionService` to generate SNs on packaging step
  - When packaging finished goods (pre-rolls, flower packs), generate SN for each unit
  - Link SN to batch number
  - Store SNs in `SerialNumber` table
- [ ] **8.3.4** Add batch number to `TransactionDetails` when creating movements
  - All production outputs include batch number
  - All production inputs link to source batch number

**Validation**:
- [ ] Production batches created with correct batch numbers
- [ ] Processing steps linked to parent batch
- [ ] Serial numbers generated on packaging (one per unit)
- [ ] SNs traceable to source batch
- [ ] Unit tests passing

---

### 8.4 Integration with Retail POS Module

**Tasks**:
- [ ] **8.4.1** Update POS scanning to accept serial numbers
  - Scan full SN (28 digits) or short SN (13 digits)
  - Look up product via `SerialNumberRepository.GetBySerialNumberAsync`
  - Add product to cart with SN attached
- [ ] **8.4.2** Update checkout to record SNs in `TransactionDetails`
  - Store SN in `TransactionDetails.SerialNumber` column
  - Mark SN as sold via `SerialNumberService.MarkAsSoldAsync`
- [ ] **8.4.3** Update receipt to print batch numbers and SNs
  - Display batch number for all items
  - Display SN for serialized items (edibles, vapes, packaged flower)
- [ ] **8.4.4** Add batch/SN traceability UI
  - Page: "Trace Product" - Enter SN or batch number, show full history (source batch → steps → sale)

**Validation**:
- [ ] POS can scan and lookup SNs
- [ ] Checkout records SNs correctly
- [ ] SNs marked as sold after checkout
- [ ] Receipt displays batch/SN
- [ ] Traceability query works (SN → batch → steps → source)

---

### 8.5 Test End-to-End Traceability

**Tasks**:
- [ ] **8.5.1** Create test: Cultivation → Production → Retail traceability
  - Create harvest batch (HARVEST-2025-001)
  - Create production batch (BATCH-2025-003) using harvest batch as input
  - Process through steps (bucking → milling → pre-roll → packaging)
  - Generate SNs on packaging
  - Sell pre-roll via POS (scan SN)
  - Trace SN back to harvest batch (verify full chain)
- [ ] **8.5.2** Create test: Batch recall simulation
  - Identify all SNs linked to specific batch
  - Identify all sales linked to those SNs
  - Verify customer contact info retrievable (for recall notification)
- [ ] **8.5.3** Create test: SOH by batch number
  - Calculate SOH grouped by batch number
  - Verify accuracy (movements correctly link to batches)
- [ ] **8.5.4** Performance test: Lookup SN with 100K+ SNs in database (<100ms)

**Validation**:
- [ ] Full traceability demonstrated (seed-to-sale)
- [ ] Batch recall query functional
- [ ] SOH by batch accurate
- [ ] Performance acceptable
- [ ] All tests passing

---

### PHASE 8 SUCCESS CRITERIA ✅

- [ ] Batch numbers generated with embedded metadata (site, type, date, sequence)
- [ ] Serial numbers generated (full 28-digit + short 13-digit formats)
- [ ] Check digits calculated correctly
- [ ] Production batches link to source batches
- [ ] Processing steps linked to parent batch
- [ ] SNs generated on packaging (one per unit)
- [ ] POS can scan and validate SNs
- [ ] SNs marked as sold after checkout
- [ ] Receipts display batch/SN
- [ ] Full traceability operational (plant → batch → step → SN → sale)
- [ ] Batch recall query functional
- [ ] All unit + integration tests passing (100%)
- [ ] Build status: 0 errors, <5 warnings

---

## PHASE 9: RETAIL POS COMPLETION

**Week**: 3
**Priority**: 🔴 CRITICAL
**Target**: 85-90% POS feature completeness

### 9.1 Barcode & Serial Number Scanning

**Tasks**:
- [ ] **9.1.1** Implement barcode scanning component
  - Support EAN-13, Code128, QR codes
  - Input field that captures scanner input
  - Lookup product by EAN or SKU
- [ ] **9.1.2** Implement SN scanning component
  - Scan full SN (28 digits) or short SN (13 digits)
  - Lookup product via `SerialNumberRepository`
  - Validate SN status (available for sale, not sold, not expired)
  - Add to cart with SN attached
- [ ] **9.1.3** Add product quick search (manual fallback)
  - Search by name, SKU, category
  - Display product image, price, SOH
- [ ] **9.1.4** Add scale integration (stub for future)
  - Detect scale input (weight in grams)
  - Calculate price based on weight (for bulk flower)
  - **PoC**: Stub implementation (manual weight entry)

**Validation**:
- [ ] Barcode scanning works (test with real barcode scanner)
- [ ] SN scanning works (test with generated SNs)
- [ ] Product quick search functional
- [ ] Scale integration stubbed (manual entry works)

---

### 9.2 Line-Level & Header-Level Discounts

**Tasks**:
- [ ] **9.2.1** Implement line-level discount UI
  - Button on each cart line: "Apply Discount"
  - Modal: Enter discount (% or R amount)
  - Recalculate line total and VAT
- [ ] **9.2.2** Implement line-level discount logic in `VATCalculationService`
  - If discount applied, reduce line total
  - Recalculate VAT on discounted amount
  - Example: R115 item with 10% discount = R103.50 total, VAT = R13.50
- [ ] **9.2.3** Implement header-level discount UI
  - Button above checkout: "Apply Transaction Discount"
  - Modal: Enter discount (% or R amount)
  - Prorate discount across all lines proportionally
- [ ] **9.2.4** Implement header-level discount logic in `VATCalculationService`
  - Prorate discount: `lineDiscount = (lineTotal / transactionSubtotal) * headerDiscount`
  - Recalculate VAT per line after proration
- [ ] **9.2.5** Update receipt to show discounts
  - Display line-level discounts per line
  - Display header-level discount at transaction level
  - Show original price, discount, final price

**Validation**:
- [ ] Line discount applied correctly (VAT recalculated)
- [ ] Header discount prorated correctly across lines
- [ ] Receipt shows discounts accurately
- [ ] Unit tests for discount scenarios passing (10+ tests)

---

### 9.3 Multi-Tender Checkout

**Tasks**:
- [ ] **9.3.1** Implement multi-tender UI
  - After entering transaction total, show payment panel
  - Buttons: Cash, Card, EFT, Mobile Payment
  - Allow multiple payment methods (e.g., R200 cash + R300 card)
- [ ] **9.3.2** Implement cash tender logic
  - Input: Amount tendered
  - Calculate change
  - Validate tendered >= transaction total
- [ ] **9.3.3** Implement card tender logic
  - Integration with Yoco/iKhokha/Zapper (stub for PoC)
  - **PoC**: Assume card payment successful (no actual integration)
  - Record payment method and amount
- [ ] **9.3.4** Implement EFT tender logic
  - Generate EFT reference number
  - Record EFT payment (manual confirmation)
- [ ] **9.3.5** Implement mobile payment tender logic
  - Display QR code for SnapScan/Zapper (stub for PoC)
  - **PoC**: Manual confirmation (cashier clicks "Paid")
- [ ] **9.3.6** Implement split tender validation
  - Ensure total payments >= transaction total
  - Allow overpayment (record as change)
  - Prevent underpayment (show warning)
- [ ] **9.3.7** Update `Payment` entity to support multiple payments per transaction
  - Link multiple `Payment` records to one `TransactionHeader` (1:many)

**Validation**:
- [ ] Cash payment calculates change correctly
- [ ] Card payment records correctly (stubbed)
- [ ] EFT payment records reference number
- [ ] Mobile payment records correctly (stubbed)
- [ ] Split tender validated (multiple payments sum to total)
- [ ] Receipt shows all payment methods

---

### 9.4 Refund Workflow

**Tasks**:
- [ ] **9.4.1** Create refund UI page (`POSRefund.razor`)
  - Input: Original transaction number or scan receipt barcode
  - Lookup original transaction
  - Display original transaction details (items, prices, totals)
- [ ] **9.4.2** Implement full refund logic
  - Select "Full Refund"
  - Create refund transaction (negative totals)
  - Link to original transaction (`OriginalTransactionId`)
  - Generate refund transaction number (RFND-YYYYMMDD-XXX)
  - Create IN movements (stock returning)
  - Mark SNs as returned (if applicable)
  - Process refund payment (cash/card reversal)
- [ ] **9.4.3** Implement partial refund logic
  - Select specific items to refund (checkboxes)
  - Allow partial quantity (e.g., refund 2 out of 5 units)
  - Allow partial amount (e.g., damaged goods - refund 50%)
  - Create refund transaction for selected items only
  - Generate IN movements for returned items only
- [ ] **9.4.4** Update `TransactionService` to support refunds
  - `CreateRefundAsync(originalTransactionId, refundItems, refundReason, refundMethod)`
  - Validate refund items exist in original transaction
  - Calculate refund total
  - Create refund header and details
  - Call `MovementService.GenerateMovementsAsync(TransactionType.Refund, refundHeaderId)`
- [ ] **9.4.5** Generate credit note (receipt for refund)
  - Display original transaction number
  - List refunded items
  - Show refund totals (negative)
  - Print/email to customer

**Validation**:
- [ ] Full refund creates correct negative transaction
- [ ] Partial refund calculates correct amounts
- [ ] IN movements created for refunded items
- [ ] SNs marked as returned
- [ ] SOH increases correctly
- [ ] Credit note generated
- [ ] Unit tests passing (10+ refund scenarios)

---

### 9.5 Cash Drop & Cash Out

**Tasks**:
- [ ] **9.5.1** Create `CashDrop` entity
  - Columns: `CashDropId` (PK), `Amount`, `Reason`, `CashierId`, `Timestamp`
- [ ] **9.5.2** Implement cash drop UI
  - Button: "Cash Drop"
  - Modal: Enter amount, reason (e.g., "Excess cash to safe")
  - Record cash drop
  - **Note**: Cash drop does NOT affect inventory (financial only)
- [ ] **9.5.3** Create `CashOut` entity
  - Columns: `CashOutId` (PK), `CashierId`, `StartTime`, `EndTime`, `ExpectedCash`, `ActualCash`, `ExpectedCard`, `ActualCard`, `Variance`, `Notes`
- [ ] **9.5.4** Implement cash out (end-of-shift reconciliation) UI
  - Button: "Cash Out"
  - Calculate expected cash (sum of cash sales - cash drops)
  - Input actual cash counted
  - Calculate variance (expected - actual)
  - Input card/EFT totals (expected vs actual)
  - Generate cash out report

**Validation**:
- [ ] Cash drop recorded correctly
- [ ] Cash out calculation accurate (expected vs actual)
- [ ] Variance tracked
- [ ] Cash out report generated

---

### 9.6 Transaction Cancellation

**Tasks**:
- [ ] **9.6.1** Implement cancel before payment
  - Button: "Cancel Transaction"
  - Clear cart
  - No movements created
- [ ] **9.6.2** Implement cancel after payment (requires manager override)
  - Button: "Cancel Completed Transaction" (hidden, requires manager)
  - Modal: Enter manager PIN, reason for cancellation
  - Soft-delete transaction (set `Status = Cancelled`)
  - Soft-delete movements (set `IsDeleted = true`)
  - Create reversal entries in audit log
  - **Note**: Cannot cancel if stock already consumed (e.g., sold to customer who left)
- [ ] **9.6.3** Add transaction status tracking
  - Enum: `Pending`, `Completed`, `Cancelled`, `Refunded`
  - Update UI to show status
- [ ] **9.6.4** Add audit logging for cancellations
  - Log cancellation reason, manager ID, timestamp

**Validation**:
- [ ] Cancel before payment clears cart (no movements)
- [ ] Cancel after payment requires manager override
- [ ] Cancelled transactions soft-deleted (not hard-deleted)
- [ ] Movements soft-deleted (SOH recalculated correctly)
- [ ] Audit log records cancellation

---

### 9.7 Age Verification Enhancement

**Tasks**:
- [ ] **9.7.1** Implement ID scanning (South African ID cards)
  - SA ID format: `YYMMDDGSSSCAZ` (13 digits)
  - Extract date of birth from first 6 digits
  - Calculate age
  - Validate age >= 18
- [ ] **9.7.2** Implement age verification UI
  - Before checkout, prompt: "Verify Customer Age"
  - Option 1: Scan ID barcode
  - Option 2: Manual entry (DOB)
  - Option 3: Cashier override (manager PIN required)
- [ ] **9.7.3** Enforce age verification
  - Cannot complete checkout without age verification
  - Log age verification attempt (timestamp, cashier, method, outcome)
- [ ] **9.7.4** Link sale to customer record (optional)
  - If ID scanned, create/update customer record
  - Link transaction to customer (for loyalty/compliance)
  - Track purchase history (for possession limit warnings)

**Validation**:
- [ ] ID scanning extracts DOB correctly
- [ ] Age calculation correct (verify test cases)
- [ ] Age verification enforced (cannot checkout without verification)
- [ ] Audit log records verification
- [ ] Purchase history tracked (for possession limits)

---

### 9.8 Compliant Receipt Generation

**Tasks**:
- [ ] **9.8.1** Update receipt template
  - **Required fields** (SARS + Cannabis Act):
    - Store name, address, VAT number
    - Transaction number (unique)
    - Date & time
    - Cashier name/ID
    - List of items (description, qty, unit price, line total)
    - **Batch numbers** (traceability - SAHPRA)
    - **Serial numbers** (serialized items - SAHPRA)
    - Discounts (if applied)
    - VAT breakdown (VAT-exclusive, VAT amount, VAT-inclusive)
    - Payment method(s)
    - Change given (if cash)
    - Legal disclaimer (cannabis use warnings, age restriction notice)
- [ ] **9.8.2** Implement receipt printing
  - Generate PDF receipt
  - Send to thermal printer (integration stub for PoC)
  - Option to email receipt (capture customer email)
- [ ] **9.8.3** Implement credit note (refund receipt)
  - Similar format to receipt
  - Marked as "CREDIT NOTE"
  - Reference original transaction number
  - Negative totals

**Validation**:
- [ ] Receipt contains all required fields
- [ ] Batch/SN displayed correctly
- [ ] VAT breakdown accurate
- [ ] Receipt printable (PDF generated)
- [ ] Email receipt functional (if customer provides email)
- [ ] Credit note generated for refunds

---

### 9.9 Movement Generation Optimization

**Tasks**:
- [ ] **9.9.1** Profile movement generation performance
  - Test checkout with 10, 50, 100 items
  - Measure time to generate movements
  - Identify bottlenecks
- [ ] **9.9.2** Optimize batch insert
  - Use `AddRangeAsync` instead of individual inserts
  - Single `SaveChangesAsync` call
- [ ] **9.9.3** Implement async movement generation
  - Don't block UI while generating movements
  - Show "Processing..." spinner
  - Background task for movement generation
- [ ] **9.9.4** Add error handling and retry logic
  - If movement generation fails, rollback transaction
  - Retry up to 3 times
  - Log errors to audit log

**Validation**:
- [ ] Movement generation <1 second for 100 items
- [ ] Batch insert operational
- [ ] Async generation doesn't block UI
- [ ] Error handling prevents partial transactions

---

### PHASE 9 SUCCESS CRITERIA ✅

- [ ] Barcode/QR scanning operational (EAN, SN)
- [ ] Line-level and header-level discounts functional
- [ ] Multi-tender checkout complete (Cash, Card, EFT, Mobile)
- [ ] Refund workflow complete (full & partial)
- [ ] Cash drop/cash out functional
- [ ] Transaction cancellation with manager override
- [ ] Age verification enhanced (ID scanning)
- [ ] Compliant receipts generated (with batch/SN, VAT breakdown)
- [ ] Movement generation optimized (<1s for 100 items)
- [ ] All unit + integration tests passing (100%)
- [ ] Performance acceptable (checkout <3 seconds)
- [ ] Build status: 0 errors, <5 warnings

---

## PHASE 10: PRODUCTION DAL EXPANSION

**Week**: 4
**Priority**: 🟡 MEDIUM
**Target**: Complete Retail Production DAL (75%) + stub general production (45%)

### 10.1 ProductionBatch Repository

**Tasks**:
- [ ] **10.1.1** Expand `IProductionBatchRepository` interface
  ```csharp
  Task<ProductionBatch?> GetByBatchNumberAsync(string batchNumber);
  Task<IEnumerable<ProductionBatch>> GetActiveBatchesAsync();
  Task<IEnumerable<ProductionBatch>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
  Task<IEnumerable<ProductionBatch>> GetByProductTypeAsync(string productType);
  Task<ProductionBatch?> GetWithStepsAsync(int batchId);
  ```
- [ ] **10.1.2** Implement `ProductionBatchRepository` methods
  - Use `Include` for eager loading (steps, QC, lab tests)
  - Optimize queries with indexes
- [ ] **10.1.3** Add business queries
  - `GetBatchesNeedingQCAsync()` - Batches awaiting QC
  - `GetBatchesNeedingLabTestAsync()` - Batches awaiting lab testing
  - `GetCompletedBatchesAsync(DateTime startDate, DateTime endDate)` - Completed batches for reporting

**Validation**:
- [ ] All repository methods functional
- [ ] Eager loading works (no N+1 queries)
- [ ] Unit tests passing (10+ tests)

---

### 10.2 ProcessingStep Repository

**Tasks**:
- [ ] **10.2.1** Create `IProcessingStepRepository` interface
  ```csharp
  Task<ProcessingStep> CreateAsync(ProcessingStep step);
  Task<ProcessingStep?> GetByIdAsync(int stepId);
  Task<IEnumerable<ProcessingStep>> GetByBatchIdAsync(int batchId);
  Task<ProcessingStep> UpdateAsync(ProcessingStep step);
  Task CompleteStepAsync(int stepId, decimal outputQuantity, decimal outputMass, decimal lossMass, string? lossReason);
  ```
- [ ] **10.2.2** Implement `ProcessingStepRepository`
  - CRUD operations
  - `CompleteStepAsync` - Mark step as completed, record outputs/losses
- [ ] **10.2.3** Add step validation
  - Cannot complete step if previous step not completed
  - Cannot delete step if subsequent steps exist
  - Output mass <= input mass

**Validation**:
- [ ] All repository methods functional
- [ ] Step validation enforced
- [ ] Unit tests passing (10+ tests)

---

### 10.3 Retail Production Repository & Service

**Tasks**:
- [ ] **10.3.1** Create `IRetailProductionRepository` interface
  ```csharp
  Task<RetailProductionBatch> CreatePreRollBatchAsync(int sourceFlowerBatchId, decimal inputMass);
  Task<RetailProductionBatch> CreatePackagedFlowerBatchAsync(int sourceFlowerBatchId, decimal inputMass);
  Task RecordStepAsync(int batchId, string stepName, decimal inputMass, decimal outputMass, decimal lossMass);
  Task AssignSerialNumbersAsync(int batchId, List<string> serialNumbers);
  ```
- [ ] **10.3.2** Implement `RetailProductionRepository`
  - Focus on pre-roll and packaged flower workflows
  - Step-based recording (milling → filling → capping → packaging)
  - SN assignment on packaging step
- [ ] **10.3.3** Create `IRetailProductionService` interface
  ```csharp
  Task<ProductionBatchDto> StartPreRollProductionAsync(StartPreRollProductionDto dto);
  Task<ProcessingStepDto> CompleteStepAsync(int batchId, int stepNumber, CompleteStepDto dto);
  Task<List<string>> PackageAndGenerateSerialNumbersAsync(int batchId, int quantity);
  Task<ProductionReportDto> GetBatchReportAsync(int batchId);
  ```
- [ ] **10.3.4** Implement `RetailProductionService`
  - Orchestrate pre-roll production workflow
  - Generate movements (input OUT, output IN)
  - Call `SerialNumberGeneratorService` on packaging
  - Calculate yields and efficiency

**Validation**:
- [ ] Pre-roll production workflow complete (start → steps → packaging)
- [ ] Movements generated correctly (input OUT, output IN)
- [ ] SNs assigned to finished goods
- [ ] Yield calculations accurate
- [ ] Unit tests passing (15+ tests covering full workflow)

---

### 10.4 Production Movement Generation

**Tasks**:
- [ ] **10.4.1** Integrate production with `MovementService`
  - When recording production input, call `MovementService.GenerateMovementsAsync(TransactionType.ProductionInput, batchId)`
  - When recording production output, call `MovementService.GenerateMovementsAsync(TransactionType.ProductionOutput, batchId)`
- [ ] **10.4.2** Create production transaction details
  - Input: Create `TransactionDetail` with `TransactionType = ProductionInput`, `ProductId = sourceProduct`, `Quantity = inputMass`, `BatchNumber = sourceBatch`
  - Output: Create `TransactionDetail` with `TransactionType = ProductionOutput`, `ProductId = finishedProduct`, `Quantity = outputQuantity`, `BatchNumber = productionBatch`
- [ ] **10.4.3** Test end-to-end production flow
  - Start pre-roll batch (10kg flower input)
  - Complete milling step (9.8kg output)
  - Complete filling step (9,500 pre-rolls output)
  - Complete capping step (9,400 pre-rolls output)
  - Complete packaging step (940 packs output, SNs assigned)
  - Verify movements: OUT -10kg flower, IN +940 packs
  - Verify SOH: Flower decreased, Pre-rolls increased

**Validation**:
- [ ] Production inputs create OUT movements
- [ ] Production outputs create IN movements
- [ ] Movements link to batch number
- [ ] SOH updated correctly
- [ ] Integration tests passing

---

### 10.5 Production Reporting

**Tasks**:
- [ ] **10.5.1** Create production reports
  - Batch summary report (inputs, outputs, losses, yields)
  - Daily production report (batches completed today)
  - Weekly efficiency report (average yields by product type)
- [ ] **10.5.2** Implement reporting queries
  ```csharp
  Task<BatchSummaryDto> GetBatchSummaryAsync(int batchId);
  Task<List<BatchSummaryDto>> GetDailyProductionReportAsync(DateTime date);
  Task<EfficiencyReportDto> GetWeeklyEfficiencyReportAsync(DateTime startDate);
  ```
- [ ] **10.5.3** Create reporting UI (basic)
  - Page: "Production Reports"
  - Select report type
  - Display results in table
  - Export to CSV (optional)

**Validation**:
- [ ] Reports generate correct data
- [ ] Yields calculated accurately
- [ ] Efficiency metrics correct
- [ ] UI displays reports

---

### 10.6 Stub General Production & Cultivation

**Tasks**:
- [ ] **10.6.1** Create stub `ManufacturingService` (extraction, formulation)
  - Basic entity structure (ManufacturingBatch, ManufacturingStep)
  - Stub methods (CreateExtractionBatchAsync, CreateFormulationBatchAsync)
  - **Note**: Full implementation deferred to post-PoC
- [ ] **10.6.2** Create stub `CultivationService`
  - Basic entity structure (Plant, GrowCycle, HarvestBatch) ✅ Already created in Phase 5
  - Stub methods (CreatePlantAsync, CreateGrowCycleAsync, RecordHarvestAsync)
  - **Note**: Full implementation deferred to post-PoC
- [ ] **10.6.3** Link harvest batches to production
  - When creating production batch, specify source harvest batch
  - Traceability: Production batch → Harvest batch → Grow cycle → Plants

**Validation**:
- [ ] Stub services created
- [ ] Basic entity structure operational
- [ ] Harvest batch linkage functional

---

### PHASE 10 SUCCESS CRITERIA ✅

- [ ] `ProductionBatchRepository` expanded with business queries
- [ ] `ProcessingStepRepository` complete
- [ ] `RetailProductionRepository` complete (pre-rolls, packaged flower)
- [ ] `RetailProductionService` complete (75% feature completeness)
- [ ] Pre-roll production workflow functional (milling → filling → capping → packaging)
- [ ] Packaged flower workflow functional (selection → weighing → labeling)
- [ ] Production movements generated correctly (inputs OUT, outputs IN)
- [ ] SNs assigned to finished goods
- [ ] Yield and efficiency calculations accurate
- [ ] Production reports operational (batch summary, daily, weekly efficiency)
- [ ] General production and cultivation stubbed (35-45% completeness)
- [ ] All unit + integration tests passing (100%)
- [ ] Build status: 0 errors, <5 warnings

---

## PHASE 11: INVENTORY SOH ENGINE

**Week**: 5
**Priority**: 🔴 CRITICAL
**Target**: Performant multi-table SOH calculation with caching

### 11.1 StockLevelsCache Table & Service

**Tasks**:
- [ ] **11.1.1** Create `StockLevelsCache` table
  - Columns: `StockLevelCacheId` (PK), `ProductId`, `LocationId`, `BatchNumber`, `QuantityOnHand`, `MassOnHand`, `ValueOnHand`, `LastCalculated`, `LastMovementId`
  - Indexes: `IX_StockLevelsCache_ProductId_LocationId`, `IX_StockLevelsCache_BatchNumber`
- [ ] **11.1.2** Create `ISOHCalculationService` interface
  ```csharp
  Task<decimal> CalculateSOHAsync(int productId, int? locationId = null, DateTime? asOfDate = null);
  Task<Dictionary<int, decimal>> CalculateSOHBulkAsync(List<int> productIds);
  Task RefreshCacheAsync(int productId);
  Task RefreshCacheIncrementalAsync(int productId);
  Task ReconcileAsync(int productId);
  ```
- [ ] **11.1.3** Implement `SOHCalculationService`
  - `CalculateSOHAsync`: Query movements, SUM (IN - OUT)
  - `RefreshCacheAsync`: Full recalculation, update cache
  - `RefreshCacheIncrementalAsync`: Calculate delta since last cache update, update cache
  - `ReconcileAsync`: Compare cache vs actual movements, flag discrepancies
- [ ] **11.1.4** Implement automatic cache refresh
  - Whenever movements created, call `RefreshCacheIncrementalAsync` for affected products
  - Background job: Full cache refresh nightly (reconciliation)
- [ ] **11.1.5** Register services in DI container

**Validation**:
- [ ] Cache table created
- [ ] SOH calculation accurate (test against movements)
- [ ] Incremental refresh functional
- [ ] Full refresh (reconciliation) functional
- [ ] Unit tests passing (15+ tests)

---

### 11.2 StockTransfer Workflow

**Tasks**:
- [ ] **11.2.1** Create `StockTransfer` entity
  - Columns: `StockTransferId` (PK), `TransferNumber`, `FromLocationId`, `ToLocationId`, `TransferDate`, `Status`, `Notes`
- [ ] **11.2.2** Create `StockTransferDetail` (or use unified `TransactionDetails`)
  - Link to unified `TransactionDetails` with `TransactionType = TransferOut/TransferIn`
- [ ] **11.2.3** Create `IStockTransferRepository` interface
  ```csharp
  Task<StockTransfer> CreateTransferAsync(StockTransfer transfer);
  Task<StockTransfer?> GetByIdAsync(int transferId);
  Task<StockTransfer?> GetByTransferNumberAsync(string transferNumber);
  Task<IEnumerable<StockTransfer>> GetPendingTransfersAsync();
  Task ConfirmReceiptAsync(int transferId);
  ```
- [ ] **11.2.4** Implement `StockTransferRepository`
- [ ] **11.2.5** Create `IStockTransferService` interface
  ```csharp
  Task<StockTransferDto> CreateTransferAsync(CreateStockTransferDto dto);
  Task<StockTransferDto> ConfirmReceiptAsync(int transferId);
  Task<List<StockTransferDto>> GetPendingTransfersAsync(int locationId);
  ```
- [ ] **11.2.6** Implement `StockTransferService`
  - Create transfer: Generate TransferOut movements at source location
  - Confirm receipt: Generate TransferIn movements at destination location
- [ ] **11.2.7** Create stock transfer UI
  - Page: "Stock Transfers"
  - Create transfer: Select from location, to location, products, quantities
  - Confirm receipt: List pending transfers, confirm received

**Validation**:
- [ ] Transfer creates OUT movements at source location
- [ ] Receipt creates IN movements at destination location
- [ ] SOH decreases at source, increases at destination
- [ ] Unit tests passing (10+ tests)

---

### 11.3 StockAdjustment Workflow

**Tasks**:
- [ ] **11.3.1** Create `StockAdjustment` entity
  - Columns: `StockAdjustmentId` (PK), `AdjustmentNumber`, `AdjustmentType`, `Reason`, `AdjustmentDate`, `LocationId`, `ApprovedBy`, `Notes`
  - AdjustmentType: `StocktakeCorrection`, `Shrinkage`, `Damage`, `Expiry`, `Found`, `Other`
- [ ] **11.3.2** Link to unified `TransactionDetails` with `TransactionType = AdjustmentIn/AdjustmentOut`
- [ ] **11.3.3** Create `IStockAdjustmentRepository` interface
  ```csharp
  Task<StockAdjustment> CreateAdjustmentAsync(StockAdjustment adjustment);
  Task<StockAdjustment?> GetByIdAsync(int adjustmentId);
  Task<IEnumerable<StockAdjustment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
  Task<IEnumerable<StockAdjustment>> GetByReasonAsync(string reason);
  ```
- [ ] **11.3.4** Implement `StockAdjustmentRepository`
- [ ] **11.3.5** Create `IStockAdjustmentService` interface
  ```csharp
  Task<StockAdjustmentDto> CreateAdjustmentAsync(CreateStockAdjustmentDto dto);
  Task<List<StockAdjustmentDto>> GetAdjustmentsAsync(DateTime startDate, DateTime endDate);
  Task<decimal> GetTotalShrinkageAsync(DateTime startDate, DateTime endDate);
  ```
- [ ] **11.3.6** Implement `StockAdjustmentService`
  - Positive adjustment: Generate AdjustmentIn movements
  - Negative adjustment: Generate AdjustmentOut movements
  - Require manager approval (capture manager ID)
- [ ] **11.3.7** Create stock adjustment UI
  - Page: "Stock Adjustments"
  - Create adjustment: Select products, enter adjusted quantities, reason
  - Require manager PIN for approval
  - List historical adjustments

**Validation**:
- [ ] Positive adjustment creates IN movements
- [ ] Negative adjustment creates OUT movements
- [ ] SOH updated correctly
- [ ] Manager approval enforced
- [ ] Unit tests passing (10+ tests)

---

### 11.4 StockCount (Stocktake) Workflow

**Tasks**:
- [ ] **11.4.1** Create `StockCount` entity
  - Columns: `StockCountId` (PK), `StockCountNumber`, `CountDate`, `LocationId`, `Status`, `CountedBy`, `ApprovedBy`, `Notes`
- [ ] **11.4.2** Create `StockCountDetail` entity
  - Columns: `StockCountDetailId` (PK), `StockCountId`, `ProductId`, `BatchNumber`, `ExpectedQty`, `CountedQty`, `VarianceQty`, `Notes`
- [ ] **11.4.3** Create `IStockCountRepository` interface
  ```csharp
  Task<StockCount> CreateStockCountAsync(StockCount stockCount);
  Task<StockCount?> GetByIdAsync(int stockCountId);
  Task<StockCount?> GetWithDetailsAsync(int stockCountId);
  Task RecordCountAsync(int stockCountId, Dictionary<int, decimal> countedQuantities);
  Task ApproveStockCountAsync(int stockCountId, int approverId);
  Task GenerateAdjustmentsAsync(int stockCountId);
  ```
- [ ] **11.4.4** Implement `StockCountRepository`
- [ ] **11.4.5** Create `IStockCountService` interface
  ```csharp
  Task<StockCountDto> StartStockCountAsync(int locationId, List<int> productIds);
  Task<StockCountDto> RecordCountedQuantitiesAsync(int stockCountId, Dictionary<int, decimal> countedQuantities);
  Task<List<VarianceDto>> CalculateVariancesAsync(int stockCountId);
  Task ApproveAndAdjustAsync(int stockCountId, int approverId);
  ```
- [ ] **11.4.6** Implement `StockCountService`
  - Start stocktake: Create `StockCount`, populate with expected SOH
  - Record counts: Update `CountedQty` for each product
  - Calculate variances: `VarianceQty = CountedQty - ExpectedQty`
  - Approve and adjust: Generate stock adjustments for variances, create movements
- [ ] **11.4.7** Create stock count UI
  - Page: "Stock Counts"
  - Start count: Select location, products (or all)
  - Count entry: Scan/enter counted quantities per product
  - Review variances: Display expected vs counted, highlight discrepancies
  - Approve: Manager approves, system generates adjustments

**Validation**:
- [ ] Stocktake records expected SOH correctly
- [ ] Counted quantities recorded
- [ ] Variances calculated accurately
- [ ] Adjustments generated for variances
- [ ] Movements created (IN/OUT based on variance)
- [ ] SOH corrected after approval
- [ ] Unit tests passing (15+ tests)

---

### 11.5 Low-Stock Alerts

**Tasks**:
- [ ] **11.5.1** Add `MinStockLevel` and `ReorderLevel` to `Product` entity
- [ ] **11.5.2** Create `ILowStockAlertService` interface
  ```csharp
  Task<List<LowStockAlertDto>> GetLowStockProductsAsync(int? locationId = null);
  Task<List<LowStockAlertDto>> GetOutOfStockProductsAsync(int? locationId = null);
  Task SendLowStockNotificationsAsync();
  ```
- [ ] **11.5.3** Implement `LowStockAlertService`
  - Query products where `SOH <= ReorderLevel`
  - Generate low-stock alert list
  - Send email notifications (stub for PoC)
- [ ] **11.5.4** Create low-stock alerts UI
  - Dashboard widget: "Low Stock Alerts" (count of products below reorder level)
  - Page: "Low Stock Products" (list of products with SOH, reorder level)

**Validation**:
- [ ] Low-stock query returns correct products
- [ ] Out-of-stock query returns correct products
- [ ] Notifications sent (stubbed)
- [ ] UI displays alerts

---

### 11.6 Inventory Reports

**Tasks**:
- [ ] **11.6.1** Create inventory reports
  - SOH by product (current stock levels)
  - SOH by location (stock at each store/warehouse)
  - SOH by batch number (batch-level stock)
  - Movement history report (all movements for product, date range)
  - Shrinkage report (total adjustments OUT by reason)
  - Stocktake variance report (variances from stock counts)
- [ ] **11.6.2** Implement reporting queries
  ```csharp
  Task<List<SOHReportDto>> GetSOHByProductAsync(int? locationId = null);
  Task<List<SOHReportDto>> GetSOHByLocationAsync();
  Task<List<SOHReportDto>> GetSOHByBatchNumberAsync(string? batchNumber = null);
  Task<List<MovementHistoryDto>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);
  Task<ShrinkageReportDto> GetShrinkageReportAsync(DateTime startDate, DateTime endDate);
  Task<List<VarianceReportDto>> GetStocktakeVariancesAsync(DateTime startDate, DateTime endDate);
  ```
- [ ] **11.6.3** Create inventory reports UI
  - Page: "Inventory Reports"
  - Select report type
  - Filter by location, date range, product category
  - Display results in table
  - Export to CSV

**Validation**:
- [ ] All reports generate correct data
- [ ] Filters work correctly
- [ ] Export to CSV functional
- [ ] UI displays reports

---

### 11.7 Performance Optimization

**Tasks**:
- [ ] **11.7.1** Profile SOH calculation performance
  - Simulate 1M movements
  - Measure SOH query time (target: <200ms)
- [ ] **11.7.2** Implement movement table partitioning
  - Partition by `TransactionDate` (monthly/quarterly)
  - Archive old partitions (>2 years) to separate filegroup
- [ ] **11.7.3** Optimize indexes
  - Composite index: `(ProductId, TransactionDate, Direction) INCLUDE (Quantity, Mass)`
  - Covering indexes for common queries
- [ ] **11.7.4** Implement SOH cache read-through pattern
  - Always read from cache (unless cache miss)
  - Cache miss triggers full SOH calculation and cache write
- [ ] **11.7.5** Implement background cache refresh job
  - Nightly job: Full SOH recalculation for all products
  - Reconcile cache vs actual movements
  - Flag discrepancies for investigation

**Validation**:
- [ ] SOH query <200ms with 1M movements
- [ ] Partitioning operational
- [ ] Cache hit rate >95%
- [ ] Background job functional

---

### PHASE 11 SUCCESS CRITERIA ✅

- [ ] `StockLevelsCache` table created and operational
- [ ] SOH calculation from movements accurate
- [ ] Incremental cache refresh functional
- [ ] Stock transfer workflow complete (OUT at source, IN at destination)
- [ ] Stock adjustment workflow complete (requires manager approval)
- [ ] Stock count (stocktake) workflow complete (variance detection, auto-adjustment)
- [ ] Low-stock alerts operational
- [ ] Inventory reports complete (SOH, movement history, shrinkage, variance)
- [ ] Performance optimized (SOH <200ms with 1M movements)
- [ ] All unit + integration tests passing (100%)
- [ ] Build status: 0 errors, <5 warnings

---

## PHASE 12: PURCHASING WORKFLOW

**Week**: 6
**Priority**: 🟠 HIGH
**Target**: Complete GRV, RTS workflows with movement generation

### 12.1 GRV (Goods Received Voucher) Workflow

**Tasks**:
- [ ] **12.1.1** Create `GRV` entity (or use `PurchaseHeader` with type discriminator)
  - Columns: `GRVId` (PK), `GRVNumber`, `SupplierId`, `OrderId` (FK to PurchaseOrder, nullable), `ReceiveDate`, `ReceivedBy`, `Status`, `Notes`
- [ ] **12.1.2** Link to unified `TransactionDetails` with `TransactionType = GRV`
- [ ] **12.1.3** Create `IGRVRepository` interface
  ```csharp
  Task<GRV> CreateGRVAsync(GRV grv);
  Task<GRV?> GetByIdAsync(int grvId);
  Task<GRV?> GetByGRVNumberAsync(string grvNumber);
  Task<IEnumerable<GRV>> GetBySupplierAsync(int supplierId);
  Task<IEnumerable<GRV>> GetPendingGRVsAsync();
  ```
- [ ] **12.1.4** Implement `GRVRepository`
- [ ] **12.1.5** Create `IGRVService` interface
  ```csharp
  Task<GRVDto> CreateGRVAsync(CreateGRVDto dto);
  Task<GRVDto> AddLineItemAsync(int grvId, AddGRVLineDto dto);
  Task AssignBatchNumberAsync(int grvDetailId, string batchNumber);
  Task AttachCOAAsync(int grvId, byte[] coaPdf);
  Task CompleteGRVAsync(int grvId);
  ```
- [ ] **12.1.6** Implement `GRVService`
  - Create GRV: Initialize header
  - Add line items: Create transaction details
  - Assign batch numbers: Link received stock to supplier batches (traceability)
  - Attach COA: Upload Certificate of Analysis (SAHPRA requirement)
  - Complete GRV: Generate IN movements, update SOH
- [ ] **12.1.7** Create GRV UI
  - Page: "Goods Received"
  - Create GRV: Select supplier, optionally link to purchase order
  - Add items: Scan/search products, enter quantities received
  - Assign batches: Enter supplier batch number for traceability
  - Attach COA: Upload PDF (lab test results)
  - Complete: Generate movements, mark GRV as completed

**Validation**:
- [ ] GRV creates IN movements
- [ ] Batch numbers assigned and traceable
- [ ] COA attached (PDF stored)
- [ ] SOH increased correctly
- [ ] Unit tests passing (15+ tests)

---

### 12.2 RTS (Return to Supplier) Workflow

**Tasks**:
- [ ] **12.2.1** Create `RTS` entity (or use `PurchaseHeader` with type discriminator)
  - Columns: `RTSId` (PK), `RTSNumber`, `SupplierId`, `GRVId` (FK to original GRV, nullable), `ReturnDate`, `ReturnReason`, `ReturnedBy`, `Status`, `Notes`
- [ ] **12.2.2** Link to unified `TransactionDetails` with `TransactionType = RTS`
- [ ] **12.2.3** Create `IRTSRepository` interface
  ```csharp
  Task<RTS> CreateRTSAsync(RTS rts);
  Task<RTS?> GetByIdAsync(int rtsId);
  Task<RTS?> GetByRTSNumberAsync(string rtsNumber);
  Task<IEnumerable<RTS>> GetBySupplierAsync(int supplierId);
  ```
- [ ] **12.2.4** Implement `RTSRepository`
- [ ] **12.2.5** Create `IRTSService` interface
  ```csharp
  Task<RTSDto> CreateRTSAsync(CreateRTSDto dto);
  Task<RTSDto> AddLineItemAsync(int rtsId, AddRTSLineDto dto);
  Task AttachSupportingDocumentsAsync(int rtsId, List<byte[]> documents);
  Task CompleteRTSAsync(int rtsId);
  ```
- [ ] **12.2.6** Implement `RTSService`
  - Create RTS: Initialize header, specify reason (defect, damage, expired)
  - Add line items: Select items to return (can lookup from original GRV)
  - Attach documents: Upload photos, QC reports, contamination evidence
  - Complete RTS: Generate OUT movements, update SOH, notify supplier
- [ ] **12.2.7** Create RTS UI
  - Page: "Returns to Supplier"
  - Create RTS: Select supplier, select reason
  - Add items: Select products to return, quantities, batch numbers
  - Attach evidence: Upload photos/documents
  - Complete: Generate movements, mark RTS as completed

**Validation**:
- [ ] RTS creates OUT movements
- [ ] SOH decreased correctly
- [ ] Supporting documents attached
- [ ] Unit tests passing (10+ tests)

---

### 12.3 Supplier Order Tracking

**Tasks**:
- [ ] **12.3.1** Create `PurchaseOrder` entity
  - Columns: `PurchaseOrderId` (PK), `OrderNumber`, `SupplierId`, `OrderDate`, `ExpectedDeliveryDate`, `Status`, `Total`, `Notes`
  - Status: `Pending`, `Confirmed`, `PartiallyReceived`, `FullyReceived`, `Cancelled`
- [ ] **12.3.2** Create `PurchaseOrderDetail` entity (or use unified `TransactionDetails`)
  - Link to unified `TransactionDetails` with `TransactionType = PurchaseOrder`
- [ ] **12.3.3** Create `IPurchaseOrderRepository` interface
  ```csharp
  Task<PurchaseOrder> CreateOrderAsync(PurchaseOrder order);
  Task<PurchaseOrder?> GetByIdAsync(int orderId);
  Task<IEnumerable<PurchaseOrder>> GetPendingOrdersAsync();
  Task<IEnumerable<PurchaseOrder>> GetBySupplierAsync(int supplierId);
  Task UpdateStatusAsync(int orderId, string status);
  ```
- [ ] **12.3.4** Implement `PurchaseOrderRepository`
- [ ] **12.3.5** Create `IPurchaseOrderService` interface
  ```csharp
  Task<PurchaseOrderDto> CreateOrderAsync(CreatePurchaseOrderDto dto);
  Task<PurchaseOrderDto> ConfirmOrderAsync(int orderId);
  Task<List<PurchaseOrderDto>> GetPendingOrdersAsync();
  Task<PurchaseOrderDto> LinkGRVToOrderAsync(int orderId, int grvId);
  ```
- [ ] **12.3.6** Implement `PurchaseOrderService`
  - Create order: Generate order number, add line items
  - Confirm order: Mark as confirmed (sent to supplier)
  - Link GRV: When goods received, link GRV to order, update order status
  - Auto-update status: If all items received, mark order as `FullyReceived`
- [ ] **12.3.7** Create purchase order UI (basic)
  - Page: "Purchase Orders"
  - Create order: Select supplier, add products, quantities, expected delivery date
  - View orders: List all orders, filter by status
  - Link GRV: When creating GRV, optionally link to existing order

**Validation**:
- [ ] Purchase orders created correctly
- [ ] Orders linked to GRVs
- [ ] Status auto-updated when fully received
- [ ] Unit tests passing (10+ tests)

---

### 12.4 Supplier Invoice Processing

**Tasks**:
- [ ] **12.4.1** Create `SupplierInvoice` entity
  - Columns: `SupplierInvoiceId` (PK), `InvoiceNumber`, `SupplierId`, `GRVId` (FK, nullable), `OrderId` (FK, nullable), `InvoiceDate`, `DueDate`, `Total`, `Status`, `Notes`
  - Status: `Pending`, `Approved`, `Paid`, `Disputed`
- [ ] **12.4.2** Create `ISupplierInvoiceRepository` interface
  ```csharp
  Task<SupplierInvoice> CreateInvoiceAsync(SupplierInvoice invoice);
  Task<SupplierInvoice?> GetByIdAsync(int invoiceId);
  Task<IEnumerable<SupplierInvoice>> GetPendingInvoicesAsync();
  Task<IEnumerable<SupplierInvoice>> GetBySupplierAsync(int supplierId);
  Task ApproveInvoiceAsync(int invoiceId, int approverId);
  Task MarkAsPaidAsync(int invoiceId, DateTime paymentDate, string paymentReference);
  ```
- [ ] **12.4.3** Implement `SupplierInvoiceRepository`
- [ ] **12.4.4** Create `ISupplierInvoiceService` interface
  ```csharp
  Task<SupplierInvoiceDto> CreateInvoiceAsync(CreateSupplierInvoiceDto dto);
  Task<SupplierInvoiceDto> MatchToGRVAsync(int invoiceId, int grvId);
  Task<ThreeWayMatchResultDto> PerformThreeWayMatchAsync(int invoiceId);
  Task ApproveForPaymentAsync(int invoiceId, int approverId);
  ```
- [ ] **12.4.5** Implement `SupplierInvoiceService`
  - Create invoice: Capture invoice details
  - Match to GRV: Link invoice to GRV (verify quantities, prices)
  - Three-way match: Order → GRV → Invoice (verify consistency)
  - Approve: Manager approves invoice for payment
- [ ] **12.4.6** Create supplier invoice UI (basic)
  - Page: "Supplier Invoices"
  - Create invoice: Enter supplier, invoice number, date, total
  - Match to GRV: Select related GRV, system highlights variances
  - Approve: Manager reviews three-way match, approves

**Validation**:
- [ ] Invoices created and linked to GRVs
- [ ] Three-way matching identifies variances
- [ ] Approval workflow functional
- [ ] Unit tests passing (10+ tests)

---

### 12.5 Three-Way Matching

**Tasks**:
- [ ] **12.5.1** Implement three-way match logic
  ```csharp
  public class ThreeWayMatchResultDto
  {
      public bool IsMatch { get; set; }
      public List<VarianceDto> Variances { get; set; }
      public decimal OrderTotal { get; set; }
      public decimal GRVTotal { get; set; }
      public decimal InvoiceTotal { get; set; }
  }
  ```
- [ ] **12.5.2** Compare order, GRV, invoice
  - Quantity variance: Order qty vs GRV qty vs Invoice qty
  - Price variance: Order unit price vs Invoice unit price
  - Total variance: Order total vs GRV total vs Invoice total
- [ ] **12.5.3** Highlight variances
  - If variances exist, flag for manager review
  - Allow manager to approve despite variances (with notes)
- [ ] **12.5.4** Create three-way match report
  - Display order, GRV, invoice side-by-side
  - Highlight variances in red
  - Manager approval required if variances >5%

**Validation**:
- [ ] Three-way match detects quantity variances
- [ ] Three-way match detects price variances
- [ ] Variances highlighted in UI
- [ ] Manager approval enforced for significant variances
- [ ] Unit tests passing (10+ scenarios)

---

### 12.6 COA (Certificate of Analysis) Management

**Tasks**:
- [ ] **12.6.1** Add COA fields to `GRV` entity
  - `COAAttached` (bool), `COAFileName` (string), `COAFileData` (byte[]), `COALabName` (string), `COATestDate` (DateTime)
- [ ] **12.6.2** Implement COA upload
  - Upload PDF file
  - Store filename, file data, lab name, test date
- [ ] **12.6.3** Implement COA validation
  - Require COA for all cannabis raw materials (SAHPRA GMP requirement)
  - Cannot complete GRV without COA for cannabis products
- [ ] **12.6.4** Create COA viewer
  - Display attached COA PDF in UI
  - Download COA
- [ ] **12.6.5** Link COA to finished goods
  - When production uses GRV'd raw materials, link finished goods to source COAs
  - Traceability: Finished goods → Production batch → Input batches → GRV → COA

**Validation**:
- [ ] COA upload functional
- [ ] COA validation enforced (cannot complete GRV without COA)
- [ ] COA viewer displays PDF
- [ ] COA traceability linkage functional
- [ ] Unit tests passing (5+ tests)

---

### 12.7 Purchasing Reports

**Tasks**:
- [ ] **12.7.1** Create purchasing reports
  - GRV summary report (GRVs by date range, supplier)
  - RTS summary report (returns by reason, supplier)
  - Supplier performance report (on-time delivery %, return rate %)
  - Outstanding orders report (pending orders, expected delivery dates)
  - Three-way match variance report (invoices with variances)
- [ ] **12.7.2** Implement reporting queries
  ```csharp
  Task<List<GRVSummaryDto>> GetGRVSummaryAsync(DateTime startDate, DateTime endDate, int? supplierId = null);
  Task<List<RTSSummaryDto>> GetRTSSummaryAsync(DateTime startDate, DateTime endDate, int? supplierId = null);
  Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(DateTime startDate, DateTime endDate);
  Task<List<OutstandingOrderDto>> GetOutstandingOrdersAsync();
  Task<List<VarianceDto>> GetThreeWayMatchVariancesAsync(DateTime startDate, DateTime endDate);
  ```
- [ ] **12.7.3** Create purchasing reports UI
  - Page: "Purchasing Reports"
  - Select report type
  - Filter by date range, supplier
  - Display results in table
  - Export to CSV

**Validation**:
- [ ] All reports generate correct data
- [ ] Supplier performance calculations accurate
- [ ] Filters work correctly
- [ ] Export to CSV functional

---

### 12.8 Integration with Inventory

**Tasks**:
- [ ] **12.8.1** Test GRV → Inventory integration
  - Create GRV with 5 products, 10 units each
  - Verify IN movements created
  - Verify SOH increased by 50 units total
- [ ] **12.8.2** Test RTS → Inventory integration
  - Create RTS with 2 products, 5 units each
  - Verify OUT movements created
  - Verify SOH decreased by 10 units total
- [ ] **12.8.3** Test batch traceability
  - GRV assigns supplier batch number
  - Production uses GRV'd raw materials (linked by batch)
  - Finished goods traceable to supplier batch → GRV → COA
- [ ] **12.8.4** End-to-end integration test
  - Create purchase order
  - Receive goods (GRV)
  - Receive supplier invoice
  - Perform three-way match
  - Approve invoice
  - Verify SOH updated
  - Verify movements created
  - Verify traceability complete

**Validation**:
- [ ] GRV integration functional
- [ ] RTS integration functional
- [ ] Batch traceability functional
- [ ] End-to-end test passing
- [ ] Integration tests passing (5+ tests)

---

### PHASE 12 SUCCESS CRITERIA ✅

- [ ] GRV workflow complete (receive goods, assign batches, attach COA, generate IN movements)
- [ ] RTS workflow complete (return goods, specify reason, attach evidence, generate OUT movements)
- [ ] Purchase order tracking operational
- [ ] Supplier invoice processing operational
- [ ] Three-way matching functional (Order → GRV → Invoice)
- [ ] COA management complete (upload, validate, link to finished goods)
- [ ] Purchasing reports operational (GRV summary, RTS summary, supplier performance, variances)
- [ ] Integration with inventory seamless (movements generated, SOH updated, traceability maintained)
- [ ] All unit + integration tests passing (100%)
- [ ] Build status: 0 errors, <5 warnings

---

## ✅ POC COMPLETION VALIDATION

### Final Validation Checklist

After completing Phases 7-12, validate:

**Architecture**:
- [ ] Option A movement architecture operational (specialized headers + unified details)
- [ ] All transaction types write to unified `TransactionDetails`
- [ ] `MovementService` generates movements consistently (all types)
- [ ] SOH calculated accurately from movement ledger
- [ ] Performance acceptable (<200ms SOH query with 1M movements)

**Traceability**:
- [ ] Batch numbers generated with embedded metadata
- [ ] Serial numbers generated (full 28-digit + short 13-digit)
- [ ] Full seed-to-sale traceability operational (plant → batch → step → SN → sale)
- [ ] Batch recall query functional
- [ ] COA traceability functional (finished goods → inputs → GRV → COA)

**Retail POS**:
- [ ] Barcode/SN scanning operational
- [ ] VAT calculated correctly (15% VAT-inclusive)
- [ ] Multi-tender checkout functional
- [ ] Refunds (full & partial) operational
- [ ] Discounts (line & header) functional
- [ ] Age verification enforced (18+)
- [ ] Compliant receipts generated
- [ ] Performance acceptable (checkout <3 seconds)

**Inventory**:
- [ ] SOH calculation accurate (from movements)
- [ ] Cache operational (incremental refresh)
- [ ] Stock transfers functional
- [ ] Stock adjustments functional (manager approval)
- [ ] Stock counts functional (variance detection, auto-adjustment)
- [ ] Low-stock alerts operational
- [ ] Inventory reports complete

**Purchasing**:
- [ ] GRV creates IN movements
- [ ] RTS creates OUT movements
- [ ] Purchase order tracking operational
- [ ] Supplier invoice processing operational
- [ ] Three-way matching functional
- [ ] COA management complete

**Production**:
- [ ] Retail production workflow complete (pre-rolls, packaging)
- [ ] Production movements generated (inputs OUT, outputs IN)
- [ ] Batch traceability maintained through production steps
- [ ] SNs assigned to finished goods
- [ ] Yields and losses tracked
- [ ] Production reports operational

**Quality**:
- [ ] All unit tests passing (100%)
- [ ] All integration tests passing (100%)
- [ ] Test coverage >80% for critical services
- [ ] Build status: 0 errors, <5 warnings
- [ ] Performance targets met

**Compliance**:
- [ ] POPIA data protection implemented (soft delete, audit trails, encryption)
- [ ] Cannabis Act requirements met (age verification, batch tracking)
- [ ] SAHPRA GMP standards followed (lab testing, traceability, COA management)
- [ ] SA VAT calculation correct (15% VAT-inclusive)
- [ ] 7-year audit retention operational

---

## 📈 PROGRESS TRACKING

**Last Updated**: 2025-12-13

| Phase | Status | Start Date | End Date | Completion % | Notes |
|-------|--------|------------|----------|--------------|-------|
| Phase 7: Movement Architecture | 🟢 Completed | 2025-12-11 | 2025-12-13 | 100% | 7A/7B/7C all complete, 51 tests |
| Phase 8: Batch & Serial Number | 🟢 Completed | 2025-12-12 | 2025-12-13 | 100% | BatchNumber + SerialNumber services, 98 tests |
| Phase 9: Retail POS Completion | 🔵 Not Started | - | - | 0% | NEXT PHASE |
| Phase 10: Production DAL Expansion | 🔵 Not Started | - | - | 0% | - |
| Phase 11: Inventory SOH Engine | 🔵 Not Started | - | - | 0% | - |
| Phase 12: Purchasing Workflow | 🔵 Not Started | - | - | 0% | - |

**Status Legend**:
- 🔵 Not Started
- 🟡 In Progress
- 🟢 Completed
- 🔴 Blocked

**Current Metrics**:
- **Total Tests**: 523 passing
- **Database Tables**: 34 (Project420_Dev) + 5 (Project420_Shared)
- **Build Status**: 0 errors (excluding Android SDK)

---

## 🚀 GETTING STARTED

1. **Read the specification suite** (`00-MAIN-SPECIFICATION.md` → `03-MOVEMENT-ARCHITECTURE.md` → `01-MODULE-DESCRIPTIONS.md`)
2. **Review current codebase** against specification (identify gaps)
3. **Start with Phase 7** (Movement Architecture - most critical)
4. **Work systematically** through Phases 7-12
5. **Test thoroughly** at each phase (unit + integration + compliance scenarios)
6. **Update this roadmap** as you progress (mark tasks complete, track blockers)
7. **Document as you go** (code comments, architecture diagrams, DevNotes)

---

**Document Status**: ✅ COMPREHENSIVE TODO ROADMAP COMPLETE
**Target**: 85-90% PoC COMPLETENESS
**Timeline**: 6 WEEKS (PHASES 7-12)
**Priority**: START WITH PHASE 7 (MOVEMENT ARCHITECTURE)

---

*Ready to build a world-class cannabis management system* 🌿
