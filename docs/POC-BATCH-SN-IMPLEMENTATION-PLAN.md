# PoC Batch & Serial Number Implementation Plan
## Analysis of Reference Documents & Project420 Adaptation

**Created**: 2025-12-08
**Status**: ACTIVE - Planning Phase
**Purpose**: Define batch/SN system adapted to Project420's SA cannabis compliance requirements

---

## 📚 Reference Document Analysis

### 1. MVP Modules for Seed-Sale Traceability (`mvp_modules_seed_traceability.md`)

**Key Requirements Identified:**
- **Batch Management**: Production, Stock Take, Transfer, and Sales batches
- **Serial Number Management**: Internal SNs (14-char) + External SNs (customer-facing)
- **Inventory Module**: Multi-location stock tracking with per-batch inventory
- **Traceability Ledger**: Full audit trail (seed-to-sale lineage)
- **Settings & Configuration**: Batch type configuration, SN generation rules

**Alignment with Project420:**
✅ We have: Cultivation, Production, Inventory modules with audit trails
❌ We need: Batch entity, SerialNumber entity, batch/SN generation services
✅ We have: Multi-location support via GrowRooms, StockMovements
❌ We need: Batch lifecycle tracking (Open → Active → Closed)

---

### 2. System Evolution Roadmap (`system_evolution_roadmap.md`)

**🌱 PoC Phase Requirements (Current):**
- ✅ 12-14 character batch format
- ✅ Basic CRUD for batches
- ❌ Batch lifecycle: Open → Closed
- ✅ 14-character SN generator
- ❌ SN ↔ Batch linking
- ❌ Collision-free generation rules
- ✅ Basic inventory tracking (we have StockMovements)
- ✅ Batch history, serial history (via audit trails)
- ✅ Minimal UI (Blazor) - we have POSCheckout.razor
- ✅ Single database (Project420 on JASON\SQLDEVED)

**🧪 Prototype Phase (Next - Deferred):**
- Authentication & Authorization
- Enhanced Batch Management (metadata, attachments)
- Advanced Serial Number Layer (bulk generation, QR codes, external SNs)
- Multi-facility support

**🚀 Production Phase (Future - Deferred):**
- Compliance Engine (immutable audit ledger)
- High-Volume Serial Engine
- Public QR Validation Portal
- Multi-Tenant Mode

**Project420 Status:**
- **Current**: PoC phase infrastructure complete (entities, DbContexts, validators)
- **Next**: Implement PoC batch/SN system (basic functionality only)
- **Defer**: Prototype and Production features until PoC validated

---

### 3. Batch & Serial Number Best Practices (`sn_batch_best_practices.md`)

**Recommended Batch Format:**
```
TT-YYMMDD-RRRR (12-14 characters)
```
- **TT**: Batch Type Code (PR=Production, ST=Stock Take, TR=Transfer, SL=Sales)
- **YYMMDD**: Creation Date
- **RRRR**: 4-digit random/sequential counter

**Recommended Internal SN Format (14-char):**
```
BBXXXXYYYYZZZZ
```
- **BB**: Encoded batch reference (2 chars)
- **XXXX**: Time/sequence component (base-32)
- **YYYY**: Random entropy block
- **ZZZZ**: Incrementing per-batch counter

**External SN Format:**
```
{BatchNumber}-{ItemSeq}-{Checksum}
Example: PR-250308-A7K3-004291-X9
```

**Key Principles:**
1. **Uniqueness**: Every batch number and SN must be globally unique
2. **Traceability**: SNs must reference their originating batch
3. **Scalability**: Support millions of serials
4. **Non-Ambiguity**: Avoid confusing characters (0/O, 1/I/l)
5. **System Safety**: Prevent collisions, allow validation (checksum)

---

## 🎯 Project420 Adaptation Strategy

### Differences from Reference Documents

**1. South African Compliance Focus:**
- Reference docs are generic seed-to-sale traceability
- Project420 adds: SAHPRA, DALRRD, POPIA, Cannabis Act 2024 compliance
- We need: License tracking, GMP compliance, SARS audit trails

**2. Existing Module Architecture:**
- Reference docs assume greenfield implementation
- Project420 has: Cultivation, Production, Inventory modules already built
- We need: Batch/SN system that integrates with existing entities

**3. Database Schema:**
- Reference docs suggest separate Batch and Serial tables
- Project420 has: HarvestBatch, ProductionBatch entities already
- We need: Unified batch system OR extend existing batch entities

---

## 🏗️ Proposed Implementation for PoC

### Option A: Unified Batch System (Recommended)

Create a **universal Batch entity** that links to module-specific batches:

```csharp
// Shared/Project420.Shared.Core/Entities/Batch.cs
public class Batch : AuditableEntity
{
    public int Id { get; set; }
    public string BatchNumber { get; set; }  // TT-YYMMDD-RRRR
    public BatchType BatchType { get; set; }  // Production, StockTake, Transfer, Sales
    public BatchStatus Status { get; set; }  // Open, Active, Closed
    public DateTime CreatedDate { get; set; }
    public string FacilityCode { get; set; }  // GrowRoom, ProductionFacility, etc.

    // Links to module-specific batches
    public int? HarvestBatchId { get; set; }  // FK to Cultivation.HarvestBatch
    public int? ProductionBatchId { get; set; }  // FK to Production.ProductionBatch
    public string? TransferNumber { get; set; }  // FK to Inventory.StockTransfer
    public string? SalesNumber { get; set; }  // FK to POS.TransactionHeader

    // Metadata
    public string? StrainName { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public ICollection<SerialNumber> SerialNumbers { get; set; }
}
```

**Advantages:**
- ✅ Centralized batch management
- ✅ Consistent batch numbering across all modules
- ✅ Easy cross-module traceability
- ✅ Simpler reporting and auditing

**Disadvantages:**
- ❌ Adds complexity (extra table, FKs)
- ❌ Requires refactoring existing HarvestBatch, ProductionBatch

---

### Option B: Extend Existing Batch Entities (Simpler for PoC)

Add batch numbering to **existing** entities:

```csharp
// Cultivation/Models/HarvestBatch.cs (MODIFY)
public class HarvestBatch : AuditableEntity
{
    // Existing properties...
    public string BatchNumber { get; set; }  // NEW: Add universal batch number

    public string BatchNumber { get; set; }  // PR-YYMMDD-RRRR
    public BatchStatus Status { get; set; }  // Open, Active, Closed

    // Navigation
    public ICollection<SerialNumber> SerialNumbers { get; set; }  // NEW
}
```

Do the same for:
- `Production.ProductionBatch`
- `Inventory.StockTransfer`
- `POS.TransactionHeader`

**Advantages:**
- ✅ Minimal changes to existing architecture
- ✅ Faster implementation (PoC focus)
- ✅ No need for complex FK relationships

**Disadvantages:**
- ❌ Duplicate logic across modules
- ❌ Harder to enforce consistent numbering
- ❌ More complex cross-module queries

---

### Recommended Approach: **Option B for PoC** → **Option A for Prototype**

**Reasoning:**
- PoC goal: Prove batch/SN concept works
- Keep it simple: Extend existing entities, add batch numbering service
- Prototype phase: Refactor to unified Batch system for scalability

---

## 📦 SerialNumber Entity Design

```csharp
// Shared/Project420.Shared.Core/Entities/SerialNumber.cs
public class SerialNumber : AuditableEntity
{
    public int Id { get; set; }

    // Internal SN (14-char, system use only)
    public string InternalSerialNumber { get; set; }  // BBXXXXYYYYZZZZ

    // External SN (customer-facing, optional)
    public string? ExternalSerialNumber { get; set; }  // {Batch}-{Seq}-{Check}

    // Link to batch
    public string BatchNumber { get; set; }  // FK to whichever batch entity
    public BatchType BatchType { get; set; }  // Which module's batch?

    // SN Status
    public SerialStatus Status { get; set; }  // Created, Assigned, Sold, Destroyed

    // Traceability
    public string? ProductSKU { get; set; }
    public string? StrainName { get; set; }
    public DateTime GeneratedDate { get; set; }

    // QR Code (optional for PoC)
    public string? QRCodeData { get; set; }
}
```

**SerialStatus Enum:**
```csharp
public enum SerialStatus
{
    Created = 1,      // Generated but not assigned
    Assigned = 2,     // Assigned to product/package
    InStock = 3,      // In inventory
    Sold = 4,         // Sold to customer
    Transferred = 5,  // Transferred to another facility
    Destroyed = 6     // Waste/destroyed (SAHPRA requirement)
}
```

---

## 🔧 Service Implementation

### 1. BatchNumberGeneratorService

```csharp
public interface IBatchNumberGeneratorService
{
    Task<string> GenerateBatchNumberAsync(BatchType batchType, string facilityCode = null);
    bool ValidateBatchNumber(string batchNumber);
}

public class BatchNumberGeneratorService : IBatchNumberGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<string> GenerateBatchNumberAsync(BatchType batchType, string facilityCode = null)
    {
        // Format: TT-YYMMDD-RRRR
        var typeCode = GetBatchTypeCode(batchType);  // PR, ST, TR, SL
        var dateComponent = DateTime.UtcNow.ToString("yyMMdd");
        var randomComponent = GenerateRandomAlphanumeric(4);  // A7K3

        var batchNumber = $"{typeCode}-{dateComponent}-{randomComponent}";

        // Ensure uniqueness (check database)
        while (await BatchNumberExistsAsync(batchNumber))
        {
            randomComponent = GenerateRandomAlphanumeric(4);
            batchNumber = $"{typeCode}-{dateComponent}-{randomComponent}";
        }

        return batchNumber;
    }

    private string GetBatchTypeCode(BatchType type) => type switch
    {
        BatchType.Production => "PR",
        BatchType.StockTake => "ST",
        BatchType.Transfer => "TR",
        BatchType.Sales => "SL",
        _ => throw new ArgumentException("Invalid batch type")
    };

    private string GenerateRandomAlphanumeric(int length)
    {
        // Use A-Z, 2-9 (avoid 0/O, 1/I confusion)
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
```

---

### 2. SerialNumberGeneratorService

```csharp
public interface ISerialNumberGeneratorService
{
    Task<SerialNumber> GenerateInternalSerialAsync(string batchNumber);
    Task<List<SerialNumber>> GenerateBulkSerialsAsync(string batchNumber, int quantity);
    string GenerateExternalSerial(string batchNumber, int sequenceNumber);
}

public class SerialNumberGeneratorService : ISerialNumberGeneratorService
{
    public async Task<SerialNumber> GenerateInternalSerialAsync(string batchNumber)
    {
        // Format: BBXXXXYYYYZZZZ (14 chars)
        var batchHash = GetBatchHash(batchNumber);  // 2 chars
        var timeComponent = GetTimeComponent();  // 4 chars (base-32)
        var randomBlock = GenerateRandom(4);  // 4 chars
        var counter = await GetNextCounterAsync(batchNumber);  // 4 chars

        var internalSN = $"{batchHash}{timeComponent}{randomBlock}{counter:D4}";

        return new SerialNumber
        {
            InternalSerialNumber = internalSN,
            BatchNumber = batchNumber,
            Status = SerialStatus.Created,
            GeneratedDate = DateTime.UtcNow
        };
    }

    public string GenerateExternalSerial(string batchNumber, int sequenceNumber)
    {
        // Format: {BatchNumber}-{Seq}-{Checksum}
        // Example: PR-251208-A7K3-004291-X9
        var checksum = CalculateChecksum(batchNumber, sequenceNumber);
        return $"{batchNumber}-{sequenceNumber:D6}-{checksum}";
    }

    private string GetBatchHash(string batchNumber)
    {
        // Simple hash: First letter of type + last digit of date
        // PR-251208-A7K3 → PA
        var parts = batchNumber.Split('-');
        return $"{parts[0][0]}{parts[1][^1]}";
    }
}
```

---

## 📋 PoC Implementation Checklist

### Phase 6.1: Batch System Foundation
- [ ] Create BatchType enum (Production, StockTake, Transfer, Sales)
- [ ] Create BatchStatus enum (Open, Active, Closed)
- [ ] Create SerialStatus enum (Created, Assigned, Sold, Destroyed, etc.)
- [ ] Create SerialNumber entity (Shared.Core)
- [ ] Add BatchNumber property to existing batch entities (HarvestBatch, ProductionBatch, etc.)
- [ ] Create migrations for SerialNumber table and batch entity updates

### Phase 6.2: Batch Number Generation
- [ ] Implement IBatchNumberGeneratorService
- [ ] Implement BatchNumberGeneratorService with TT-YYMMDD-RRRR format
- [ ] Add uniqueness validation (check database for collisions)
- [ ] Register service in DI container
- [ ] Write unit tests (generate 100 batch numbers, ensure no duplicates)

### Phase 6.3: Serial Number Generation
- [ ] Implement ISerialNumberGeneratorService
- [ ] Implement internal SN generation (14-char format)
- [ ] Implement external SN generation (with checksum)
- [ ] Implement bulk SN generation (for efficiency)
- [ ] Add SN validation logic (format, checksum)
- [ ] Register service in DI container
- [ ] Write unit tests (generate 1000 SNs, ensure uniqueness)

### Phase 6.4: Integration with Existing Modules
- [ ] Update HarvestBatch creation to auto-generate batch number
- [ ] Update ProductionBatch creation to auto-generate batch number
- [ ] Update StockTransfer to auto-generate transfer batch number
- [ ] Update TransactionHeader (POS) to auto-generate sales batch number
- [ ] Link SerialNumbers to batches in workflows

### Phase 6.5: Traceability Workflow Testing
- [ ] Test Plant → HarvestBatch → Batch Number
- [ ] Test HarvestBatch → ProductionBatch → SN generation
- [ ] Test ProductionBatch → Inventory → SN tracking
- [ ] Test Inventory → Sale → SN consumption
- [ ] Generate traceability report (SN → Batch → Plant → GrowCycle)

---

## 🚦 PoC Completion Criteria

**Before Moving to Prototype:**

1. **Batch Numbering Works:**
   - ✅ Can generate unique batch numbers for all 4 types
   - ✅ Batch numbers follow TT-YYMMDD-RRRR format
   - ✅ No collisions in 10,000+ batch generations

2. **Serial Numbering Works:**
   - ✅ Can generate unique internal SNs (14-char)
   - ✅ Can generate customer-facing external SNs
   - ✅ SNs correctly link back to batch numbers

3. **Traceability Works:**
   - ✅ Can trace SN → Batch → Product → Plant → GrowCycle
   - ✅ Audit trail complete (who, when, what)
   - ✅ SAHPRA compliance: plant tracking, waste tracking

4. **Integration Works:**
   - ✅ Batch numbers auto-generated when creating HarvestBatch
   - ✅ SNs auto-generated during packaging/production
   - ✅ Stock movements track batch/SN changes

5. **Performance Acceptable:**
   - ✅ Generate 1 batch number: <100ms
   - ✅ Generate 1000 SNs: <1 second
   - ✅ Traceability query (full lineage): <500ms

---

## 🔄 Comparison: Reference Docs vs. Project420

| Feature | Reference Docs | Project420 Adaptation |
|---------|---------------|----------------------|
| **Batch Format** | TT-YYMMDD-RRRR (12-14 char) | ✅ Same format |
| **Batch Types** | Production, StockTake, Transfer, Sales | ✅ Same (aligned with modules) |
| **Internal SN** | 14-char (BBXXXXYYYYZZZZ) | ✅ Same format |
| **External SN** | {Batch}-{Seq}-{Check} | ✅ Same format |
| **Database Schema** | Separate Batch + Serial tables | ⚠️ Option B for PoC (extend existing) |
| **Compliance** | Generic traceability | ✅ Added SAHPRA, POPIA, Cannabis Act |
| **QR Codes** | Recommended | ⏸️ Defer to Prototype phase |
| **Multi-Tenant** | Production feature | ⏸️ Defer to Production phase |
| **Bulk SN Generation** | High-volume requirement | ⏸️ Basic version in PoC, optimize in Prototype |

---

## 🎯 Next Immediate Actions

1. **Review this document** with stakeholders
2. **Decide**: Option A (unified) vs Option B (extend existing) for PoC
3. **Create Phase 6.1 tasks** (entities, enums, migrations)
4. **Implement BatchNumberGeneratorService**
5. **Test batch number generation**
6. **Proceed to SerialNumber implementation**

---

## 📊 Where We Are vs. Where We're Going

### Current State (Phase 6 Complete):
- ✅ All MVP entities created (Cultivation, Production, Inventory)
- ✅ Database migrations applied (12 tables on JASON\SQLDEVED)
- ✅ FluentValidation integrated (32 validators)
- ✅ Audit trails in place (AuditableEntity base class)
- ❌ No batch numbering system yet
- ❌ No serial number tracking yet

### PoC Target (Phase 7):
- ✅ Batch numbers auto-generated (TT-YYMMDD-RRRR)
- ✅ Serial numbers tracked (14-char internal + external)
- ✅ Full seed-to-sale traceability working
- ✅ SAHPRA compliance validated
- ✅ Performance benchmarks met

### Prototype Target (Phase 8+):
- Unified Batch system (refactor from Option B → Option A)
- QR code generation and public validation
- Bulk SN operations (10,000+ SNs per batch)
- Advanced reporting and analytics
- Multi-facility workflows

---

**STATUS**: ✅ **ANALYSIS COMPLETE - READY FOR IMPLEMENTATION**
**RECOMMENDED PATH**: Option B (Extend Existing) for PoC → Option A (Unified) for Prototype
**ESTIMATED EFFORT**: 3-5 days for PoC batch/SN system

*This document should be reviewed and approved before proceeding with implementation.*
