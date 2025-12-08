# Advanced Batch & Serial Number System - Design Document

**Status**: Design Complete - Implementation Deferred to Phase 4+
**Last Updated**: 2025-12-06
**Priority**: High (Critical for Production & Traceability modules)

---

## 🎯 Executive Summary

This document outlines the **production-ready batch and serial number system** for Project420, designed to meet:
- **SAHPRA seed-to-sale traceability** requirements
- **Barcode/QR code compatibility** (all numeric)
- **Multi-site operations** (franchise support)
- **Human-readable** formats (visual inspection)
- **Compliance ready** (Cannabis Act, POPIA, SARS)

---

## 📊 Number Formats Overview

### **1. BATCH NUMBERS** (16 digits)

**Format:** `SSTTYYYYMMDDNNNN`

| Component | Digits | Example | Description |
|-----------|--------|---------|-------------|
| Site ID | 2 | `01` | Site #1 (supports 01-99 sites) |
| Batch Type | 2 | `10` | Production (see type codes below) |
| Date | 8 | `20251206` | December 6, 2025 |
| Sequence | 4 | `0001` | First batch of this type/site/date |

**Example:** `0110202512060001`
- Site 01, Production (10), Dec 6 2025, batch #1

**Batch Type Codes:**
- `10` = Production (manufacturing/growing)
- `20` = Transfer (inter-location)
- `30` = Stock Take (inventory counts)
- `40` = Adjustment (corrections)
- `50` = Return to Supplier
- `60` = Destruction/Waste
- `70` = Customer Return
- `80` = Quarantine
- `90` = Reserved

**Key Features:**
- ✅ Site isolation (each site independent sequences)
- ✅ Type visible (instant identification)
- ✅ Date embedded (FIFO/FEFO management)
- ✅ Daily reset per type/site (keeps numbers manageable)
- ✅ 16 digits (barcode-friendly)

---

### **2. FULL SERIAL NUMBER** (28 digits - QR Code)

**Format:** `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC`

| Component | Digits | Example | Description |
|-----------|--------|---------|-------------|
| Site ID | 2 | `01` | Site #1 |
| Strain Code | 3 | `100` | Sativa strain #1 (1xx=Sativa, 2xx=Indica, 3xx=Hybrid) |
| Batch Type | 2 | `10` | Production |
| Date | 8 | `20251206` | December 6, 2025 |
| Batch Seq | 4 | `0001` | First batch of this type/site/date |
| Unit Seq | 5 | `00001` | First unit in this batch |
| Weight | 4 | `0035` | 3.5 grams (in tenths: 0035 = 3.5g) |
| Qty/Pack | 1 | `1` | Single unit (1=single, 2=pack of 2, etc.) |
| Check Digit | 1 | `7` | Luhn algorithm validation |

**Example:** `0110010202512060001000010035517`

**Breakdown:**
- `01` = Site 1
- `100` = Sativa strain #1
- `10` = Production
- `20251206` = Dec 6, 2025
- `0001` = Batch #1
- `00001` = Unit #1
- `0035` = 3.5 grams
- `1` = Single unit
- `7` = Check digit

**Strain Code Encoding:**
- **100-199**: Sativa strains (visual ID by first digit = 1)
- **200-299**: Indica strains (first digit = 2)
- **300-399**: Hybrid strains (first digit = 3)
- **400-499**: CBD-only strains (reserved)
- **500-999**: Future use

**Weight Encoding (4 digits in tenths of grams):**
- `0001` = 0.1g
- `0035` = 3.5g
- `0070` = 7g (quarter oz)
- `0140` = 14g (half oz)
- `0280` = 28g (oz)
- `9999` = 999.9g (max ~1kg)

**Qty/Pack Codes:**
- `1` = Single unit
- `2` = Pack of 2
- `5` = Pack of 5
- `0` = Bulk (no unit count, weight only)

**Use Cases:**
- ✅ QR code on packaging (customers can scan)
- ✅ Internal tracking (all info embedded)
- ✅ Database master record (links to Short SN)
- ✅ Compliance documentation (full audit trail)

---

### **3. SHORT SERIAL NUMBER** (13 digits - Barcode)

**Format:** `SSYYMMDDNNNNN`

| Component | Digits | Example | Description |
|-----------|--------|---------|-------------|
| Site ID | 2 | `01` | Site #1 |
| Date | 6 | `251206` | Dec 6, 2025 (YY-MM-DD) |
| Sequence | 5 | `00001` | Sequential for this site/date |

**Example:** `0125120600001`

**Database Mapping:**
```
Short SN:  0125120600001
Full SN:   0110010202512060001000010035517
Linkage:   One-to-one mapping table (SerialNumberMappings)
```

**Use Cases:**
- ✅ Barcode labels (compact, scannable)
- ✅ Manual entry (shorter, easier to type)
- ✅ Visual ID (staff can eyeball date)
- ✅ Links to Full SN (database lookup for details)

---

## 🔍 Check Digit Algorithm

**Luhn Algorithm** (credit card standard)

**Purpose:**
- Catches 99% of single-digit errors
- Catches 98% of transposition errors
- Industry standard (widely understood)

**Calculation:**
1. Start from rightmost digit (excluding check digit)
2. Double every second digit
3. If doubled digit > 9, subtract 9
4. Sum all digits
5. Check digit = (10 - (sum % 10)) % 10

**Example for:** `011001020251206000100001003551`
- Sum: 63
- Check digit: (10 - (63 % 10)) % 10 = 7
- Final: `0110010202512060001000010035517`

---

## 🏭 Workflow Examples

### **Example 1: Production Run**

**Scenario:** Site 01 produces 100 units of Sativa strain #1, 3.5g pre-rolls on Dec 6, 2025

**Step 1: Create Batch**
```
Batch Number: 0110202512060001
- Site: 01
- Type: 10 (Production)
- Date: 20251206
- Seq: 0001 (first production batch today)
```

**Step 2: Generate Serial Numbers**

**Unit #1:**
```
Full SN:  0110010202512060001000010035517
Short SN: 0125120600001

Components:
- Site: 01
- Strain: 100 (Sativa #1)
- Type: 10 (Production)
- Date: 20251206
- Batch: 0001
- Unit: 00001
- Weight: 0035 (3.5g)
- Qty: 1 (single)
- Check: 7
```

**Unit #2:**
```
Full SN:  0110010202512060001000020035518
Short SN: 0125120600002

(Same batch, unit 00002, check digit changes)
```

**Step 3: Label Printing**
- **QR Code:** Full SN (all product info)
- **Barcode:** Short SN (for quick scanning)

---

### **Example 2: Stock Transfer**

**Scenario:** Transfer 50 units from Site 01 to Site 02 on Dec 6, 2025

**Step 1: Create Transfer Batch**
```
Batch Number: 0120202512060001
- Site: 01 (origin site)
- Type: 20 (Transfer)
- Date: 20251206
- Seq: 0001
```

**Step 2: Link Existing Serials to Transfer Batch**
```
Original units (Production batch):
- Full SN: 011001020251206000100001...
- Full SN: 011001020251206000100002...
- etc.

Transfer tracking:
- TransferBatchNumber: 0120202512060001
- FromSite: 01
- ToSite: 02
- UnitCount: 50
- LinkedSerials: [list of 50 Full SNs]
```

**Key Point:** Units keep their original serial numbers, transfer batch just tracks movement.

---

### **Example 3: Stock Take**

**Scenario:** Site 01 performs physical inventory count on Dec 31, 2025

**Step 1: Create Stock Take Batch**
```
Batch Number: 0130202512310001
- Site: 01
- Type: 30 (Stock Take)
- Date: 20251231
- Seq: 0001
```

**Step 2: Scan Units During Count**
```
Scanned Short SNs:
- 0125120600001 → Lookup → Full SN → Extract info
- 0125120600002 → ...
- etc.

System records:
- StockTakeBatchNumber: 0130202512310001
- CountedUnits: [list of SNs]
- ExpectedVsActual: Comparison
```

---

## 📦 Database Schema

### **Table: Batches**
```sql
CREATE TABLE Batches (
    BatchNumber CHAR(16) PRIMARY KEY,          -- SSTTYYYYMMDDNNNN
    SiteId TINYINT NOT NULL,                   -- 01-99
    BatchType TINYINT NOT NULL,                -- 10, 20, 30, etc.
    BatchDate DATE NOT NULL,                   -- YYYY-MM-DD
    DailySequence SMALLINT NOT NULL,           -- 0001-9999
    Description NVARCHAR(500),
    Status NVARCHAR(50),                       -- Active, Completed, Cancelled
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    INDEX IX_Batches_SiteType (SiteId, BatchType, BatchDate),
    INDEX IX_Batches_Date (BatchDate)
);
```

### **Table: SerialNumbers**
```sql
CREATE TABLE SerialNumbers (
    FullSerialNumber CHAR(28) PRIMARY KEY,     -- Full SN with check digit
    ShortSerialNumber CHAR(13) UNIQUE NOT NULL, -- Short SN for barcode
    BatchNumber CHAR(16) NOT NULL,             -- Links to Batches
    SiteId TINYINT NOT NULL,
    StrainCode SMALLINT NOT NULL,              -- 100-999
    ProductionDate DATE NOT NULL,
    UnitSequence INT NOT NULL,                 -- 00001-99999
    WeightGrams DECIMAL(6,1) NOT NULL,         -- From weight code
    PackSize TINYINT NOT NULL,                 -- 1, 2, 5, etc.
    ProductId INT,                             -- Links to Products table
    CurrentStatus NVARCHAR(50),                -- InStock, Sold, Destroyed, etc.
    CurrentLocation INT,                       -- SiteId where it currently is
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    FOREIGN KEY (BatchNumber) REFERENCES Batches(BatchNumber),
    INDEX IX_SerialNumbers_Short (ShortSerialNumber),
    INDEX IX_SerialNumbers_Batch (BatchNumber),
    INDEX IX_SerialNumbers_Strain (StrainCode),
    INDEX IX_SerialNumbers_Status (CurrentStatus)
);
```

### **Table: SerialNumberMovements** (Audit Trail)
```sql
CREATE TABLE SerialNumberMovements (
    MovementId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FullSerialNumber CHAR(28) NOT NULL,
    MovementType NVARCHAR(50) NOT NULL,        -- Production, Transfer, Sale, etc.
    FromLocation INT,                          -- Previous site
    ToLocation INT,                            -- New site
    AssociatedBatch CHAR(16),                  -- Transfer/Sale batch
    MovementDate DATETIME2 NOT NULL,
    MovedBy NVARCHAR(100) NOT NULL,
    Notes NVARCHAR(1000),
    FOREIGN KEY (FullSerialNumber) REFERENCES SerialNumbers(FullSerialNumber),
    INDEX IX_Movements_Serial (FullSerialNumber),
    INDEX IX_Movements_Date (MovementDate)
);
```

---

## 🔄 Implementation Phases

### **Phase 1: Database Setup** (Week 1)
- [ ] Create Batches table
- [ ] Create SerialNumbers table
- [ ] Create SerialNumberMovements table
- [ ] Add Site master table (if doesn't exist)
- [ ] Add Strain master table (with type codes)
- [ ] Create indexes for performance

### **Phase 2: Core Services** (Week 2)
- [ ] BatchNumberGenerator service
- [ ] SerialNumberGenerator service (Full + Short)
- [ ] LuhnCheckDigit utility
- [ ] SerialNumberParser (decode Full SN)
- [ ] SerialNumberMapper (Short ↔ Full lookup)

### **Phase 3: Business Logic** (Week 3)
- [ ] Production batch creation workflow
- [ ] Serial number assignment during production
- [ ] Transfer batch creation and tracking
- [ ] Stock take batch creation and scanning
- [ ] Adjustment batch handling

### **Phase 4: UI Integration** (Week 4)
- [ ] Batch management screens
- [ ] Serial number scanning interface
- [ ] Label printing (QR + Barcode templates)
- [ ] Serial lookup / history view
- [ ] Batch reports and analytics

### **Phase 5: Testing & QA** (Week 5)
- [ ] Unit tests (99%+ coverage)
- [ ] Integration tests (batch-to-serial workflows)
- [ ] Performance tests (100k+ serials)
- [ ] Barcode/QR scanning tests
- [ ] Check digit validation tests

---

## ✅ Benefits Summary

| Benefit | Description |
|---------|-------------|
| **Full Traceability** | Every unit traceable from seed to sale |
| **Compliance Ready** | Meets SAHPRA, POPIA, SARS requirements |
| **Multi-Site Support** | Franchise-ready with site isolation |
| **Human Readable** | Staff can visually identify key info |
| **Barcode Compatible** | Short SN works with standard barcodes |
| **QR Code Rich** | Full SN embeds all product details |
| **Recall Ready** | Instant identification of affected units |
| **Fraud Prevention** | Check digit catches 99% of errors |
| **Future Proof** | Supports 99 sites, 900 strains, unlimited units |
| **Database Efficient** | Numeric keys for fast indexing |

---

## 🚨 Critical Notes

1. **Non-Tracked Products:**
   - Get batch numbers but NO serial numbers
   - Examples: water, packaging, consumables
   - Track only by batch + quantity/weight

2. **Check Digit is Mandatory:**
   - Validates data entry and scanning
   - Prevents 99% of typos
   - Industry standard (Luhn)

3. **Short ↔ Full Mapping:**
   - Always generate both
   - Store mapping in database
   - Short SN is for convenience only

4. **Sequence Resets:**
   - Batch sequences: Daily per site/type
   - Serial sequences: Per batch (not global)
   - Prevents number exhaustion

5. **Site ID Assignment:**
   - Plan site numbering scheme upfront
   - Document site ID allocations
   - Reserve ranges for acquisitions

---

## 📚 References

- **SAHPRA Guidelines**: Seed-to-sale traceability requirements
- **Cannabis Act 2024**: Section on batch tracking
- **GS1 Standards**: Barcode/GTIN best practices
- **ISO/IEC 15420**: EAN/UPC barcode symbology
- **Luhn Algorithm**: Check digit calculation (ISO/IEC 7812-1)

---

**Document Owner**: Development Team
**Review Cycle**: Quarterly or before Phase 4 implementation
**Next Review**: When Production module development begins
