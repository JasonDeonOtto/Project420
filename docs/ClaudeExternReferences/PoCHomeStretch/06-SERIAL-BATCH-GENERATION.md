# Project420 - Serial Number & Batch Generation System
## Enterprise-Grade Traceability Through Embedded Metadata

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Part of**: PoC Home Stretch Specification Suite
**Priority**: 🔴 CRITICAL - Required for SAHPRA compliance

---

## 📋 TABLE OF CONTENTS

1. [Overview](#1-overview)
2. [Batch Number System](#2-batch-number-system)
3. [Serial Number System](#3-serial-number-system)
4. [Check Digit Algorithms](#4-check-digit-algorithms)
5. [Implementation Patterns](#5-implementation-patterns)
6. [Traceability Workflows](#6-traceability-workflows)
7. [Database Schema](#7-database-schema)
8. [Usage Examples](#8-usage-examples)

---

## 1. OVERVIEW

### 1.1 Purpose

Project420 requires enterprise-grade batch and serial number generation that embeds traceability metadata directly into the identifier. This enables:

- **Full traceability**: From serial number alone, identify site, strain, batch, date, weight
- **Compliance**: SAHPRA seed-to-sale tracking requirements met
- **Recall capability**: Quickly identify all products from specific batch
- **Anti-counterfeiting**: Check digits validate authenticity
- **Operational efficiency**: Barcode scanning with embedded data

### 1.2 Design Principles

1. **Embedded Metadata**: Numbers contain traceability information (not just random sequences)
2. **Human-Readable**: Can be manually entered if scanner fails
3. **Unique**: No duplicates, ever (system-wide uniqueness)
4. **Scannable**: Compatible with standard barcodes (EAN-13 for short SNs)
5. **Validated**: Check digits prevent transcription errors
6. **Auditable**: All generated numbers logged with timestamp, user, context

### 1.3 Number Types

| Type | Format | Length | Purpose |
|------|--------|--------|---------|
| **Batch Number** | `SSTTYYYYMMDDNNNN` | 16 digits | Production batch identification |
| **Full Serial Number** | `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC` | 28 digits | Full traceability (QR code) |
| **Short Serial Number** | `SSYYMMDDNNNNN` | 13 digits | Barcode scanning (EAN-13) |
| **Harvest Batch** | `HARVEST-YYYY-NNNN` | 15-17 chars | Cultivation batch (alphanumeric) |

---

## 2. BATCH NUMBER SYSTEM

### 2.1 Batch Number Format

**Format**: `SSTTYYYYMMDDNNNN` (16 digits)

**Component Breakdown**:
```
SS       = Site ID (01-99)
TT       = Batch Type (01-09)
YYYYMMDD = Date of batch creation
NNNN     = Sequence number (0001-9999, resets daily)
```

**Example**: `0102202501150042`
- Site: 01 (Main Production Facility)
- Type: 02 (Production Batch)
- Date: 2025-01-15
- Sequence: 0042 (42nd batch created on this date)

### 2.2 Batch Type Codes

| Code | Type | Description |
|------|------|-------------|
| **01** | Cultivation | Harvest batch from grow cycle |
| **02** | Production | General production batch |
| **03** | Retail Production | Pre-rolls, packaging |
| **04** | Extraction | Extraction/distillation |
| **05** | Formulation | Edibles, vapes, oils |
| **06** | Bucking | Post-harvest processing |
| **07** | Transfer | Inter-location transfer batch |
| **08** | Import | Imported goods batch |
| **09** | Quality Control | QC testing batch |

### 2.3 Batch Number Generation Algorithm

```csharp
public class BatchNumberGeneratorService : IBatchNumberGeneratorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BatchNumberGeneratorService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GenerateBatchNumberAsync(int siteId, BatchType batchType)
    {
        await _lock.WaitAsync();
        try
        {
            // Validate site ID
            if (siteId < 1 || siteId > 99)
                throw new ArgumentException("Site ID must be between 01 and 99", nameof(siteId));

            // Get today's date
            var today = DateTime.Today;
            var dateString = today.ToString("yyyyMMdd");

            // Get next sequence number for today
            var sequenceNumber = await GetNextSequenceNumberAsync(siteId, batchType, today);

            // Format batch number
            var batchNumber = $"{siteId:D2}{(int)batchType:D2}{dateString}{sequenceNumber:D4}";

            // Log generation
            _logger.LogInformation("Generated batch number: {BatchNumber} for Site {SiteId}, Type {BatchType}",
                batchNumber, siteId, batchType);

            return batchNumber;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<int> GetNextSequenceNumberAsync(int siteId, BatchType batchType, DateTime date)
    {
        // Get highest sequence number used today
        var datePrefix = $"{siteId:D2}{(int)batchType:D2}{date:yyyyMMdd}";

        var lastBatch = await _context.Batches
            .Where(b => b.BatchNumber.StartsWith(datePrefix))
            .OrderByDescending(b => b.BatchNumber)
            .FirstOrDefaultAsync();

        if (lastBatch == null)
            return 1; // First batch of the day

        // Extract sequence number from last batch
        var lastSequence = int.Parse(lastBatch.BatchNumber.Substring(12, 4));
        return lastSequence + 1;
    }
}
```

### 2.4 Batch Entity

```csharp
public class Batch : AuditableEntity
{
    public int BatchId { get; set; }

    [Required]
    [StringLength(16)]
    [Index(IsUnique = true)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public int SiteId { get; set; }

    [Required]
    public BatchType BatchType { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Completed, Archived, Recalled

    // Source traceability
    [StringLength(16)]
    public string? SourceBatchNumber { get; set; } // Parent batch (for traceability)
    public virtual Batch? SourceBatch { get; set; }

    // Child batches (if this batch is split/transformed)
    public virtual ICollection<Batch>? ChildBatches { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<Movement>? Movements { get; set; }
    public virtual ICollection<ProcessingStep>? ProcessingSteps { get; set; }
    public virtual ICollection<SerialNumber>? SerialNumbers { get; set; }
}

public enum BatchType
{
    Cultivation = 1,
    Production = 2,
    RetailProduction = 3,
    Extraction = 4,
    Formulation = 5,
    Bucking = 6,
    Transfer = 7,
    Import = 8,
    QualityControl = 9
}
```

---

## 3. SERIAL NUMBER SYSTEM

### 3.1 Full Serial Number Format (28 digits)

**Format**: `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC` (28 digits)

**Component Breakdown**:
```
SSSSS    = Site + Strain ID (5 digits: SS=Site, SSS=Strain)
TT       = Product Type (01-09)
YYYYMMDD = Date of packaging
BBBBB    = Batch sequence (5 digits, from batch number)
UUUUU    = Unit sequence within batch (00001-99999)
WWWW     = Weight in grams (4 digits: 0100 = 1.00g, 2850 = 28.50g)
QC       = Check digit (2 digits, Luhn algorithm)
```

**Example**: `0142320250115000420000100035094`
- Site: 01 (Main Production Facility)
- Strain: 423 (Gelato strain)
- Product Type: 02 (Pre-roll)
- Date: 2025-01-15
- Batch: 00042 (from batch 0102202501150042)
- Unit: 00001 (first unit in batch)
- Weight: 0100 (1.00g)
- Check Digit: 94

### 3.2 Short Serial Number Format (13 digits)

**Format**: `SSYYMMDDNNNNN` (13 digits, EAN-13 compatible)

**Component Breakdown**:
```
SS       = Site ID (01-99)
YYMMDD   = Date (last 2 digits of year + month + day)
NNNNN    = Sequence number (00001-99999, resets daily)
```

**Example**: `0125011500042`
- Site: 01
- Date: 2025-01-15
- Sequence: 00042

**Usage**: Short SNs used for barcode scanning when full traceability not required (e.g., non-serialized flower packs). Full SN stored in database, short SN printed on barcode.

### 3.3 Product Type Codes

| Code | Type | Description |
|------|------|-------------|
| **01** | Flower | Packaged cannabis flower |
| **02** | Pre-Roll | Pre-rolled joints |
| **03** | Edible | Edibles (gummies, chocolates, etc.) |
| **04** | Vape | Vape cartridges |
| **05** | Oil | Cannabis oils, tinctures |
| **06** | Concentrate | Concentrates (wax, shatter, etc.) |
| **07** | Topical | Topicals, creams, lotions |
| **08** | Capsule | Capsules, pills |
| **09** | Other | Other product types |

### 3.4 Serial Number Generation Algorithm

```csharp
public class SerialNumberGeneratorService : ISerialNumberGeneratorService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SerialNumberGeneratorService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<string> GenerateFullSerialNumberAsync(
        int siteId,
        int strainId,
        ProductType productType,
        string batchNumber,
        int unitSequence,
        decimal weightInGrams)
    {
        await _lock.WaitAsync();
        try
        {
            // Validate inputs
            if (siteId < 1 || siteId > 99)
                throw new ArgumentException("Site ID must be between 01 and 99", nameof(siteId));

            if (strainId < 1 || strainId > 999)
                throw new ArgumentException("Strain ID must be between 001 and 999", nameof(strainId));

            // Extract date from batch number
            var batchDate = ExtractDateFromBatchNumber(batchNumber);
            var dateString = batchDate.ToString("yyyyMMdd");

            // Extract batch sequence from batch number (last 4 digits)
            var batchSequence = int.Parse(batchNumber.Substring(12, 4));

            // Format components
            var siteStrainPart = $"{siteId:D2}{strainId:D3}"; // SSSSS
            var productTypePart = $"{(int)productType:D2}";    // TT
            var datePart = dateString;                          // YYYYMMDD
            var batchPart = $"{batchSequence:D5}";             // BBBBB
            var unitPart = $"{unitSequence:D5}";               // UUUUU
            var weightPart = FormatWeight(weightInGrams);       // WWWW

            // Combine all parts (without check digit)
            var snWithoutCheckDigit = $"{siteStrainPart}{productTypePart}{datePart}{batchPart}{unitPart}{weightPart}";

            // Calculate check digit
            var checkDigit = CalculateLuhnCheckDigit(snWithoutCheckDigit);

            // Complete serial number
            var fullSerialNumber = $"{snWithoutCheckDigit}{checkDigit:D2}";

            // Log generation
            _logger.LogInformation("Generated full SN: {SerialNumber} for Batch {BatchNumber}, Unit {UnitSequence}",
                fullSerialNumber, batchNumber, unitSequence);

            return fullSerialNumber;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> GenerateShortSerialNumberAsync(int siteId)
    {
        await _lock.WaitAsync();
        try
        {
            // Get today's date
            var today = DateTime.Today;
            var dateString = today.ToString("yyMMdd");

            // Get next sequence number for today
            var sequenceNumber = await GetNextShortSNSequenceAsync(siteId, today);

            // Format short SN
            var shortSN = $"{siteId:D2}{dateString}{sequenceNumber:D5}";

            // Log generation
            _logger.LogInformation("Generated short SN: {SerialNumber} for Site {SiteId}",
                shortSN, siteId);

            return shortSN;
        }
        finally
        {
            _lock.Release();
        }
    }

    private DateTime ExtractDateFromBatchNumber(string batchNumber)
    {
        // Batch format: SSTTYYYYMMDDNNNN
        var year = int.Parse(batchNumber.Substring(4, 4));
        var month = int.Parse(batchNumber.Substring(8, 2));
        var day = int.Parse(batchNumber.Substring(10, 2));
        return new DateTime(year, month, day);
    }

    private string FormatWeight(decimal weightInGrams)
    {
        // Convert to 4-digit format: 1.00g = 0100, 28.50g = 2850
        var weightValue = (int)(weightInGrams * 100);
        return $"{weightValue:D4}";
    }

    private async Task<int> GetNextShortSNSequenceAsync(int siteId, DateTime date)
    {
        var datePrefix = $"{siteId:D2}{date:yyMMdd}";

        var lastSN = await _context.SerialNumbers
            .Where(sn => sn.ShortSerialNumber.StartsWith(datePrefix))
            .OrderByDescending(sn => sn.ShortSerialNumber)
            .FirstOrDefaultAsync();

        if (lastSN == null)
            return 1; // First SN of the day

        // Extract sequence number
        var lastSequence = int.Parse(lastSN.ShortSerialNumber.Substring(8, 5));
        return lastSequence + 1;
    }
}
```

### 3.5 SerialNumber Entity

```csharp
public class SerialNumber : AuditableEntity
{
    public int SerialNumberId { get; set; }

    [Required]
    [StringLength(28)]
    [Index(IsUnique = true)]
    public string FullSerialNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(13)]
    [Index]
    public string ShortSerialNumber { get; set; } = string.Empty;

    [Required]
    public int ProductId { get; set; }
    public virtual Product? Product { get; set; }

    [Required]
    [StringLength(16)]
    public string BatchNumber { get; set; } = string.Empty;
    public virtual Batch? Batch { get; set; }

    [Required]
    public int SiteId { get; set; }

    public int? StrainId { get; set; }
    public virtual Strain? Strain { get; set; }

    [Required]
    public ProductType ProductType { get; set; }

    [Required]
    public DateTime PackagedDate { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal WeightInGrams { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Available"; // Available, Sold, Returned, Destroyed

    // Sale information
    public DateTime? SoldDate { get; set; }
    public int? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public int? TransactionId { get; set; }
    public virtual Transaction? Transaction { get; set; }

    // Return information
    public DateTime? ReturnedDate { get; set; }
    public string? ReturnReason { get; set; }

    // Destruction information
    public DateTime? DestroyedDate { get; set; }
    public string? DestructionReason { get; set; }
}

public enum ProductType
{
    Flower = 1,
    PreRoll = 2,
    Edible = 3,
    Vape = 4,
    Oil = 5,
    Concentrate = 6,
    Topical = 7,
    Capsule = 8,
    Other = 9
}
```

---

## 4. CHECK DIGIT ALGORITHMS

### 4.1 Luhn Algorithm (Modulo 10)

**Purpose**: Detect single-digit errors and most adjacent transpositions.

**Algorithm**:
1. Starting from the rightmost digit (excluding check digit), double every second digit
2. If doubled digit > 9, subtract 9
3. Sum all digits
4. Check digit = (10 - (sum % 10)) % 10

**Example**: Calculate check digit for `01423202501150004200001000350`

```csharp
private int CalculateLuhnCheckDigit(string number)
{
    int sum = 0;
    bool alternate = false;

    // Process digits from right to left
    for (int i = number.Length - 1; i >= 0; i--)
    {
        int digit = int.Parse(number[i].ToString());

        if (alternate)
        {
            digit *= 2;
            if (digit > 9)
                digit -= 9;
        }

        sum += digit;
        alternate = !alternate;
    }

    // Check digit makes total divisible by 10
    int checkDigit = (10 - (sum % 10)) % 10;
    return checkDigit;
}

private bool ValidateLuhnCheckDigit(string numberWithCheckDigit)
{
    if (numberWithCheckDigit.Length < 2)
        return false;

    var number = numberWithCheckDigit.Substring(0, numberWithCheckDigit.Length - 1);
    var providedCheckDigit = int.Parse(numberWithCheckDigit.Substring(numberWithCheckDigit.Length - 1));
    var calculatedCheckDigit = CalculateLuhnCheckDigit(number);

    return providedCheckDigit == calculatedCheckDigit;
}
```

### 4.2 Check Digit Generation for Short SNs

For short SNs (13 digits), use Luhn algorithm with single-digit check digit appended:

```csharp
public string GenerateShortSNWithCheckDigit(int siteId, DateTime date, int sequence)
{
    var snWithoutCheck = $"{siteId:D2}{date:yyMMdd}{sequence:D5}";
    var checkDigit = CalculateLuhnCheckDigit(snWithoutCheck);
    return $"{snWithoutCheck}{checkDigit}"; // 13 digits total
}
```

### 4.3 Batch Number Check Digit (Optional)

For batch numbers, consider adding 2-digit check digit to make 18 digits total:

```csharp
public string GenerateBatchNumberWithCheckDigit(int siteId, BatchType batchType, DateTime date, int sequence)
{
    var batchWithoutCheck = $"{siteId:D2}{(int)batchType:D2}{date:yyyyMMdd}{sequence:D4}";
    var checkDigit = CalculateLuhnCheckDigit(batchWithoutCheck);
    return $"{batchWithoutCheck}{checkDigit:D2}"; // 18 digits total
}
```

---

## 5. IMPLEMENTATION PATTERNS

### 5.1 Service Registration

```csharp
// Program.cs
builder.Services.AddScoped<IBatchNumberGeneratorService, BatchNumberGeneratorService>();
builder.Services.AddScoped<ISerialNumberGeneratorService, SerialNumberGeneratorService>();
```

### 5.2 Usage in Production Service

```csharp
public class RetailProductionService : IRetailProductionService
{
    private readonly IBatchNumberGeneratorService _batchNumberService;
    private readonly ISerialNumberGeneratorService _serialNumberService;
    private readonly IProductionBatchRepository _batchRepository;
    private readonly ISerialNumberRepository _serialNumberRepository;

    public async Task<ProductionBatchDto> StartPreRollProductionAsync(StartPreRollProductionDto dto)
    {
        // Generate batch number
        var batchNumber = await _batchNumberService.GenerateBatchNumberAsync(
            siteId: dto.SiteId,
            batchType: BatchType.RetailProduction
        );

        // Create batch record
        var batch = new Batch
        {
            BatchNumber = batchNumber,
            SiteId = dto.SiteId,
            BatchType = BatchType.RetailProduction,
            CreatedDate = DateTime.UtcNow,
            Status = "Active",
            SourceBatchNumber = dto.SourceFlowerBatchNumber
        };

        await _batchRepository.CreateAsync(batch);

        return _mapper.Map<ProductionBatchDto>(batch);
    }

    public async Task<List<string>> PackageAndGenerateSerialNumbersAsync(
        int batchId,
        int quantity,
        decimal weightPerUnit,
        int strainId)
    {
        var batch = await _batchRepository.GetByIdAsync(batchId);
        if (batch == null)
            throw new NotFoundException($"Batch {batchId} not found");

        var serialNumbers = new List<string>();

        // Generate SN for each unit
        for (int i = 1; i <= quantity; i++)
        {
            var fullSN = await _serialNumberService.GenerateFullSerialNumberAsync(
                siteId: batch.SiteId,
                strainId: strainId,
                productType: ProductType.PreRoll,
                batchNumber: batch.BatchNumber,
                unitSequence: i,
                weightInGrams: weightPerUnit
            );

            var shortSN = await _serialNumberService.GenerateShortSerialNumberAsync(batch.SiteId);

            // Create SN record
            var snRecord = new SerialNumber
            {
                FullSerialNumber = fullSN,
                ShortSerialNumber = shortSN,
                ProductId = dto.ProductId,
                BatchNumber = batch.BatchNumber,
                SiteId = batch.SiteId,
                StrainId = strainId,
                ProductType = ProductType.PreRoll,
                PackagedDate = DateTime.UtcNow,
                WeightInGrams = weightPerUnit,
                Status = "Available"
            };

            await _serialNumberRepository.CreateAsync(snRecord);

            serialNumbers.Add(fullSN);
        }

        return serialNumbers;
    }
}
```

### 5.3 Usage in POS Service

```csharp
public class POSTransactionService : ITransactionService
{
    private readonly ISerialNumberRepository _serialNumberRepository;

    public async Task<CheckoutResultDto> CheckoutAsync(CheckoutRequestDto request)
    {
        // ... transaction logic ...

        // For each serialized item in cart
        foreach (var item in request.Items.Where(i => !string.IsNullOrEmpty(i.SerialNumber)))
        {
            // Lookup SN
            var sn = await _serialNumberRepository.GetBySerialNumberAsync(item.SerialNumber);
            if (sn == null)
                throw new NotFoundException($"Serial number {item.SerialNumber} not found");

            if (sn.Status != "Available")
                throw new InvalidOperationException($"Serial number {item.SerialNumber} is not available (Status: {sn.Status})");

            // Mark as sold
            await _serialNumberRepository.MarkAsSoldAsync(
                serialNumber: item.SerialNumber,
                customerId: request.CustomerId,
                transactionId: transaction.TransactionId,
                soldDate: DateTime.UtcNow
            );
        }

        // ... rest of checkout logic ...
    }
}
```

---

## 6. TRACEABILITY WORKFLOWS

### 6.1 Forward Traceability (Production → Sale)

**Scenario**: Track where a batch went (which customers bought it)

```csharp
public async Task<ForwardTraceabilityDto> TraceForwardAsync(string batchNumber)
{
    // Get batch
    var batch = await _batchRepository.GetByBatchNumberAsync(batchNumber);

    // Get all SNs from this batch
    var serialNumbers = await _serialNumberRepository.GetByBatchNumberAsync(batchNumber);

    // Get all sales containing these SNs
    var sales = new List<SaleDto>();
    foreach (var sn in serialNumbers.Where(s => s.Status == "Sold"))
    {
        var transaction = await _transactionRepository.GetByIdAsync(sn.TransactionId.Value);
        sales.Add(_mapper.Map<SaleDto>(transaction));
    }

    return new ForwardTraceabilityDto
    {
        BatchNumber = batchNumber,
        TotalUnitsProduced = serialNumbers.Count,
        UnitsSold = serialNumbers.Count(s => s.Status == "Sold"),
        UnitsAvailable = serialNumbers.Count(s => s.Status == "Available"),
        Sales = sales
    };
}
```

### 6.2 Backward Traceability (Sale → Source)

**Scenario**: Customer bought product, trace back to source cultivation batch

```csharp
public async Task<BackwardTraceabilityDto> TraceBackwardAsync(string serialNumber)
{
    // Get SN record
    var sn = await _serialNumberRepository.GetBySerialNumberAsync(serialNumber);
    if (sn == null)
        throw new NotFoundException($"Serial number {serialNumber} not found");

    // Get production batch
    var productionBatch = await _batchRepository.GetByBatchNumberAsync(sn.BatchNumber);

    // Trace back to source batches
    var sourceBatches = new List<Batch>();
    var currentBatch = productionBatch;

    while (currentBatch?.SourceBatchNumber != null)
    {
        var sourceBatch = await _batchRepository.GetByBatchNumberAsync(currentBatch.SourceBatchNumber);
        if (sourceBatch != null)
        {
            sourceBatches.Add(sourceBatch);
            currentBatch = sourceBatch;
        }
        else
        {
            break;
        }
    }

    // Get processing steps
    var steps = await _processingStepRepository.GetByBatchIdAsync(productionBatch.BatchId);

    return new BackwardTraceabilityDto
    {
        SerialNumber = serialNumber,
        ProductionBatch = _mapper.Map<BatchDto>(productionBatch),
        SourceBatches = _mapper.Map<List<BatchDto>>(sourceBatches),
        ProcessingSteps = _mapper.Map<List<ProcessingStepDto>>(steps),
        FullTraceabilityChain = BuildTraceabilityChain(serialNumber, productionBatch, sourceBatches, steps)
    };
}

private string BuildTraceabilityChain(string serialNumber, Batch productionBatch, List<Batch> sourceBatches, List<ProcessingStep> steps)
{
    // Example: SN-0142320250115000420000100035094 → BATCH-0102202501150042 (Pre-Roll Production)
    //          → BATCH-0101202501100015 (Flower Harvest) → CYCLE-2024-050 (Grow Cycle)

    var chain = $"SN-{serialNumber} → {productionBatch.BatchNumber} ({productionBatch.BatchType})";

    foreach (var sourceBatch in sourceBatches)
    {
        chain += $" → {sourceBatch.BatchNumber} ({sourceBatch.BatchType})";
    }

    return chain;
}
```

### 6.3 Batch Recall

**Scenario**: Batch contaminated, recall all products from that batch

```csharp
public async Task<BatchRecallDto> InitiateRecallAsync(string batchNumber, string recallReason)
{
    // Get batch
    var batch = await _batchRepository.GetByBatchNumberAsync(batchNumber);

    // Get all SNs from this batch
    var serialNumbers = await _serialNumberRepository.GetByBatchNumberAsync(batchNumber);

    // Get all customers who bought products from this batch
    var affectedCustomers = serialNumbers
        .Where(sn => sn.Status == "Sold" && sn.CustomerId.HasValue)
        .Select(sn => sn.CustomerId.Value)
        .Distinct()
        .ToList();

    var customers = new List<Customer>();
    foreach (var customerId in affectedCustomers)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer != null)
            customers.Add(customer);
    }

    // Update batch status
    await _batchRepository.UpdateStatusAsync(batch.BatchId, "Recalled");

    // Log recall
    await _auditLogService.CreateAuditLogAsync(
        userId: "SYSTEM",
        action: "BATCH_RECALL",
        entityType: "Batch",
        entityId: batch.BatchId,
        notes: $"Batch {batchNumber} recalled. Reason: {recallReason}"
    );

    return new BatchRecallDto
    {
        BatchNumber = batchNumber,
        RecallReason = recallReason,
        TotalUnitsProduced = serialNumbers.Count,
        UnitsSold = serialNumbers.Count(s => s.Status == "Sold"),
        UnitsAvailable = serialNumbers.Count(s => s.Status == "Available"),
        AffectedCustomers = _mapper.Map<List<CustomerDto>>(customers),
        RecallNotifications = $"{customers.Count} customers to be notified"
    };
}
```

---

## 7. DATABASE SCHEMA

### 7.1 Table Relationships

```
Batches (1) ──────────┐
  │                    │
  │ (1:many)           │ (1:many)
  ↓                    ↓
ProcessingSteps    SerialNumbers (1)
                       │
                       │ (many:1)
                       ↓
                   Transactions (Sales)
                       │
                       │ (many:1)
                       ↓
                   Customers
```

### 7.2 Indexes for Performance

```sql
-- Batch lookups
CREATE UNIQUE INDEX IX_Batches_BatchNumber ON Batches(BatchNumber);
CREATE INDEX IX_Batches_SiteId_BatchType ON Batches(SiteId, BatchType);
CREATE INDEX IX_Batches_SourceBatchNumber ON Batches(SourceBatchNumber);

-- Serial number lookups
CREATE UNIQUE INDEX IX_SerialNumbers_FullSN ON SerialNumbers(FullSerialNumber);
CREATE UNIQUE INDEX IX_SerialNumbers_ShortSN ON SerialNumbers(ShortSerialNumber);
CREATE INDEX IX_SerialNumbers_BatchNumber ON SerialNumbers(BatchNumber);
CREATE INDEX IX_SerialNumbers_Status ON SerialNumbers(Status);
CREATE INDEX IX_SerialNumbers_CustomerId ON SerialNumbers(CustomerId) WHERE CustomerId IS NOT NULL;

-- Traceability queries
CREATE INDEX IX_SerialNumbers_SoldDate ON SerialNumbers(SoldDate) WHERE SoldDate IS NOT NULL;
CREATE INDEX IX_Batches_CreatedDate ON Batches(CreatedDate);
```

---

## 8. USAGE EXAMPLES

### 8.1 Example: Pre-Roll Production

```csharp
// Step 1: Create production batch
var batchNumber = await _batchService.GenerateBatchNumberAsync(
    siteId: 1,
    batchType: BatchType.RetailProduction
);
// Result: 0103202501150042

// Step 2: Process through steps
await _productionService.ProcessStepAsync(batchNumber, "Milling", inputMass: 10.0, outputMass: 9.8);
await _productionService.ProcessStepAsync(batchNumber, "Filling", inputMass: 9.8, outputMass: 9.5);
await _productionService.ProcessStepAsync(batchNumber, "Capping", inputMass: 9.5, outputMass: 9.4);

// Step 3: Package and generate SNs
var serialNumbers = await _productionService.PackageAndGenerateSerialNumbersAsync(
    batchNumber: "0103202501150042",
    quantity: 940,
    weightPerUnit: 1.0m,
    strainId: 423 // Gelato
);
// Result: 940 serial numbers generated
// Example SN: 0142320250115000420000100035094
```

### 8.2 Example: POS Sale with SN

```csharp
// Customer buys pre-roll (serialized item)
var cart = new CheckoutRequestDto
{
    CustomerId = 123,
    Items = new List<CartItemDto>
    {
        new()
        {
            ProductId = 201,
            Quantity = 1,
            SerialNumber = "0142320250115000420000100035094" // Scanned SN
        }
    }
};

var result = await _posService.CheckoutAsync(cart);
// SN marked as sold, linked to customer, transaction recorded
```

### 8.3 Example: Traceability Query

```csharp
// Customer calls: "I bought a pre-roll, it made me sick. Can you tell me where it came from?"
// Customer provides receipt with SN: 0142320250115000420000100035094

var trace = await _traceabilityService.TraceBackwardAsync("0142320250115000420000100035094");

/*
Result:
- Serial Number: 0142320250115000420000100035094
- Production Batch: 0103202501150042 (Retail Production)
  - Date: 2025-01-15
  - Processing Steps: Milling → Filling → Capping → Packaging
- Source Batch: 0101202501100015 (Flower Harvest)
  - Harvest Date: 2025-01-10
- Grow Cycle: CYCLE-2024-050
  - Planted: 2024-10-01
  - Harvested: 2025-01-10
  - Strain: Gelato (423)
- Lab Test (COA): Lab-2025-0100
  - Lab: ABC Accredited Labs (ISO/IEC 17025)
  - Test Date: 2025-01-12
  - THC: 22.5%
  - CBD: 0.8%
  - Contaminants: PASS
*/
```

---

## ✅ IMPLEMENTATION CHECKLIST

Phase 8 (Batch & Serial Number System):

- [ ] Define batch number format (16 digits)
- [ ] Define full serial number format (28 digits)
- [ ] Define short serial number format (13 digits)
- [ ] Implement `BatchNumberGeneratorService`
- [ ] Implement `SerialNumberGeneratorService`
- [ ] Implement Luhn check digit algorithm
- [ ] Create `Batch` entity and repository
- [ ] Create `SerialNumber` entity and repository
- [ ] Integrate batch generation with Production module
- [ ] Integrate SN generation with Retail Production (packaging step)
- [ ] Integrate SN scanning with POS module
- [ ] Implement forward traceability (batch → sales)
- [ ] Implement backward traceability (SN → source)
- [ ] Implement batch recall workflow
- [ ] Create traceability reports UI
- [ ] Add database indexes for performance
- [ ] Unit test batch number generation (20+ tests)
- [ ] Unit test serial number generation (20+ tests)
- [ ] Unit test check digit calculation (10+ tests)
- [ ] Integration test end-to-end traceability (5+ scenarios)
- [ ] Performance test: Generate 10,000 SNs (<10 seconds)

---

**Document Status**: ✅ COMPLETE
**Critical**: This system is required for SAHPRA compliance and product recall capability.
**Next**: Implement in Phase 8 (Week 2 of PoC home stretch)
