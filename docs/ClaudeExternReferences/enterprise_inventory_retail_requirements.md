# Enterprise-Grade Inventory, Retail, and Movement Tracking Requirements

## 1. Overview
This document defines the enterprise-grade requirements for **Inventory**, **Sales (Wholesale & Retail)**, **Purchasing**, and **Product Movements** within the cannabis manufacturing, processing, and retail ecosystem. These requirements form the basis of the next phase of system development and provide the guidance needed for generating advanced implementations using Claude.

Inventory is the **core mandatory module** that enables all other modules. Retail is our **market-entry point** and must be highly robust. Wholesale and Production/Cultivation are optional licensing components but depend on Inventory.

---

# 2. Core Principles
- Inventory must be correct, reliable, and legally compliant.
- Movements must be immutable, auditable, and efficient at scale.
- SOH must never be based on a single table—only **summed movements**.
- Sales, Purchases, and Production must all generate consistent movement entries.
- Batch, SN, and Product lineage must always remain traceable.
- Header/Detail relationships must maintain modular but clean separation.
- All modules must remain extensible for future law changes.

---

# 3. Inventory Module Requirements (Core Mandatory Module)

## 3.1 Purpose
Inventory exists to:
- Track quantities and mass accurately.
- Provide real-time SOH visibility.
- Enable sales, production, purchasing, and compliance processes.
- Track all product types (raw, WIP, finished goods, serialized items).
- Ensure legal compliance and auditability.

## 3.2 Functional Requirements
### 3.2.1 Stock Tracking
- Track **qty**, **mass**, **potency**, **batch**, and **serial numbers**.
- Track **WIP** and **finished goods**.
- Track expiry dates, packaging, and product shelf changes.

### 3.2.2 SOH Calculation Model
SOH is calculated from:
```
SUM(all positive movements) - SUM(all negative movements)
```
Stock movement types include:
- Purchases (GRV)
- Returns to Supplier (RTS)
- Wholesale Sales
- Retail Sales (POS)
- Production Inputs
- Production Outputs
- Transfer In/Out
- Adjustments

SOH is **never** directly edited. It is derived from movement lines.

### 3.2.3 Movement Tracking Model
All movements must:
- Be immutable.
- Contain references to batch, SN, product, and transaction source.
- Contain movement reason/notes.
- Support traceability from inception to consumption.
- Be optimized for mass scale.

Movement types include:
- `IN` Movements (GRV, Return from Customer, Transfer In, Production Output)
- `OUT` Movements (Sale, RTS, Transfer Out, Production Input)

### 3.2.4 Compliance & Traceability
- Maintain full batch lineage.
- Maintain step lineage for manufactured goods.
- Support cannabis-specific compliance fields.
- Track variance, waste, and shrinkage legally.
- Audit trail for every change.

---

# 4. Movement Architecture (Critical Component)
This is the system’s backbone. If movement tracking is flawed, the entire platform fails.

## 4.1 Architectural Mandates
- **No single movement table SOH dependency** (SOH always recalculated).
- **Movements must be created by every module** (Sales, Purchasing, Production, Transfers).
- **Movement table must be optimized for scale** using:
  - partitioning
  - indexing
  - cached SOH projections
  - incremental aggregation models

## 4.2 Movement Table Structure
Minimal required fields:
- MovementID
- MovementType
- Direction (+/-)
- ProductID
- BatchID (null for non-batched items)
- SerialNumber (nullable for non-serialized)
- Qty
- Mass
- Value
- TransactionType
- HeaderID
- DetailID
- MovementReason
- Timestamp
- UserID / SystemID

## 4.3 Movement Creation Standards
Every movement:
- Must originate from a valid header/detail source.
- Must be traceable back to its origin.
- Must create one movement per stock-affecting unit.
- Must maintain link to transaction type (Sale, GRV, Production, etc.)

---

# 5. Transaction Header/Detail Architecture
This is the point of debate.

## 5.1 Two Design Options
### **Option A — Specialized Headers + Shared Detail Table (Recommended)**
Tables:
- WholesaleTransactionHeaders
- RetailTransactionHeaders
- PurchaseHeaders
- ProductionHeaders
- TransferHeaders

And one detail table:
- TransactionDetails

Where:
```
TransactionDetails.HeaderID + TransactionType → identifies the correct header
```

### Benefits of Option A
- Reduces schema duplication.
- Consistent detail model.
- Easy to scale transaction types.
- Ideal for multi-module systems.
- Efficient reporting.

### Drawbacks of Option A
- Slight complexity around linking types.
- Requires strict referential integrity.

---

### **Option B — One Header/One Detail per Module**
Example:
- WholesaleHeader → WholesaleDetail
- RetailHeader → RetailDetail
- PurchaseHeader → PurchaseDetail

### Benefits of Option B
- Simpler for beginners.
- Strong natural referential integrity.

### Drawbacks of Option B
- Does not scale well.
- Repeating fields across many tables.
- Harder to build unified reporting.
- Poor for enterprise systems.

---

# 5.2 Decision
**Option A is the enterprise-grade choice and should be implemented.**

Why?
- Cleaner architecture.
- Shared detail logic.
- Extensible for future regulatory or product-type expansions.
- Centralized movement creation logic.

---

# 6. Sales Module Requirements
Sales consists of two major modes:
- **Wholesale** (B2B)
- **Retail/POS** (B2C)

Both must integrate tightly with Inventory and Movement Tracking.

---

# 6.1 Wholesale Requirements (Optional Module)
Wholesale must support:
- Quotation → Order → Dispatch → Invoice
- Tax rules
- Batch-specific picking
- Serialized picking where applicable
- Credit Notes
- Returns

Headers include:
- WholesaleTransactionHeaders (Type=Wholesale)

Details stored in:
- TransactionDetails

Movements created:
- OUT when invoicing
- IN when returning

---

# 6.2 Retail/POS Requirements (Essential, Optional Licensing)
Retail will be our **market entry feature** and must be robust.

## 6.2.1 POS Workflow
- Scan EAN (non-serialized)
- Scan SN (serialized)
- Manual product search
- Client ID scanning or manual entry
- Add line discounts
- Add transaction discounts
- Accept multiple tender types
- Handle payouts/cashdrops
- Process refunds, full or partial
- Cancel transactions

## 6.2.2 Key POS Requirements
- Accurate VAT calculation
- High-speed scanning
- Scale integration
- Auto SN selection from available stock
- Cash drawer integration
- Standalone offline cache (future phase)

## 6.2.3 POS Movement Logic
- Sale → OUT movement
- Refund → IN movement
- Cancel → No movement
- Payouts/cashdrops → Financial only (no inventory)

---

# 7. Purchasing Module (Required)
Purchasing must support:
- GRV (Goods Received Voucher)
- RTS (Return to Supplier)
- Supplier Invoice
- Supplier Order tracking

Headers:
- PurchaseHeaders

Details:
- TransactionDetails

Movements:
- GRV → IN
- RTS → OUT

---

# 8. Production Module (Optional Licensing)
Production includes:
- Bucking
- Flower Manufacturing (Pre-rolls)
- Extraction
- Formulation (Edibles, Oils)

Uses the **single-batch multi-step manufacturing model** documented separately.

Production movements:
- Input → OUT
- Output → IN

All steps must track WIP, yields, losses, potency (where applicable).

---

# 9. Licensing Model
Modules and licensing split:

| Module | Required | Notes |
|--------|----------|-------|
| **Inventory** | Yes | Core module, always included |
| **Retail Sales (POS)** | Optional | Main entry point to market |
| **Wholesale Sales** | Optional | Add-on module |
| **Production (FG/WIP)** | Optional | Required for manufacturers |
| **Bucking** | Optional | Could be bundled with Production |
| **Purchasing** | Yes | Required for all businesses |

A license must always contain:
- Inventory
- Purchasing
- At least one sales module (Retail or Wholesale)
- Production only if manufacturing

---

# 10. Summary
This document defines the enterprise-grade architecture needed to:
- Create a world-class inventory system
- Handle aggressive movement tracking requirements
- Support cannabis manufacturing and retail
- Maintain compliance and traceability
- Scale to large product catalogs and high-volume POS environments

This is the foundation Claude must use when generating architecture, code, and database schema for the system.

