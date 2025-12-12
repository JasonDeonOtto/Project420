# Project420 - Enterprise Cannabis Management System
## Comprehensive PoC Home Stretch Specification

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: ACTIVE - PoC Final Phase
**Purpose**: Complete enterprise-grade specification for Project420 PoC completion
**Compliance**: SA Cannabis for Private Purposes Act 2024, SAHPRA, DALRRD, POPIA, SARS

---

## 📋 DOCUMENT SUITE OVERVIEW

This specification suite defines the complete enterprise-grade architecture, requirements, and implementation guidance for Project420 - a vertically integrated cannabis inventory, production, and retail management system designed specifically for the South African cannabis industry.

**Target**: 85-90% PoC completeness across all mandatory modules
**Focus**: Retail POS (primary market entry) + Enterprise Inventory + Production foundations

---

## 🎯 EXECUTIVE SUMMARY

### What is Project420?

Project420 is an enterprise-grade cannabis traceability and ERP-style system designed for the South African market. It addresses the unique requirements of cannabis cultivation, production, distribution, and retail while ensuring full compliance with evolving South African cannabis regulations.

### System Philosophy

1. **Traceability First**: Every plant, batch, product, and transaction must be traceable from seed to sale
2. **Compliance by Design**: Legal requirements (SAHPRA, DALRRD, POPIA, Cannabis Act 2024) are embedded in the architecture, not bolted on
3. **Performance at Scale**: Movement tables and SOH calculations must remain performant with millions of records
4. **Modular Licensing**: Businesses only pay for modules they need (cultivation, production, retail, wholesale)
5. **Future-Proof**: Architecture anticipates 2026-2027 commercial cannabis regulations in South Africa

### Core Capabilities

**Mandatory Modules**:
- **Inventory Management** - Real-time stock tracking, movement ledger, SOH calculation engine
- **Purchasing Module** - GRV, RTS, Supplier Orders, Supplier Invoices
- **One Sales Module** - Either Retail POS or Wholesale Sales (Retail is market entry focus)

**Optional Modules**:
- **Retail POS** - Enterprise-grade point of sale with age verification, VAT, multi-tender, refunds
- **Wholesale Sales** - B2B quotations, orders, invoices, batch picking
- **Production Processing** - Bucking, manufacturing, extraction, formulation, retail production
- **Cultivation Management** - Plant tracking, grow cycles, harvest batches (future expansion)

### Key Architectural Decisions

1. **Movement Architecture**: Specialized header tables + unified movement detail table (Option A)
2. **Batch Model**: Single-batch multi-step manufacturing (batch flows through steps, maintains lineage)
3. **SOH Calculation**: Never calculated from a single table - aggregated from multiple movement sources
4. **Header-Detail Pattern**: All transactions follow Header → Detail structure for consistency
5. **Compliance Integration**: Age verification, batch tracking, lab testing, audit trails embedded throughout

---

## 📚 DOCUMENTATION STRUCTURE

This specification is split into manageable sections for clarity:

### **01-MODULE-DESCRIPTIONS.md**
Detailed descriptions of each module tailored specifically to Project420's cannabis management needs:
- Inventory Module (core, mandatory)
- Retail POS Module (market entry focus)
- Wholesale Sales Module (optional)
- Purchasing Module (mandatory)
- Production Module (Bucking, Manufacturing, Retail Production)
- Cultivation Module (basic/stub implementation for PoC)

### **02-INVENTORY-MODULE-SPEC.md**
Enterprise-grade inventory specification:
- Stock tracking rules and movement types
- SOH calculation model (multi-table aggregation)
- Legal traceability considerations (SAHPRA, DALRRD)
- Performance optimization strategies
- Movement generation patterns

### **03-MOVEMENT-ARCHITECTURE.md** ⚠️ CRITICAL
The system's backbone - if movement tracking is flawed, the platform fails:
- Header/Detail patterns
- Specialized Header + single Detail table vs per-module Details
- **Recommended architecture with reasoning** (Option A)
- Examples of production and retail movements
- Batch transformation model (step-based manufacturing flow)
- Performance considerations at scale

### **04-RETAIL-POS-REQUIREMENTS.md**
85-90% PoC completeness specification:
- Scanning (serialized/non-serialized items)
- VAT calculation (15% SA VAT-inclusive)
- Multi-tender support (Cash, Card, EFT, Mobile)
- Refund workflows (full/partial)
- Cashouts/cashdrops
- Discounts (header & line-level)
- Scanner/scale/weight integration
- Client ID scanning (age verification)
- Transaction cancellation
- Line item search/quick lookup
- Movement generation rules
- Legal requirements (Cannabis Act 2024)

### **05-PRODUCTION-MODEL.md**
Full structured production model:
- Single batch flows through multiple steps
- Step-based manufacturing (bucking → WIP → manufacturing → final product)
- Each step consumes inputs and creates outputs
- Transformation tracking (yield, losses, by-products)
- Movement + batch step record linkage
- Legal compliance integration
- Example process diagrams (ASCII)

### **06-SERIAL-BATCH-GENERATION.md**
Enterprise-grade number generation:
- Batch number format: `SSTTYYYYMMDDNNNN` (16 digits)
- Full Serial numbers: `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC` (28 digits with QR)
- Short Serial numbers: `SSYYMMDDNNNNN` (13 digits for barcodes)
- Embedded traceability (Site, Strain, Batch Type, Date, Weight, Check Digit)
- Usage across production, retail, transfers, stocktake

### **07-DATABASE-SCHEMA.md**
Conceptual database design (not code):
- Product, Batches, BatchSteps entities
- Movements, Transaction Headers, Movement Details
- Retail headers/details, Purchasing headers/details
- Relationships and foreign keys
- Index strategy for performance

### **08-LICENSING-MODEL.md**
Required vs optional modules:
- Mandatory module dependencies
- Configuration examples (Retail-only, Wholesale-only, Full vertical integration)
- Pricing model implications
- Module activation patterns

### **09-COMPLIANCE-FUTURE-PROOFING.md**
Future-proofing and compliance:
- Hooks for upcoming SA cannabis regulations (2026-2027)
- Expandability for export markets
- Scalability for multi-store and multi-site production
- SAHPRA reporting requirements
- DALRRD hemp permit integration
- POPIA data protection enforcement

### **10-GLOSSARY.md**
System and legal terminology:
- System batches vs legal cultivation batches
- System serial numbers vs regulatory serial numbers
- Production batch steps vs retail production batches
- Movements vs transactions
- WIP vs finished goods
- Cannabis-specific terms (cultivation batch, bucking, curing, extraction)

---

## 🎯 POC COMPLETION TARGETS

### Current Status (Phase 6 Complete)
- ✅ All MVP modules created (Management, POS, OnlineOrders, Cultivation, Production, Inventory)
- ✅ Database schema migrated (12 tables across 3 DbContexts)
- ✅ FluentValidation integrated (32 validators)
- ✅ Shared services operational (VAT, Transaction Numbers, Audit Logs)
- ✅ Testing framework established (224 tests, 100% pass rate)
- ✅ Retail POS vertical slice complete (DAL → BLL → UI)

### PoC Home Stretch Goals

**Module Completion Targets**:

| Module | Current | Target | Priority | Notes |
|--------|---------|--------|----------|-------|
| **Inventory** | 35% | 90% | CRITICAL | Enterprise-grade, SOH engine, movement ledger |
| **Retail POS** | 60% | 85-90% | HIGH | Market entry module, full checkout workflow |
| **Purchasing** | 25% | 75% | HIGH | GRV, RTS, required for inventory flow |
| **Production (Retail)** | 60% | 75% | MEDIUM | Pre-rolls, packaging, retail production |
| **Production (General)** | 20% | 45% (stub) | LOW | Basic batch tracking, step recording |
| **Cultivation** | 15% | 35% (stub) | LOW | Basic plant tracking, harvest batches |
| **Wholesale** | 0% | 25% (stub) | LOW | Basic order/invoice structure |

**Key Deliverables**:
1. **Comprehensive Movement Architecture** - Implement Option A (specialized headers + unified details)
2. **Batch & Serial Number System** - Full implementation with embedded traceability
3. **Retail POS Completion** - 85-90% feature completeness (scanning, VAT, refunds, discounts)
4. **Production DAL Expansion** - Focus on Retail Production (pre-rolls, packaging)
5. **Inventory SOH Engine** - Multi-table movement aggregation with caching
6. **Purchasing Workflow** - GRV and RTS with movement generation

---

## 🚨 CRITICAL SUCCESS FACTORS

### 1. Movement Architecture Correctness
If movement tracking is wrong, the entire system fails. The movement table must:
- ✅ Capture every physical stock event
- ✅ Support efficient SOH calculation at scale
- ✅ Maintain full traceability (batch, serial, transaction source)
- ✅ Remain performant with millions of records
- ✅ Support regulatory audits (7-year retention)

### 2. Retail POS Market Readiness
POS is our market entry point. It must:
- ✅ Handle high-volume scanning (100+ items/minute)
- ✅ Calculate SA VAT correctly (15% VAT-inclusive)
- ✅ Support age verification (18+ Cannabis Act requirement)
- ✅ Process multiple payment methods
- ✅ Generate compliant receipts
- ✅ Create accurate movement records

### 3. Production Traceability
Cannabis regulations require seed-to-sale tracking:
- ✅ Single batch flows through multiple steps
- ✅ Each step records inputs, outputs, losses, yields
- ✅ Finished goods maintain lineage to source batches
- ✅ Serial numbers embed full traceability
- ✅ Lab testing integrated at each critical step

### 4. Legal Compliance
Non-negotiable requirements:
- ✅ POPIA data protection (encryption, audit trails, soft delete)
- ✅ Cannabis Act 2024 (age verification, possession limits)
- ✅ SAHPRA GMP (batch tracking, lab testing, traceability)
- ✅ DALRRD permits (cultivation tracking, THC reporting)
- ✅ SARS tax compliance (VAT calculation, record keeping)

---

## 📖 HOW TO USE THIS SPECIFICATION

### For Developers
1. Read **00-MAIN-SPECIFICATION.md** (this file) for context
2. Review **03-MOVEMENT-ARCHITECTURE.md** - CRITICAL for understanding data flow
3. Study **02-INVENTORY-MODULE-SPEC.md** - Core system foundation
4. Reference **04-RETAIL-POS-REQUIREMENTS.md** for POS implementation
5. Consult **10-GLOSSARY.md** when encountering unfamiliar terms
6. Cross-reference with `CODING-STRUCTURE.md` for implementation patterns

### For Architects
1. Start with **00-MAIN-SPECIFICATION.md** (this file)
2. Analyze **03-MOVEMENT-ARCHITECTURE.md** - Understand Option A vs Option B decision
3. Review **02-INVENTORY-MODULE-SPEC.md** - SOH calculation strategy
4. Study **05-PRODUCTION-MODEL.md** - Single-batch multi-step approach
5. Plan using **07-DATABASE-SCHEMA.md** - Schema design guidance
6. Consider **08-LICENSING-MODEL.md** - Module dependencies

### For Compliance Officers
1. Read **09-COMPLIANCE-FUTURE-PROOFING.md** - Regulatory landscape
2. Review **04-RETAIL-POS-REQUIREMENTS.md** - Customer-facing compliance
3. Study **06-SERIAL-BATCH-GENERATION.md** - Traceability implementation
4. Consult **10-GLOSSARY.md** - Legal terminology
5. Reference `SA_Cannabis_Software_Guide.md` (in project root)
6. Reference `south-africa-cannabis-cultivation-production-laws-guide.md` (in project root)

### For Product Managers
1. Start with **00-MAIN-SPECIFICATION.md** (this file) - Executive summary
2. Review **01-MODULE-DESCRIPTIONS.md** - Feature breakdown
3. Study **08-LICENSING-MODEL.md** - Packaging and pricing implications
4. Consider **09-COMPLIANCE-FUTURE-PROOFING.md** - Market evolution

---

## 🏗️ ARCHITECTURAL PRINCIPLES

### 1. Every Physical Stock Event Creates Movement Records
```
Stock Event (GRV, Sale, Production, Transfer, Adjustment)
    ↓
Transaction Header Created
    ↓
Transaction Details Created (one per line item)
    ↓
Movement Records Generated (one per stock-affecting unit)
    ↓
SOH Recalculated (aggregated from all movements)
```

### 2. Movement Tables Must Scale
**Expected Volume**:
- Retail POS: 1,000 transactions/day = 5,000+ movements/day = 1.8M movements/year
- Production: 100 batches/month with 10 steps each = 12K movements/year
- Purchasing: 200 GRVs/month = 2.4K movements/year
- **Total**: ~2M movements/year per business

**Performance Strategy**:
- Partitioning by date (monthly/quarterly)
- Indexed by Product, Batch, Transaction, Date
- Cached SOH projections (refreshed incrementally)
- Archive old movements (7-year retention in cold storage)

### 3. SOH is NEVER Directly Stored
```sql
-- WRONG: Storing SOH in product table (data integrity nightmare)
UPDATE Products SET StockOnHand = StockOnHand - @Qty WHERE ProductId = @Id;

-- CORRECT: SOH calculated from movements
SELECT SUM(CASE WHEN Direction = 'IN' THEN Qty ELSE -Qty END) AS SOH
FROM Movements
WHERE ProductId = @ProductId
  AND IsDeleted = 0
  AND TransactionDate <= @AsOfDate;
```

**SOH Caching**:
- Calculate SOH once per product per day
- Store in `StockLevels` cache table
- Refresh incrementally as new movements added
- Always reconcile cache against movement ledger

### 4. Batch Flows Through Steps (No Batch Explosion)
```
BATCH-2025-001 (Cannabis Flower, 10kg)
  ↓ Step 1: Bucking (removes stems/seeds)
  ├── Input: 10kg flower
  ├── Output: 8kg bucked flower (WIP)
  └── Loss: 2kg stems/seeds (waste tracked)

  ↓ Step 2: Milling (grinds to consistency)
  ├── Input: 8kg bucked flower
  ├── Output: 7.8kg milled flower (WIP)
  └── Loss: 0.2kg dust/over-processing

  ↓ Step 3: Pre-roll Filling (mechanical assembly)
  ├── Input: 7.8kg milled flower
  ├── Output: 7,500 pre-rolls @ 1g each (7.5kg)
  └── Loss: 0.3kg spillage/QC rejects

  ↓ Step 4: Packaging (retail units)
  ├── Input: 7,500 pre-rolls
  ├── Output: 750 packs (10 pre-rolls/pack)
  └── Each pack gets unique SN: SN-PR-20250101-001 to SN-PR-20250101-750
```

**Key Points**:
- Same batch number throughout (`BATCH-2025-001`)
- Each step records transformation (input → output + loss)
- WIP tracked at each step (not separate batches)
- Serial numbers assigned to finished goods
- Full traceability maintained (SN → Batch → Steps → Source Material)

### 5. Retail Transactions are Enterprise-Grade
```
POS Transaction (SALE-20250101-001)
  ├── Header: Customer, Cashier, Date, Payment Method, Status
  ├── Details:
  │   ├── Line 1: Product A, Qty 2, Unit Price R150, VAT R39.13, Line Total R300
  │   ├── Line 2: Product B, Qty 1, Unit Price R80, VAT R10.43, Line Total R80
  │   └── Line 3: Product C (Discounted 10%), Qty 1, Unit Price R100, Discount R10, VAT R11.74, Line Total R90
  ├── Totals:
  │   ├── Subtotal (VAT Exclusive): R408.70
  │   ├── VAT (15%): R61.30
  │   ├── Discount: R10.00
  │   └── Total (VAT Inclusive): R470.00
  ├── Payments:
  │   ├── Cash: R200.00
  │   ├── Card: R270.00
  │   └── Change: R0.00
  ├── Compliance:
  │   ├── Age Verified: Yes (18+, ID scanned)
  │   ├── Possession Limit: Within legal limits
  │   └── Receipt Generated: SALE-20250101-001 (compliant format)
  └── Movements Generated:
      ├── Product A: OUT -2 units, Batch BATCH-2025-042
      ├── Product B: OUT -1 unit, SN SN-ED-20250101-123
      └── Product C: OUT -1 unit, Batch BATCH-2025-038
```

---

## 📊 DATA ARCHITECTURE DEBATE: OPTION A vs OPTION B

### The Question
Should we use:
- **(A) Specialized Header tables + single unified Details table**
- **(B) Separate Header/Detail tables per module**

### Option A: Specialized Headers + Shared Detail Table ✅ RECOMMENDED

**Structure**:
```
WholesaleTransactionHeaders
RetailTransactionHeaders
PurchaseHeaders
ProductionHeaders
TransferHeaders
    ↓ all link to ↓
TransactionDetails (unified)
    ↓ generates ↓
Movements (unified ledger)
```

**Benefits**:
- ✅ Reduces schema duplication (one detail model)
- ✅ Consistent detail structure across all transaction types
- ✅ Easy to add new transaction types (just add header table)
- ✅ Ideal for multi-module systems like Project420
- ✅ Efficient reporting (single JOIN to get all transaction details)
- ✅ Centralized movement generation logic
- ✅ Easier to maintain (changes to detail structure in one place)

**Drawbacks**:
- ⚠️ Slight complexity in linking (need TransactionType discriminator)
- ⚠️ Requires strict referential integrity enforcement

**Implementation**:
```csharp
public class TransactionDetail : AuditableEntity
{
    public int TransactionDetailId { get; set; }
    public int HeaderId { get; set; }
    public TransactionType TransactionType { get; set; } // Discriminator
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VATAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    // ... navigation properties
}
```

### Option B: One Header + One Detail Per Module ❌ NOT RECOMMENDED

**Structure**:
```
WholesaleHeader → WholesaleDetail
RetailHeader → RetailDetail
PurchaseHeader → PurchaseDetail
ProductionHeader → ProductionDetail
TransferHeader → TransferDetail
```

**Benefits**:
- ✅ Simple for beginners (clear 1:1 relationship)
- ✅ Strong natural referential integrity

**Drawbacks**:
- ❌ Schema duplication (5+ detail tables with 90% identical columns)
- ❌ Harder to build unified reporting (UNION ALL across 5+ tables)
- ❌ More work to add new transaction types
- ❌ Movement generation logic duplicated
- ❌ Poor for enterprise systems (doesn't scale)

### **Decision: Option A** ✅

**Rationale**:
1. Project420 is an enterprise system with multiple transaction types
2. We need unified reporting across retail, wholesale, purchasing, production
3. Movement generation must be consistent regardless of source
4. Future expansion likely (export sales, inter-company transfers, consignment)
5. Option A is standard practice in mature ERP systems

**Implementation Plan**:
- Create specialized header tables for each module
- Create single `TransactionDetails` table with `TransactionType` discriminator
- Use `MovementService` to generate movement records from any detail source
- Implement efficient indexes on `TransactionDetails(HeaderId, TransactionType)`

---

## 🎯 POC HOME STRETCH ROADMAP

### Phase 7: Movement Architecture Implementation (Week 1)
**Goal**: Implement Option A architecture with unified movement ledger

Tasks:
1. Create `TransactionDetails` unified table
2. Refactor existing headers to link to unified details
3. Implement `MovementService` with transaction-agnostic movement generation
4. Add `TransactionType` enum and discriminator column
5. Create database migration
6. Test movement generation from retail, purchasing, production sources
7. Validate SOH calculation from unified movement ledger

**Success Criteria**:
- ✅ All transaction types write to unified `TransactionDetails`
- ✅ Movements generated consistently regardless of source
- ✅ SOH calculated accurately from movement ledger
- ✅ Performance acceptable (<500ms for SOH query with 100K movements)

### Phase 8: Batch & Serial Number System (Week 2)
**Goal**: Implement enterprise-grade batch and serial number generation

Tasks:
1. Create `Batch` entity and repository
2. Implement `BatchNumberGeneratorService` (16-digit format)
3. Create `SerialNumber` entity and repository
4. Implement `SerialNumberGeneratorService` (28-digit full, 13-digit short)
5. Integrate batch/SN into Production module (batch steps)
6. Integrate SN into Retail POS (scanning, printing)
7. Add batch/SN validation in inventory movements
8. Test end-to-end traceability (plant → batch → step → SN → sale)

**Success Criteria**:
- ✅ Batch numbers generated with embedded metadata (site, type, date)
- ✅ Serial numbers traceable to source batch and processing steps
- ✅ POS can scan and validate serial numbers
- ✅ Production steps linked to parent batch
- ✅ Full traceability demonstrated (seed-to-sale)

### Phase 9: Retail POS Completion (Week 3)
**Goal**: Achieve 85-90% POS feature completeness

Tasks:
1. Implement barcode/QR scanning (EAN, SN)
2. Add line-level discounts
3. Add header-level discounts
4. Implement multi-tender checkout
5. Build refund workflow (full & partial)
6. Add cash drop/cash out functionality
7. Implement transaction cancellation
8. Add scale/weight integration
9. Enhance age verification UI (ID scanning)
10. Generate compliant receipts (with batch/SN)
11. Optimize movement generation performance

**Success Criteria**:
- ✅ Full checkout workflow operational
- ✅ VAT calculated correctly (15% VAT-inclusive)
- ✅ Age verification enforced (18+)
- ✅ Multi-tender processing complete
- ✅ Refunds create correct movement reversals
- ✅ Compliant receipts generated
- ✅ Performance acceptable (checkout < 3 seconds)

### Phase 10: Production DAL Expansion (Week 4)
**Goal**: Complete Retail Production DAL and expand Production foundations

Tasks:
1. Expand `ProductionBatch` repository with business queries
2. Implement `ProcessingStep` repository (step-based manufacturing)
3. Create `RetailProduction` repository (pre-rolls, packaging)
4. Add `QualityControl` repository methods
5. Add `LabTest` repository methods
6. Implement production movement generation (inputs → WIP → outputs)
7. Create production reporting queries (yields, losses, efficiency)
8. Stub out general production and cultivation repositories

**Success Criteria**:
- ✅ Retail production workflow complete (bucking → milling → pre-roll → packaging)
- ✅ Production movements generated correctly (IN/OUT for each step)
- ✅ Batch flows through steps maintaining lineage
- ✅ Yield and loss calculations accurate
- ✅ QC and lab testing integrated
- ✅ General production and cultivation stubbed (35-45% completeness)

### Phase 11: Inventory SOH Engine (Week 5)
**Goal**: Implement performant multi-table SOH calculation engine

Tasks:
1. Create `StockLevelsCache` table (cached SOH)
2. Implement `SOHCalculationService` (aggregate from movements)
3. Add incremental cache refresh on new movements
4. Implement `StockTransfer` workflow with movement generation
5. Implement `StockAdjustment` workflow with movement generation
6. Implement `StockCount` workflow with variance detection
7. Add low-stock alerts
8. Create inventory reports (SOH by location, product, batch)
9. Optimize movement queries (indexes, partitioning)
10. Test performance at scale (simulate 1M+ movements)

**Success Criteria**:
- ✅ SOH calculated from unified movement ledger
- ✅ Cache refreshed incrementally (no full recalc)
- ✅ Transfers, adjustments, counts generate movements correctly
- ✅ Performance acceptable (SOH query < 200ms with 1M movements)
- ✅ Inventory reports operational
- ✅ Low-stock alerts functional

### Phase 12: Purchasing Workflow (Week 6)
**Goal**: Complete purchasing module (45% → 75%)

Tasks:
1. Implement `GRV` (Goods Received Voucher) workflow
2. Implement `RTS` (Return to Supplier) workflow
3. Add `SupplierOrder` tracking
4. Add `SupplierInvoice` processing
5. Implement purchasing movement generation (GRV = IN, RTS = OUT)
6. Add batch assignment on GRV (link received stock to batches)
7. Create purchasing reports (GRV summary, supplier performance)
8. Integrate with inventory (GRV creates movements)
9. Add three-way matching (Order → GRV → Invoice)

**Success Criteria**:
- ✅ GRV creates IN movements with batch assignment
- ✅ RTS creates OUT movements
- ✅ Supplier orders tracked
- ✅ Supplier invoices linked to GRVs
- ✅ Three-way matching functional
- ✅ Purchasing reports operational
- ✅ Integration with inventory seamless

---

## 📈 SUCCESS METRICS

### Technical Metrics
- ✅ **Build Status**: 0 errors, <5 warnings
- ✅ **Test Coverage**: >80% for critical services (Inventory, POS, Production)
- ✅ **Test Pass Rate**: 100%
- ✅ **Performance**: SOH query <200ms, POS checkout <3s, movement insert <50ms
- ✅ **Database Size**: Support 1M+ movements without degradation

### Feature Completeness
- ✅ **Inventory**: 90% complete (enterprise-grade SOH engine operational)
- ✅ **Retail POS**: 85-90% complete (full checkout, scanning, refunds, discounts)
- ✅ **Purchasing**: 75% complete (GRV, RTS, three-way matching)
- ✅ **Production (Retail)**: 75% complete (pre-rolls, packaging workflow)
- ✅ **Production (General)**: 45% complete (basic batch tracking, step recording)
- ✅ **Cultivation**: 35% complete (stub implementation)

### Compliance Metrics
- ✅ **Age Verification**: 100% enforced in POS
- ✅ **Batch Tracking**: 100% of products traced to source batches
- ✅ **Serial Numbers**: Generated for all serialized items
- ✅ **Lab Testing**: Required for all production batches
- ✅ **Audit Trails**: Immutable logs for all transactions (7-year retention)
- ✅ **VAT Calculation**: Accurate 15% VAT-inclusive calculations
- ✅ **POPIA Compliance**: Soft delete, encryption, consent tracking

---

## 🚀 NEXT STEPS

1. **Read the detailed specification sections** (01-10)
2. **Review current codebase** against specification
3. **Identify gaps** between current state and target state
4. **Prioritize work** (Phases 7-12)
5. **Execute systematically** (one phase at a time)
6. **Test thoroughly** (unit + integration + compliance scenarios)
7. **Document as you go** (update DevNotes, add code comments)

---

## 📞 SPECIFICATION MAINTAINERS

**Primary Author**: Claude (Project420 Architect Agent)
**Review Cycle**: Weekly during PoC home stretch
**Feedback**: Update as implementation proceeds
**Version Control**: All specification files tracked in Git

---

## ✅ DOCUMENT VALIDATION CHECKLIST

When using this specification suite:

- [ ] Read **00-MAIN-SPECIFICATION.md** (this file) for overview
- [ ] Identify which sections are relevant to your current work
- [ ] Cross-reference with `CLAUDE.md` for project context
- [ ] Cross-reference with `CODING-STRUCTURE.md` for implementation patterns
- [ ] Consult compliance guides (`SA_Cannabis_Software_Guide.md`, `south-africa-cannabis-cultivation-production-laws-guide.md`)
- [ ] Validate against current codebase state
- [ ] Update specification as implementation decisions are made
- [ ] Keep **10-GLOSSARY.md** handy for terminology

---

**STATUS**: ✅ **SPECIFICATION SUITE READY**
**TARGET**: **85-90% POC COMPLETENESS**
**TIMELINE**: **6 WEEKS (PHASES 7-12)**
**FOCUS**: **RETAIL POS + ENTERPRISE INVENTORY + PRODUCTION FOUNDATIONS**

---

*Building the future of compliant cannabis management in South Africa* 🌿
