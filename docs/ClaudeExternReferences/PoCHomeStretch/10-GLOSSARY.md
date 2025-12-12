# Project420 - Glossary & Terminology Reference
## System and Legal Terminology for Cannabis Management

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Part of**: PoC Home Stretch Specification Suite
**Purpose**: Clarify terminology used in Project420 specifications

---

## 📋 TABLE OF CONTENTS

1. [System Terms](#1-system-terms)
2. [Database & Architecture Terms](#2-database--architecture-terms)
3. [Transaction & Movement Terms](#3-transaction--movement-terms)
4. [Batch & Serial Number Terms](#4-batch--serial-number-terms)
5. [Production Terms](#5-production-terms)
6. [Cannabis-Specific Terms](#6-cannabis-specific-terms)
7. [Legal & Compliance Terms](#7-legal--compliance-terms)
8. [South African Legal Terms](#8-south-african-legal-terms)

---

## 1. SYSTEM TERMS

### AuditableEntity
Base class for all database entities. Provides audit fields: `CreatedAt`, `CreatedBy`, `ModifiedAt`, `ModifiedBy`, `IsDeleted`, `DeletedAt`, `DeletedBy`. All entities in Project420 must inherit from this class to ensure POPIA compliance (7-year audit trail).

### DTO (Data Transfer Object)
Object used to transfer data between layers (BLL → UI, BLL → API). Entities are never exposed directly; always mapped to DTOs. Prevents tight coupling and allows controlled data exposure.

### Repository Pattern
Data access pattern that abstracts database operations. All database queries go through repositories, not direct `DbContext` access. Enables unit testing with mocks and centralizes data logic.

### Service Layer
Business logic layer (BLL). Contains services that orchestrate business operations, validate data, enforce rules, and coordinate between repositories. Example: `ProductService`, `TransactionService`.

### Soft Delete
Deletion strategy where records are marked as deleted (`IsDeleted = true`) but not physically removed from database. Required for POPIA compliance (data must be retained for 7 years for audits).

### SOH (Stock on Hand)
Current quantity of stock available. CRITICAL: SOH is NEVER stored in a column; always calculated from movements. `SOH = SUM(IN movements) - SUM(OUT movements)`.

### Three-Tier Architecture
Architectural pattern used in Project420:
- **UI Layer**: Blazor/MAUI (user interface)
- **BLL Layer**: Services, validators, DTOs (business logic)
- **DAL Layer**: Repositories, DbContexts (data access)

### Validator
FluentValidation class that enforces business rules on DTOs. Example: `CreateProductValidator` ensures product name is not empty, THC% is valid, lab test date is provided. All DTOs must have validators.

---

## 2. DATABASE & ARCHITECTURE TERMS

### DbContext
Entity Framework Core class that manages database connections and entity tracking. Example: `PosDbContext`, `ManagementDbContext`. Each module has its own DbContext.

### Fluent API
Entity Framework Core configuration method using method chaining. Used in `OnModelCreating` to define relationships, indexes, constraints. Preferred over Data Annotations for complex configurations.

### Migration
Database schema change. Generated with `dotnet ef migrations add MigrationName`. Applied with `dotnet ef database update`. All schema changes must go through migrations (no manual SQL).

### Index
Database performance optimization. Created on frequently queried columns. Example: `IX_Movements_ProductId_TransactionDate` for SOH queries.

### Partitioning
Database optimization technique. Large tables (like `Movements`) partitioned by date (monthly/quarterly) for faster queries. Old partitions archived after 2 years.

### Foreign Key (FK)
Database relationship constraint. Links two tables. Example: `TransactionDetail.ProductId` references `Products.ProductId`. Ensures referential integrity.

### Primary Key (PK)
Unique identifier for database record. Usually named `EntityNameId` (e.g., `ProductId`, `BatchId`). Always identity column (auto-increment).

---

## 3. TRANSACTION & MOVEMENT TERMS

### Transaction
Business event that may affect stock. Example: Sale, GRV, Production Run, Transfer. Structure: Header (summary) + Details (line items). Transactions generate movements.

### Transaction Header
Summary record for transaction. Contains: transaction number, date, customer/supplier, totals, status. Example: `RetailTransactionHeader`, `PurchaseHeader`.

### Transaction Detail
Line item in transaction. Contains: product, quantity, price, discounts, VAT, batch, serial number. Links to header via `HeaderId`. All transaction types use unified `TransactionDetails` table (Option A architecture).

### Movement
Physical stock change. Example: Stock IN (GRV, production output), Stock OUT (sale, production input). Created automatically from transaction details. Used to calculate SOH.

### Movement Type
Classification of movement. Examples: `GRV`, `Sale`, `ProductionInput`, `ProductionOutput`, `TransferIn`, `TransferOut`, `AdjustmentIn`, `AdjustmentOut`.

### Direction
Movement direction: `IN` (increases SOH) or `OUT` (decreases SOH).

### Option A Architecture
Recommended architecture for Project420. Specialized header tables per module (RetailTransactionHeaders, PurchaseHeaders, ProductionHeaders) + single unified `TransactionDetails` table. Enables consistent detail structure across all transaction types.

### Option B Architecture
Alternative architecture (NOT recommended). Separate header + detail tables per module (RetailTransactionHeaders → RetailTransactionDetails, PurchaseHeaders → PurchaseDetails). Causes schema duplication and reporting complexity.

### TransactionType Discriminator
Enum field in `TransactionDetails` that indicates transaction type: `Sale`, `GRV`, `ProductionInput`, etc. Used to link detail record to correct header table.

---

## 4. BATCH & SERIAL NUMBER TERMS

### Batch
Group of products manufactured together. Identified by batch number. Enables traceability (if contamination found, recall entire batch). Example: `BATCH-2025-0001` or `0102202501150042`.

### Batch Number
Unique identifier for batch. Format: `SSTTYYYYMMDDNNNN` (16 digits). Embeds site, type, date, sequence. Example: `0102202501150042` = Site 01, Production batch, 2025-01-15, sequence 0042.

### System Batch
Batch number generated by Project420 system (format: 16 digits). Used for internal traceability and SOH tracking.

### Legal Cultivation Batch
Batch number required by SAHPRA for cultivation tracking. May differ from system batch. Links grow cycle to harvest.

### Serial Number (SN)
Unique identifier for individual unit. Required for serialized products (edibles, vapes, packaged flower). Enables unit-level traceability (which customer bought which specific unit).

### Full Serial Number
28-digit serial number with embedded metadata: site, strain, product type, date, batch, unit sequence, weight, check digit. Used for QR codes. Example: `0142320250115000420000100035094`.

### Short Serial Number
13-digit serial number (EAN-13 compatible) for barcodes. Format: `SSYYMMDDNNNNN`. Example: `0125011500042`.

### Check Digit
Validation digit calculated using Luhn algorithm. Detects transcription errors. Last 1-2 digits of serial number.

### Traceability
Ability to track product from source to sale (forward) or from sale back to source (backward). Required for SAHPRA compliance. Example: SN → Batch → Processing Steps → Source Batch → Grow Cycle → Plant.

---

## 5. PRODUCTION TERMS

### Production Batch
Batch created during manufacturing. Tracks inputs (raw materials consumed) and outputs (finished goods produced). Links to source batches for traceability.

### Processing Step
Stage in production workflow. Example: Bucking → Milling → Filling → Capping → Packaging. Each step records inputs, outputs, losses, duration, operator.

### WIP (Work in Progress)
Partially completed product. Tracked internally in processing steps, not sold. Example: Milled flower (after milling step, before filling step).

### Finished Goods
Completed product ready for sale. Created at final processing step (e.g., packaging). Assigned serial numbers.

### Single-Batch Multi-Step Manufacturing
Production model where one batch flows through multiple steps without creating new batch numbers. Batch number remains same from input to output. Simplifies traceability.

### Input
Raw material consumed in production. Creates OUT movement (reduces SOH). Example: 10kg flower used in pre-roll production.

### Output
Finished product produced. Creates IN movement (increases SOH). Example: 9,500 pre-rolls produced.

### Loss
Material lost during production (spillage, QC rejects, over-processing). Tracked per step but not added back to inventory. Example: 0.2kg dust from milling.

### Yield
Percentage of input that becomes output. Formula: `Yield = (Output / Input) * 100`. Example: 10kg input → 9.5kg output = 95% yield.

### Bucking
Post-harvest processing. Removes stems, seeds, fan leaves from harvested flower. First step in production chain.

### Retail Production
Production of retail-ready products (pre-rolls, packaged flower). Distinct from general production (extraction, formulation).

---

## 6. CANNABIS-SPECIFIC TERMS

### Strain
Cannabis genetics. Example: Gelato, OG Kush, Sour Diesel. Each strain has unique THC%, CBD%, terpene profile. Tracked in system for product identification and traceability.

### THC (Tetrahydrocannabinol)
Primary psychoactive compound in cannabis. Measured as percentage. Required on all product labels and receipts. Example: 22.5% THC.

### CBD (Cannabidiol)
Non-psychoactive compound in cannabis. Measured as percentage. Required on all product labels and receipts. Example: 0.8% CBD.

### COA (Certificate of Analysis)
Lab test report. Required for all cannabis products (SAHPRA GMP). Contains: THC%, CBD%, contaminants, microbial tests, heavy metals. Must be from ISO/IEC 17025 accredited lab.

### Cannabis Flower
Dried cannabis buds. Primary product form. Sold by weight (grams). Can be smoked or used as input for production.

### Pre-Roll
Pre-rolled cannabis joint. Manufactured product. Usually 0.5g, 1.0g, or 2.0g. Must be serialized.

### Edible
Cannabis-infused food product. Example: Gummies, chocolates, baked goods. Highly regulated, must be serialized, lab tested, properly labeled.

### Vape/Cartridge
Cannabis vape cartridge. Contains distillate or live resin. Must be serialized, lab tested.

### Concentrate
Cannabis extract. Example: Wax, shatter, budder, live resin. High THC content (60-90%). Must be lab tested.

### Trim
Leaves trimmed from cannabis flower during bucking. Lower quality than flower. Can be sold as-is or used for extraction.

### Seed-to-Sale Traceability
Regulatory requirement. Track every cannabis plant from seed/clone through cultivation, harvesting, processing, distribution, to final sale. Required by SAHPRA.

---

## 7. LEGAL & COMPLIANCE TERMS

### SAHPRA (South African Health Products Regulatory Authority)
Regulatory body overseeing medicinal cannabis in South Africa. Issues Section 22C licenses. Requires seed-to-sale tracking, GMP compliance, lab testing, monthly reporting.

### Section 22C License
SAHPRA license for cultivation, production, or sale of medicinal cannabis. Businesses must hold appropriate Section 22C to operate legally.

### DALRRD (Department of Agriculture, Land Reform and Rural Development)
Issues hemp permits. Hemp defined as cannabis with <0.001% THC (flower) or <0.01% THC (other plant parts). Requires crop reporting, THC testing.

### POPIA (Protection of Personal Information Act)
South African data protection law. Requires: encryption, consent tracking, audit trails, 7-year retention, right to erasure. Effective July 1, 2021. Non-compliance penalties: up to R10 million fine or 10 years imprisonment.

### Cannabis for Private Purposes Act 2024
South African law signed May 28, 2024. Legalizes personal cannabis use (18+). Limits: 1.2kg dried cannabis at home, 600g on person, 4 flowering plants per person (8 per household). Commercial sales still illegal (expected 2026-2027).

### GMP (Good Manufacturing Practice)
Quality standard required by SAHPRA for cannabis production. Includes: batch tracking, QC testing, lab testing, contamination prevention, documented procedures, audit trails.

### Age Verification
Legal requirement: Cannot sell cannabis to persons under 18. System must enforce age verification before every sale. ID scanning or manual confirmation required.

### Audit Trail
Immutable log of all system actions. Required for POPIA compliance. Must track: who, what, when, why. Retention: 7 years minimum (SAHPRA + SARS).

### Possession Limit
Legal limit on cannabis quantity. Cannabis Act 2024: 1.2kg at home, 600g on person. System should warn (not prevent) if customer exceeds recommended limits.

### Destruction Record
Required when destroying cannabis stock (expired, contaminated, non-compliant). Must document: quantity, reason, method, date, witnesses. Required for SAHPRA audits.

---

## 8. SOUTH AFRICAN LEGAL TERMS

### SARS (South African Revenue Service)
Tax authority. Requires accurate VAT calculations, tax reporting, 5-year record retention.

### VAT (Value-Added Tax)
15% sales tax in South Africa. Applied as VAT-inclusive pricing (price shown includes VAT). Example: R115 price = R100 VAT-exclusive + R15 VAT.

### VAT-Inclusive Pricing
South African standard. Prices displayed to customers include VAT. Formula: `VAT = Price / 1.15 * 0.15`.

### VAT201
Monthly VAT return submitted to SARS. Reports: output VAT (on sales), input VAT (on purchases), net VAT payable/receivable.

### Three-Way Matching
Purchasing validation. Compare Purchase Order → GRV (Goods Received) → Supplier Invoice. Ensures quantities and prices match before payment approval.

### GRV (Goods Received Voucher)
Document confirming goods received from supplier. Creates IN movements in inventory. Links to purchase order and supplier invoice.

### RTS (Return to Supplier)
Document for returning goods to supplier (defective, damaged, incorrect). Creates OUT movements in inventory. Supplier issues credit note.

### Credit Note
Document for customer refund or supplier return credit. Negative transaction. Creates IN movements (customer refund) or reduces payable (supplier credit).

### Cash Drop
Removing excess cash from till (secure to safe). Financial only, does not affect inventory.

### Cash Out
End-of-shift reconciliation. Compare expected cash/card totals (from system) vs actual counted. Record variance.

### Till Float
Starting cash in till. Usually fixed amount (e.g., R500). Excludes from cash out calculations.

---

## 🔍 QUICK TERMINOLOGY LOOKUPS

### Batch vs Serial Number
- **Batch**: Group of products (example: 10,000 pre-rolls from same production run)
- **Serial Number**: Individual unit (example: one specific pre-roll from that batch)

### Transaction vs Movement
- **Transaction**: Business event (example: customer sale)
- **Movement**: Physical stock change (example: OUT movement reducing SOH)

### System Batch vs Legal Batch
- **System Batch**: Internal tracking number (16 digits)
- **Legal Batch**: SAHPRA-required tracking (may differ from system)

### WIP vs Finished Goods
- **WIP**: Partially complete product (tracked in processing steps, not sold)
- **Finished Goods**: Complete product ready for sale

### Soft Delete vs Hard Delete
- **Soft Delete**: Mark as deleted but retain in database (POPIA compliant)
- **Hard Delete**: Physically remove from database (NEVER use in Project420)

### SOH vs SOC
- **SOH (Stock on Hand)**: Physical quantity available now
- **SOC (Stock on Commit)**: Quantity committed to orders but not yet dispatched (future enhancement)

### IN Movement vs OUT Movement
- **IN Movement**: Increases SOH (GRV, production output, customer return)
- **OUT Movement**: Decreases SOH (sale, production input, supplier return)

### Retail vs Wholesale
- **Retail**: B2C (selling to end consumers, requires age verification)
- **Wholesale**: B2B (selling to other businesses, requires license verification)

### Cultivation vs Production
- **Cultivation**: Growing cannabis plants (DALRRD hemp permits, SAHPRA Section 22C)
- **Production**: Processing harvested flower into products (SAHPRA Section 22C, GMP required)

### Flower vs Concentrate
- **Flower**: Dried cannabis buds (10-25% THC typically)
- **Concentrate**: Extracted cannabis (60-90% THC, highly regulated)

---

## 📊 ACRONYM REFERENCE

| Acronym | Full Term | Context |
|---------|-----------|---------|
| **API** | Application Programming Interface | Web services, integrations |
| **BLL** | Business Logic Layer | Service layer, business rules |
| **COGS** | Cost of Goods Sold | Financial reporting |
| **COA** | Certificate of Analysis | Lab testing, compliance |
| **CRUD** | Create, Read, Update, Delete | Basic data operations |
| **DAL** | Data Access Layer | Repository layer, database |
| **DALRRD** | Dept. Agriculture, Land Reform, Rural Development | Hemp permits, cultivation |
| **DTO** | Data Transfer Object | Layer communication |
| **EAN** | European Article Number | Barcode standard (EAN-13) |
| **EF** | Entity Framework | ORM (Object-Relational Mapper) |
| **FIFO** | First In, First Out | Inventory valuation method |
| **FK** | Foreign Key | Database relationship |
| **GMP** | Good Manufacturing Practice | Quality standard (SAHPRA) |
| **GRV** | Goods Received Voucher | Purchasing, receiving |
| **ISO** | International Organization for Standardization | Lab accreditation (ISO 17025) |
| **MAUI** | Multi-platform App UI | Mobile app framework |
| **MVP** | Minimum Viable Product | Development methodology |
| **ORM** | Object-Relational Mapper | Database abstraction (EF Core) |
| **PK** | Primary Key | Database unique identifier |
| **PoC** | Proof of Concept | Early-stage development |
| **POPIA** | Protection of Personal Information Act | Data protection law |
| **POS** | Point of Sale | Retail checkout system |
| **QC** | Quality Control | Testing, inspection |
| **RTS** | Return to Supplier | Purchasing, returns |
| **SAHPRA** | SA Health Products Regulatory Authority | Medicinal cannabis regulator |
| **SARS** | South African Revenue Service | Tax authority |
| **SKU** | Stock Keeping Unit | Product identifier |
| **SN** | Serial Number | Unit-level identifier |
| **SOH** | Stock on Hand | Current inventory quantity |
| **SQL** | Structured Query Language | Database query language |
| **THC** | Tetrahydrocannabinol | Cannabis psychoactive compound |
| **UI** | User Interface | Presentation layer |
| **VAT** | Value-Added Tax | 15% sales tax (SA) |
| **WIP** | Work in Progress | Partially complete product |

---

## 🎓 COMPLIANCE TERMINOLOGY QUICK REFERENCE

### SAHPRA Requirements
- **Section 22C License**: Required for cultivation, production, sale of medicinal cannabis
- **GMP Compliance**: Manufacturing quality standard (batch tracking, QC, lab testing)
- **Seed-to-Sale**: Track every plant/product from origin to sale
- **Monthly Reporting**: Submit inventory, production, sales, destruction reports
- **Lab Testing**: ISO/IEC 17025 accredited labs only
- **COA**: Certificate of Analysis required for every batch

### POPIA Requirements
- **Encryption**: AES-256 at rest, TLS 1.3 in transit
- **Consent Tracking**: Document customer consent for data collection
- **Audit Trails**: Immutable logs, 7-year retention
- **Right to Erasure**: Soft delete with anonymization capability
- **Data Breach Notification**: Report within 72 hours
- **Penalties**: Up to R10M fine or 10 years imprisonment

### Cannabis Act 2024 Requirements
- **Age Verification**: 18+ mandatory (R100K fine or 2 years for underage sales)
- **Possession Limits**: 1.2kg home, 600g on person (warning only, not enforced)
- **Cultivation Limits**: 4 flowering plants per person, 8 per household (private use)
- **Commercial Sales**: Still illegal (expected 2026-2027)

### SARS Requirements
- **VAT**: 15% VAT-inclusive pricing
- **VAT201**: Monthly VAT return
- **Record Retention**: 5 years tax records, 7 years SAHPRA/POPIA
- **Excise Duty**: Future cannabis-specific excise (expected 2026+)

---

**Document Status**: ✅ COMPLETE
**Purpose**: Reference for all Project420 documentation and development
**Usage**: Consult when encountering unfamiliar terms in specifications
