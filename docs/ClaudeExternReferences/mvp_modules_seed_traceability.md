# MVP Modules for a Cannabis Seed–Sale Traceability System

This document outlines the essential **Minimum Viable Product (MVP)** modules required to build a functional, compliant, and fully traceable cannabis seed–sale system. These modules represent the smallest version of the system that still delivers real operational and regulatory value.

---

## 1. Authentication & User Management
- Secure login/logout
- Role-based access control (Admin, Production, QC, Inventory, Sales)
- Permission control per module
- User activity tracking

---

## 2. Product & Strain Master Data
- Product catalog (Seeds, Seed Packs, Clones, Flower, etc.)
- Strain registry
- SKU management
- Active/inactive lifecycle

---

## 3. Batch Management
Supports multiple batch types:
- Production Batches
- Stock Take Batches
- Transfer Batches
- Sales Batches

Core batch functions:
- Create batches with automatic numbering
- Assign batch type
- Track batch lifecycle (Open → Active → Closed)
- Store metadata (strain, facility, notes)
- Full batch history log

---

## 4. Serial Number (SN) Management
- Generate **Internal SNs** (14-char format)
- Generate customer-facing **External SNs**
- Link SNs to batch
- SN status tracking (Created, Assigned, Sold, Destroyed)
- Bulk SN generation
- SN validation rules

---

## 5. Inventory / Stock Module
- Multi-location stock tracking
- Receive stock from production or transfers
- Stock adjustments with reason codes
- Stock movement log
- Real-time stock on hand by product and batch
- Per-batch inventory tracking

---

## 6. Stock Take / Cycle Count
- Create stock take batches
- Record counted vs expected quantities
- Variance analysis and approval workflow
- Apply adjustments to inventory

---

## 7. Transfers Module
- Create transfer batches
- Assign products, quantities, and serials to transfer
- From → To facility tracking
- Dispatch and receiving workflows
- Transfer audit logs

---

## 8. Sales / Order Fulfillment
- Simple sales order creation
- Assign batches and serials to orders
- Deduct stock-after confirmation
- Generate packing slips or labels

---

## 9. Compliance & Traceability Ledger
- Full audit trail of all actions
- Batch lifecycle traceability
- Serial lifecycle traceability
- Exportable traceability reports (CSV/PDF)
- Seed-to-sale lineage tracking

---

## 10. Basic Reporting & Dashboards
- Inventory overview
- Batch status reports
- Serial usage summaries
- Stock movement logs
- Sales history

---

## 11. Settings & Configuration
- Configure batch types and numbering
- Configure serial generation rules
- Define facilities and locations
- System-level preferences (prefixes, counters, permissions)

---

# Optional (Recommended After MVP)

## A. QR Code API + Serial Validation Portal
- Scan SN → view origin, strain, and batch information
- Public verification interface

## B. Packaging & Label Printing
- Generate labels for seed packs, batches, and transfers
- Print-ready formats

## C. Quality Control Module
- Germination logs
- Laboratory test uploads
- Compliance certificate storage

## D. Advanced Analytics
- Sales trends
- Inventory forecasting
- Batch performance analysis

---

# Summary
The above modules form a complete MVP capable of supporting cannabis seed-to-sale traceability, inventory control, serial tracking, and compliance. Additional optional modules can extend the system into a full commercial SaaS traceability platform.

