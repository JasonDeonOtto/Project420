# Project420 - Module Descriptions
## Detailed Module Specifications Tailored to Cannabis Management

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Part of**: PoC Home Stretch Specification Suite
**Purpose**: Comprehensive description of each Project420 module

---

## 📋 TABLE OF CONTENTS

1. [Inventory Module (Core, Mandatory)](#1-inventory-module-core-mandatory)
2. [Retail POS Module (Market Entry Focus)](#2-retail-pos-module-market-entry-focus)
3. [Wholesale Sales Module (Optional)](#3-wholesale-sales-module-optional)
4. [Purchasing Module (Mandatory)](#4-purchasing-module-mandatory)
5. [Production Module](#5-production-module)
   - Bucking Sub-Module
   - Manufacturing Sub-Module
   - Retail Production Sub-Module
6. [Cultivation Module (Stub Implementation)](#6-cultivation-module-stub-implementation)
7. [Module Dependencies](#7-module-dependencies)

---

## 1. INVENTORY MODULE (Core, Mandatory)

**Status**: CRITICAL - Cannot function without it
**Licensing**: Included in all licenses
**Current Implementation**: 35%
**Target**: 90% (Enterprise-grade)

### 1.1 Purpose

The Inventory module is the **beating heart** of Project420. All other modules depend on it. It provides:

- **Real-time stock tracking** across all locations, batches, and serial numbers
- **Movement ledger** (immutable audit trail of every stock event)
- **SOH calculation engine** (aggregates from movements, never stored directly)
- **Traceability foundation** (seed-to-sale tracking for SAHPRA compliance)
- **Multi-location support** (warehouses, stores, production facilities)
- **Batch management** (FIFO, FEFO, batch expiry tracking)
- **Serial number tracking** (individual unit traceability for edibles/vapes)

### 1.2 Key Features

#### 1.2.1 Stock Tracking
- Track **quantity** (count of units)
- Track **mass** (weight in grams/kg for flower, trim, concentrates)
- Track **potency** (THC%, CBD% where applicable)
- Track **batch numbers** (link to source cultivation/production batches)
- Track **serial numbers** (individual unit identification for regulated items)
- Track **WIP** (work-in-progress) vs **Finished Goods**
- Track **expiry dates** (SAHPRA requirement)
- Track **storage location** (warehouse, shelf, bin)

#### 1.2.2 Movement Types
```
IN Movements (increase stock):
├── GRV (Goods Received Voucher from supplier)
├── Production Output (finished goods from manufacturing)
├── Transfer In (from another location)
├── Adjustment In (stocktake correction, found stock)
├── Return from Customer (retail refund)
└── Return from Wholesale Customer (B2B return)

OUT Movements (decrease stock):
├── Retail Sale (POS transaction)
├── Wholesale Sale (B2B invoice)
├── Production Input (raw materials consumed in manufacturing)
├── Transfer Out (to another location)
├── Adjustment Out (stocktake correction, shrinkage, waste)
├── Return to Supplier (RTS, defective goods)
└── Destruction (expired, contaminated, or non-compliant stock - SAHPRA requirement)
```

#### 1.2.3 SOH Calculation Model
**CRITICAL PRINCIPLE**: SOH is NEVER stored directly in the Product table.

```sql
-- SOH is always calculated from movements
SELECT
    m.ProductId,
    p.ProductName,
    p.SKU,
    SUM(CASE
        WHEN m.MovementType IN ('GRV', 'ProductionOutput', 'TransferIn', 'AdjustmentIn', 'ReturnFromCustomer')
        THEN m.Quantity
        ELSE -m.Quantity
    END) AS StockOnHand,
    SUM(CASE
        WHEN m.MovementType IN ('GRV', 'ProductionOutput', 'TransferIn', 'AdjustmentIn', 'ReturnFromCustomer')
        THEN m.Mass
        ELSE -m.Mass
    END) AS StockOnHandMass
FROM Movements m
INNER JOIN Products p ON m.ProductId = p.ProductId
WHERE m.IsDeleted = 0
  AND m.TransactionDate <= @AsOfDate
GROUP BY m.ProductId, p.ProductName, p.SKU
HAVING SUM(CASE
    WHEN m.MovementType IN ('GRV', 'ProductionOutput', 'TransferIn', 'AdjustmentIn', 'ReturnFromCustomer')
    THEN m.Quantity
    ELSE -m.Quantity
END) > 0;
```

**Performance Optimization** (Phase 11):
- Cache SOH in `StockLevelsCache` table (refreshed incrementally)
- Partition `Movements` table by date (monthly/quarterly)
- Index on `(ProductId, TransactionDate, MovementType)`
- Archive movements older than 7 years to cold storage (SAHPRA retention requirement)

#### 1.2.4 Traceability Requirements (SAHPRA GMP)
Every movement must record:
- **Source**: Where did this stock come from? (Batch, Supplier, Production Run)
- **Destination**: Where is it going? (Customer, Location, Production Process)
- **Reason**: Why is stock moving? (Sale, Transfer, Adjustment, Destruction)
- **Operator**: Who performed the action? (UserId, Timestamp)
- **Audit Trail**: Immutable record (soft delete only, 7-year retention)

### 1.3 Cannabis-Specific Features

#### 1.3.1 Batch Tracking
- **Cultivation Batches**: Link finished goods to source grow cycles (SAHPRA plant tracking)
- **Production Batches**: Track manufactured goods through processing steps (bucking → milling → pre-rolls)
- **Batch Expiry**: Alert when batches approach expiry (GMP requirement)
- **Batch Recall**: Ability to identify all stock and sales from a specific batch (compliance requirement)

#### 1.3.2 Potency Tracking
- **THC Percentage**: Required for all cannabis products (Cannabis Act)
- **CBD Percentage**: Required for all cannabis products
- **COA (Certificate of Analysis)**: Link to lab test results (ISO/IEC 17025 accredited labs)
- **Potency Degradation**: Track potency over time (for aged products)

#### 1.3.3 Compliance Features
- **Age Verification**: Ensure all stock movements to customers verify 18+ requirement
- **Possession Limits**: Track customer purchase history (prevent exceeding legal limits)
- **Destruction Tracking**: Record waste, contamination, expired stock destruction (SAHPRA requirement)
- **Audit Reports**: Generate SAHPRA-compliant inventory reports (monthly submissions)

### 1.4 Integration Points

**Inventory** integrates with:
- **Retail POS**: Creates OUT movements on sale
- **Wholesale**: Creates OUT movements on invoice
- **Purchasing**: Creates IN movements on GRV, OUT movements on RTS
- **Production**: Creates OUT movements for inputs, IN movements for outputs
- **Cultivation**: Links finished goods to source plants/harvests
- **Quality Control**: Prevents sale of stock without passing QC

### 1.5 PoC Implementation Plan (35% → 90%)

**Phase 11 Tasks** (Week 5):
1. Create `StockLevelsCache` table
2. Implement `SOHCalculationService` (aggregate from movements)
3. Add incremental cache refresh
4. Implement `StockTransfer` workflow
5. Implement `StockAdjustment` workflow
6. Implement `StockCount` workflow (variance detection)
7. Add low-stock alerts
8. Create inventory reports (SOH by location, product, batch)
9. Optimize movement queries (indexes, partitioning)
10. Test performance at scale (simulate 1M+ movements)

---

## 2. RETAIL POS MODULE (Market Entry Focus)

**Status**: HIGH PRIORITY - Primary market entry
**Licensing**: Optional (most businesses will choose this over wholesale)
**Current Implementation**: 60%
**Target**: 85-90% (PoC-ready for pilot deployment)

### 2.1 Purpose

Retail POS is Project420's **primary market entry product**. It must be:
- **Fast**: 100+ items scanned per minute
- **Reliable**: <1% transaction failure rate
- **Compliant**: 100% age verification, accurate VAT, audit trails
- **User-Friendly**: Minimal training required
- **Feature-Rich**: Match competitor POS systems

### 2.2 Core POS Features (85-90% Target)

#### 2.2.1 Product Scanning & Lookup
- ✅ **Barcode Scanning**: EAN-13, Code128, QR codes
- ✅ **Serial Number Scanning**: Scan individual unit SNs for edibles/vapes
- ✅ **Manual Search**: Quick product lookup by name, SKU, category
- ✅ **Product Images**: Display product photo on scan
- ✅ **Stock Availability**: Real-time SOH check before adding to cart
- ⚠️ **Weight Integration**: Scale integration for bulk flower (future enhancement)

#### 2.2.2 Shopping Cart Management
- ✅ **Add Items**: Scan or search to add items
- ✅ **Adjust Quantity**: Increase/decrease line quantities
- ✅ **Remove Items**: Delete line items from cart
- ✅ **Line Discounts**: Apply discount to individual line (% or R amount)
- ✅ **Cart Clear**: Empty entire cart
- ⚠️ **Suspended Transactions**: Park transaction for later completion (future enhancement)

#### 2.2.3 VAT Calculation (CRITICAL - SARS Compliance)
South Africa uses **VAT-inclusive pricing** (15% VAT):
```
Price displayed to customer = VAT-inclusive price
VAT = Price / 1.15 * 0.15
VAT-exclusive = Price / 1.15
```

**Example**:
- Product displayed at R115.00
- VAT-exclusive: R115.00 / 1.15 = R100.00
- VAT: R100.00 * 0.15 = R15.00
- Customer pays: R115.00

**Implementation**:
- Use `VATCalculationService` (already implemented in Phase 3)
- Calculate VAT per line item
- Sum VAT across all lines
- Apply rounding adjustment to header (if needed)
- Generate compliant receipt (show VAT breakdown)

#### 2.2.4 Discounts
**Line-Level Discounts**:
- Discount applied to individual line item
- Discount reduces line total, VAT recalculated on discounted amount
- Example: R115 item with 10% discount = R103.50 total, VAT = R13.50

**Header-Level Discounts**:
- Discount applied to entire transaction
- Prorated across all line items proportionally
- VAT recalculated per line after proration

#### 2.2.5 Multi-Tender Support
- **Cash**: Accept cash payment, calculate change
- **Card**: Integrate with Yoco/iKhokha/Zapper (SA card processors)
- **EFT**: Bank transfer (provide reference number)
- **Mobile Payment**: SnapScan, Zapper, PayFast
- **Split Tender**: Accept multiple payment methods (e.g., R200 cash + R100 card)

#### 2.2.6 Age Verification (Cannabis Act 2024 - MANDATORY)
- **Before Every Sale**: Prompt cashier to verify customer is 18+
- **ID Scanning**: Scan SA ID card (barcode on back) to auto-extract age
- **Manual Entry**: Cashier can manually confirm age if ID barcode fails
- **Customer Record**: Link sale to customer record (optional, for loyalty/compliance)
- **Audit Trail**: Log age verification attempt (timestamp, cashier, outcome)

**Legal Requirement**: Cannabis for Private Purposes Act 2024 prohibits sale to persons under 18.
**Penalty**: R100,000 fine or 2 years imprisonment for selling to minors.

#### 2.2.7 Transaction Workflows
**New Sale**:
1. Open POS
2. Scan/search products
3. Add to cart
4. Apply discounts (if any)
5. Verify customer age (18+)
6. Process payment(s)
7. Generate receipt
8. Create movements (stock OUT)

**Refund (Full)**:
1. Lookup original transaction (by transaction number or scan receipt)
2. Verify items being returned
3. Process refund payment (cash/card reversal)
4. Generate credit note
5. Create movements (stock IN - reversal)

**Refund (Partial)**:
1. Lookup original transaction
2. Select specific items to refund
3. Adjust refund amount (partial quantity or damaged goods)
4. Process refund payment
5. Generate credit note
6. Create movements (stock IN - quantity returned)

**Transaction Cancellation**:
- **Before Payment**: Can cancel and clear cart
- **After Payment**: Requires manager override + reason
- **No Movement**: Cancelled transactions do not create movements

#### 2.2.8 Cash Management
**Cash Drop**:
- Remove excess cash from till
- Record amount, reason, operator
- Does not affect inventory (financial only)

**Cash Out**:
- End-of-shift reconciliation
- Count cash, card, EFT totals
- Compare to expected (from system)
- Record variance (if any)

#### 2.2.9 Receipt Generation (SARS Compliant)
Required receipt fields:
- Store name, address, VAT number
- Transaction number (unique)
- Date & time
- Cashier name/ID
- List of items (description, qty, unit price, line total)
- VAT breakdown (VAT-exclusive, VAT amount, VAT-inclusive)
- Discounts (if applied)
- Payment method(s)
- Change given (if cash)
- **Batch numbers** (for traceability - SAHPRA)
- **Serial numbers** (for serialized items - SAHPRA)
- Return/exchange policy
- Legal disclaimer (cannabis use warnings)

### 2.3 Cannabis-Specific POS Features

#### 2.3.1 Possession Limit Enforcement
**Private Use Limits** (Cannabis Act 2024):
- **Home**: 1.2kg dried cannabis per household
- **Public**: 600g on person
- **Cultivation**: 4 flowering plants per person, 8 per household

**POS Implementation**:
- Track customer purchase history (linked by ID number)
- Warn cashier if sale would exceed recommended limits
- **Note**: System cannot legally prevent sale, only warn (customer responsibility)

#### 2.3.2 Product Information Display
On scan/select, display:
- Product name, image, description
- THC% and CBD% (Cannabis Act requirement)
- Batch number (traceability)
- Lab test date (GMP compliance)
- Expiry date (if applicable)
- Strain information (indica/sativa/hybrid)
- Recommended use (SAHPRA medical license info if applicable)

#### 2.3.3 Medical Cannabis Support (Future Enhancement)
- **Medical Licenses**: Link to patient's SAHPRA Section 21 approval
- **Prescription Tracking**: Ensure purchase matches prescription
- **Dispensing Records**: Log medical cannabis dispensing (SAHPRA reporting requirement)

### 2.4 Integration Points

**Retail POS** integrates with:
- **Inventory**: Creates OUT movements on sale, IN movements on refund
- **Customers (Debtors)**: Links sale to customer record (for loyalty/compliance)
- **Pricelists**: Applies correct pricing based on customer/time/promotion
- **Audit Logs**: Records all transactions (immutable, 7-year retention)
- **Financial Reporting**: Feeds daily sales totals, VAT, payment methods
- **SAHPRA Reporting**: Provides sales data for monthly submissions

### 2.5 PoC Implementation Plan (60% → 85-90%)

**Phase 9 Tasks** (Week 3):
1. Implement barcode/QR scanning
2. Add line-level discounts
3. Add header-level discounts
4. Implement multi-tender checkout
5. Build refund workflow (full & partial)
6. Add cash drop/cash out functionality
7. Implement transaction cancellation
8. Add scale/weight integration (stub)
9. Enhance age verification UI (ID scanning)
10. Generate compliant receipts (with batch/SN)
11. Optimize movement generation performance

---

## 3. WHOLESALE SALES MODULE (Optional)

**Status**: LOW PRIORITY - Stub implementation for PoC
**Licensing**: Optional (businesses selling B2B)
**Current Implementation**: 0%
**Target**: 25% (Basic structure only)

### 3.1 Purpose

Wholesale module supports **B2B sales** to:
- Retailers (dispensaries, stores)
- Distributors
- Other manufacturers (bulk ingredients)
- Export customers (when regulations permit)

### 3.2 Key Features (Stub Implementation)

#### 3.2.1 Quotation → Order → Invoice Workflow
```
Quotation (Quote-2025-001)
    ↓ (Customer accepts)
Sales Order (SO-2025-001)
    ↓ (Pick & pack goods)
Dispatch Note (DN-2025-001)
    ↓ (Goods shipped)
Sales Invoice (SI-2025-001)
    ↓ (Payment received)
Payment Allocation (RCPT-2025-001)
```

#### 3.2.2 Batch-Specific Picking
- Customer orders Product A (qty 100)
- System allocates from available batches (FIFO/FEFO)
- Dispatch note shows batch numbers (traceability)
- Movements created on dispatch (OUT movements)

#### 3.2.3 Wholesale Pricing
- Volume discounts (tier pricing)
- Customer-specific pricelists
- Contract pricing (fixed price for 6/12 months)

### 3.3 Compliance Features
- **Cannabis Distribution License**: Verify customer has valid license (SAHPRA)
- **Transport Permits**: Generate transport documentation (SAPS requirements)
- **COA Provision**: Include Certificate of Analysis with shipment (GMP requirement)
- **Traceability**: Link wholesale sale to source batches (seed-to-sale)

### 3.4 PoC Implementation (Stub Only)
- Create basic entity structure (WholesaleHeader, WholesaleDetail)
- Link to unified TransactionDetails table
- Stub out quotation/order/invoice workflows
- **Defer full implementation to post-PoC**

---

## 4. PURCHASING MODULE (Mandatory)

**Status**: HIGH PRIORITY - Required for inventory flow
**Licensing**: Included in all licenses
**Current Implementation**: 25%
**Target**: 75% (Full GRV, RTS workflows)

### 4.1 Purpose

Purchasing module handles:
- **Receiving goods from suppliers** (GRV)
- **Returning defective goods** (RTS)
- **Tracking supplier orders**
- **Managing supplier invoices**
- **Three-way matching** (Order → GRV → Invoice)

### 4.2 Key Workflows

#### 4.2.1 GRV (Goods Received Voucher)
```
Supplier Order (PO-2025-001) [optional]
    ↓
Goods Arrive at Warehouse
    ↓
GRV Created (GRV-2025-001)
├── Scan/enter products received
├── Verify quantities, quality
├── Assign batch numbers (link to supplier batch)
├── Record COA (Certificate of Analysis) if applicable
├── Capture GRV details (date, supplier, reference)
└── Save GRV
    ↓
Movements Created (IN movements)
├── Product A: IN +50 units, Batch BATCH-SUPP-001
├── Product B: IN +100 units, Batch BATCH-SUPP-002
└── ...
    ↓
Inventory Updated (SOH increases)
    ↓
Supplier Invoice Received (SI-2025-001)
    ↓
Three-Way Match:
├── Order: R10,000
├── GRV: R9,800 (2 items shorted)
└── Invoice: R9,800
    ↓
Approve for Payment
```

**Key Points**:
- GRV creates IN movements immediately (stock available for sale)
- Batch assignment critical (traceability)
- COA capture (SAHPRA GMP requirement - lab testing on all raw materials)
- Variance handling (shorted items, damaged goods)

#### 4.2.2 RTS (Return to Supplier)
```
Identify Defective/Non-Compliant Stock
├── Failed QC (contamination, incorrect potency)
├── Damaged in transit
├── Expired on arrival
└── Incorrect product received
    ↓
Create RTS (RTS-2025-001)
├── Select items to return
├── Specify reason (defect, damage, expired)
├── Attach supporting documentation (photos, COA, QC report)
└── Save RTS
    ↓
Movements Created (OUT movements)
├── Product X: OUT -10 units, Batch BATCH-SUPP-001, Reason: Failed QC
└── ...
    ↓
Inventory Updated (SOH decreases)
    ↓
Supplier Credit Note Received
    ↓
Payment Allocated (credit applied to next order)
```

#### 4.2.3 Supplier Order Tracking
- Create purchase orders (PO)
- Track order status (Pending, Confirmed, Dispatched, Received, Closed)
- Link GRVs to orders (multi-GRV for partial deliveries)
- Monitor lead times (supplier performance)

#### 4.2.4 Supplier Invoice Processing
- Receive supplier invoice
- Match to GRV (verify quantities, prices)
- Match to order (three-way match)
- Identify variances (price differences, quantity differences)
- Approve for payment
- Track payment status

### 4.3 Cannabis-Specific Features

#### 4.3.1 COA (Certificate of Analysis) Management
- **Requirement**: SAHPRA GMP mandates lab testing on all raw materials
- **Capture**: Upload COA PDF on GRV
- **Fields**: Lab name (ISO/IEC 17025 accredited), test date, THC%, CBD%, contaminants, microbial tests
- **Validation**: Reject GRV if COA missing or fails specifications
- **Traceability**: Link finished goods to source material COAs

#### 4.3.2 Supplier Compliance
- **License Verification**: Ensure supplier has valid SAHPRA/DALRRD license
- **Quality Audits**: Track supplier quality scores (reject rates, contamination incidents)
- **Transport Documentation**: Ensure proper permits for cannabis transport (SAPS requirements)

### 4.4 Integration Points

**Purchasing** integrates with:
- **Inventory**: Creates IN movements (GRV), OUT movements (RTS)
- **Suppliers**: Links to supplier master data (Creditors module)
- **Quality Control**: GRV can trigger QC inspection before stock release
- **Financial**: Supplier invoices feed accounts payable
- **Production**: Raw materials received via GRV feed production processes

### 4.5 PoC Implementation Plan (25% → 75%)

**Phase 12 Tasks** (Week 6):
1. Implement GRV workflow (scan products, assign batches, create movements)
2. Implement RTS workflow (select items, specify reason, create movements)
3. Add supplier order tracking (PO creation, status updates)
4. Add supplier invoice processing (capture, match to GRV)
5. Implement three-way matching (Order → GRV → Invoice)
6. Add COA capture and validation
7. Create purchasing reports (GRV summary, supplier performance, variance analysis)
8. Integrate with inventory (GRV creates movements, RTS creates movements)
9. Test end-to-end (PO → GRV → Invoice → Payment)

---

## 5. PRODUCTION MODULE

**Status**: MIXED PRIORITY
**Licensing**: Optional (required for manufacturers)
**Current Implementation**: 20-60% (varies by sub-module)
**Target**: 45-75% (depends on sub-module)

### 5.1 Module Overview

Production module consists of three sub-modules:

| Sub-Module | Purpose | Priority | Target % |
|------------|---------|----------|----------|
| **Bucking** | Remove stems/seeds from flower | LOW | 45% (stub) |
| **Manufacturing** | Extraction, distillation, formulation | LOW | 45% (stub) |
| **Retail Production** | Pre-rolls, packaging, retail units | MEDIUM | 75% (good implementation) |

### 5.2 Bucking Sub-Module (Stub Implementation)

**Purpose**: Post-harvest processing of cannabis flower

**Workflow**:
```
Harvest Batch (HARVEST-2025-001)
    ↓
Bucking Process (BATCH-2025-001)
├── Input: 10kg raw flower (stems, seeds, leaves)
├── Process: Manual bucking (remove stems/seeds)
├── Output: 8kg bucked flower (WIP)
├── Waste: 2kg stems/seeds (tracked)
└── Duration: 4 hours, 3 operators
    ↓
Movements Created:
├── IN: +8kg bucked flower (WIP), Batch BATCH-2025-001
└── Note: Raw flower movement created at harvest time
```

**PoC Implementation** (Stub Only):
- Create basic `BuckingBatch` entity
- Record input/output/waste
- Generate movements (IN for output)
- **Defer detailed workflow to post-PoC**

### 5.3 Manufacturing Sub-Module (Stub Implementation)

**Purpose**: Extraction, distillation, formulation (oils, edibles, vapes)

**Workflow** (Simplified):
```
Batch (BATCH-2025-002) - Extraction
    ↓ Step 1: Decarboxylation
    ├── Input: 5kg flower
    ├── Output: 4.8kg decarbed flower (WIP)
    └── Loss: 0.2kg (moisture evaporation)
    ↓ Step 2: Ethanol Extraction
    ├── Input: 4.8kg decarbed flower + 20L ethanol
    ├── Output: 500g crude extract (WIP)
    └── Waste: 4.3kg spent biomass + ethanol (recovered)
    ↓ Step 3: Winterization
    ├── Input: 500g crude extract
    ├── Output: 450g winterized extract (WIP)
    └── Waste: 50g fats/waxes
    ↓ Step 4: Distillation
    ├── Input: 450g winterized extract
    ├── Output: 400g distillate (finished goods)
    └── Loss: 50g residue
    ↓ Step 5: Formulation (Edibles)
    ├── Input: 400g distillate + 10kg gummy base
    ├── Output: 5,000 gummies @ 10mg THC each (50g THC total)
    └── Each gummy gets SN: SN-ED-20250101-0001 to SN-ED-20250101-5000
```

**PoC Implementation** (Stub Only):
- Create basic `ManufacturingBatch` entity
- Record multi-step process (steps table)
- Track inputs/outputs/losses per step
- **Defer full extraction/formulation workflows to post-PoC**

### 5.4 Retail Production Sub-Module (Good Implementation - 75%)

**Purpose**: Creating retail-ready products (pre-rolls, packaged flower)

**Priority**: MEDIUM (common use case for dispensaries)

#### 5.4.1 Pre-Roll Production Workflow
```
Batch (BATCH-2025-003) - Pre-Roll Production
    ↓ Step 1: Milling
    ├── Input: 10kg bucked flower (Batch BATCH-2025-001)
    ├── Process: Mechanical grinding
    ├── Output: 9.8kg milled flower (WIP)
    └── Loss: 0.2kg (dust, over-processing)
    ↓ Step 2: Pre-Roll Filling
    ├── Input: 9.8kg milled flower
    ├── Process: Automated filling machine (1g per pre-roll)
    ├── Output: 9,500 filled pre-rolls (WIP)
    └── Loss: 0.3kg (spillage, QC rejects)
    ↓ Step 3: Capping & QC
    ├── Input: 9,500 filled pre-rolls
    ├── Process: Attach filter, inspect
    ├── Output: 9,400 finished pre-rolls
    └── Waste: 100 rejects (damaged, incorrect weight)
    ↓ Step 4: Packaging
    ├── Input: 9,400 pre-rolls
    ├── Process: Pack in retail units (10 per pack)
    ├── Output: 940 packs (finished goods)
    └── Each pack gets SN: SN-PR-20250101-001 to SN-PR-20250101-940
    ↓
Movements Created:
├── OUT: -10kg flower (input), Batch BATCH-2025-001
└── IN: +940 pre-roll packs, Batch BATCH-2025-003, SNs assigned
```

#### 5.4.2 Packaged Flower Workflow
```
Batch (BATCH-2025-004) - Packaged Flower
    ↓ Step 1: Quality Selection
    ├── Input: 5kg premium flower (Batch BATCH-2025-001)
    ├── Process: Hand-select top buds
    ├── Output: 4kg premium buds (WIP)
    └── Grade B: 1kg (redirect to pre-rolls)
    ↓ Step 2: Weighing & Packaging
    ├── Input: 4kg premium buds
    ├── Process: Weigh into retail units (3.5g, 7g, 14g, 28g)
    ├── Output:
    │   ├── 200 x 3.5g packs = 700g
    │   ├── 150 x 7g packs = 1,050g
    │   ├── 100 x 14g packs = 1,400g
    │   └── 30 x 28g packs = 840g
    │   └── Total: 3,990g (4kg packaged)
    └── Loss: 10g (spillage, QC)
    ↓ Step 3: Labeling
    ├── Attach labels: Strain, THC%, CBD%, Batch, Expiry, Warnings
    ├── Assign SNs: SN-FLW-20250101-001 to SN-FLW-20250101-480
    └── All packs now ready for sale
    ↓
Movements Created:
├── OUT: -5kg flower (input), Batch BATCH-2025-001
└── IN: +480 packaged flower units, Batch BATCH-2025-004, SNs assigned
```

#### 5.4.3 Key Features (Retail Production)
- **Step-based workflow**: Each step records inputs, outputs, losses
- **Batch traceability**: Finished goods link to source flower batches
- **Serial number generation**: Automatic SN assignment on packaging
- **Yield tracking**: Calculate efficiency (output / input)
- **Loss analysis**: Track waste by type (spillage, QC rejects, over-processing)
- **QC integration**: Cannot proceed to next step if QC fails
- **Lab testing**: Finished goods require COA before sale (SAHPRA)

### 5.5 Production Movements

All production processes create movements:
- **Inputs**: OUT movements (raw materials consumed)
- **Outputs**: IN movements (finished goods produced)
- **WIP**: Tracked internally (not sold, but counted in inventory)

**Example**:
```sql
-- Pre-roll production movements
-- Input (consume flower)
INSERT INTO Movements (ProductId, MovementType, Direction, Quantity, Mass, BatchNumber, TransactionType, HeaderId, DetailId, Reason)
VALUES (101, 'ProductionInput', 'OUT', 1, 10.0, 'BATCH-2025-001', 'Production', 1, 1, 'Pre-roll production batch BATCH-2025-003');

-- Output (produce pre-rolls)
INSERT INTO Movements (ProductId, MovementType, Direction, Quantity, Mass, BatchNumber, TransactionType, HeaderId, DetailId, Reason)
VALUES (201, 'ProductionOutput', 'IN', 940, 0.94, 'BATCH-2025-003', 'Production', 1, 2, 'Pre-roll production batch BATCH-2025-003 output');
```

### 5.6 PoC Implementation Plan

**Phase 10 Tasks** (Week 4):
1. Expand `ProductionBatch` repository (business queries)
2. Implement `ProcessingStep` repository (step-based manufacturing)
3. Create `RetailProduction` repository (pre-rolls, packaging)
4. Add `QualityControl` repository methods
5. Add `LabTest` repository methods
6. Implement production movement generation
7. Create production reporting queries (yields, losses, efficiency)
8. Stub out general production and cultivation repositories

**Focus**: Retail Production (pre-rolls, packaging) to 75% completeness

---

## 6. CULTIVATION MODULE (Stub Implementation)

**Status**: LOW PRIORITY - Future expansion
**Licensing**: Optional (required for growers)
**Current Implementation**: 15%
**Target**: 35% (Basic structure only)

### 6.1 Purpose

Cultivation module tracks:
- **Individual plants** (SAHPRA Section 22C requirement)
- **Grow cycles** (planting → vegetative → flowering → harvest)
- **Grow rooms/areas** (environmental controls, lighting schedules)
- **Harvest batches** (link finished goods to source plants)

### 6.2 Key Features (Stub Implementation)

#### 6.2.1 Plant Tracking
- **Plant ID**: Unique identifier per plant (QR code tag)
- **Strain**: Genetics (indica/sativa/hybrid)
- **Mother Plant**: Clone source (if applicable)
- **Planting Date**: When planted/transplanted
- **Growth Stage**: Seedling → Vegetative → Flowering → Harvested
- **Location**: Grow room, shelf, position
- **Health Status**: Healthy, stressed, diseased, dead

#### 6.2.2 Grow Cycle Management
- **Cycle ID**: Unique identifier per grow cycle
- **Strain**: What is being grown
- **Plant Count**: Number of plants in cycle
- **Start Date**: Planting date
- **Vegetative Duration**: Weeks in vegetative stage
- **Flowering Duration**: Weeks in flowering stage
- **Harvest Date**: When harvested
- **Yield**: Total weight harvested (wet + dry)

#### 6.2.3 Harvest Batch Creation
```
Grow Cycle (CYCLE-2025-001)
├── 50 plants (Strain: Gelato)
├── Planted: 2025-01-01
├── Vegetative: 4 weeks
├── Flowering: 8 weeks
└── Harvest: 2025-03-01
    ↓
Harvest (HARVEST-2025-001)
├── Wet weight: 25kg (500g per plant)
├── Drying: 14 days
├── Dry weight: 5kg (20% moisture retention)
├── Trim: 2kg (sold separately or used for extraction)
└── Waste: 18kg (stems, roots, fan leaves)
    ↓
Movements Created:
├── IN: +5kg dried flower, Batch HARVEST-2025-001
└── IN: +2kg trim, Batch HARVEST-2025-001-TRIM
```

### 6.3 Compliance Features
- **Plant Limits**: Track against legal limits (4 flowering plants per person, 8 per household - private use)
- **SAHPRA Tracking**: Individual plant tracking for licensed growers (Section 22C)
- **DALRRD Hemp Permits**: THC testing, crop reporting (for hemp cultivation)
- **Destruction Records**: Track destroyed plants (male plants, hermaphrodites, diseased)

### 6.4 PoC Implementation (Stub Only)
- Basic entity structure (Plant, GrowCycle, GrowRoom, HarvestBatch) ✅ Created in Phase 5
- Link harvest batches to finished goods inventory
- **Defer detailed cultivation workflows to post-PoC**

---

## 7. MODULE DEPENDENCIES

### 7.1 Dependency Graph

```
                    ┌─────────────┐
                    │  Inventory  │ ◄─── CORE MODULE (all depend on this)
                    │  (Mandatory)│
                    └─────────────┘
                          ▲
         ┌────────────────┼────────────────┐
         │                │                │
    ┌────┴─────┐    ┌────┴─────┐    ┌────┴─────┐
    │ Retail   │    │Wholesale │    │Purchasing│
    │   POS    │    │  Sales   │    │(Mandatory│
    │(Optional)│    │(Optional)│    └──────────┘
    └──────────┘    └──────────┘          ▲
         ▲                │                │
         │                │                │
         │                └────────┐       │
         │                         │       │
    ┌────┴─────┐            ┌─────▼───────┴──┐
    │Production│            │   Suppliers    │
    │(Optional)│            │   (Creditors)   │
    └────┬─────┘            └─────────────────┘
         │
         │
    ┌────▼─────────┐
    │ Cultivation  │
    │  (Optional)  │
    └──────────────┘
```

### 7.2 Licensing Configurations

**Minimum License** (Retail Store):
- ✅ Inventory (mandatory)
- ✅ Purchasing (mandatory)
- ✅ Retail POS (one sales module required)
- ❌ Production (not needed)
- ❌ Cultivation (not needed)
- ❌ Wholesale (not needed)

**Manufacturer License**:
- ✅ Inventory (mandatory)
- ✅ Purchasing (mandatory)
- ✅ Wholesale (sell to retailers)
- ✅ Production (manufacturing required)
- ✅ Cultivation (if growing own raw materials)
- ❌ Retail POS (not selling direct to consumer)

**Vertically Integrated License** (Full operation):
- ✅ Inventory (mandatory)
- ✅ Purchasing (mandatory)
- ✅ Retail POS (dispensary)
- ✅ Wholesale (sell to other retailers)
- ✅ Production (manufacturing)
- ✅ Cultivation (grow own cannabis)

### 7.3 Data Flow Example (Full Vertical Integration)

```
1. CULTIVATION
   ├── Plant seeds (50 plants)
   ├── Grow cycle (12 weeks)
   └── Harvest: 5kg flower
        ↓ Movement: IN +5kg, Batch HARVEST-2025-001

2. PRODUCTION (Bucking)
   ├── Input: 5kg raw flower
   ├── Process: Remove stems/seeds
   └── Output: 4kg bucked flower
        ↓ Movement: OUT -5kg (input), IN +4kg (output)

3. PRODUCTION (Pre-Rolls)
   ├── Input: 4kg bucked flower
   ├── Process: Milling → Filling → Packaging
   └── Output: 3,800 pre-rolls (3.8kg)
        ↓ Movement: OUT -4kg (input), IN +3,800 units (output)

4. INVENTORY
   ├── SOH: 3,800 pre-rolls available
   └── Ready for sale

5a. RETAIL POS
   ├── Customer buys 20 pre-rolls
   ├── Sale: R400 (20 x R20)
   └── Movement: OUT -20 units
        ↓ SOH: 3,780 pre-rolls

5b. WHOLESALE
   ├── Retailer orders 1,000 pre-rolls
   ├── Invoice: R15,000 (wholesale price R15 each)
   └── Movement: OUT -1,000 units
        ↓ SOH: 2,780 pre-rolls

6. PURCHASING
   ├── Buy packaging materials (boxes, labels)
   ├── GRV: +10,000 boxes received
   └── Movement: IN +10,000 boxes
        ↓ SOH: 10,000 boxes (used in production packaging step)
```

---

## ✅ MODULE IMPLEMENTATION PRIORITY

| Priority | Module | Target % | Reason |
|----------|--------|----------|--------|
| 🔴 CRITICAL | Inventory | 90% | Core module, all others depend on it |
| 🔴 CRITICAL | Movement Architecture | 95% | Backbone of entire system |
| 🟠 HIGH | Retail POS | 85-90% | Market entry product |
| 🟠 HIGH | Purchasing | 75% | Required for inventory flow (GRV, RTS) |
| 🟡 MEDIUM | Production (Retail) | 75% | Common use case (pre-rolls, packaging) |
| 🟢 LOW | Production (General) | 45% | Stub for future expansion |
| 🟢 LOW | Cultivation | 35% | Stub for future expansion |
| 🟢 LOW | Wholesale | 25% | Stub for future expansion |

---

**Document Status**: ✅ COMPLETE
**Next**: Read **02-INVENTORY-MODULE-SPEC.md** for detailed inventory architecture
