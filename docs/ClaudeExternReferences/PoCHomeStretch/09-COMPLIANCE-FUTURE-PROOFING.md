# 09 - COMPLIANCE & FUTURE-PROOFING SPECIFICATION

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: Regulatory Landscape & Future Readiness
**Related Documents**:
- [00-MAIN-SPECIFICATION.md](00-MAIN-SPECIFICATION.md) - Central specification index
- [01-MODULE-DESCRIPTIONS.md](01-MODULE-DESCRIPTIONS.md) - All module overviews
- [02-INVENTORY-MODULE-SPEC.md](02-INVENTORY-MODULE-SPEC.md) - Batch traceability
- [10-GLOSSARY.md](10-GLOSSARY.md) - Terminology reference

---

## DOCUMENT PURPOSE

This document addresses the **regulatory compliance landscape** for South African cannabis businesses and outlines **future-proofing strategies** to ensure Project420 remains compliant as regulations evolve. It covers:

- **Current SA Cannabis Regulations** (SAHPRA, DALRRD, Cannabis Act 2024, POPIA)
- **Compliance Requirements** (licensing, reporting, traceability, tax)
- **Audit & Record-Keeping** (SARS, POPIA, seed-to-sale traceability)
- **Future Regulatory Changes** (anticipated amendments, international trends)
- **Future-Proofing Architecture** (modularity, adaptability, versioning)
- **Risk Mitigation** (regulatory non-compliance penalties, data protection)

**Status**: This is a **forward-looking document** that addresses both current compliance (PoC must meet today's requirements) and future readiness (system must adapt to regulatory changes).

**Target Audience**:
- Compliance officers (understanding regulatory obligations)
- Legal advisors (ensuring system meets legal requirements)
- Product management (prioritizing compliance features)
- Development team (implementing compliance architecture)

---

## TABLE OF CONTENTS

1. [Executive Summary](#1-executive-summary)
2. [South African Cannabis Regulatory Landscape](#2-south-african-cannabis-regulatory-landscape)
3. [SAHPRA Compliance](#3-sahpra-compliance)
4. [DALRRD Compliance](#4-dalrrd-compliance)
5. [Cannabis Act 2024 Compliance](#5-cannabis-act-2024-compliance)
6. [POPIA Compliance (Data Protection)](#6-popia-compliance-data-protection)
7. [SARS Compliance (Tax & VAT)](#7-sars-compliance-tax--vat)
8. [Seed-to-Sale Traceability](#8-seed-to-sale-traceability)
9. [Audit & Record-Keeping](#9-audit--record-keeping)
10. [Compliance Reporting](#10-compliance-reporting)
11. [Penalties & Risk Mitigation](#11-penalties--risk-mitigation)
12. [Future Regulatory Changes](#12-future-regulatory-changes)
13. [Future-Proofing Architecture](#13-future-proofing-architecture)
14. [International Expansion Readiness](#14-international-expansion-readiness)

---

## 1. EXECUTIVE SUMMARY

### 1.1 Compliance Overview

**Project420 Compliance Status**: **Designed for Full South African Cannabis Compliance**

**Regulatory Bodies**:
1. **SAHPRA** (South African Health Products Regulatory Authority) - Medical cannabis licensing, product registration
2. **DALRRD** (Department of Agriculture, Land Reform and Rural Development) - Cultivation permits, crop tracking
3. **SARS** (South African Revenue Service) - Tax, VAT compliance
4. **POPIA** (Protection of Personal Information Act) - Data privacy
5. **Department of Health** - Cannabis Act 2024 enforcement

**Compliance Pillars**:
1. **Licensing Tracking**: Store and monitor SAHPRA/DALRRD licenses (expiry alerts)
2. **Seed-to-Sale Traceability**: Track every product from plant → batch → serial → sale
3. **Regulatory Reporting**: Automated generation of SAHPRA/DALRRD reports
4. **Data Protection**: POPIA-compliant data handling (consent, retention, deletion)
5. **Audit Trails**: Immutable audit logs for all transactions (7-year retention)

### 1.2 Compliance Readiness

**PoC Status (Current)**:
- ✅ Batch number tracking (cultivation → production → sale)
- ✅ Serial number tracking (individual product traceability)
- ✅ Movement tracking (all stock changes recorded)
- ✅ Soft delete (7-year retention for SARS)
- ✅ Audit trails (CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
- 🟡 SAHPRA reporting (basic structure, needs templates)
- 🟡 DALRRD reporting (basic structure, needs templates)
- ❌ Customer ID verification (API integration needed)
- ❌ Daily purchase limit tracking (requires centralized database - future)

**Post-PoC Requirements**:
- SAHPRA monthly stock report automation
- DALRRD quarterly traceability report automation
- Customer ID verification integration (Home Affairs API)
- Daily limit tracking (local + future centralized registry)
- License renewal alerts (SAHPRA, DALRRD, site licenses)

### 1.3 Risk Assessment

| Compliance Risk | Severity | Likelihood | Mitigation |
|----------------|----------|------------|------------|
| **Inaccurate batch tracking** | Critical | Low | Movement-based SOH (100% accurate) |
| **Missing traceability** | Critical | Low | Batch/serial numbers mandatory |
| **POPIA data breach** | High | Medium | Encryption, access controls, audit logs |
| **Expired licenses** | High | Medium | Automated expiry alerts |
| **Incorrect VAT calculation** | Medium | Low | 15% VAT hardcoded, tested |
| **Customer ID not verified** | Medium | Medium | Manual verification (PoC), automated (post-PoC) |
| **Daily limit exceeded** | Medium | Medium | Local tracking (PoC), centralized (future) |
| **Audit trail gaps** | Medium | Low | Immutable audit logs, soft delete |

---

## 2. SOUTH AFRICAN CANNABIS REGULATORY LANDSCAPE

### 2.1 Regulatory Timeline

**2018**: Constitutional Court decriminalizes private adult cannabis use (Prince v Minister of Justice)

**2020**: DALRRD begins issuing cultivation permits for hemp (THC < 0.2%)

**2021**: SAHPRA framework for medical cannabis licensing established

**2022**: SAHPRA begins licensing cannabis cultivation, manufacturing, distribution

**2023**: DALRRD expands permits to include psychoactive cannabis (THC > 0.2%) for licensed entities

**2024**: Cannabis Act 2024 passed (comprehensive framework for legal cannabis industry)

**2025**: Commercial cannabis retail market opens (limited licenses issued)

**Future (2026-2030)**: Anticipated regulatory evolution (see Section 12)

### 2.2 Regulatory Framework

```
┌─────────────────────────────────────────────────────────────┐
│            SOUTH AFRICAN CANNABIS REGULATION                 │
└─────────────────────────────────────────────────────────────┘

┌──────────────┐   ┌──────────────┐   ┌──────────────┐
│   SAHPRA     │   │   DALRRD     │   │    SARS      │
│ (Licensing)  │   │ (Cultivation)│   │  (Tax/VAT)   │
└──────────────┘   └──────────────┘   └──────────────┘
       │                  │                   │
       │                  │                   │
       ▼                  ▼                   ▼
┌─────────────────────────────────────────────────────┐
│             CANNABIS ACT 2024                        │
│  - Age restrictions (21+)                           │
│  - Licensing requirements                           │
│  - Daily purchase limits                            │
│  - Seed-to-sale traceability                        │
│  - Quality standards                                │
│  - Advertising restrictions                         │
└─────────────────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────────────────┐
│               POPIA (Data Protection)                │
│  - Customer data protection                         │
│  - Consent management                               │
│  - 7-year retention (financial records)            │
│  - Right to be forgotten (soft delete)              │
└─────────────────────────────────────────────────────┘
```

### 2.3 License Types

**SAHPRA Licenses**:
1. **Cultivation License** (CL-XXX-2024) - Grow cannabis plants
2. **Manufacturing License** (ML-XXX-2024) - Process flower into products (pre-rolls, concentrates, edibles)
3. **Distribution License** (DL-XXX-2024) - Wholesale distribution to retailers
4. **Retail License** (RL-XXX-2024) - Sell to end consumers (dispensaries)

**DALRRD Permits**:
1. **Cultivation Permit** (CP-XXX-2024) - Agricultural permit for cannabis crops
2. **Import/Export Permit** (IE-XXX-2024) - Import genetics, export products (future)

**Multiple Licenses**:
- Vertically integrated businesses require multiple licenses (e.g., CL + ML + RL)
- Each license has separate compliance requirements

---

## 3. SAHPRA COMPLIANCE

### 3.1 SAHPRA Requirements

**Licensing**:
- Cultivation: R50,000 application fee, annual renewal R25,000
- Manufacturing: R75,000 application fee, annual renewal R35,000
- Retail: R30,000 application fee, annual renewal R15,000

**Reporting Requirements**:
- **Monthly Stock Reports**: SOH by product, batch number, location (due 7th of following month)
- **Incident Reports**: Adverse events, product recalls (within 24 hours)
- **Annual Audits**: Third-party audit of premises, processes, records (submit within 30 days)

**Product Registration**:
- Every product (SKU) must be registered with SAHPRA
- Registration includes: Product name, THC/CBD content, batch format, packaging details
- Registration fee: R5,000 per product, valid 5 years

**Quality Standards**:
- Good Manufacturing Practice (GMP) compliance
- Lab testing for potency (THC/CBD %), contaminants (pesticides, heavy metals, molds)
- Batch certificates of analysis (CoA) required for every batch

### 3.2 SAHPRA Monthly Stock Report

**Report Format** (Excel/CSV submission):

| Product Code | Product Name | Batch Number | SOH (Opening) | Purchased | Manufactured | Sold | Wastage | SOH (Closing) | Site | Date |
|--------------|--------------|--------------|---------------|-----------|--------------|------|---------|---------------|------|------|
| PRE-001 | Indica Pre-Roll 1g | 0101202412010001 | 100 | 0 | 50 | 30 | 5 | 115 | JHB-WH-01 | 2024-12-01 |
| FLW-002 | Sativa Flower 3.5g | 0102202411250003 | 500g | 1000g | 0 | 800g | 50g | 650g | JHB-WH-01 | 2024-12-01 |

**Project420 Implementation**:
```sql
-- SAHPRA Monthly Stock Report Query
WITH MonthlyMovements AS (
    SELECT
        p.ProductCode,
        p.ProductName,
        m.BatchNumber,
        m.SiteId,
        SUM(CASE WHEN m.MovementType IN ('GRV', 'ProductionOutput') THEN m.Quantity ELSE 0 END) AS Purchased_Manufactured,
        SUM(CASE WHEN m.MovementType = 'Sale' THEN m.Quantity ELSE 0 END) AS Sold,
        SUM(CASE WHEN m.MovementType IN ('Wastage', 'AdjustmentOut') THEN m.Quantity ELSE 0 END) AS Wastage
    FROM Movements m
    JOIN Products p ON p.ProductId = m.ProductId
    WHERE m.MovementDate >= @MonthStart
      AND m.MovementDate < @MonthEnd
      AND m.IsDeleted = 0
      AND m.IsReversed = 0
    GROUP BY p.ProductCode, p.ProductName, m.BatchNumber, m.SiteId
)
SELECT
    ProductCode,
    ProductName,
    BatchNumber,
    -- Opening SOH (calculated from previous month)
    -- Movements (purchased, manufactured, sold, wastage)
    -- Closing SOH (opening + in - out)
FROM MonthlyMovements;
```

**Automation**:
- Scheduled job runs on 1st of each month
- Generates CSV report
- Emails to compliance officer for review
- Submit to SAHPRA portal by 7th

### 3.3 SAHPRA License Tracking

**License Entity**:
```csharp
public class SAHPRALicense
{
    public int SAHPRALicenseId { get; set; }
    public string LicenseNumber { get; set; } // e.g., "RL-JHB-001-2024"
    public string LicenseType { get; set; } // 'Cultivation', 'Manufacturing', 'Retail'
    public int SiteId { get; set; } // Which site this license applies to
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } // 'Active', 'Expiring', 'Expired', 'Suspended'
    public decimal AnnualFee { get; set; }
    public DateTime? LastRenewalDate { get; set; }
}
```

**Expiry Alerts**:
```csharp
public async Task<IEnumerable<SAHPRALicense>> GetExpiringLicensesAsync(int daysBeforeExpiry = 60)
{
    var expiryThreshold = DateTime.UtcNow.AddDays(daysBeforeExpiry);

    return await _licenseRepo.GetAsync(l =>
        l.ExpiryDate <= expiryThreshold &&
        l.Status == "Active"
    );
}

// Email alert: "Your SAHPRA Retail License (RL-JHB-001-2024) expires in 45 days. Renew now to avoid suspension."
```

---

## 4. DALRRD COMPLIANCE

### 4.1 DALRRD Requirements

**Cultivation Permits**:
- Required for all cannabis cultivation (hemp THC < 0.2%, or psychoactive THC > 0.2%)
- Application: R10,000 (one-time), annual renewal R5,000
- Site inspection required (DALRRD officials visit cultivation site)

**Reporting Requirements**:
- **Quarterly Crop Reports**: Hectares planted, strains, expected yield (due 15th of following quarter)
- **Harvest Reports**: Actual yield, batch numbers generated, wet/dry weight (within 7 days of harvest)
- **Destruction Reports**: Waste, rejected plants, destroyed batches (within 24 hours)

**Traceability Requirements**:
- **Plant Tagging**: Each plant must have unique tag (RFID or barcode)
- **Batch Tracking**: Harvest batches must be traceable to specific plants
- **Seed-to-Sale**: Full traceability from seed/clone → plant → harvest → batch → product → sale

**Inspections**:
- Unannounced site inspections (DALRRD officials)
- Must produce records on demand (plant tags, batch numbers, movement logs)
- Non-compliance: Permit suspension or revocation

### 4.2 DALRRD Quarterly Crop Report

**Report Format** (PDF submission):

```
DALRRD QUARTERLY CROP REPORT
Period: Q1 2025 (Jan - Mar)
Permit Number: CP-JHB-001-2024
Permit Holder: GreenLeaf Cultivation (Pty) Ltd
Site: JHB-FARM-01

CROP DETAILS:
- Total Hectares Planted: 2.5 ha
- Strains: Indica (1.2 ha), Sativa (1.0 ha), Hybrid (0.3 ha)
- Plant Count: 5,000 plants
- Expected Harvest Date: April 2025
- Expected Yield: 1,500 kg (dry weight)

ENVIRONMENTAL CONTROLS:
- Irrigation: Drip irrigation system
- Pest Management: Organic (neem oil, ladybugs)
- Fertilizers: Organic compost

COMPLIANCE NOTES:
- All plants tagged with RFID tags
- Weekly inspections conducted (logs available on request)
- No incidents to report
```

**Project420 Implementation**:
```csharp
public class DALRRDQuarterlyReport
{
    public string PermitNumber { get; set; }
    public string ReportPeriod { get; set; }
    public decimal HectaresPlanted { get; set; }
    public int PlantCount { get; set; }
    public decimal ExpectedYield { get; set; }
    public List<StrainDetail> Strains { get; set; }
    public string EnvironmentalControls { get; set; }
    public string ComplianceNotes { get; set; }
}

// Generate report from CultivationBatches data
public async Task<DALRRDQuarterlyReport> GenerateQuarterlyReportAsync(string permitNumber, DateTime quarterStart, DateTime quarterEnd)
{
    var batches = await _cultivationRepo.GetBatchesByQuarterAsync(quarterStart, quarterEnd);

    return new DALRRDQuarterlyReport
    {
        PermitNumber = permitNumber,
        ReportPeriod = $"Q{GetQuarter(quarterStart)} {quarterStart.Year}",
        PlantCount = batches.Sum(b => b.PlantCount),
        ExpectedYield = batches.Sum(b => b.ExpectedYield),
        Strains = batches.GroupBy(b => b.StrainName).Select(g => new StrainDetail
        {
            StrainName = g.Key,
            PlantCount = g.Sum(b => b.PlantCount)
        }).ToList()
    };
}
```

### 4.3 DALRRD Harvest Report

**Report Trigger**: Within 7 days of harvest completion.

**Report Content**:
- Harvest date
- Batch number generated
- Wet weight (immediately after harvest)
- Dry weight (after curing)
- Quality grade (A, B, C)
- Destination (sale to manufacturer, internal processing, destruction)

**Project420 Implementation**:
```csharp
public async Task SubmitHarvestReportAsync(int cultivationBatchId)
{
    var batch = await _cultivationRepo.GetByIdAsync(cultivationBatchId);

    var report = new DALRRDHarvestReport
    {
        PermitNumber = batch.PermitNumber,
        HarvestDate = batch.HarvestDate.Value,
        BatchNumber = batch.BatchNumber,
        WetWeight = batch.WetWeight,
        DryWeight = batch.DryWeight,
        QualityGrade = batch.QualityGrade,
        Destination = "Internal Processing"
    };

    // Generate PDF
    var pdf = _pdfGenerator.GenerateHarvestReport(report);

    // Email to DALRRD (manual submission for PoC, API integration post-PoC)
    await _emailService.SendAsync("dalrrd@agriculture.gov.za", "Harvest Report", pdf);
}
```

---

## 5. CANNABIS ACT 2024 COMPLIANCE

### 5.1 Cannabis Act 2024 Key Provisions

**Age Restrictions**:
- Minimum age: **21 years** for legal cannabis purchase
- ID verification mandatory for every sale
- No sales to minors (penalty: R100,000 fine + license suspension)

**Daily Purchase Limits**:
- **100g dried flower** per person per day (across all dispensaries)
- **10g concentrate** per person per day
- **Future**: Centralized database to track limits (not yet implemented by government)

**Product Standards**:
- THC limits: Medical (no limit with prescription), Recreational (max 25% THC)
- Packaging: Child-resistant, opaque, health warnings
- Labeling: THC/CBD content, batch number, expiry date, health warnings

**Advertising Restrictions**:
- No advertising visible to minors
- No health claims (e.g., "cures cancer")
- No celebrity endorsements
- No sponsorship of events/sports

**Consumption Restrictions**:
- No public consumption (private property only)
- No driving under influence (0.05% blood THC limit)

### 5.2 ID Verification (Age 21+)

**Manual Verification (PoC)**:
```csharp
public async Task<IDVerificationResult> VerifyCustomerIDAsync(string idNumber)
{
    // 1. Validate SA ID format (13 digits)
    if (!IsValidSAIDNumber(idNumber))
        return new IDVerificationResult { IsValid = false, ErrorMessage = "Invalid SA ID number" };

    // 2. Extract date of birth from ID number
    var dob = ExtractDOBFromIDNumber(idNumber);
    var age = CalculateAge(dob);

    // 3. Check minimum age (21)
    if (age < 21)
        return new IDVerificationResult { IsValid = false, ErrorMessage = "Customer must be 21 or older" };

    return new IDVerificationResult
    {
        IsValid = true,
        IDNumber = idNumber,
        DateOfBirth = dob,
        Age = age
    };
}

private bool IsValidSAIDNumber(string idNumber)
{
    if (idNumber.Length != 13 || !idNumber.All(char.IsDigit))
        return false;

    // Luhn algorithm validation (checksum)
    return ValidateLuhnChecksum(idNumber);
}

private DateTime ExtractDOBFromIDNumber(string idNumber)
{
    var year = int.Parse(idNumber.Substring(0, 2));
    var month = int.Parse(idNumber.Substring(2, 2));
    var day = int.Parse(idNumber.Substring(4, 2));

    // Determine century (00-24 = 2000s, 25-99 = 1900s)
    year += year <= 24 ? 2000 : 1900;

    return new DateTime(year, month, day);
}
```

**Automated Verification (Post-PoC)**:
- Integration with Home Affairs API (government ID verification service)
- Real-time ID validation
- Detect fake/expired IDs

### 5.3 Daily Purchase Limits

**Local Tracking (PoC)**:
```csharp
public async Task<DailyLimitCheckResult> CheckDailyLimitsAsync(string idNumber, List<int> productIds)
{
    const decimal MAX_FLOWER_GRAMS = 100m;
    const decimal MAX_CONCENTRATE_GRAMS = 10m;

    // Get today's purchases for this customer (same site only - PoC limitation)
    var todaysPurchases = await _transactionRepo.GetTodaysTransactionsByIDNumberAsync(idNumber);

    // Calculate total purchased today
    var flowerTotal = todaysPurchases
        .SelectMany(t => t.TransactionDetails)
        .Where(d => d.Product.ProductType == ProductType.FinishedGood && d.Product.ProductCategory.CategoryName.Contains("Flower"))
        .Sum(d => d.Quantity);

    var concentrateTotal = todaysPurchases
        .SelectMany(t => t.TransactionDetails)
        .Where(d => d.Product.ProductCategory.CategoryName.Contains("Concentrate"))
        .Sum(d => d.Quantity);

    // Check limits
    if (flowerTotal > MAX_FLOWER_GRAMS)
        return new DailyLimitCheckResult
        {
            IsWithinLimit = false,
            ErrorMessage = $"Daily limit exceeded: Flower {flowerTotal}g / {MAX_FLOWER_GRAMS}g"
        };

    if (concentrateTotal > MAX_CONCENTRATE_GRAMS)
        return new DailyLimitCheckResult
        {
            IsWithinLimit = false,
            ErrorMessage = $"Daily limit exceeded: Concentrate {concentrateTotal}g / {MAX_CONCENTRATE_GRAMS}g"
        };

    return new DailyLimitCheckResult
    {
        IsWithinLimit = true,
        FlowerTotal = flowerTotal,
        ConcentrateTotal = concentrateTotal
    };
}
```

**Centralized Tracking (Future - Government Database)**:
- Government-run centralized database (similar to US state systems)
- All dispensaries submit sales in real-time
- Database checks customer limits across ALL dispensaries
- Project420 integration: API calls to check/update limits

---

## 6. POPIA COMPLIANCE (DATA PROTECTION)

### 6.1 POPIA Overview

**POPIA** (Protection of Personal Information Act, 2013): South Africa's data protection law (equivalent to EU GDPR).

**Key Principles**:
1. **Accountability**: Responsible for protecting customer data
2. **Processing Limitation**: Only collect data for specific purposes
3. **Purpose Specification**: Inform customers why data is collected
4. **Further Processing Limitation**: Don't use data for unrelated purposes
5. **Information Quality**: Keep data accurate and up-to-date
6. **Openness**: Transparent about data processing
7. **Security Safeguards**: Protect data from breaches
8. **Data Subject Participation**: Customers can access/correct/delete their data

### 6.2 POPIA Requirements for Project420

**Customer Data**:
- **Personal Information**: Name, ID number, phone, email, address
- **Consent**: Must obtain explicit consent to collect/process data
- **Retention**: 7 years for financial records (SARS requirement)
- **Deletion**: After 7 years, data must be anonymized or deleted
- **Access**: Customers can request their data (POPIA request)

**Data Minimization**:
- Don't collect unnecessary data (e.g., don't require ID number for walk-in customers)
- Optional customer registration (only required if customer wants loyalty program)

**Security Safeguards**:
- **Encryption**: Encrypt sensitive data (ID numbers, payment details)
- **Access Controls**: Role-based access (cashiers can't see full ID numbers)
- **Audit Logs**: Track who accessed customer data (compliance audits)
- **Breach Notification**: Notify Information Regulator within 24 hours of data breach

### 6.3 Soft Delete & 7-Year Retention

**Principle**: Never physically delete financial records (SARS requires 7 years).

**Implementation**:
```csharp
// Soft delete: Mark as deleted, retain data
public async Task DeleteCustomerAsync(int customerId, string userId)
{
    var customer = await _customerRepo.GetByIdAsync(customerId);

    // Soft delete
    customer.IsDeleted = true;
    customer.DeletedDate = DateTime.UtcNow;
    customer.DeletedBy = userId;

    await _customerRepo.UpdateAsync(customer);

    // Schedule for hard delete after 7 years
    var hardDeleteDate = DateTime.UtcNow.AddYears(7);
    await _scheduledJobService.ScheduleHardDeleteAsync(customerId, hardDeleteDate);
}

// Hard delete: Anonymize or purge after 7 years
public async Task HardDeleteCustomerAsync(int customerId)
{
    var customer = await _customerRepo.GetByIdAsync(customerId);

    // Anonymize (retain financial records, remove PII)
    customer.FirstName = "DELETED";
    customer.LastName = "DELETED";
    customer.Email = null;
    customer.PhoneNumber = null;
    customer.IDNumber = null;
    customer.AddressLine1 = null;

    await _customerRepo.UpdateAsync(customer);

    // Note: Transactions remain (financial records), but customer PII is gone
}
```

**Global Query Filter** (EF Core):
```csharp
modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);

// Queries automatically exclude deleted records
var activeCustomers = await _context.Customers.ToListAsync();

// Include deleted records explicitly
var allCustomers = await _context.Customers.IgnoreQueryFilters().ToListAsync();
```

### 6.4 Data Subject Rights (POPIA Requests)

**Customer Rights**:
1. **Right to Access**: Customer requests their data
2. **Right to Correction**: Customer corrects inaccurate data
3. **Right to Deletion**: Customer requests deletion (subject to 7-year retention)
4. **Right to Objection**: Customer objects to data processing

**POPIA Request Workflow**:
```csharp
public async Task<CustomerDataExport> HandlePOPIARequestAsync(string idNumber)
{
    var customer = await _customerRepo.GetByIDNumberAsync(idNumber);
    if (customer == null)
        throw new NotFoundException("Customer not found");

    // Export all customer data
    var export = new CustomerDataExport
    {
        PersonalInfo = customer,
        PurchaseHistory = await _transactionRepo.GetByCustomerAsync(customer.CustomerId),
        LoyaltyPoints = customer.LoyaltyPoints,
        ExportDate = DateTime.UtcNow
    };

    // Generate PDF
    var pdf = _pdfGenerator.GeneratePOPIAExport(export);

    return export;
}
```

---

## 7. SARS COMPLIANCE (TAX & VAT)

### 7.1 VAT Compliance

**VAT Rate**: 15% (standard rate in South Africa)

**VAT Registration**:
- Mandatory if annual turnover > R1 million
- Voluntary registration if turnover < R1 million

**VAT Invoicing**:
- Tax invoice must include:
  - Vendor name, address, VAT number
  - Invoice number (unique sequential)
  - Date of supply
  - Description of goods
  - **VAT amount clearly shown**
  - Total amount (including VAT)

**VAT Returns**:
- Submitted every 2 months (bi-monthly)
- Due 25th of month following VAT period
- Electronic filing (SARS eFiling system)

**Project420 Implementation**:
```csharp
public decimal CalculateVAT(decimal subTotal)
{
    const decimal VAT_RATE = 0.15m;

    // Tax-inclusive pricing (shelf price includes VAT)
    var vatAmount = subTotal / (1 + VAT_RATE) * VAT_RATE;

    return Math.Round(vatAmount, 2);
}

// Example: Shelf price R115 (incl. VAT)
// VAT = R115 / 1.15 * 0.15 = R15
// Net = R115 - R15 = R100
```

**VAT Report** (Bi-Monthly):
```sql
SELECT
    SUM(SubTotal) AS TotalSales,
    SUM(VATAmount) AS TotalVAT,
    SUM(TotalAmount) AS TotalInclVAT
FROM RetailTransactionHeaders
WHERE TransactionDate >= @PeriodStart
  AND TransactionDate < @PeriodEnd
  AND Status = 'Completed'
  AND IsDeleted = 0;
```

### 7.2 Income Tax Compliance

**Corporate Income Tax**: 27% (standard rate for companies)

**Financial Records Retention**: **7 years** (SARS requirement)
- Sales invoices
- Purchase invoices
- Bank statements
- General ledger

**Audit Risk**: SARS can audit any business within 7 years.

**Project420 Compliance**:
- Soft delete (retain all transactions for 7 years)
- Export to accounting software (Xero, QuickBooks, Sage)
- Audit-ready reports (income statement, balance sheet)

---

## 8. SEED-TO-SALE TRACEABILITY

### 8.1 Traceability Requirements

**Principle**: Full traceability from seed/clone → plant → harvest → batch → product → sale.

**Regulatory Drivers**:
- **SAHPRA**: Product recalls (if quality issue, trace all affected batches)
- **DALRRD**: Crop tracking (ensure legal cultivation, no diversion to black market)
- **Cannabis Act 2024**: Consumer protection (trace contaminated products to source)

**Traceability Levels**:
1. **Plant Level**: Plant tag → Cultivation batch
2. **Batch Level**: Cultivation batch → Production batch
3. **Serial Number Level**: Production batch → Individual product (pre-roll, vape)
4. **Sale Level**: Serial number → Customer (retail transaction)

### 8.2 Traceability Flow

```
┌─────────────────────────────────────────────────────────────┐
│              SEED-TO-SALE TRACEABILITY FLOW                  │
└─────────────────────────────────────────────────────────────┘

CULTIVATION:
Plant Tag: PLANT-001
    ↓ Grown, harvested
Cultivation Batch: CULT-BATCH-001 (1kg dry weight)
    ↓ Sold to manufacturer

PRODUCTION:
Production Batch: PROD-BATCH-002 (100 pre-rolls)
    ↓ Input: CULT-BATCH-001 (1kg flower)
    ↓ Step 1: Grinding
    ↓ Step 2: Filling
    ↓ Step 3: QC
    ↓ Step 4: Packaging & Serialization
    ↓ Output: 100 pre-rolls with serial numbers

SERIAL NUMBERS:
SN-0124121100001, SN-0124121100002, ..., SN-0124121100100
    ↓ Linked to PROD-BATCH-002
    ↓ Linked to CULT-BATCH-001 (raw material)

RETAIL SALE:
Customer buys SN-0124121100042
    ↓ Retail Transaction: SALE-2024-12345
    ↓ Customer ID: ***6789 (last 4 digits)

TRACEABILITY QUERY:
"Who bought SN-0124121100042?"
    → Customer ***6789, Transaction SALE-2024-12345, Date 2024-12-11

"Where did SN-0124121100042 come from?"
    → Production Batch PROD-BATCH-002
    → Raw Material CULT-BATCH-001
    → Plant Tags: PLANT-001, PLANT-002, PLANT-003 (if tracked)
```

### 8.3 Traceability Queries

**Forward Traceability** (Plant → Sale):
```sql
-- "Where did Cultivation Batch CULT-001 go?"
WITH BatchFlow AS (
    SELECT
        'Cultivation' AS Stage,
        'CULT-001' AS BatchNumber,
        NULL AS DestinationBatch
    UNION ALL
    SELECT
        'Production',
        pi.BatchNumber AS InputBatch,
        pb.BatchNumber AS OutputBatch
    FROM ProductionInputs pi
    JOIN ProductionBatches pb ON pb.ProductionBatchId = pi.ProductionBatchId
    WHERE pi.BatchNumber = 'CULT-001'
    UNION ALL
    SELECT
        'Retail Sale',
        td.BatchNumber,
        rth.TransactionNumber
    FROM TransactionDetails td
    JOIN RetailTransactionHeaders rth ON rth.TransactionId = td.HeaderId AND td.TransactionType = 'Retail'
    WHERE td.BatchNumber LIKE 'PROD-%'
)
SELECT * FROM BatchFlow;
```

**Backward Traceability** (Sale → Plant):
```sql
-- "Where did Serial Number SN-0124121100042 come from?"
SELECT
    sn.SerialNumber,
    sn.BatchNumber AS ProductionBatch,
    pi.BatchNumber AS CultivationBatch,
    p.PlantTag
FROM SerialNumbers sn
JOIN ProductionBatches pb ON pb.BatchNumber = sn.BatchNumber
JOIN ProductionInputs pi ON pi.ProductionBatchId = pb.ProductionBatchId
LEFT JOIN Plants p ON p.CultivationBatchId = (
    SELECT CultivationBatchId FROM CultivationBatches WHERE BatchNumber = pi.BatchNumber
)
WHERE sn.SerialNumber = 'SN-0124121100042';
```

**Batch Recall** (Find all affected sales):
```sql
-- "Recall all products from Cultivation Batch CULT-001"
SELECT
    sn.SerialNumber,
    rth.TransactionNumber,
    rth.CustomerIDNumber,
    rth.TransactionDate,
    c.FullName,
    c.PhoneNumber
FROM SerialNumbers sn
JOIN ProductionBatches pb ON pb.BatchNumber = sn.BatchNumber
JOIN ProductionInputs pi ON pi.ProductionBatchId = pb.ProductionBatchId
JOIN RetailTransactionHeaders rth ON rth.TransactionId = sn.SoldTransactionId
LEFT JOIN Customers c ON c.CustomerId = rth.CustomerId
WHERE pi.BatchNumber = 'CULT-001'
  AND sn.Status = 'Sold';
```

---

## 9. AUDIT & RECORD-KEEPING

### 9.1 Audit Trail Requirements

**Principle**: Immutable audit logs for all business transactions.

**Audit Columns** (Standard for All Tables):
```csharp
public abstract class AuditableEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
}
```

**Audit Log Table** (Comprehensive Change Tracking):
```sql
CREATE TABLE AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(100) NOT NULL,
    RecordId INT NOT NULL,
    OperationType NVARCHAR(20) NOT NULL, -- 'INSERT', 'UPDATE', 'DELETE'
    OldValues NVARCHAR(MAX), -- JSON
    NewValues NVARCHAR(MAX), -- JSON
    ChangedBy NVARCHAR(100) NOT NULL,
    ChangedDate DATETIME NOT NULL DEFAULT GETDATE(),
    IPAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL
);

CREATE INDEX IX_AuditLog_Table ON AuditLogs(TableName);
CREATE INDEX IX_AuditLog_Record ON AuditLogs(TableName, RecordId);
CREATE INDEX IX_AuditLog_Date ON AuditLogs(ChangedDate);
```

**Audit Log Capture** (EF Core):
```csharp
public override int SaveChanges()
{
    var auditEntries = new List<AuditLog>();

    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
        {
            var auditLog = new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                RecordId = (int)entry.Property("Id").CurrentValue,
                OperationType = entry.State.ToString(),
                OldValues = JsonSerializer.Serialize(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p])),
                NewValues = JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p])),
                ChangedBy = _currentUser.UserId,
                ChangedDate = DateTime.UtcNow
            };

            auditEntries.Add(auditLog);
        }
    }

    var result = base.SaveChanges();

    // Save audit logs
    AuditLogs.AddRange(auditEntries);
    base.SaveChanges();

    return result;
}
```

### 9.2 Audit Reports

**Who Changed What When**:
```sql
SELECT
    a.ChangedDate,
    a.ChangedBy,
    a.TableName,
    a.RecordId,
    a.OperationType,
    a.OldValues,
    a.NewValues
FROM AuditLogs a
WHERE a.TableName = 'Products'
  AND a.RecordId = 123
ORDER BY a.ChangedDate DESC;
```

**Data Integrity Check**:
```sql
-- Find records modified multiple times in short period (suspicious activity)
SELECT
    TableName,
    RecordId,
    COUNT(*) AS ModificationCount,
    MIN(ChangedDate) AS FirstChange,
    MAX(ChangedDate) AS LastChange
FROM AuditLogs
WHERE OperationType = 'UPDATE'
  AND ChangedDate >= DATEADD(HOUR, -1, GETDATE())
GROUP BY TableName, RecordId
HAVING COUNT(*) > 5;
```

---

## 10. COMPLIANCE REPORTING

### 10.1 Automated Report Generation

**Monthly Reports**:
- SAHPRA Stock Report (due 7th of following month)
- VAT Report (due 25th of month following VAT period)

**Quarterly Reports**:
- DALRRD Crop Report (due 15th of following quarter)

**Annual Reports**:
- SAHPRA Annual Audit (submit within 30 days)
- SARS Income Tax Return (due within 12 months of financial year-end)

**Scheduled Jobs** (Hangfire / Quartz.NET):
```csharp
[RecurringJob("0 0 1 * *")] // 1st of every month at midnight
public async Task GenerateSAHPRAMonthlyReportAsync()
{
    var previousMonth = DateTime.UtcNow.AddMonths(-1);
    var report = await _sahpraReportService.GenerateMonthlyStockReportAsync(previousMonth);

    // Email to compliance officer
    await _emailService.SendAsync("compliance@greenleaf.co.za", "SAHPRA Monthly Report", report);
}

[RecurringJob("0 0 1 1,4,7,10 *")] // 1st of Jan, Apr, Jul, Oct
public async Task GenerateDALRRDQuarterlyReportAsync()
{
    var previousQuarter = DateTime.UtcNow.AddMonths(-3);
    var report = await _dalrrdReportService.GenerateQuarterlyReportAsync(previousQuarter);

    // Email to compliance officer
    await _emailService.SendAsync("compliance@greenleaf.co.za", "DALRRD Quarterly Report", report);
}
```

### 10.2 Compliance Dashboard

**Key Metrics**:
- License expiry dates (red if < 30 days)
- Days since last SAHPRA report submission (red if overdue)
- Outstanding compliance tasks (e.g., "Submit Q1 DALRRD report")
- Audit readiness score (% of records with complete traceability)

**Dashboard UI** (Blazor):
```razor
<div class="compliance-dashboard">
    <div class="license-status">
        <h3>License Status</h3>
        @foreach (var license in Licenses)
        {
            <div class="license-card @GetStatusClass(license)">
                <span>@license.LicenseType: @license.LicenseNumber</span>
                <span>Expires: @license.ExpiryDate.ToString("yyyy-MM-dd")</span>
                @if (license.ExpiryDate < DateTime.UtcNow.AddDays(30))
                {
                    <span class="alert">⚠ Renew now!</span>
                }
            </div>
        }
    </div>

    <div class="report-status">
        <h3>Outstanding Reports</h3>
        <ul>
            @foreach (var report in OutstandingReports)
            {
                <li class="@GetReportStatusClass(report)">
                    @report.ReportType - Due: @report.DueDate.ToString("yyyy-MM-dd")
                </li>
            }
        </ul>
    </div>

    <div class="audit-readiness">
        <h3>Audit Readiness</h3>
        <progress value="@AuditReadinessScore" max="100"></progress>
        <span>@AuditReadinessScore%</span>
    </div>
</div>
```

---

## 11. PENALTIES & RISK MITIGATION

### 11.1 Non-Compliance Penalties

| Violation | Penalty | Regulatory Body |
|-----------|---------|----------------|
| **Sale to minor** | R100,000 fine + license suspension | Cannabis Act 2024 |
| **Missing traceability** | R50,000 fine + warning | SAHPRA |
| **Late SAHPRA report** | R10,000 fine per month | SAHPRA |
| **Unlicensed operation** | R500,000 fine + criminal prosecution | SAHPRA |
| **POPIA data breach** | R10M fine (max) | Information Regulator |
| **VAT fraud** | 200% penalty + criminal prosecution | SARS |
| **Expired license** | Immediate cease of operations | SAHPRA / DALRRD |

### 11.2 Risk Mitigation Strategies

**Risk 1: License Expiry**
- **Mitigation**: Automated expiry alerts (60, 30, 14, 7 days before expiry)
- **Backup**: Manual calendar reminders

**Risk 2: Inaccurate Batch Tracking**
- **Mitigation**: Movement-based SOH (100% accurate)
- **Backup**: Monthly reconciliation reports

**Risk 3: POPIA Data Breach**
- **Mitigation**: Encryption, access controls, audit logs
- **Backup**: Cyber insurance, incident response plan

**Risk 4: Missing Regulatory Reports**
- **Mitigation**: Automated report generation, scheduled reminders
- **Backup**: Dedicated compliance officer

**Risk 5: Customer ID Not Verified**
- **Mitigation**: Mandatory ID verification at POS (transaction blocked if not verified)
- **Backup**: Manager override with audit log

---

## 12. FUTURE REGULATORY CHANGES

### 12.1 Anticipated SA Cannabis Regulatory Changes (2026-2030)

**Centralized Traceability Database** (Likely 2026-2027)
- Government-run centralized database (similar to US state systems: METRC, BioTrack)
- All licensed businesses must submit data in real-time
- Track plant tags, batch numbers, sales across entire industry
- **Project420 Readiness**: API integration layer (future sprint)

**Daily Purchase Limits - Centralized Enforcement** (Likely 2026)
- Centralized database tracks customer purchases across all dispensaries
- Real-time limit checks (customers can't exceed 100g/day across multiple stores)
- **Project420 Readiness**: API integration for limit checks

**Expanded Product Categories** (Likely 2027-2028)
- Edibles (currently restricted, may be legalized)
- Beverages (cannabis-infused drinks)
- Topicals (creams, balms)
- **Project420 Readiness**: Extensible product categorization system

**Interstate/International Trade** (Likely 2028-2030)
- Export permits for international markets (Europe, Australia, Canada)
- Interstate trade within SADC region (Southern African Development Community)
- **Project420 Readiness**: Multi-currency, multi-regulatory-framework support

**Tax Changes** (Possible 2026+)
- Excise tax on cannabis (similar to alcohol/tobacco) - currently not in place
- Possible VAT rate adjustment (currently 15%, may increase)
- **Project420 Readiness**: Configurable tax engine

### 12.2 International Cannabis Regulatory Trends

**US State Systems** (Lessons Learned):
- METRC (Marijuana Enforcement Tracking Reporting Compliance) - seed-to-sale tracking
- Mandatory batch testing (potency, contaminants)
- Child-resistant packaging
- Cannabis tax stamps (tax collection verification)

**Canadian System** (Federal Legalization):
- Health Canada oversight (licensing, product approvals)
- Cannabis Tracking System (CTS) - national database
- Standardized packaging (plain packaging, health warnings)
- Strict advertising restrictions

**European Markets** (Medical Focus):
- Medical-only in most countries (Germany, UK, Italy)
- Pharmacy distribution (not dispensaries)
- GMP (Good Manufacturing Practice) compliance
- EuPEA (European Union Pharmaceutical Environment Association) tracking

**Project420 International Expansion Considerations**:
- Modularity (swap out compliance modules per country)
- Multi-language support
- Multi-currency support
- API integrations with country-specific tracking systems

---

## 13. FUTURE-PROOFING ARCHITECTURE

### 13.1 Modular Compliance Design

**Principle**: Compliance logic is modular, not hardcoded.

**Compliance Module Interface**:
```csharp
public interface IComplianceModule
{
    string CountryCode { get; } // "ZA", "CA", "DE"
    string RegulatoryBody { get; } // "SAHPRA", "HealthCanada", "BfArM"

    Task<ValidationResult> ValidateTransactionAsync(Transaction transaction);
    Task<ComplianceReport> GenerateMonthlyReportAsync(DateTime month);
    Task<LicenseStatus> CheckLicenseStatusAsync(string licenseNumber);
}

// South African compliance module
public class SAHPRAComplianceModule : IComplianceModule
{
    public string CountryCode => "ZA";
    public string RegulatoryBody => "SAHPRA";

    public async Task<ValidationResult> ValidateTransactionAsync(Transaction transaction)
    {
        // SA-specific rules: ID verification (21+), daily limits (100g flower)
        if (!await VerifyCustomerAge(transaction.CustomerID, minAge: 21))
            return ValidationResult.Fail("Customer must be 21+");

        if (!await CheckDailyLimits(transaction.CustomerID, transaction.Items))
            return ValidationResult.Fail("Daily limit exceeded");

        return ValidationResult.Success();
    }

    public async Task<ComplianceReport> GenerateMonthlyReportAsync(DateTime month)
    {
        // Generate SAHPRA-specific monthly stock report
        return await _sahpraReportService.GenerateMonthlyStockReportAsync(month);
    }
}

// Canadian compliance module (future)
public class HealthCanadaComplianceModule : IComplianceModule
{
    public string CountryCode => "CA";
    public string RegulatoryBody => "HealthCanada";

    public async Task<ValidationResult> ValidateTransactionAsync(Transaction transaction)
    {
        // Canadian-specific rules: Age 19+ (or 18+ in some provinces)
        if (!await VerifyCustomerAge(transaction.CustomerID, minAge: 19))
            return ValidationResult.Fail("Customer must be 19+");

        // No daily limits in Canada (unlike SA)
        return ValidationResult.Success();
    }

    public async Task<ComplianceReport> GenerateMonthlyReportAsync(DateTime month)
    {
        // Generate Health Canada-specific CTS (Cannabis Tracking System) report
        return await _healthCanadaReportService.GenerateCTSReportAsync(month);
    }
}
```

**Compliance Module Selection**:
```csharp
public class ComplianceService
{
    private readonly IEnumerable<IComplianceModule> _complianceModules;

    public ComplianceService(IEnumerable<IComplianceModule> complianceModules)
    {
        _complianceModules = complianceModules;
    }

    public async Task<ValidationResult> ValidateTransactionAsync(Transaction transaction, string countryCode)
    {
        var module = _complianceModules.FirstOrDefault(m => m.CountryCode == countryCode);
        if (module == null)
            throw new Exception($"No compliance module found for country: {countryCode}");

        return await module.ValidateTransactionAsync(transaction);
    }
}

// Dependency injection registration
services.AddTransient<IComplianceModule, SAHPRAComplianceModule>();
services.AddTransient<IComplianceModule, HealthCanadaComplianceModule>(); // Future
```

### 13.2 Configuration-Driven Compliance Rules

**Compliance Configuration** (JSON):
```json
{
  "CountryCode": "ZA",
  "RegulatoryBody": "SAHPRA",
  "MinimumAge": 21,
  "DailyLimits": {
    "Flower": 100,
    "Concentrate": 10
  },
  "ReportingSchedule": {
    "MonthlyStockReport": {
      "Frequency": "Monthly",
      "DueDay": 7
    },
    "QuarterlyCropReport": {
      "Frequency": "Quarterly",
      "DueDay": 15
    }
  },
  "LicenseTypes": [
    { "Code": "CL", "Name": "Cultivation License", "AnnualFee": 25000 },
    { "Code": "ML", "Name": "Manufacturing License", "AnnualFee": 35000 },
    { "Code": "RL", "Name": "Retail License", "AnnualFee": 15000 }
  ]
}
```

**Dynamic Rule Evaluation**:
```csharp
public async Task<bool> CheckDailyLimitsAsync(string customerId, List<Product> products)
{
    var config = await _configService.GetComplianceConfigAsync("ZA");

    var flowerPurchased = await CalculateDailyPurchaseAsync(customerId, "Flower");
    var concentratePurchased = await CalculateDailyPurchaseAsync(customerId, "Concentrate");

    if (flowerPurchased > config.DailyLimits.Flower)
        return false;

    if (concentratePurchased > config.DailyLimits.Concentrate)
        return false;

    return true;
}
```

### 13.3 API Integration Layer (Future Readiness)

**Centralized Tracking System Integration**:
```csharp
public interface ICentralizedTrackingAPI
{
    Task<TrackingResult> SubmitPlantTagAsync(PlantTag tag);
    Task<TrackingResult> SubmitBatchAsync(Batch batch);
    Task<TrackingResult> SubmitSaleAsync(Sale sale);
    Task<DailyLimitResult> CheckDailyLimitsAsync(string customerId);
}

// South Africa (future government system)
public class SAHPRACentralizedTrackingAPI : ICentralizedTrackingAPI
{
    private readonly HttpClient _httpClient;

    public async Task<TrackingResult> SubmitBatchAsync(Batch batch)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "https://api.sahpra.gov.za/tracking/batch",
            new
            {
                BatchNumber = batch.BatchNumber,
                ProductId = batch.ProductId,
                Quantity = batch.Quantity,
                ProductionDate = batch.ProductionDate
            });

        return await response.Content.ReadFromJsonAsync<TrackingResult>();
    }
}

// US METRC (if expanding to US states)
public class METRCTrackingAPI : ICentralizedTrackingAPI
{
    // Similar implementation for METRC
}
```

---

## 14. INTERNATIONAL EXPANSION READINESS

### 14.1 Multi-Country Support

**Database Schema**:
```sql
CREATE TABLE Countries (
    CountryId INT PRIMARY KEY,
    CountryCode NVARCHAR(2) NOT NULL UNIQUE, -- 'ZA', 'CA', 'DE'
    CountryName NVARCHAR(100) NOT NULL,
    DefaultCurrency NVARCHAR(3) NOT NULL, -- 'ZAR', 'CAD', 'EUR'
    VATRate DECIMAL(5,2) NULL
);

CREATE TABLE Sites (
    SiteId INT PRIMARY KEY,
    CountryId INT NOT NULL,
    -- Other site fields...
    CONSTRAINT FK_Sites_Country FOREIGN KEY (CountryId) REFERENCES Countries(CountryId)
);
```

**Multi-Currency Support**:
```csharp
public class Price
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } // 'ZAR', 'CAD', 'EUR'

    public decimal ConvertTo(string targetCurrency, decimal exchangeRate)
    {
        return Amount * exchangeRate;
    }
}
```

**Multi-Language Support**:
```csharp
// Resource files: Resources/en-US.resx, Resources/af-ZA.resx, Resources/fr-CA.resx

public class LocalizationService
{
    public string GetString(string key, string culture)
    {
        var resourceManager = new ResourceManager($"Resources.{culture}", Assembly.GetExecutingAssembly());
        return resourceManager.GetString(key);
    }
}

// Usage
var welcomeMessage = _localizationService.GetString("WelcomeMessage", "af-ZA"); // Afrikaans
```

### 14.2 Country-Specific Modules

**Module Registry**:
```csharp
public class ModuleRegistry
{
    private readonly Dictionary<string, List<Type>> _countryModules = new()
    {
        { "ZA", new List<Type> { typeof(SAHPRAComplianceModule), typeof(DALRRDReportingModule) } },
        { "CA", new List<Type> { typeof(HealthCanadaComplianceModule), typeof(CTSReportingModule) } },
        { "DE", new List<Type> { typeof(BfArMComplianceModule), typeof(EuPEAReportingModule) } }
    };

    public IEnumerable<IComplianceModule> GetModulesForCountry(string countryCode)
    {
        if (!_countryModules.ContainsKey(countryCode))
            throw new Exception($"No modules registered for country: {countryCode}");

        return _countryModules[countryCode]
            .Select(t => (IComplianceModule)Activator.CreateInstance(t))
            .ToList();
    }
}
```

---

## APPENDIX A: COMPLIANCE CHECKLIST

**PoC Compliance Checklist** (Must Have):
- [x] Batch number tracking
- [x] Serial number tracking
- [x] Movement tracking (all stock changes)
- [x] Soft delete (7-year retention)
- [x] Audit trails (CreatedBy, CreatedDate)
- [ ] SAHPRA monthly report automation
- [ ] DALRRD quarterly report automation
- [ ] Customer ID verification (manual PoC, automated post-PoC)
- [ ] Daily purchase limit tracking (local)
- [ ] License expiry alerts

**Post-PoC Compliance Enhancements**:
- [ ] Centralized tracking API integration (government database)
- [ ] Automated Home Affairs ID verification
- [ ] Batch recall workflow
- [ ] POPIA data subject request portal
- [ ] Compliance dashboard (license status, report deadlines)

---

## APPENDIX B: REGULATORY CONTACTS

**SAHPRA**:
- Website: https://www.sahpra.org.za
- Email: registrations@sahpra.org.za
- Phone: +27 12 842 0000

**DALRRD**:
- Website: https://www.dalrrd.gov.za
- Email: callcentre@dalrrd.gov.za
- Phone: +27 12 319 6000

**Information Regulator (POPIA)**:
- Website: https://www.justice.gov.za/inforeg/
- Email: inforeg@justice.gov.za
- Phone: +27 10 023 5200

**SARS**:
- Website: https://www.sars.gov.za
- Email: Via eFiling system
- Phone: 0800 00 7277

---

**END OF COMPLIANCE & FUTURE-PROOFING SPECIFICATION**
