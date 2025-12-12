# 08 - LICENSING MODEL SPECIFICATION

**Document Version**: 1.0
**Last Updated**: 2025-12-11
**Status**: Future Commercial Strategy (Post-PoC)
**Related Documents**:
- [00-MAIN-SPECIFICATION.md](00-MAIN-SPECIFICATION.md) - Central specification index
- [01-MODULE-DESCRIPTIONS.md](01-MODULE-DESCRIPTIONS.md) - All module overviews
- [10-GLOSSARY.md](10-GLOSSARY.md) - Terminology reference

---

## DOCUMENT PURPOSE

This document defines the **commercial licensing model** for Project420 as a **Software-as-a-Service (SaaS)** platform for the South African cannabis industry. It outlines:

- **Module-based licensing** (pay for what you use)
- **Tiered pricing** (Starter, Professional, Enterprise)
- **User-based licensing** (concurrent user limits)
- **Feature gates** (which features require which license tier)
- **Compliance costs** (SAHPRA, DALRRD reporting)
- **Support tiers** (basic, premium, white-glove)

**Status**: This is a **forward-looking document** for post-PoC commercialization. The PoC will be **fully unlicensed** (all modules enabled) for development and testing purposes.

**Target Audience**:
- Business stakeholders (pricing strategy)
- Sales & Marketing (value proposition)
- Product Management (feature prioritization)
- Development team (license enforcement implementation)

---

## TABLE OF CONTENTS

1. [Executive Summary](#1-executive-summary)
2. [Licensing Philosophy](#2-licensing-philosophy)
3. [Module-Based Licensing](#3-module-based-licensing)
4. [Tiered Pricing Model](#4-tiered-pricing-model)
5. [User-Based Licensing](#5-user-based-licensing)
6. [Feature Gates](#6-feature-gates)
7. [Add-On Services](#7-add-on-services)
8. [Compliance Costs](#8-compliance-costs)
9. [Support Tiers](#9-support-tiers)
10. [License Enforcement](#10-license-enforcement)
11. [Pricing Examples](#11-pricing-examples)
12. [Competitive Positioning](#12-competitive-positioning)
13. [Future Expansion](#13-future-expansion)

---

## 1. EXECUTIVE SUMMARY

### 1.1 Licensing Overview

**Project420 Licensing Model**: **Modular SaaS with Tiered Pricing**

**Core Principles**:
1. **Pay for What You Use**: Customers only license modules they need
2. **Scalable**: Pricing scales with business size (users, transactions, locations)
3. **Transparent**: No hidden fees, clear pricing structure
4. **Compliance-Inclusive**: Regulatory reporting included in all tiers
5. **South African Focus**: Pricing optimized for SA market (ZAR pricing, local support)

**Revenue Model**:
- **Monthly Recurring Revenue (MRR)**: Predictable subscription income
- **Annual Contracts**: Discounts for annual commitments (2 months free)
- **Enterprise Custom**: Tailored pricing for large operators (10+ locations)

### 1.2 Target Market Segments

**Segment 1: Small Dispensaries** (Starter Tier)
- 1-2 retail locations
- 1-5 users (cashiers, managers)
- Monthly transaction volume: <1,000 sales
- **Revenue Potential**: R2,500 - R5,000/month

**Segment 2: Mid-Size Operators** (Professional Tier)
- 3-5 locations (retail + warehouse)
- 6-15 users
- Light production (pre-rolls only)
- Monthly transaction volume: 1,000 - 5,000 sales
- **Revenue Potential**: R10,000 - R25,000/month

**Segment 3: Vertically Integrated Enterprises** (Enterprise Tier)
- 5+ locations
- 15+ users
- Full production (cultivation → manufacturing → retail)
- Monthly transaction volume: 5,000+ sales
- **Revenue Potential**: R50,000+ /month (custom pricing)

### 1.3 Pricing Philosophy

**Fair Value Pricing**:
- Pricing reflects **business value delivered**, not arbitrary feature lists
- Small businesses pay less; large enterprises pay more
- No price gouging; competitive with alternatives

**Price Anchoring**:
- **Starter**: R2,500/month (entry point for small dispensaries)
- **Professional**: R12,500/month (sweet spot for mid-market)
- **Enterprise**: R50,000+/month (custom, includes white-glove support)

**Discounts**:
- **Annual Contract**: 2 months free (16.7% discount)
- **Multi-Location**: 10% off per additional location after 3rd
- **Early Adopter**: 25% off for first 12 months (limited time)

---

## 2. LICENSING PHILOSOPHY

### 2.1 Module-Based Licensing

**Principle**: Customers select and pay for only the modules they need.

**Module Categories**:
1. **Core Modules** (Always Included):
   - Inventory Management (SOH tracking, batch traceability)
   - Movement Architecture (transaction tracking)
   - Reporting (basic sales reports, compliance reports)

2. **Optional Modules** (Add-On Pricing):
   - Retail POS (point-of-sale for retail stores)
   - Purchasing (supplier management, GRVs, POs)
   - Production (pre-roll assembly, manufacturing)
   - Cultivation (plant tracking, harvest management) - Future

3. **Future Modules** (Roadmap):
   - Wholesale (B2B sales, distributor management)
   - E-Commerce (online store integration)
   - Loyalty Program (customer rewards, promotions)
   - Advanced Analytics (BI dashboards, predictive analytics)

**Example**:
- **Dispensary-Only**: Core + Retail POS (no production/cultivation needed)
- **Vertically Integrated**: Core + Retail POS + Purchasing + Production + Cultivation

### 2.2 Tiered Pricing vs. Module Pricing

**Hybrid Model**: Combine tiers + modules for maximum flexibility.

**Tiers Define**:
- Number of users
- Number of locations
- Transaction volume limits
- Support level
- Advanced features (e.g., multi-currency, API access)

**Modules Define**:
- Which functional areas are enabled
- Industry-specific features (retail, production, cultivation)

**Example**:
- **Starter Tier + Retail POS Module**: R2,500/month (5 users, 1 location)
- **Professional Tier + Retail POS + Production Modules**: R12,500/month (15 users, 5 locations)

---

## 3. MODULE-BASED LICENSING

### 3.1 Module Pricing

| Module | Included in Core? | Add-On Price (Monthly) | Target Customer |
|--------|-------------------|------------------------|-----------------|
| **Core (Inventory + Movements)** | ✅ Always | R0 (base) | All customers |
| **Retail POS** | ❌ Optional | R1,500/location | Dispensaries, retail stores |
| **Purchasing** | ❌ Optional | R800/location | Warehouses, retail chains |
| **Production (Retail)** | ❌ Optional | R1,200 | Pre-roll assembly |
| **Production (Manufacturing)** | ❌ Optional | R2,500 | Concentrates, edibles, oils |
| **Cultivation** | ❌ Optional | R3,000 | Cultivation facilities |
| **Wholesale** | ❌ Optional | R2,000 | Distributors, B2B |
| **E-Commerce** | ❌ Optional | R1,500 | Online retail |
| **Advanced Analytics** | ❌ Optional | R3,500 | Data-driven businesses |

### 3.2 Module Bundles

**Bundle 1: Retail Essentials** (Most Popular)
- Core + Retail POS + Purchasing
- **Price**: R3,000/month (save R300 vs. à la carte)
- **Target**: Small dispensaries with basic supplier management

**Bundle 2: Retail + Production**
- Core + Retail POS + Purchasing + Production (Retail)
- **Price**: R5,500/month (save R1,000 vs. à la carte)
- **Target**: Dispensaries producing their own pre-rolls

**Bundle 3: Full Vertical Integration**
- Core + Retail POS + Purchasing + Production (Both) + Cultivation
- **Price**: R12,000/month (save R2,800 vs. à la carte)
- **Target**: Seed-to-sale operations

### 3.3 Module Dependencies

**Dependency Rules**:
- **Retail POS** → Requires Core (Inventory)
- **Production** → Requires Core + Purchasing (raw material tracking)
- **Cultivation** → Requires Core + Production (harvest → processing)
- **Wholesale** → Requires Core + Inventory (B2B stock allocation)

**Enforcement**:
```csharp
public bool CanEnableModule(ModuleType module, License currentLicense)
{
    switch (module)
    {
        case ModuleType.RetailPOS:
            return currentLicense.HasModule(ModuleType.Core);

        case ModuleType.Production:
            return currentLicense.HasModule(ModuleType.Core) &&
                   currentLicense.HasModule(ModuleType.Purchasing);

        case ModuleType.Cultivation:
            return currentLicense.HasModule(ModuleType.Core) &&
                   currentLicense.HasModule(ModuleType.Production);

        default:
            return true;
    }
}
```

---

## 4. TIERED PRICING MODEL

### 4.1 Tier Definitions

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **Price (Base)** | R2,500/month | R12,500/month | Custom (R50,000+) |
| **Users** | 5 users | 15 users | Unlimited |
| **Locations** | 1 location | 5 locations | Unlimited |
| **Transaction Volume** | 1,000/month | 10,000/month | Unlimited |
| **Modules Included** | Core + 1 module | Core + 3 modules | All modules |
| **Support** | Email (24-48hr) | Email + Phone (4hr) | Dedicated account manager |
| **Compliance Reporting** | Basic (SAHPRA) | Full (SAHPRA + DALRRD) | Full + Custom |
| **API Access** | ❌ No | ✅ Yes (rate-limited) | ✅ Yes (unlimited) |
| **White-Label** | ❌ No | ❌ No | ✅ Yes |
| **Custom Development** | ❌ No | ❌ No | ✅ Yes (billable) |
| **SLA Uptime** | 99.0% | 99.5% | 99.9% |
| **Data Export** | CSV | CSV + Excel | CSV + Excel + API |
| **Training** | Self-service docs | 2 hours onboarding | Unlimited training |

### 4.2 Tier Selection Logic

**Starter Tier** (Best for):
- Single-location dispensaries
- New businesses (first 6-12 months)
- Testing Project420 before full commitment
- **Revenue**: <R500k/month

**Professional Tier** (Best for):
- Multi-location retail chains
- Businesses with light production (pre-rolls)
- Growing businesses (R500k - R5M/month revenue)
- **Revenue**: R500k - R5M/month

**Enterprise Tier** (Best for):
- Vertically integrated operations (cultivation → retail)
- Large-scale manufacturers (concentrates, edibles)
- Businesses requiring customization
- **Revenue**: R5M+/month

### 4.3 Tier Migration

**Upgrade Path**:
- Starter → Professional: Automatic when limits exceeded (users, locations, transactions)
- Professional → Enterprise: Sales-led (custom negotiation)

**Downgrade Path**:
- Customers can downgrade at contract renewal
- Grace period: 30 days to reduce users/locations if over new tier limit

**Prorated Billing**:
- Mid-month upgrades: Prorated charge for remaining days
- Mid-month downgrades: Credit applied to next invoice

---

## 5. USER-BASED LICENSING

### 5.1 User Types & Pricing

**Named Users**:
- **Definition**: Specific individuals with login credentials
- **Pricing**: Included in tier limits (5 users Starter, 15 users Professional)
- **Overage**: R200/user/month beyond tier limit

**Concurrent Users**:
- **Definition**: Maximum simultaneous logins (not applicable for PoC)
- **Pricing**: Future model for very large enterprises
- **Example**: 100 named users, 20 concurrent = cheaper than 100 named

**Role-Based Licensing** (No Extra Cost):
- Admin, Manager, Cashier, Production Worker
- All roles included in user count
- No "power user" premium (simplified pricing)

### 5.2 User Overage Handling

**Scenario**: Starter tier customer (5 users) adds 6th user.

**Options**:
1. **Automatic Upgrade**: Upgrade to Professional tier (if within budget)
2. **Add-On User**: Pay R200/user/month for 6th user (cheaper if only need 1-2 extra)
3. **Deactivate User**: Remove an existing user to stay within limit

**Enforcement**:
```csharp
public bool CanAddUser(License license)
{
    var activeUsers = _userRepo.GetActiveUserCount(license.TenantId);

    if (activeUsers >= license.MaxUsers)
    {
        // Option 1: Suggest upgrade
        if (license.Tier == LicenseTier.Starter)
        {
            SuggestUpgradeToProfessional();
            return false;
        }

        // Option 2: Offer add-on user
        OfferAddOnUser(cost: 200);
        return false;
    }

    return true;
}
```

---

## 6. FEATURE GATES

### 6.1 Feature Gate Matrix

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **Basic Inventory** | ✅ | ✅ | ✅ |
| **Batch Tracking** | ✅ | ✅ | ✅ |
| **Serial Number Tracking** | ❌ | ✅ | ✅ |
| **Multi-Location** | ❌ (1 only) | ✅ (up to 5) | ✅ (unlimited) |
| **Stock Transfers** | ❌ | ✅ | ✅ |
| **Cycle Counting** | ❌ | ✅ | ✅ |
| **Retail POS** | ✅ (add-on) | ✅ (add-on) | ✅ (included) |
| **Multi-Payment** | ✅ | ✅ | ✅ |
| **Customer Loyalty** | ❌ | ✅ (basic) | ✅ (advanced) |
| **Purchasing Module** | ✅ (add-on) | ✅ (add-on) | ✅ (included) |
| **Supplier Portal** | ❌ | ❌ | ✅ |
| **Production (Retail)** | ✅ (add-on) | ✅ (add-on) | ✅ (included) |
| **Production (Manufacturing)** | ❌ | ✅ (add-on) | ✅ (included) |
| **Cultivation** | ❌ | ❌ | ✅ (add-on) |
| **API Access** | ❌ | ✅ (limited) | ✅ (full) |
| **Custom Reports** | ❌ | ✅ (5 reports) | ✅ (unlimited) |
| **White-Label Branding** | ❌ | ❌ | ✅ |
| **Multi-Currency** | ❌ | ❌ | ✅ |
| **Multi-Language** | ❌ | ❌ | ✅ |

### 6.2 Feature Gate Implementation

**Code Example**:
```csharp
public async Task<bool> CanAccessFeatureAsync(string tenantId, Feature feature)
{
    var license = await _licenseRepo.GetByTenantIdAsync(tenantId);

    switch (feature)
    {
        case Feature.SerialNumberTracking:
            return license.Tier >= LicenseTier.Professional;

        case Feature.MultiLocation:
            return license.Tier >= LicenseTier.Professional;

        case Feature.StockTransfers:
            return license.Tier >= LicenseTier.Professional;

        case Feature.APIAccess:
            return license.Tier >= LicenseTier.Professional;

        case Feature.WhiteLabel:
            return license.Tier == LicenseTier.Enterprise;

        case Feature.SupplierPortal:
            return license.Tier == LicenseTier.Enterprise;

        default:
            return true; // Feature available to all tiers
    }
}

// Usage in controller
[HttpGet("serial-numbers")]
public async Task<IActionResult> GetSerialNumbers()
{
    var tenantId = User.GetTenantId();

    if (!await _licenseService.CanAccessFeatureAsync(tenantId, Feature.SerialNumberTracking))
    {
        return Forbidden("Serial number tracking requires Professional tier or higher. Upgrade now!");
    }

    // Feature logic...
}
```

---

## 7. ADD-ON SERVICES

### 7.1 Service Add-Ons

**Data Migration** (One-Time Fee)
- **Price**: R10,000 - R50,000 (depends on data volume)
- **Includes**: Import from spreadsheets, legacy systems, competitor platforms
- **Target**: Businesses switching from manual processes or competitors

**Custom Development** (Hourly Billable)
- **Price**: R1,500/hour (Enterprise customers only)
- **Includes**: Custom reports, integrations, workflow automation
- **Commitment**: Minimum 20 hours

**Training & Onboarding** (Per Session)
- **Price**: R2,500/session (4 hours)
- **Includes**: On-site or virtual training for staff
- **Target**: Businesses with high staff turnover

**Compliance Audit Support** (Annual Fee)
- **Price**: R15,000/year
- **Includes**: Quarterly compliance report review, SAHPRA/DALRRD audit prep
- **Target**: Risk-averse businesses, large operators

### 7.2 Integration Add-Ons

**Accounting Integration** (Monthly Fee)
- **Price**: R500/month
- **Integrations**: Xero, QuickBooks, Sage
- **Includes**: Automatic sync of sales, purchases, payments

**Payment Gateway Integration** (Transaction Fee)
- **Price**: 0.5% per transaction (on top of gateway fees)
- **Gateways**: Yoco, PayGate, Peach Payments
- **Alternative**: Free integration if customer uses preferred partner gateway

**E-Commerce Platform Integration** (Monthly Fee)
- **Price**: R1,000/month
- **Platforms**: WooCommerce, Shopify, custom
- **Includes**: Real-time inventory sync, order fulfillment

---

## 8. COMPLIANCE COSTS

### 8.1 Regulatory Reporting (Included)

**All Tiers Include**:
- SAHPRA monthly stock reports (automated generation)
- DALRRD quarterly traceability reports
- SARS VAT reporting (sales summaries)

**Enterprise Tier Adds**:
- Custom compliance reports (tailored to specific license conditions)
- Audit trail exports (pre-formatted for inspectors)
- Batch recall management (proactive alerts)

### 8.2 Compliance Add-Ons

**Enhanced Compliance Package** (R3,000/month)
- Real-time compliance alerts (low stock, expiring batches, license renewals)
- Pre-inspection readiness checks (automated compliance score)
- Compliance consultant hotline (30 min/month consultation)

**Audit Trail Storage** (Beyond 7 Years)
- **Standard**: 7 years (POPIA requirement) - Included
- **Extended**: 10+ years - R500/year per GB

---

## 9. SUPPORT TIERS

### 9.1 Support Levels

| Support Feature | Starter | Professional | Enterprise |
|----------------|---------|--------------|------------|
| **Channel** | Email only | Email + Phone | Dedicated Slack/Teams |
| **Response Time** | 24-48 hours | 4 hours | 1 hour |
| **Availability** | Business hours | Extended hours (8am-8pm) | 24/7 |
| **Onboarding** | Self-service docs | 2 hours virtual | Unlimited + on-site |
| **Training** | Video tutorials | Quarterly webinars | Custom workshops |
| **Account Manager** | ❌ No | ❌ No | ✅ Yes |
| **Priority Bug Fixes** | ❌ No | ✅ Yes | ✅ Yes + SLA |
| **Feature Requests** | Community voting | Direct submission | Roadmap influence |

### 9.2 Premium Support Add-On

**Premium Support Upgrade** (R5,000/month)
- Available to Starter and Professional tiers
- Unlocks: 1-hour response time, phone support, priority bug fixes
- **Value**: Get Enterprise-level support without full Enterprise cost

---

## 10. LICENSE ENFORCEMENT

### 10.1 License Validation

**License Check Points**:
1. **Login**: Validate license on every user login
2. **Feature Access**: Check license before enabling gated features
3. **Transaction Limits**: Monitor monthly transaction count
4. **User Limits**: Prevent adding users beyond tier limit
5. **Location Limits**: Prevent creating sites beyond tier limit

**Grace Period**:
- **7 days**: Soft warning (email notification, in-app banner)
- **14 days**: Hard warning (feature lockout warnings)
- **30 days**: License suspension (read-only mode, no new transactions)

### 10.2 License Management API

**License Entity**:
```csharp
public class License
{
    public int LicenseId { get; set; }
    public string TenantId { get; set; }
    public LicenseTier Tier { get; set; } // Starter, Professional, Enterprise
    public DateTime StartDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public LicenseStatus Status { get; set; } // Active, Suspended, Expired

    // Limits
    public int MaxUsers { get; set; }
    public int MaxLocations { get; set; }
    public int MaxTransactionsPerMonth { get; set; }

    // Modules
    public List<ModuleType> EnabledModules { get; set; }

    // Usage tracking
    public int CurrentUsers { get; set; }
    public int CurrentLocations { get; set; }
    public int CurrentMonthTransactions { get; set; }
}

public enum LicenseTier
{
    Starter,
    Professional,
    Enterprise
}

public enum LicenseStatus
{
    Active,
    GracePeriod,
    Suspended,
    Expired
}
```

**Validation Service**:
```csharp
public class LicenseValidationService
{
    public async Task<LicenseValidationResult> ValidateLicenseAsync(string tenantId)
    {
        var license = await _licenseRepo.GetByTenantIdAsync(tenantId);

        if (license == null)
            return LicenseValidationResult.NoLicense();

        if (license.ExpiryDate < DateTime.UtcNow)
            return LicenseValidationResult.Expired();

        if (license.Status == LicenseStatus.Suspended)
            return LicenseValidationResult.Suspended();

        // Check usage limits
        if (license.CurrentUsers > license.MaxUsers)
            return LicenseValidationResult.UserLimitExceeded();

        if (license.CurrentLocations > license.MaxLocations)
            return LicenseValidationResult.LocationLimitExceeded();

        if (license.CurrentMonthTransactions > license.MaxTransactionsPerMonth)
            return LicenseValidationResult.TransactionLimitExceeded();

        return LicenseValidationResult.Valid();
    }
}
```

---

## 11. PRICING EXAMPLES

### 11.1 Example 1: Small Dispensary (Starter Tier)

**Business Profile**:
- 1 retail location (Johannesburg)
- 3 users (owner, 2 cashiers)
- 500 sales/month
- Modules needed: Core + Retail POS

**Pricing**:
- Base (Starter Tier): R2,500/month
- Retail POS Module: Included in tier
- **Total**: **R2,500/month** (R30,000/year)

**Annual Contract Discount**:
- R30,000 - R5,000 (2 months free) = **R25,000/year**
- **Monthly Equivalent**: R2,083/month

### 11.2 Example 2: Multi-Location Retailer (Professional Tier)

**Business Profile**:
- 3 retail locations (JHB, CPT, DBN)
- 10 users (3 managers, 7 cashiers)
- 3,000 sales/month
- Modules needed: Core + Retail POS + Purchasing + Production (Retail)

**Pricing**:
- Base (Professional Tier): R12,500/month
- Retail POS: Included in tier (3 modules included)
- Purchasing: Included
- Production (Retail): Included
- **Total**: **R12,500/month** (R150,000/year)

**Annual Contract Discount**:
- R150,000 - R25,000 (2 months free) = **R125,000/year**
- **Monthly Equivalent**: R10,417/month

### 11.3 Example 3: Vertically Integrated Enterprise

**Business Profile**:
- 5 locations (2 retail, 1 warehouse, 1 production facility, 1 cultivation farm)
- 25 users
- 10,000 sales/month
- Modules needed: All modules (Core + Retail + Purchasing + Production + Cultivation)

**Pricing**:
- Base (Enterprise Tier): R50,000/month (custom negotiation)
- All modules: Included
- Dedicated account manager: Included
- **Total**: **R50,000/month** (R600,000/year)

**Annual Contract Discount**:
- R600,000 - R100,000 (2 months free) = **R500,000/year**
- **Monthly Equivalent**: R41,667/month

**Add-Ons**:
- Custom development (20 hours): R30,000 (one-time)
- Enhanced compliance package: R3,000/month
- **Total Year 1**: R500,000 + R30,000 + (R3,000 × 12) = **R566,000**

---

## 12. COMPETITIVE POSITIONING

### 12.1 Competitor Comparison

| Feature | Project420 | Competitor A (Generic ERP) | Competitor B (Cannabis-Specific) |
|---------|------------|---------------------------|----------------------------------|
| **Pricing (Starter)** | R2,500/month | R5,000/month | R3,500/month |
| **Cannabis Compliance** | ✅ Built-in | ❌ Manual | ✅ Built-in |
| **Seed-to-Sale Traceability** | ✅ Yes | ❌ No | ✅ Yes |
| **Multi-Location** | ✅ Professional+ | ✅ All tiers | ✅ All tiers (expensive) |
| **SA Regulatory Compliance** | ✅ SAHPRA + DALRRD | ❌ Generic | ❌ US-focused |
| **Local Support** | ✅ SA-based | ❌ International | ❌ US-based |
| **ZAR Pricing** | ✅ Yes | ❌ USD/EUR | ❌ USD |
| **API Access** | ✅ Professional+ | ✅ All tiers | ✅ Enterprise only |

**Value Proposition**:
> "Project420 is the only South African-built, cannabis-specific inventory and retail platform with built-in SAHPRA and DALRRD compliance, priced affordably for local dispensaries."

### 12.2 Competitive Advantages

**1. South African Focus**:
- Local support (no time zone issues)
- ZAR pricing (no forex risk)
- SA compliance baked in (not bolted on)

**2. Cannabis-Specific**:
- Seed-to-sale traceability (not possible with generic ERP)
- Batch and serial number tracking (critical for compliance)
- Cannabis-specific workflows (not "configured" from generic)

**3. Affordable for SMEs**:
- Starter tier at R2,500/month (vs. R5,000+ for competitors)
- No hidden fees (compliance included, not charged extra)
- Transparent pricing (no "contact sales" gatekeeping)

**4. Modular & Scalable**:
- Pay only for modules you need (not forced into "all-in-one" pricing)
- Easy upgrade path (Starter → Professional → Enterprise)

---

## 13. FUTURE EXPANSION

### 13.1 Geographic Expansion

**Phase 1: South Africa** (Year 1-2)
- Focus on SA market (SAHPRA, DALRRD compliance)
- ZAR pricing, local support

**Phase 2: Southern Africa** (Year 3-4)
- Expand to: Lesotho, eSwatini, Zambia, Zimbabwe
- Add multi-currency support
- Localized compliance modules per country

**Phase 3: International** (Year 5+)
- Target: Canada, Europe, Australia (mature cannabis markets)
- Partner with local compliance consultants
- White-label partnerships with regional distributors

### 13.2 New Module Roadmap

**2026**:
- **Wholesale Module** (B2B sales, distributor management)
- **E-Commerce Module** (online store integration, delivery tracking)
- **Advanced Analytics** (BI dashboards, predictive demand forecasting)

**2027**:
- **Loyalty Program** (customer rewards, promotions, referral tracking)
- **Delivery Management** (route optimization, driver tracking)
- **Supplier Portal** (self-service ordering, shipment tracking)

**2028**:
- **IoT Integration** (RFID tracking, environmental monitoring for cultivation)
- **AI-Powered Inventory** (demand forecasting, automated reordering)
- **Blockchain Traceability** (immutable seed-to-sale records)

### 13.3 Pricing Evolution

**Year 1-2: Penetration Pricing**
- Aggressive pricing to capture market share
- Early adopter discounts (25% off first year)
- Goal: 100 customers in 12 months

**Year 3-4: Value Pricing**
- Gradual price increases (5-10% annually)
- Justify with new features, improved service
- Goal: R10M ARR (Annual Recurring Revenue)

**Year 5+: Premium Positioning**
- Market leader pricing (10-20% premium over competitors)
- Justify with brand reputation, comprehensive features, proven compliance
- Goal: R50M+ ARR

---

## APPENDIX A: LICENSE TIER COMPARISON TABLE

| Feature | Starter | Professional | Enterprise |
|---------|---------|--------------|------------|
| **Base Price** | R2,500/mo | R12,500/mo | R50,000+/mo |
| **Users** | 5 | 15 | Unlimited |
| **Locations** | 1 | 5 | Unlimited |
| **Transactions/Month** | 1,000 | 10,000 | Unlimited |
| **Modules Included** | Core + 1 | Core + 3 | All |
| **Support Response** | 24-48hr | 4hr | 1hr |
| **Compliance Reporting** | Basic | Full | Full + Custom |
| **API Access** | No | Yes (limited) | Yes (full) |
| **White-Label** | No | No | Yes |
| **Custom Development** | No | No | Yes |
| **SLA Uptime** | 99.0% | 99.5% | 99.9% |

---

## APPENDIX B: MODULE ADD-ON PRICING

| Module | Monthly Price | Annual Price | Target Customer |
|--------|---------------|--------------|-----------------|
| Retail POS | R1,500/location | R15,000 | Dispensaries |
| Purchasing | R800/location | R8,000 | Multi-location |
| Production (Retail) | R1,200 | R12,000 | Pre-roll producers |
| Production (Manufacturing) | R2,500 | R25,000 | Manufacturers |
| Cultivation | R3,000 | R30,000 | Growers |
| Wholesale | R2,000 | R20,000 | Distributors |
| E-Commerce | R1,500 | R15,000 | Online retailers |
| Advanced Analytics | R3,500 | R35,000 | Data-driven |

---

**END OF LICENSING MODEL SPECIFICATION**
