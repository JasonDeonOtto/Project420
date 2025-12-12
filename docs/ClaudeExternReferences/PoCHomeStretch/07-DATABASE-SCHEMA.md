# 07 - DATABASE SCHEMA SPECIFICATION

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: PoC Conceptual Schema Design
**Related Documents**:
- [00-MAIN-SPECIFICATION.md](00-MAIN-SPECIFICATION.md) - Central specification index
- [01-MODULE-DESCRIPTIONS.md](01-MODULE-DESCRIPTIONS.md) - All module overviews
- [02-INVENTORY-MODULE-SPEC.md](02-INVENTORY-MODULE-SPEC.md) - Inventory entities
- [03-MOVEMENT-ARCHITECTURE.md](03-MOVEMENT-ARCHITECTURE.md) - Movement tables
- [04-RETAIL-POS-REQUIREMENTS.md](04-RETAIL-POS-REQUIREMENTS.md) - Retail entities
- [05-PRODUCTION-MODEL.md](05-PRODUCTION-MODEL.md) - Production entities
- [10-GLOSSARY.md](10-GLOSSARY.md) - Terminology reference

---

## DOCUMENT PURPOSE

This document provides the **conceptual database schema** for Project420, covering all modules and their relationships. It serves as a **central reference** for database design, showing:

- **Table relationships** (foreign keys, one-to-many, many-to-many)
- **Key indexes** (performance optimization)
- **Data integrity constraints** (unique constraints, check constraints)
- **Audit trail patterns** (soft delete, change tracking)
- **Module boundaries** (which tables belong to which module)

**Target Audience**:
- Database Administrators (schema implementation)
- Software Architects (understanding data model)
- Developers (entity relationship reference)
- DevOps (migration planning, backup strategies)

---

## TABLE OF CONTENTS

1. [Schema Overview](#1-schema-overview)
2. [Module Table Distribution](#2-module-table-distribution)
3. [Core Tables (All Modules)](#3-core-tables-all-modules)
4. [Inventory Module Tables](#4-inventory-module-tables)
5. [Retail POS Module Tables](#5-retail-pos-module-tables)
6. [Purchasing Module Tables](#6-purchasing-module-tables)
7. [Production Module Tables](#7-production-module-tables)
8. [Cultivation Module Tables (Stub)](#8-cultivation-module-tables-stub)
9. [Movement Architecture Tables](#9-movement-architecture-tables)
10. [Audit & Compliance Tables](#10-audit--compliance-tables)
11. [Entity Relationship Diagram](#11-entity-relationship-diagram)
12. [Indexing Strategy](#12-indexing-strategy)
13. [Data Integrity & Constraints](#13-data-integrity--constraints)
14. [Migration Strategy](#14-migration-strategy)

---

## 1. SCHEMA OVERVIEW

### 1.1 Database Structure

**Database Name**: `project420_main`
**Database Engine**: PostgreSQL 17
**Schema Organization**: Single database, logical module grouping

**Total Tables** (PoC Target):
- **Core/Shared**: 8 tables (Products, Sites, Users, etc.)
- **Inventory**: 6 tables (Movements, SOH tracking, etc.)
- **Retail POS**: 4 tables (Transactions, Payments, Customers, Receipts)
- **Purchasing**: 5 tables (POs, GRVs, Suppliers, etc.)
- **Production**: 7 tables (Batches, Steps, Inputs/Outputs, etc.)
- **Cultivation**: 3 tables (stub - basic entities only)
- **Movement Architecture**: 2 tables (TransactionDetails, Movements)
- **Audit/Compliance**: 3 tables (Audit logs, compliance tracking)

**Total**: ~38 tables

### 1.2 Naming Conventions

**Tables**: PascalCase, plural (e.g., `Products`, `RetailTransactionHeaders`)
**Columns**: PascalCase, singular (e.g., `ProductId`, `ProductName`)
**Foreign Keys**: `FK_{ChildTable}_{ParentTable}` (e.g., `FK_Products_ProductCategory`)
**Indexes**: `IX_{Table}_{Column(s)}` (e.g., `IX_Products_ProductCode`)
**Unique Constraints**: `UK_{Table}_{Column(s)}` (e.g., `UK_Products_ProductCode`)

### 1.3 Audit Trail Pattern

**All tables inherit**:
```sql
-- Audit columns (standard for all tables)
IsDeleted BIT NOT NULL DEFAULT 0,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
CreatedBy NVARCHAR(100) NOT NULL,
ModifiedDate DATETIME NULL,
ModifiedBy NVARCHAR(100) NULL,
DeletedDate DATETIME NULL,
DeletedBy NVARCHAR(100) NULL
```

**Soft Delete**:
- No physical deletes (except for truly temporary data)
- Set `IsDeleted = 1` instead of DELETE
- Global query filter in EF Core: `.Where(x => !x.IsDeleted)`

---

## 2. MODULE TABLE DISTRIBUTION

### 2.1 Table Count by Module

| Module | Table Count | Completion % | Priority |
|--------|-------------|--------------|----------|
| **Core/Shared** | 8 | 100% | Critical |
| **Inventory** | 6 | 90% | Critical |
| **Movement Architecture** | 2 | 90% | Critical |
| **Retail POS** | 4 | 85% | High |
| **Purchasing** | 5 | 75% | Medium |
| **Production** | 7 | 60-75% | Medium |
| **Cultivation** | 3 | 35% (stub) | Low |
| **Audit/Compliance** | 3 | 50% | Medium |
| **TOTAL** | **38** | **~75%** | - |

### 2.2 Module Dependencies

```
Core/Shared (Foundation)
    ├── Inventory (depends on Core)
    │   ├── Movements (depends on Inventory)
    │   │   ├── Retail POS (depends on Movements)
    │   │   ├── Purchasing (depends on Movements)
    │   │   └── Production (depends on Movements)
    │   └── Cultivation (depends on Inventory) - STUB
    └── Audit/Compliance (cross-cutting)
```

---

## 3. CORE TABLES (ALL MODULES)

### 3.1 Products

**Purpose**: Master data for all inventory items.

**Relationships**:
- Many-to-One: `ProductCategory`
- One-to-Many: `Movements`, `TransactionDetails`, `SerialNumbers`

**Key Columns**:
```sql
ProductId INT PK,
ProductCode NVARCHAR(50) UNIQUE NOT NULL,
ProductName NVARCHAR(200) NOT NULL,
ProductCategoryId INT FK,
ProductType NVARCHAR(50),
IsCannabisProduct BIT,
IsBatchTracked BIT,
IsSerialTracked BIT,
StandardSellPrice DECIMAL(18,2),
ReorderLevel DECIMAL(18,4)
```

**Indexes**:
- `PK_Products` (ProductId) - Clustered
- `IX_Products_ProductCode` (ProductCode) - Unique
- `IX_Products_ProductCategory` (ProductCategoryId)
- `IX_Products_Active` (IsActive, IsDeleted)

### 3.2 ProductCategories

**Purpose**: Hierarchical categorization of products.

**Relationships**:
- Self-referencing: `ParentCategoryId` → `ProductCategoryId`
- One-to-Many: `Products`

**Key Columns**:
```sql
ProductCategoryId INT PK,
CategoryCode NVARCHAR(50) UNIQUE NOT NULL,
CategoryName NVARCHAR(200) NOT NULL,
ParentCategoryId INT NULL FK (self-reference),
CategoryLevel INT,
CategoryPath NVARCHAR(500), -- e.g., "/1/5/12/"
IsCannabisCategory BIT
```

**Indexes**:
- `IX_ProductCategories_Parent` (ParentCategoryId)
- `IX_ProductCategories_Path` (CategoryPath)

### 3.3 Sites

**Purpose**: Physical locations (warehouses, retail stores, production facilities).

**Relationships**:
- One-to-Many: `Zones`, `Movements`, `Transactions`, `ProductionBatches`

**Key Columns**:
```sql
SiteId INT PK,
SiteCode NVARCHAR(50) UNIQUE NOT NULL,
SiteName NVARCHAR(200) NOT NULL,
SiteType NVARCHAR(50), -- 'Warehouse', 'RetailStore', 'ProductionFacility'
LicenseNumber NVARCHAR(100), -- Cannabis license
LicenseExpiry DATE
```

**Indexes**:
- `IX_Sites_SiteCode` (SiteCode) - Unique
- `IX_Sites_Active` (IsActive, IsDeleted)

### 3.4 Zones

**Purpose**: Sections within a site (e.g., Receiving, Storage, Picking).

**Relationships**:
- Many-to-One: `Site`
- One-to-Many: `Bins`, `Movements`

**Key Columns**:
```sql
ZoneId INT PK,
SiteId INT FK NOT NULL,
ZoneCode NVARCHAR(50) NOT NULL,
ZoneName NVARCHAR(200) NOT NULL,
ZoneType NVARCHAR(50) -- 'Receiving', 'Storage', 'Picking', 'Quarantine'
```

**Unique Constraint**:
- `UK_Zones_Code` (SiteId, ZoneCode) - Zone codes unique within site

### 3.5 Bins

**Purpose**: Specific storage locations within a zone.

**Relationships**:
- Many-to-One: `Zone`
- One-to-Many: `Movements`

**Key Columns**:
```sql
BinId INT PK,
ZoneId INT FK NOT NULL,
BinCode NVARCHAR(50) NOT NULL,
BinName NVARCHAR(200) NOT NULL,
Aisle NVARCHAR(10),
Rack NVARCHAR(10),
Shelf NVARCHAR(10)
```

**Unique Constraint**:
- `UK_Bins_Code` (ZoneId, BinCode) - Bin codes unique within zone

### 3.6 Users

**Purpose**: System users (cashiers, managers, production staff).

**Relationships**:
- One-to-Many: `Transactions`, `ProductionBatches`, `Movements` (via CreatedBy)

**Key Columns**:
```sql
UserId INT PK,
UserName NVARCHAR(100) UNIQUE NOT NULL,
Email NVARCHAR(200) UNIQUE,
FullName NVARCHAR(200),
EmployeeCode NVARCHAR(50) UNIQUE,
Role NVARCHAR(50), -- 'Admin', 'Manager', 'Cashier', 'ProductionWorker'
IsActive BIT
```

### 3.7 Suppliers

**Purpose**: Vendors for purchasing module.

**Relationships**:
- One-to-Many: `PurchaseOrders`, `GoodsReceivedNotes`

**Key Columns**:
```sql
SupplierId INT PK,
SupplierCode NVARCHAR(50) UNIQUE NOT NULL,
SupplierName NVARCHAR(200) NOT NULL,
ContactPerson NVARCHAR(200),
Email NVARCHAR(200),
PhoneNumber NVARCHAR(20),
VATNumber NVARCHAR(50),
PaymentTerms NVARCHAR(100) -- 'Net 30', 'COD', etc.
```

### 3.8 Customers

**Purpose**: Retail customers (optional registration).

**Relationships**:
- One-to-Many: `RetailTransactionHeaders`

**Key Columns**:
```sql
CustomerId INT PK,
CustomerNumber NVARCHAR(100) UNIQUE NOT NULL,
FirstName NVARCHAR(100) NOT NULL,
LastName NVARCHAR(100) NOT NULL,
Email NVARCHAR(200) UNIQUE,
PhoneNumber NVARCHAR(20),
IDNumber NVARCHAR(50) UNIQUE, -- SA ID for compliance
IDVerified BIT,
LoyaltyPoints INT DEFAULT 0
```

---

## 4. INVENTORY MODULE TABLES

### 4.1 Movements

**Purpose**: THE CORE TABLE. Records every stock change.

**Relationships**:
- Many-to-One: `Product`, `Site`, `Zone`, `Bin`
- Many-to-One (Optional): `TransactionDetail` (via TransactionType + TransactionId + TransactionDetailId)
- Self-referencing: `ReversedByMovementId` → `MovementId`

**Key Columns**:
```sql
MovementId INT PK,
ProductId INT FK NOT NULL,
SiteId INT FK NOT NULL,
ZoneId INT FK NULL,
BinId INT FK NULL,
MovementType NVARCHAR(50) NOT NULL, -- 'GRV', 'Sale', 'ProductionInput', etc.
MovementDirection NVARCHAR(10) NOT NULL, -- 'IN' or 'OUT'
Quantity DECIMAL(18,4) NOT NULL,
BatchNumber NVARCHAR(100),
SerialNumber NVARCHAR(50),
TransactionType NVARCHAR(50), -- 'Retail', 'Purchase', 'Production'
TransactionId INT, -- HeaderId from respective module
TransactionDetailId INT, -- DetailId from TransactionDetails
ReferenceNumber NVARCHAR(100),
IsReversed BIT DEFAULT 0,
ReversedByMovementId INT FK NULL,
MovementDate DATETIME NOT NULL
```

**Indexes** (CRITICAL for performance):
- `IX_Movements_SOH_Calculation` (ProductId, SiteId, BatchNumber) INCLUDE (Quantity, MovementDirection, IsDeleted, IsReversed)
- `IX_Movements_Batch` (BatchNumber) WHERE BatchNumber IS NOT NULL
- `IX_Movements_Serial` (SerialNumber) WHERE SerialNumber IS NOT NULL
- `IX_Movements_Transaction` (TransactionType, TransactionId)
- `IX_Movements_Date` (MovementDate)

### 4.2 StockAdjustments

**Purpose**: Stock adjustments with approval workflow.

**Relationships**:
- Many-to-One: `Product`, `Site`
- One-to-One (Optional): `Movement` (created after approval)

**Key Columns**:
```sql
StockAdjustmentId INT PK,
AdjustmentNumber NVARCHAR(100) UNIQUE NOT NULL,
ProductId INT FK NOT NULL,
SiteId INT FK NOT NULL,
AdjustmentType NVARCHAR(50) NOT NULL, -- 'AdjustmentIn', 'AdjustmentOut', 'Wastage'
Quantity DECIMAL(18,4) NOT NULL,
BatchNumber NVARCHAR(100),
Reason NVARCHAR(500) NOT NULL,
Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected'
ApprovedBy NVARCHAR(100),
ApprovedDate DATETIME,
MovementId INT FK NULL -- Created after approval
```

### 4.3 StockTransfers

**Purpose**: Inter-location stock transfers.

**Relationships**:
- Many-to-One: `Product`, `SourceSite`, `DestinationSite`
- One-to-Two: `MovementOut`, `MovementIn` (2 movements per transfer)

**Key Columns**:
```sql
StockTransferId INT PK,
TransferNumber NVARCHAR(100) UNIQUE NOT NULL,
ProductId INT FK NOT NULL,
Quantity DECIMAL(18,4) NOT NULL,
SourceSiteId INT FK NOT NULL,
DestinationSiteId INT FK NOT NULL,
BatchNumber NVARCHAR(100),
Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'InTransit', 'Completed'
MovementOutId INT FK NULL, -- Movement at source (OUT)
MovementInId INT FK NULL   -- Movement at destination (IN)
```

### 4.4 CycleCounts

**Purpose**: Stock take headers.

**Relationships**:
- Many-to-One: `Site`
- One-to-Many: `CycleCountLines`

**Key Columns**:
```sql
CycleCountId INT PK,
CountNumber NVARCHAR(100) UNIQUE NOT NULL,
SiteId INT FK NOT NULL,
CountType NVARCHAR(50) NOT NULL, -- 'Full', 'Partial', 'Batch'
ScheduledDate DATE NOT NULL,
Status NVARCHAR(50) DEFAULT 'Scheduled', -- 'Scheduled', 'InProgress', 'Completed'
AssignedTo NVARCHAR(100),
VariancesFound INT
```

### 4.5 CycleCountLines

**Purpose**: Stock take line items.

**Relationships**:
- Many-to-One: `CycleCount`, `Product`
- One-to-One (Optional): `StockAdjustment` (created if variance accepted)

**Key Columns**:
```sql
CycleCountLineId INT PK,
CycleCountId INT FK NOT NULL,
ProductId INT FK NOT NULL,
BatchNumber NVARCHAR(100),
SystemSOH DECIMAL(18,4) NOT NULL,
PhysicalCount DECIMAL(18,4),
Variance DECIMAL(18,4), -- PhysicalCount - SystemSOH
AdjustmentId INT FK NULL -- Created if variance accepted
```

### 4.6 SerialNumbers

**Purpose**: Individual tracking for serial-tracked products.

**Relationships**:
- Many-to-One: `Product`, `ProductionBatch`
- Many-to-One (Optional): `RetailTransactionHeader` (when sold)

**Key Columns**:
```sql
SerialNumberId INT PK,
SerialNumber NVARCHAR(50) UNIQUE NOT NULL, -- e.g., "0124121100015"
ProductId INT FK NOT NULL,
BatchNumber NVARCHAR(100) NOT NULL,
ProductionBatchId INT FK NULL,
Status NVARCHAR(50) DEFAULT 'Produced', -- 'Produced', 'InStock', 'Sold', 'Returned'
SoldDate DATETIME NULL,
SoldTransactionId INT FK NULL
```

**Indexes**:
- `IX_Serial_Number` (SerialNumber) - Unique
- `IX_Serial_Batch` (BatchNumber)
- `IX_Serial_Product` (ProductId)
- `IX_Serial_Status` (Status)

---

## 5. RETAIL POS MODULE TABLES

### 5.1 RetailTransactionHeaders

**Purpose**: Retail sale/return headers.

**Relationships**:
- Many-to-One: `Site`, `Customer`, `Cashier` (User)
- One-to-Many: `TransactionDetails`, `Payments`
- Self-referencing (Optional): `OriginalTransactionId` (for returns)

**Key Columns**:
```sql
TransactionId INT PK,
TransactionNumber NVARCHAR(100) UNIQUE NOT NULL,
TransactionDate DATETIME NOT NULL,
TransactionType NVARCHAR(50) DEFAULT 'Sale', -- 'Sale', 'Return'
OriginalTransactionId INT FK NULL, -- For returns
SiteId INT FK NOT NULL,
CashierId INT FK NOT NULL,
CustomerId INT FK NULL,
SubTotal DECIMAL(18,2) NOT NULL,
DiscountAmount DECIMAL(18,2) DEFAULT 0,
VATAmount DECIMAL(18,2) NOT NULL,
TotalAmount DECIMAL(18,2) NOT NULL,
PaymentStatus NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Paid', 'Refunded'
CustomerIDVerified BIT DEFAULT 0,
CustomerIDNumber NVARCHAR(50)
```

**Indexes**:
- `IX_RetailTrans_Number` (TransactionNumber) - Unique
- `IX_RetailTrans_Date` (TransactionDate)
- `IX_RetailTrans_Customer` (CustomerId)
- `IX_RetailTrans_Cashier` (CashierId)

### 5.2 Payments

**Purpose**: Payment records (supports multi-payment).

**Relationships**:
- Many-to-One: `RetailTransactionHeader`

**Key Columns**:
```sql
PaymentId INT PK,
TransactionId INT FK NOT NULL,
PaymentMethod NVARCHAR(50) NOT NULL, -- 'Cash', 'Card', 'DigitalWallet'
Amount DECIMAL(18,2) NOT NULL,
PaymentDate DATETIME NOT NULL,
CashTendered DECIMAL(18,2) NULL,
CashChange DECIMAL(18,2) NULL,
CardType NVARCHAR(50) NULL,
CardLast4Digits NVARCHAR(4) NULL,
AuthorizationCode NVARCHAR(100) NULL
```

### 5.3 Receipts

**Purpose**: Receipt metadata (content stored as text/PDF).

**Relationships**:
- One-to-One: `RetailTransactionHeader`

**Key Columns**:
```sql
ReceiptId INT PK,
TransactionId INT FK NOT NULL,
ReceiptNumber NVARCHAR(100) UNIQUE NOT NULL,
ReceiptContent NVARCHAR(MAX), -- Full receipt text
IsPrinted BIT DEFAULT 0,
PrintedDate DATETIME NULL
```

---

## 6. PURCHASING MODULE TABLES

### 6.1 PurchaseOrders

**Purpose**: Purchase order headers.

**Relationships**:
- Many-to-One: `Supplier`, `Site`
- One-to-Many: `PurchaseOrderLines`, `GoodsReceivedNotes`

**Key Columns**:
```sql
PurchaseOrderId INT PK,
PONumber NVARCHAR(100) UNIQUE NOT NULL,
SupplierId INT FK NOT NULL,
SiteId INT FK NOT NULL,
PODate DATE NOT NULL,
ExpectedDeliveryDate DATE,
Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Approved', 'PartiallyReceived', 'Received', 'Cancelled'
SubTotal DECIMAL(18,2),
VATAmount DECIMAL(18,2),
TotalAmount DECIMAL(18,2)
```

### 6.2 PurchaseOrderLines

**Purpose**: Purchase order line items.

**Relationships**:
- Many-to-One: `PurchaseOrder`, `Product`

**Key Columns**:
```sql
POLineId INT PK,
PurchaseOrderId INT FK NOT NULL,
ProductId INT FK NOT NULL,
Quantity DECIMAL(18,4) NOT NULL,
UnitPrice DECIMAL(18,2) NOT NULL,
LineTotal DECIMAL(18,2) NOT NULL
```

### 6.3 GoodsReceivedNotes

**Purpose**: GRV headers (receipt of goods).

**Relationships**:
- Many-to-One: `PurchaseOrder`, `Supplier`, `Site`
- One-to-Many: `GRVLines`, `Movements`

**Key Columns**:
```sql
GRVId INT PK,
GRVNumber NVARCHAR(100) UNIQUE NOT NULL,
PurchaseOrderId INT FK NULL, -- Can receive without PO
SupplierId INT FK NOT NULL,
SiteId INT FK NOT NULL,
GRVDate DATE NOT NULL,
Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected'
```

### 6.4 GRVLines

**Purpose**: GRV line items.

**Relationships**:
- Many-to-One: `GRV`, `Product`, `POLine`

**Key Columns**:
```sql
GRVLineId INT PK,
GRVId INT FK NOT NULL,
POLineId INT FK NULL,
ProductId INT FK NOT NULL,
QuantityOrdered DECIMAL(18,4),
QuantityReceived DECIMAL(18,4) NOT NULL,
BatchNumber NVARCHAR(100) -- Supplier batch
```

### 6.5 SupplierReturns

**Purpose**: Return to supplier headers.

**Relationships**:
- Many-to-One: `Supplier`, `GRV` (optional), `Site`

**Key Columns**:
```sql
SupplierReturnId INT PK,
ReturnNumber NVARCHAR(100) UNIQUE NOT NULL,
SupplierId INT FK NOT NULL,
GRVId INT FK NULL, -- Original GRV being returned
SiteId INT FK NOT NULL,
ReturnDate DATE NOT NULL,
Reason NVARCHAR(500) NOT NULL
```

---

## 7. PRODUCTION MODULE TABLES

### 7.1 ProductionBatches

**Purpose**: Production batch headers (one batch, multiple steps).

**Relationships**:
- Many-to-One: `Site`, `OutputProduct` (Product)
- One-to-Many: `ProductionSteps`, `ProductionInputs`, `ProductionOutputs`, `SerialNumbers`

**Key Columns**:
```sql
ProductionBatchId INT PK,
BatchNumber NVARCHAR(100) UNIQUE NOT NULL,
ProductionType NVARCHAR(50) NOT NULL, -- 'Bucking', 'GeneralManufacturing', 'RetailProduction'
ProductionDate DATE NOT NULL,
SiteId INT FK NOT NULL,
OutputProductId INT FK NOT NULL,
PlannedQuantity DECIMAL(18,4) NOT NULL,
ActualQuantity DECIMAL(18,4) NULL,
TotalSteps INT DEFAULT 1,
CurrentStep INT DEFAULT 0,
Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'InProgress', 'Completed', 'Cancelled'
StartedDate DATETIME,
CompletedDate DATETIME
```

**Indexes**:
- `IX_ProdBatch_Number` (BatchNumber) - Unique
- `IX_ProdBatch_Status` (Status)
- `IX_ProdBatch_Date` (ProductionDate)

### 7.2 ProductionSteps

**Purpose**: Individual steps in multi-step production.

**Relationships**:
- Many-to-One: `ProductionBatch`

**Key Columns**:
```sql
ProductionStepId INT PK,
ProductionBatchId INT FK NOT NULL,
StepNumber INT NOT NULL,
StepName NVARCHAR(200) NOT NULL,
InputQuantity DECIMAL(18,4),
OutputQuantity DECIMAL(18,4),
WastageQuantity DECIMAL(18,4),
Status NVARCHAR(50) DEFAULT 'NotStarted', -- 'NotStarted', 'InProgress', 'Completed', 'Failed'
StartedDate DATETIME,
CompletedDate DATETIME,
Operator NVARCHAR(100)
```

**Indexes**:
- `IX_ProdStep_Batch` (ProductionBatchId)
- `IX_ProdStep_Number` (ProductionBatchId, StepNumber)

### 7.3 ProductionInputs

**Purpose**: Raw materials consumed in production.

**Relationships**:
- Many-to-One: `ProductionBatch`, `Product`
- Many-to-One (Optional): `ProductionStep`

**Key Columns**:
```sql
ProductionInputId INT PK,
ProductionBatchId INT FK NOT NULL,
ProductionStepId INT FK NULL,
ProductId INT FK NOT NULL,
Quantity DECIMAL(18,4) NOT NULL,
BatchNumber NVARCHAR(100)
```

### 7.4 ProductionOutputs

**Purpose**: Finished goods produced.

**Relationships**:
- Many-to-One: `ProductionBatch`, `Product`
- Many-to-One (Optional): `ProductionStep`

**Key Columns**:
```sql
ProductionOutputId INT PK,
ProductionBatchId INT FK NOT NULL,
ProductionStepId INT FK NULL,
ProductId INT FK NOT NULL,
Quantity DECIMAL(18,4) NOT NULL,
BatchNumber NVARCHAR(100) NOT NULL, -- Same as ProductionBatch.BatchNumber
SerialNumber NVARCHAR(50) NULL
```

### 7.5 QualityChecks

**Purpose**: QC checkpoint records.

**Relationships**:
- Many-to-One: `ProductionStep`

**Key Columns**:
```sql
QualityCheckId INT PK,
ProductionStepId INT FK NOT NULL,
InspectedQuantity DECIMAL(18,4) NOT NULL,
ApprovedQuantity DECIMAL(18,4) NOT NULL,
RejectedQuantity DECIMAL(18,4) NOT NULL,
RejectionReason NVARCHAR(500),
Inspector NVARCHAR(100),
CheckDate DATETIME NOT NULL
```

---

## 8. CULTIVATION MODULE TABLES (STUB)

### 8.1 CultivationBatches (Stub)

**Purpose**: Cultivation batch tracking (stub for future).

**Key Columns**:
```sql
CultivationBatchId INT PK,
BatchNumber NVARCHAR(100) UNIQUE NOT NULL,
StrainName NVARCHAR(100),
PlantCount INT,
SiteId INT FK NOT NULL,
HarvestDate DATE,
Status NVARCHAR(50)
```

### 8.2 Plants (Stub)

**Purpose**: Individual plant tracking (stub).

**Key Columns**:
```sql
PlantId INT PK,
PlantTag NVARCHAR(100) UNIQUE NOT NULL,
CultivationBatchId INT FK,
StrainName NVARCHAR(100),
PlantDate DATE,
Status NVARCHAR(50) -- 'Vegetative', 'Flowering', 'Harvested', 'Destroyed'
```

### 8.3 HarvestRecords (Stub)

**Purpose**: Harvest event tracking (stub).

**Key Columns**:
```sql
HarvestId INT PK,
PlantId INT FK,
HarvestDate DATE,
WetWeight DECIMAL(18,4),
DryWeight DECIMAL(18,4)
```

---

## 9. MOVEMENT ARCHITECTURE TABLES

### 9.1 TransactionDetails (UNIFIED - Option A)

**Purpose**: Line items for ALL transaction types (Retail, Purchase, Production, etc.).

**Relationships**:
- Many-to-One: `Product`
- Many-to-One (Dynamic): Header table (via TransactionType + HeaderId)

**Key Columns**:
```sql
TransactionDetailId INT PK,
HeaderId INT NOT NULL, -- TransactionId from RetailTransactionHeaders, PurchaseOrderId, etc.
TransactionType NVARCHAR(50) NOT NULL, -- 'Retail', 'Purchase', 'Production', 'StockAdjustment'
ProductId INT FK NOT NULL,
Quantity DECIMAL(18,4) NOT NULL,
UnitPrice DECIMAL(18,2) NOT NULL,
DiscountAmount DECIMAL(18,2) DEFAULT 0,
VATAmount DECIMAL(18,2) NOT NULL,
LineTotal DECIMAL(18,2) NOT NULL,
BatchNumber NVARCHAR(100),
SerialNumber NVARCHAR(50)
```

**Indexes**:
- `IX_TransDetail_Header` (HeaderId, TransactionType)
- `IX_TransDetail_Product` (ProductId)
- `IX_TransDetail_Batch` (BatchNumber) WHERE BatchNumber IS NOT NULL
- `IX_TransDetail_Serial` (SerialNumber) WHERE SerialNumber IS NOT NULL

**Key Design Note**: This is the **CORE TABLE** of Option A architecture. All modules write to this unified table, enabling consistent reporting and movement generation.

---

## 10. AUDIT & COMPLIANCE TABLES

### 10.1 AuditLogs

**Purpose**: Comprehensive audit trail for all data changes.

**Key Columns**:
```sql
AuditLogId BIGINT PK,
TableName NVARCHAR(100) NOT NULL,
RecordId INT NOT NULL,
OperationType NVARCHAR(20) NOT NULL, -- 'INSERT', 'UPDATE', 'DELETE'
OldValues NVARCHAR(MAX), -- JSON
NewValues NVARCHAR(MAX), -- JSON
ChangedBy NVARCHAR(100) NOT NULL,
ChangedDate DATETIME NOT NULL DEFAULT GETDATE()
```

**Indexes**:
- `IX_AuditLog_Table` (TableName)
- `IX_AuditLog_Record` (TableName, RecordId)
- `IX_AuditLog_Date` (ChangedDate)

### 10.2 ComplianceReports

**Purpose**: Regulatory compliance report tracking.

**Key Columns**:
```sql
ComplianceReportId INT PK,
ReportType NVARCHAR(100) NOT NULL, -- 'SAHPRA_Monthly', 'DALRRD_Quarterly', etc.
ReportPeriodStart DATE NOT NULL,
ReportPeriodEnd DATE NOT NULL,
ReportContent NVARCHAR(MAX), -- JSON or XML
GeneratedDate DATETIME NOT NULL,
GeneratedBy NVARCHAR(100) NOT NULL,
SubmittedDate DATETIME NULL
```

### 10.3 BatchTraceability

**Purpose**: Fast batch traceability queries (denormalized for performance).

**Key Columns**:
```sql
TraceabilityId BIGINT PK,
BatchNumber NVARCHAR(100) NOT NULL,
ProductId INT FK NOT NULL,
EventType NVARCHAR(50) NOT NULL, -- 'Cultivated', 'Produced', 'Sold', 'Destroyed'
EventDate DATETIME NOT NULL,
EventReference NVARCHAR(200), -- Transaction number, production batch, etc.
LocationId INT, -- SiteId
Notes NVARCHAR(500)
```

**Indexes**:
- `IX_Trace_Batch` (BatchNumber)
- `IX_Trace_Product` (ProductId)
- `IX_Trace_Date` (EventDate)

---

## 11. ENTITY RELATIONSHIP DIAGRAM

### 11.1 Core Entity Relationships

```
┌──────────────┐
│ProductCategories│◄──────┐
│ (Hierarchy)  │        │
└──────────────┘        │ Many-to-One
        ▲               │
        │ Self-Reference│
        └───────────────┘
        ▲
        │
        │ Many-to-One
        │
┌──────────────┐      ┌──────────────┐
│   Products   │◄─────┤ Movements    │
│   (Master)   │      │  (CORE!)     │
└──────────────┘      └──────────────┘
        ▲                     ▲
        │                     │
        │ Many-to-One         │ Many-to-One
        │                     │
┌──────────────┐      ┌──────────────┐
│TransDetails  │      │    Sites     │
│ (Unified)    │      │  (Locations) │
└──────────────┘      └──────────────┘
        ▲                     │
        │                     │ One-to-Many
        │                     ▼
        │             ┌──────────────┐
        │             │    Zones     │
        │             └──────────────┘
        │                     │
        │                     │ One-to-Many
        │                     ▼
        │             ┌──────────────┐
        │             │     Bins     │
        │             └──────────────┘
        │
        │ Many-to-One (Dynamic)
        │
        ├───────► RetailTransactionHeaders
        │
        ├───────► PurchaseOrders
        │
        ├───────► ProductionBatches
        │
        └───────► StockAdjustments
```

### 11.2 Movement Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    OPTION A ARCHITECTURE                     │
└─────────────────────────────────────────────────────────────┘

┌──────────────────┐
│RetailTransHeaders│
│ (Specialized)    │
└────────┬─────────┘
         │
         │ HeaderId
         ▼
┌──────────────────┐      ┌──────────────┐
│TransactionDetails│─────►│   Movements  │
│    (Unified)     │      │   (Unified)  │
└────────┬─────────┘      └──────────────┘
         ▲
         │ HeaderId
         │
┌──────────────────┐
│ PurchaseHeaders  │
│  (Specialized)   │
└──────────────────┘
         ▲
         │ HeaderId
         │
┌──────────────────┐
│ProductionBatches │
│  (Specialized)   │
└──────────────────┘

KEY PRINCIPLE:
- Specialized headers per module (RetailTransHeaders, PurchaseHeaders, etc.)
- UNIFIED TransactionDetails table (all line items)
- UNIFIED Movements table (all stock changes)
- Consistent movement generation across ALL modules
```

### 11.3 Batch & Serial Number Traceability

```
┌──────────────────┐
│CultivationBatch  │
│ Batch: CULT-001  │
└────────┬─────────┘
         │ Input to Production
         ▼
┌──────────────────┐
│ProductionBatch   │
│ Batch: PROD-002  │
└────────┬─────────┘
         │ Generates
         ▼
┌──────────────────┐
│  SerialNumbers   │
│ SN-001, SN-002...│
└────────┬─────────┘
         │ Sold at POS
         ▼
┌──────────────────┐
│RetailTransaction │
│ SALE-2024-12345  │
└──────────────────┘

TRACEABILITY QUERY:
Serial Number → Production Batch → Cultivation Batch → Plant
```

---

## 12. INDEXING STRATEGY

### 12.1 Index Priorities

**Priority 1: SOH Calculation (Movements)**
```sql
-- MOST CRITICAL INDEX (used constantly for SOH queries)
CREATE INDEX IX_Movements_SOH_Calculation
ON Movements(ProductId, SiteId, BatchNumber)
INCLUDE (Quantity, MovementDirection, IsDeleted, IsReversed);
```

**Priority 2: Transaction Lookups**
```sql
CREATE INDEX IX_RetailTrans_Number ON RetailTransactionHeaders(TransactionNumber);
CREATE INDEX IX_RetailTrans_Date ON RetailTransactionHeaders(TransactionDate);
CREATE INDEX IX_PO_Number ON PurchaseOrders(PONumber);
```

**Priority 3: Batch/Serial Traceability**
```sql
CREATE INDEX IX_Movements_Batch ON Movements(BatchNumber) WHERE BatchNumber IS NOT NULL;
CREATE INDEX IX_Serial_Number ON SerialNumbers(SerialNumber);
CREATE INDEX IX_TransDetail_Batch ON TransactionDetails(BatchNumber) WHERE BatchNumber IS NOT NULL;
```

**Priority 4: Foreign Key Indexes**
```sql
CREATE INDEX IX_Products_Category ON Products(ProductCategoryId);
CREATE INDEX IX_Movements_Product ON Movements(ProductId);
CREATE INDEX IX_Movements_Site ON Movements(SiteId);
CREATE INDEX IX_TransDetail_Product ON TransactionDetails(ProductId);
```

### 12.2 Index Maintenance

**Weekly**:
- Analyze index fragmentation
- Rebuild indexes if fragmentation > 30%

**Monthly**:
- Update statistics on all tables
- Review unused indexes (consider dropping)

**Tools** (PostgreSQL):
```sql
-- Check index usage
SELECT * FROM pg_stat_user_indexes WHERE schemaname = 'public';

-- Reindex table
REINDEX TABLE Movements;

-- Analyze table
ANALYZE Movements;
```

---

## 13. DATA INTEGRITY & CONSTRAINTS

### 13.1 Referential Integrity

**All foreign keys enforced**:
```sql
-- Example
ALTER TABLE Movements
ADD CONSTRAINT FK_Movements_Product
FOREIGN KEY (ProductId) REFERENCES Products(ProductId);

-- ON DELETE: Default is RESTRICT (prevent delete if referenced)
-- For soft delete pattern, physical deletes are rare anyway
```

### 13.2 Unique Constraints

**Business Keys**:
```sql
-- Products
ALTER TABLE Products ADD CONSTRAINT UK_Products_ProductCode UNIQUE (ProductCode);

-- Transactions
ALTER TABLE RetailTransactionHeaders ADD CONSTRAINT UK_RetailTrans_Number UNIQUE (TransactionNumber);

-- Batches
ALTER TABLE ProductionBatches ADD CONSTRAINT UK_ProdBatch_Number UNIQUE (BatchNumber);

-- Serial Numbers
ALTER TABLE SerialNumbers ADD CONSTRAINT UK_Serial_Number UNIQUE (SerialNumber);
```

### 13.3 Check Constraints

**Data Validation**:
```sql
-- Quantities must be positive
ALTER TABLE Movements ADD CONSTRAINT CK_Movements_Quantity CHECK (Quantity > 0);

-- VAT percentage range
ALTER TABLE Products ADD CONSTRAINT CK_Products_THC CHECK (THCPercentage >= 0 AND THCPercentage <= 100);

-- Movement direction
ALTER TABLE Movements ADD CONSTRAINT CK_Movements_Direction CHECK (MovementDirection IN ('IN', 'OUT'));
```

### 13.4 Default Values

**Common Defaults**:
```sql
-- Audit columns
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
IsDeleted BIT NOT NULL DEFAULT 0,

-- Status fields
Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',

-- Financial fields
DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
```

---

## 14. MIGRATION STRATEGY

### 14.1 Migration Phases

**Phase 1: Core Tables** (Week 1)
- Products, ProductCategories
- Sites, Zones, Bins
- Users, Suppliers, Customers

**Phase 2: Movements** (Week 2)
- Movements (THE CORE TABLE)
- TransactionDetails (unified)

**Phase 3: Retail POS** (Week 3)
- RetailTransactionHeaders
- Payments
- Receipts

**Phase 4: Purchasing** (Week 4)
- PurchaseOrders, PurchaseOrderLines
- GoodsReceivedNotes, GRVLines

**Phase 5: Production** (Week 5)
- ProductionBatches, ProductionSteps
- ProductionInputs, ProductionOutputs
- SerialNumbers

**Phase 6: Audit/Compliance** (Week 6)
- AuditLogs
- ComplianceReports
- BatchTraceability

### 14.2 Migration Tools

**Entity Framework Core Migrations**:
```bash
# Create migration
dotnet ef migrations add AddMovementsTable

# Apply migration
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigration

# Generate SQL script
dotnet ef migrations script --output migration.sql
```

### 14.3 Data Seeding

**Seed Data** (for testing/demo):
- Sample product categories (Cannabis types)
- Sample products (pre-rolls, flower, cones)
- Sample sites (JHB Warehouse, CPT Retail Store)
- Test users (Admin, Cashier, Production Worker)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Seed product categories
    modelBuilder.Entity<ProductCategory>().HasData(
        new ProductCategory { ProductCategoryId = 1, CategoryCode = "CANNABIS", CategoryName = "Cannabis Products", IsCannabisCategory = true },
        new ProductCategory { ProductCategoryId = 2, CategoryCode = "PREROLL", CategoryName = "Pre-Rolls", ParentCategoryId = 1, IsCannabisCategory = true }
    );

    // Seed sites
    modelBuilder.Entity<Site>().HasData(
        new Site { SiteId = 1, SiteCode = "JHB-WH-01", SiteName = "Johannesburg Warehouse", SiteType = "Warehouse" }
    );
}
```

---

## APPENDIX A: TABLE LIST (ALPHABETICAL)

| Table Name | Module | Priority | Completion % |
|------------|--------|----------|--------------|
| AuditLogs | Audit | Medium | 50% |
| Bins | Core | High | 100% |
| ComplianceReports | Compliance | Medium | 50% |
| CultivationBatches | Cultivation | Low | 35% (stub) |
| Customers | Retail POS | High | 90% |
| CycleCountLines | Inventory | Medium | 60% |
| CycleCounts | Inventory | Medium | 60% |
| GoodsReceivedNotes | Purchasing | Medium | 75% |
| GRVLines | Purchasing | Medium | 75% |
| HarvestRecords | Cultivation | Low | 35% (stub) |
| Movements | Inventory | **CRITICAL** | 90% |
| Payments | Retail POS | High | 85% |
| Plants | Cultivation | Low | 35% (stub) |
| ProductCategories | Core | High | 100% |
| ProductionBatches | Production | Medium | 70% |
| ProductionInputs | Production | Medium | 70% |
| ProductionOutputs | Production | Medium | 70% |
| ProductionSteps | Production | Medium | 70% |
| Products | Core | High | 100% |
| PurchaseOrderLines | Purchasing | Medium | 75% |
| PurchaseOrders | Purchasing | Medium | 75% |
| QualityChecks | Production | Medium | 50% |
| Receipts | Retail POS | High | 80% |
| RetailTransactionHeaders | Retail POS | High | 85% |
| SerialNumbers | Inventory | High | 80% |
| Sites | Core | High | 100% |
| StockAdjustments | Inventory | Medium | 70% |
| StockTransfers | Inventory | Medium | 60% |
| SupplierReturns | Purchasing | Medium | 60% |
| Suppliers | Core | High | 100% |
| TransactionDetails | Movement Arch | **CRITICAL** | 90% |
| Users | Core | High | 100% |
| Zones | Core | High | 100% |

**Total Tables**: 33 (excluding stubs: ~30 active tables)

---

## APPENDIX B: SQL SCRIPT TEMPLATES

### Create Table Template
```sql
CREATE TABLE TableName (
    TableNameId INT IDENTITY(1,1) PRIMARY KEY,

    -- Business columns
    Column1 NVARCHAR(100) NOT NULL,
    Column2 DECIMAL(18,2) NULL,

    -- Foreign keys
    RelatedTableId INT NULL,

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,

    -- Audit
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NULL,
    ModifiedBy NVARCHAR(100) NULL,
    DeletedDate DATETIME NULL,
    DeletedBy NVARCHAR(100) NULL,

    -- Constraints
    CONSTRAINT FK_TableName_RelatedTable FOREIGN KEY (RelatedTableId)
        REFERENCES RelatedTable(RelatedTableId)
);

-- Indexes
CREATE INDEX IX_TableName_Column1 ON TableName(Column1);
CREATE INDEX IX_TableName_RelatedTable ON TableName(RelatedTableId);
CREATE INDEX IX_TableName_Active ON TableName(IsActive, IsDeleted);
```

---

**END OF DATABASE SCHEMA SPECIFICATION**
