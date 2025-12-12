# CLAUDE INITIALIZATION & WORKFLOW - PROJECT420

**Last Updated**: 2025-12-12
**Status**: ACTIVE - Phase 7A Complete ✅ | Movement Architecture Foundation Implemented
**Project**: Project420 - Cannabis Management System for South Africa
**Version**: 0.1.0 - Proof of Concept (Phase 7A - Movement Architecture)
**Documentation**: 11 comprehensive specification files in docs/ClaudeExternReferences/PoCHomeStretch/

---

## 🚀 CLAUDE CORE START

### CRITICAL REQUIREMENT: SA CANNABIS COMPLIANCE GUIDES

**MANDATORY REFERENCES** (BOTH documents REQUIRED for ALL work):

1. **`SA_Cannabis_Software_Guide.md`** - Software development & compliance framework
2. **`south-africa-cannabis-cultivation-production-laws-guide.md`** - Cultivation & production legal requirements

**⚠️ ABSOLUTE REQUIREMENT**: These documents are EXTREMELY IMPORTANT and MUST be:
- ✅ **Automatically loaded** at the start of EVERY conversation about Project420
- ✅ **Consulted before** implementing ANY feature (even minor changes)
- ✅ **Referenced alongside** the coding structure template for all development
- ✅ **Verified against** for all compliance-critical code
- ✅ **Cross-checked** when making architectural decisions

**ENFORCEMENT RULE**: Claude MUST NOT proceed with ANY Project420 work without first confirming familiarity with BOTH guides. If unsure, read them again.

**SA_Cannabis_Software_Guide.md** contains:
- Legal framework and compliance requirements
- Tax considerations and future-proofing
- Essential features (must-have)
- POPIA data protection requirements
- Future-proof features
- Business registration requirements
- Technical implementation notes
- API integration points
- Database schema considerations

**south-africa-cannabis-cultivation-production-laws-guide.md** contains:
- Medicinal cannabis licensing (SAHPRA Section 22C)
- Hemp cultivation regulations (DALRRD permits)
- Private personal use cannabis laws
- GMP (Good Manufacturing Practice) standards
- Seed-to-sale traceability requirements
- Laboratory testing and quality control
- Security and storage requirements
- Record keeping and penalties

**COMPLIANCE RULE**: When working on Project420, Claude MUST:
1. ✅ Read and understand **BOTH** compliance guides before implementation
2. ✅ Verify all features comply with SA Cannabis for Private Purposes Act 2024
3. ✅ Ensure SAHPRA Section 22C and DALRRD hemp permit compliance for cultivation/production modules
4. ✅ Implement GMP-compliant record keeping and traceability (seed-to-sale tracking)
5. ✅ Ensure POPIA (data protection) compliance in all data handling
6. ✅ Future-proof for expected 2026-2027 commercial regulations
7. ✅ Implement required tax calculation capabilities
8. ✅ Follow prescribed development phases from the guides

---

### MANDATORY QUICK REFERENCE: POC HOMESTRETCH DOCUMENTATION

**REQUIRED GLOSSARY CHECK** (CRITICAL for terminology consistency):

**Location**: `docs/ClaudeExternReferences/PoCHomeStretch/10-GLOSSARY.md`

**⚠️ ABSOLUTE REQUIREMENT**: The glossary MUST be checked when:
- ✅ **Using any technical term** (SOH, Movement, Batch, Serial Number, Transaction, etc.)
- ✅ **Implementing any feature** (verify terminology matches specification)
- ✅ **Writing documentation** (use consistent terms across all docs)
- ✅ **Discussing architecture** (ensure clear understanding of system concepts)
- ✅ **Communicating with user** (use precise, defined terminology)

**GLOSSARY ENFORCEMENT RULE**:
- If you encounter ANY ambiguous term, CHECK THE GLOSSARY FIRST
- The glossary defines the SINGLE SOURCE OF TRUTH for all Project420 terminology
- Examples: "SOH vs Stock on Hand", "Transaction vs Movement", "Batch vs Serial Number"

**ADDITIONAL POC HOMESTRETCH DOCUMENTATION** (Comprehensive specification suite):

All documents located in: `docs/ClaudeExternReferences/PoCHomeStretch/`

**Core Architectural Documents** (Check when working on related features):
1. **00-MAIN-SPECIFICATION.md** - Central index, 6-week roadmap (Phases 7-12)
2. **01-MODULE-DESCRIPTIONS.md** - All 7 modules detailed (6,000+ lines)
3. **03-MOVEMENT-ARCHITECTURE.md** - Option A architecture (CRITICAL DESIGN DECISION)
4. **02-INVENTORY-MODULE-SPEC.md** - SOH calculation, movement tracking (18,000+ lines)
5. **04-RETAIL-POS-REQUIREMENTS.md** - Transaction workflow, compliance (15,000+ lines)
6. **05-PRODUCTION-MODEL.md** - Single-batch multi-step manufacturing (12,000+ lines)
7. **07-DATABASE-SCHEMA.md** - Complete ERD, 38 tables, indexing strategy (10,000+ lines)

**Implementation Roadmaps**:
- **POC-TODO-ROADMAP.md** - 450+ tasks across Phases 7-12 (9,000+ lines)
- **IMMEDIATE-ACTION-TODO.md** - 7-day actionable plan (Phase 7 focus)
- **PRODUCTION-DAL-VALIDATION.md** - Production module gap analysis

**Specialized Specifications**:
- **06-SERIAL-BATCH-GENERATION.md** - Batch/SN number formats with embedded traceability
- **08-LICENSING-MODEL.md** - Module-based SaaS licensing (commercial strategy)
- **09-COMPLIANCE-FUTURE-PROOFING.md** - SAHPRA/DALRRD/POPIA compliance (11,000+ lines)

**WHEN TO REFERENCE THESE DOCS**:
- **Before starting ANY new feature**: Check relevant specification for requirements
- **When unsure about architecture**: Reference Movement Architecture or Main Specification
- **When implementing compliance**: Reference Compliance & Future-Proofing
- **When designing database changes**: Reference Database Schema
- **For terminology questions**: ALWAYS check Glossary first

**DOCUMENTATION STATISTICS**:
- **Total**: 88,500+ lines across 11 specification files
- **Coverage**: Inventory (90%), Retail POS (85-90%), Production (60-75%), All modules spec'd
- **Key Decisions**: Option A Movement Architecture, Single-Batch Multi-Step, Movement-Based SOH

---

## 📋 INITIALIZATION SEQUENCE

**WORKFLOW TRIGGER**: This configuration automatically activates upon Claude startup:

1. **Claude Initialization** → Reads this CLAUDE.md
2. **SA Guide Loading** → Automatically loads and references **BOTH** compliance guides:
   - `SA_Cannabis_Software_Guide.md` (software development framework)
   - `south-africa-cannabis-cultivation-production-laws-guide.md` (legal requirements)
3. **Coding Structure Loading** → Loads `docs/CODING-STRUCTURE.md` for architecture patterns
4. **Compliance Verification** → Verifies current work aligns with legal requirements
5. **Context Assessment** → Determines task complexity and agent needs
6. **Pattern Application** → Applies appropriate workflow patterns from coding structure template
7. **Quality Gates** → Ensures all output meets legal and technical standards

### Automatic Startup Workflow

```
Claude Start
    ↓
Load Project420 CLAUDE.md
    ↓
*** AUTO-LOAD BOTH Compliance Guides ***
├── SA_Cannabis_Software_Guide.md
└── south-africa-cannabis-cultivation-production-laws-guide.md
    ↓
*** AUTO-LOAD Coding Structure Template ***
└── docs/CODING-STRUCTURE.md
    ↓
Verify Compliance Context (SAHPRA + DALRRD + POPIA)
    ↓
Assess Task Complexity → Determine Agent Needs
    ↓
Apply Appropriate Workflow Pattern (from coding structure)
    ↓
Execute with Legal & Technical Quality Gates
```

---

## 🎯 PROJECT OVERVIEW

### Project Context

Project420 is a comprehensive cannabis industry management system designed specifically for the **South African market**. The system must comply with:

- Cannabis for Private Purposes Act 2024 (signed May 28, 2024)
- POPIA (Protection of Personal Information Act)
- SAHPRA (South African Health Products Regulatory Authority) requirements
- DALRRD (Department of Agriculture, Land Reform and Rural Development) hemp regulations
- SARS (South African Revenue Service) tax requirements

### Architecture

**3-Tier Modular Architecture**:
```
┌─────────────────────┐
│  Presentation Layer │  ← Blazor Web, MAUI Mobile
│  (UI.Blazor/Maui)  │
├─────────────────────┤
│ Business Logic Layer│  ← Services, Validation, DTOs
│       (BLL)         │
├─────────────────────┤
│ Data Access Layer   │  ← EF Core, Repositories
│       (DAL)         │
└─────────────────────┘
         ↕
    [Database]
```

### Technology Stack

- **Backend**: .NET 9, EF Core 9, C#
- **Frontend**: Blazor, MAUI (planned)
- **Database**: SQL Server, PostgreSQL, SQLite
- **Testing**: xUnit, Moq, FluentAssertions
- **Validation**: FluentValidation
- **Mapping**: AutoMapper

---

## 🔐 COMPLIANCE REQUIREMENTS (AUTO-ENFORCED)

### Legal Compliance Checklist

When working on ANY feature, verify:

- [ ] Complies with Cannabis for Private Purposes Act 2024
- [ ] POPIA data protection requirements met
- [ ] Age verification (18+) implemented where needed
- [ ] Audit trails for all business operations
- [ ] Possession/cultivation limits enforced (where applicable)
- [ ] Required license/permit tracking (SAHPRA, DALRRD)
- [ ] Tax calculation capabilities (current + future-proof)
- [ ] Data encryption at rest and in transit
- [ ] Secure authentication (OAuth 2.0, JWT)
- [ ] Role-based access control (RBAC)

### Development Phases (From SA Guide)

**Phase 1 (Months 1-3): Core Infrastructure** ← CURRENT POC
- Database architecture (POPIA-compliant) ✅
- Authentication & RBAC
- License/permit management
- Basic inventory tracking
- Audit logging

**Phase 2 (Months 4-6): Compliance Features**
- Seed-to-sale traceability
- SAHPRA/DALRRD reporting
- Lab testing integration
- Tax calculation engine
- Compliance dashboard

**Phase 3 (Months 7-9): Operations**
- Point of Sale system ← Initial focus
- Customer management ← Initial focus
- Financial reporting
- Mobile applications
- Multi-location support

**Phase 4 (Months 10-12): Advanced Features**
- E-commerce preparation (for 2027)
- Cultivation management
- Business intelligence
- API development
- Government integration (when available)

---

## 📁 PROJECT STRUCTURE

```
Project420/
├── CLAUDE.md                           <- THIS FILE (Project config)
├── SA_Cannabis_Software_Guide.md      <- CRITICAL COMPLIANCE REFERENCE
├── docs/
│   ├── PROJECT-STATUS.md              <- Current status & roadmap
│   ├── AGENT-ASSIGNMENTS.md           <- Agent responsibilities
│   └── ARCHITECTURE.md                 <- Technical architecture
│
├── src/
│   ├── Shared/                         <- Shared libraries
│   │   ├── Project420.Shared.Core
│   │   ├── Project420.Shared.Infrastructure
│   │   └── Project420.Shared.Database
│   │
│   └── Modules/
│       ├── Management/                 <- Customer, Product, Pricelist
│       │   ├── Project420.Management.Models
│       │   ├── Project420.Management.DAL
│       │   ├── Project420.Management.BLL
│       │   └── Project420.Management.UI.Blazor
│       │
│       └── Retail/
│           └── POS/                    <- Point of Sale
│               ├── Project420.Retail.POS.Models
│               ├── Project420.Retail.POS.DAL
│               ├── Project420.Retail.POS.BLL
│               └── Project420.Retail.POS.UI.Blazor
│
└── tests/                              <- Unit & Integration tests
    ├── Project420.Management.Tests
    └── Project420.Retail.POS.Tests
```

---

## 🎯 WORKFLOW PATTERNS

### Task Complexity Assessment

**Simple Task** (Direct execution):
- Single file edit
- Documentation update
- Simple query/question
- Code review

**Medium Task** (Todo tracking):
- Multi-file changes (2-4 files)
- Feature addition (single module)
- Bug fix with investigation
- Test implementation

**Complex Task** (Full Architect coordination):
- Large feature (5+ files)
- Multi-module changes
- Compliance-critical features
- Architecture decisions
- Integration with legal requirements

### Agent Deployment Rules

For **Complex Tasks** on Project420:

1. **Architect Agent** - Overall coordination and quality gates
2. **Compliance Agent** - Verifies against SA_Cannabis_Software_Guide.md
3. **Database Agent** - POPIA-compliant data models and migrations
4. **BLL Developer** - Business logic with legal validations
5. **UI/UX Agent** - User interfaces with required verifications
6. **Testing Agent** - Comprehensive testing including compliance scenarios
7. **Security Agent** - POPIA compliance and security audits
8. **Documentation Agent** - Professional documentation

---

## 🔒 SECURITY & DATA PROTECTION (POPIA)

### POPIA Compliance Requirements

**CRITICAL**: Protection of Personal Information Act enforcement since July 1, 2021

**Required Technical Implementation**:
```javascript
// From SA_Cannabis_Software_Guide.md
const POPIA_TECHNICAL = {
  encryption: {
    atRest: 'AES-256',
    inTransit: 'TLS 1.3',
    keyManagement: 'required'
  },
  accessControl: {
    rbac: true,
    leastPrivilege: true,
    mfa: 'recommended'
  },
  auditLogging: {
    allDataAccess: true,
    allModifications: true,
    immutable: true,
    retention: '7 years'
  },
  dataRetention: {
    taxRecords: '5 years',
    employmentRecords: '3 years after termination',
    customerRecords: '5 years after last transaction'
  }
};
```

**Penalties for Non-Compliance**:
- Administrative fine: Up to R10 million
- Serious violations: Up to 10 years imprisonment
- Civil liability: Compensation to affected individuals

---

## 💰 TAX CALCULATION REQUIREMENTS

### Current Tax Framework (2025)

**VAT**: 15% (mandatory if turnover > R1 million)
**Corporate Tax**: 27% standard (progressive for small business)

### Future-Proofing (2026-2027)

Design all financial modules to accommodate:
- Cannabis-specific excise duty (expected)
- Provincial/local levies (possible)
- Cultivation taxes (potential)
- License fees by category

**Implementation**: See `SA_Cannabis_Software_Guide.md` Section 2.2 for detailed tax types

---

## 📊 ESSENTIAL FEATURES (MUST-HAVE)

From SA_Cannabis_Software_Guide.md Section 3:

### 3.1 License and Permit Management
- SAHPRA Section 22C tracking
- DoH Section 22A permit management
- DALRRD hemp permit tracking
- Expiry alerts (90, 60, 30 days)
- Document storage with version control
- Audit trail

### 3.2 Seed-to-Sale Traceability
- Unique identifiers (RFID, barcode, QR)
- Plant tracking (strain, batch, growth stage)
- Processing records (harvest, curing, extraction)
- Lab testing integration (COA management)
- Chain of custody logging
- Waste tracking

### 3.3 Point of Sale (POS) System
- Age verification (18+ minimum)
- Purchase limit enforcement
- VAT + future excise calculation
- Multiple payment methods
- Receipt generation (compliant format)
- Medical patient tracking (Section 21)

### 3.4 Inventory Management
- Real-time stock tracking
- Batch management (FIFO/FEFO)
- Potency tracking (THC/CBD)
- Expiry monitoring
- Storage compliance (temp/humidity)
- Shrinkage alerts

### 3.5 Compliance and Reporting
- SAHPRA reports (production, inventory, sales, destruction)
- DALRRD reports (cultivation, THC results, harvest)
- SARS reports (VAT201, excise, income)
- GMP documentation
- Immutable audit logs (7-year retention)
- Compliance dashboard

### 3.6 Financial Reporting
- Sales reports (daily, weekly, monthly, quarterly, annual)
- Tax reports (VAT output/input, excise, liability)
- Cost tracking (COGS, labor, overhead, margins)
- Accounting integration (Sage, Xero, QuickBooks, Pastel)
- B-BBEE reporting

---

## 🚀 FUTURE-PROOF FEATURES (PHASE 4+)

Design architecture to easily add (when regulations permit):

### E-Commerce Module (Expected ~2027)
- Online ordering
- Delivery management with age verification
- Real-time inventory sync
- Loyalty programs

### Advanced Cultivation Management
- IoT sensor integration
- Environmental controls automation
- Fertigation tracking
- Yield prediction (ML)
- Genetic tracking

### Cannabis Club Features (Potential)
- Member management
- Cultivation allocation
- Cooperative ownership
- Event management

**Full details**: `SA_Cannabis_Software_Guide.md` Section 5

---

## 🎓 DEVELOPMENT STANDARDS

### Code Quality Requirements

1. **SOLID Principles**: Mandatory for all code
2. **Repository Pattern**: All data access through repositories
3. **Dependency Injection**: Service lifetime management
4. **Async/Await**: All I/O operations async
5. **XML Documentation**: All public APIs documented
6. **Unit Test Coverage**: Target 90%+
7. **Error Handling**: Comprehensive with logging
8. **Audit Trails**: All business operations logged

### Compliance-Specific Standards

1. **Data Validation**: FluentValidation for all inputs
2. **POPIA Compliance**: Encrypted PII, consent tracking, audit logs
3. **Legal Limits**: Enforce possession/cultivation limits in code
4. **Age Verification**: Required for all user-facing features
5. **Audit Logging**: Immutable, timestamped, user-attributed
6. **Tax Calculations**: Accurate, auditable, future-compatible

---

## 📖 KEY REFERENCE DOCUMENTS

### Project-Specific (CRITICAL)
- **SA Cannabis Software Guide**: `SA_Cannabis_Software_Guide.md` ⚠️ MANDATORY REFERENCE
- **SA Cultivation & Production Laws**: `south-africa-cannabis-cultivation-production-laws-guide.md` ⚠️ MANDATORY REFERENCE
- **Glossary (Terminology)**: `docs/ClaudeExternReferences/PoCHomeStretch/10-GLOSSARY.md` ⚠️ MANDATORY FOR TERMINOLOGY
- **Coding Structure Template**: `docs/CODING-STRUCTURE.md` ⚠️ MANDATORY FOR ALL DEVELOPMENT
- **Coding Standards**: `docs/CODING-STANDARDS.md` - Complete architecture patterns (if exists)
- **Project Status**: `docs/PROJECT-STATUS.md`
- **UI Standards**: `docs/PROJECT_STANDARDS.md` - Blazor UI guidelines
- **Agent Assignments**: `docs/AGENT-ASSIGNMENTS.md`
- **Technical Architecture**: `docs/ARCHITECTURE.md`

### PoCHomeStretch Specifications (COMPREHENSIVE)
**Location**: `docs/ClaudeExternReferences/PoCHomeStretch/`
- **00-MAIN-SPECIFICATION.md** - Central index, roadmap (8,500+ lines)
- **01-MODULE-DESCRIPTIONS.md** - All 7 modules (6,000+ lines)
- **02-INVENTORY-MODULE-SPEC.md** - SOH, movements (18,000+ lines)
- **03-MOVEMENT-ARCHITECTURE.md** - Option A design (5,500+ lines) ⚠️ CRITICAL
- **04-RETAIL-POS-REQUIREMENTS.md** - Transaction workflow (15,000+ lines)
- **05-PRODUCTION-MODEL.md** - Single-batch manufacturing (12,000+ lines)
- **06-SERIAL-BATCH-GENERATION.md** - Number formats & traceability
- **07-DATABASE-SCHEMA.md** - Complete ERD (10,000+ lines)
- **08-LICENSING-MODEL.md** - Commercial strategy (9,000+ lines)
- **09-COMPLIANCE-FUTURE-PROOFING.md** - SAHPRA/DALRRD/POPIA (11,000+ lines)
- **10-GLOSSARY.md** - Terminology reference ⚠️ REQUIRED FOR CONSISTENCY
- **POC-TODO-ROADMAP.md** - 450+ tasks, Phases 7-12 (9,000+ lines)
- **IMMEDIATE-ACTION-TODO.md** - 7-day action plan
- **PRODUCTION-DAL-VALIDATION.md** - Production gap analysis

### Central ClaudeCode Hub
- **Location**: `../../ClaudeCode/`
- **Master Coordination**: `../../ClaudeCode/MASTER-COORDINATION.md`
- **Activation Rules**: `../../ClaudeCode/ACTIVATION-RULES.md`
- **Agent Patterns**: `../../ClaudeCode/agents/`
- **Core Standards**: `../../ClaudeCode/core/`
- **Knowledge Base**: `../../ClaudeCode/knowledge/`
- **Templates**: `../../ClaudeCode/templates/`

---

## ✅ STARTUP CHECKLIST FOR PROJECT420

When Claude starts working on Project420, verify:

- [ ] **BOTH** compliance guides have been read and understood:
  - [ ] SA_Cannabis_Software_Guide.md
  - [ ] south-africa-cannabis-cultivation-production-laws-guide.md
- [ ] **GLOSSARY checked for terminology consistency**:
  - [ ] docs/ClaudeExternReferences/PoCHomeStretch/10-GLOSSARY.md
  - [ ] All technical terms match glossary definitions (SOH, Movement, Batch, Transaction, etc.)
- [ ] **Coding Structure Template** loaded and understood:
  - [ ] docs/CODING-STRUCTURE.md
- [ ] **PoCHomeStretch documentation** referenced for task-specific requirements:
  - [ ] Check relevant specification (Inventory, Retail POS, Production, etc.)
  - [ ] Verify architectural decisions (Movement Architecture, Database Schema)
  - [ ] Review roadmap for context (POC-TODO-ROADMAP.md, IMMEDIATE-ACTION-TODO.md)
- [ ] Current task aligns with legal compliance requirements (SAHPRA, DALRRD, POPIA)
- [ ] POPIA data protection requirements considered
- [ ] GMP compliance requirements understood (for cultivation/production modules)
- [ ] Seed-to-sale traceability requirements identified
- [ ] Tax calculation needs identified and future-proofed
- [ ] Age verification requirements understood (where applicable)
- [ ] Audit trail implementation planned
- [ ] Appropriate workflow pattern selected (from coding structure template)
- [ ] Quality gates in place for compliance-critical features
- [ ] Documentation follows professional standards
- [ ] Code follows CODING-STRUCTURE.md patterns, CODING-STANDARDS.md (if exists), and PROJECT_STANDARDS.md (UI)

---

## 🚨 CRITICAL RULES FOR PROJECT420

1. **ALWAYS Check Glossary First** - docs/ClaudeExternReferences/PoCHomeStretch/10-GLOSSARY.md is the SINGLE SOURCE OF TRUTH for all terminology (SOH, Movement, Batch, Transaction, etc.)
2. **ALWAYS Reference BOTH Compliance Guides** - SA_Cannabis_Software_Guide.md AND south-africa-cannabis-cultivation-production-laws-guide.md are mandatory reading
3. **ALWAYS Follow Coding Structure Template** - docs/CODING-STRUCTURE.md defines ALL architecture patterns, folder structure, and development standards
4. **ALWAYS Reference PoCHomeStretch Specs** - 88,500+ lines of specifications for architectural decisions, module requirements, and compliance details
5. **POPIA Compliance First** - Data protection is non-negotiable (R10M penalty / 10 years imprisonment)
6. **GMP Compliance Required** - Cultivation/production modules MUST follow SAHPRA GMP standards
7. **Seed-to-Sale Traceability** - Every plant, batch, and package MUST be tracked (SAHPRA/DALRRD requirement)
8. **Future-Proof Everything** - 2026-2027 commercial regs coming
9. **Audit Everything** - Immutable logs for 7 years minimum (SAHPRA/SARS requirement)
10. **Age Verification** - 18+ required, no exceptions (Cannabis Act 2024)
11. **Tax Accuracy** - Current VAT + future excise capabilities
12. **Legal Limits** - Enforce possession/cultivation limits in code
13. **Security Standards** - AES-256 + TLS 1.3 + RBAC mandatory
14. **Laboratory Testing** - ISO/IEC 17025 accredited labs, COA required for every batch
15. **Test Thoroughly** - Compliance scenarios must be tested
16. **Document Professionally** - Legal liability requires clear documentation
17. **Consistent Architecture** - New modules MUST match existing patterns from CODING-STRUCTURE.md
18. **Terminology Consistency** - Use glossary-defined terms in all code, comments, and documentation

---

## 📞 REGULATORY AUTHORITIES & CONTACTS

From SA_Cannabis_Software_Guide.md Section 10:

| Authority | Website | Purpose | Contact |
|-----------|---------|---------|---------|
| SAHPRA | www.sahpra.org.za | Medical cannabis licensing | enquiries@sahpra.org.za |
| DALRRD | www.dalrrd.gov.za | Hemp permits | Hemp.PIA@dalrrd.gov.za |
| SARS | www.sars.gov.za | Tax compliance | 0800 00 7277 |
| CIPC | www.cipc.co.za | Company registration | 086 100 2472 |
| Information Regulator | www.inforegulator.org.za | POPIA compliance | enquiries@inforegulator.org.za |
| DTIC | www.thedtic.gov.za | Cannabis Master Plan | - |

---

## 🎯 CURRENT PROJECT STATUS

**Phase**: POC - Phase 7A Complete ✅ | Movement Architecture Foundation Implemented 🚀
**Status**: MovementService implemented | 51 tests passing | Ready for Phase 7B (Unified Transaction Architecture)

**✅ Phase 1 Complete (2025-12-02) - Data Models**:
- [x] Solution and project setup
- [x] 3-tier architecture implementation
- [x] Modular folder structure
- [x] **AuditableEntity base class** - POPIA compliance foundation
- [x] **Product entity** - Cannabis compliance (THC/CBD, batch, strain, lab tests)
- [x] **Debtor entity** - Age verification (18+), medical licenses, credit management
- [x] **Pricelist & PricelistItem** - Flexible multi-tier pricing system
- [x] **TransactionHeader & TransactionDetail** - Complete invoice/receipt system
- [x] **Payment entity** - HOW customers pay (separate from Transaction)
- [x] **3 Enumerations** - Type-safe status/type/method enums

**✅ Phase 2 Complete (2025-12-05) - DbContext & Database**:
- [x] Created 3 DbContexts (PosDbContext, ManagementDbContext, SharedDbContext)
- [x] Installed EF Core packages (SqlServer, Tools, Design)
- [x] Configured entity relationships using Fluent API
- [x] Generated migrations and created databases
- [x] Created seed data (10 products, 4 customers, 1 pricelist)
- [x] Reorganized BLL into domain-driven structure

**✅ Phase 3 Part A Complete (2025-12-06) - Shared Services & BLL**:
- [x] **VATCalculationService** - Universal transaction logic (POS, GRV, RTS, Invoices, Credits)
  - Detail-first transaction pattern
  - SA VAT compliance (15% VAT-inclusive pricing)
  - Automatic rounding adjustment
  - 44 unit tests - 100% passing ✅
- [x] **TransactionNumberGeneratorService** - Auto-generate unique transaction numbers
  - Thread-safe implementation
  - 10 transaction types supported
  - Format: TYPE-YYYYMMDD-XXX
- [x] **AuditLogService** - POPIA/Cannabis Act/SARS compliance
  - 7-year audit trail framework
  - Security event logging
  - Data change tracking
- [x] **BLL Gap Closure** - Verified ProductCategoryService & DebtorCategoryService complete
- [x] **Dependency Injection** - All services registered in DI
- [x] **Build Status**: 0 Errors, 0 Warnings

**⏸️ DEFERRED: Advanced Batch & Serial Number System (Future Phase)**:
- **Status**: Design complete, implementation deferred to Phase 4+
- **Documentation**: See `docs/future-features/BATCH-SERIAL-NUMBER-DESIGN.md`
- **Current Implementation**: Basic TransactionNumberGeneratorService (TYPE-YYYYMMDD-XXX format)
- **Future Implementation**:
  - Batch Numbers: `SSTTYYYYMMDDNNNN` (16 digits with Site ID + Type)
  - Full Serial Numbers: `SSSSSTTYYYYMMDDBBBBBUUUUUWWWWQC` (28 digits with QR code)
  - Short Serial Numbers: `SSYYMMDDNNNNN` (13 digits for barcodes)
  - Embedded traceability: Site, Strain, Batch Type, Date, Weight, Pack Size, Check Digit
- **Reason for Deferral**: Complete Phase 3B testing first, then implement during production module development

**✅ Phase 3 Part B Complete (2025-12-06) - Testing & Quality Assurance**:
- [x] **Test Infrastructure** - Created ServiceTestBase, RepositoryTestBase, TestDbContextFactory
- [x] **Unit Tests** - 224 tests, 100% pass rate
  - CustomerService: 100% coverage (20 tests)
  - VATCalculationService: 87.6% coverage (44 tests)
  - PricelistService: 60.1% coverage (20+ tests)
  - ProductService: 54.8% coverage (11 tests)
- [x] **Validator Tests** - 123 tests covering compliance
  - CreateProductValidator: 100% coverage (62 tests)
  - UpdateProductValidator: 100% coverage (53 tests)
  - CustomerRegistrationValidator: 100% coverage (18 tests)
- [x] **Code Coverage Report** - Generated with Coverlet + ReportGenerator
  - Overall: 12.2% (includes untested DAL/Migrations - expected)
  - Core BLL Services: 54-100% (excellent)
- [x] **Compliance Verified**:
  - ✅ Age verification (18+ Cannabis Act)
  - ✅ POPIA consent tracking
  - ✅ SA VAT calculation (15%)
  - ✅ Cannabis content validation (THC/CBD)
- [x] **Documentation**: See `docs/testing/PHASE-3B-COMPLETION-REPORT.md`

**⚠️ Deferred to Phase 4+**:
- Integration tests (customer → product → sale workflows)
- Untested services: AuditLogService, TransactionNumberGeneratorService, Category services

**✅ Phase 4 (Retail) Vertical Slice Complete (2025-12-06)**:
- [x] **TransactionRepository** - Complete DAL layer (CRUD + reporting queries)
- [x] **TransactionService** - Complete BLL layer (checkout workflow)
- [x] **DTOs** - CartItemDto, CheckoutRequestDto, CheckoutResultDto
- [x] **POSCheckout.razor** - Full UI component (cart, age verification, payment, receipt)
- [x] **Dependency Injection** - All services registered
- [x] **Build Verification** - 0 errors, 0 warnings
- [x] **Cannabis Compliance** - Age verification (18+), batch tracking
- [x] **SA VAT Compliance** - 15% VAT calculation with proper breakdown
- [x] **Payment Methods** - Cash, Card, EFT, Mobile Payment support
- [x] **Transaction Numbering** - Unique transaction numbers (SALE-YYYYMMDD-XXX)
- [x] **Receipt Display** - Professional receipt with VAT breakdown

**📊 Phase 4 Statistics**:
- **Files Created**: 8 new files (2 repositories, 1 service, 3 DTOs, 1 UI page, 1 interface)
- **Lines of Code**: ~1,800 lines
- **Methods Implemented**: 11 repository methods, 3 service methods
- **Compliance Features**: Age verification, batch tracking, VAT calculation
- **Pattern**: Complete vertical slice (DAL → BLL → UI)

**🎯 What's Replicable**:
This vertical slice demonstrates the complete pattern for all future features:
1. **DAL Pattern**: Repository with interface, CRUD + business queries
2. **BLL Pattern**: Service with DTOs, validation, business logic
3. **UI Pattern**: Blazor component with form handling, service injection
4. **DI Pattern**: Registration in Program.cs
5. **Compliance**: Age verification, audit trails, VAT calculations

**✅ Phase 5 MVP Production Modules Complete (2025-12-08)**:
- [x] **Cultivation Module** - Plant tracking, grow cycles, harvest batches (17 entities total)
- [x] **Production Module** - Processing workflow, QC, lab testing
- [x] **Inventory Module** - Stock movements, transfers, adjustments, counts
- [x] **CultivationDbContext** - Complete DAL with Fluent API configurations
- [x] **ProductionDbContext** - Complete DAL with Fluent API configurations
- [x] **InventoryDbContext** - Complete DAL with Fluent API configurations
- [x] **Dependency Injection** - All 3 DbContexts registered in Program.cs
- [x] **Build Verification** - 0 errors, 3 warnings (pre-existing)
- [x] **Seed-to-Sale Architecture** - Complete traceability chain established
- [x] **Documentation** - Comprehensive summary in docs/PHASE-5-MVP-MODULES-SUMMARY.md

**📊 Phase 5 Statistics**:
- **New Modules**: 3 (Cultivation, Production, Inventory)
- **Entities Created**: 17 entities + 5 enums
- **DbContexts Created**: 3 complete DbContexts
- **Lines of Code**: ~3,600 lines
- **Files Created**: 25 new files
- **Compliance Features**: SAHPRA plant tracking, GMP processing, SARS inventory tracking
- **Traceability**: Plant → Harvest → Production → Inventory → Sale

**🎯 What Was Achieved**:
This phase establishes the foundation for complete SAHPRA-compliant seed-to-sale traceability:
1. **Cultivation Module**: Individual plant tracking (SAHPRA Section 22C requirement)
2. **Production Module**: Processing workflow with QC and lab testing (GMP compliance)
3. **Inventory Module**: Universal stock ledger (SARS reconciliation)
4. **Cross-Module Links**: Batch numbers enable full traceability across modules
5. **Compliance**: SAHPRA, DALRRD, POPIA, SARS, GMP requirements embedded

**✅ Phase 6 Validators & Database Migrations Complete (2025-12-08)**:
- [x] **FluentValidation Integration** - All DTOs now have validators
  - Added FluentValidation package to all 4 BLL projects
  - Created validators for Cultivation module (Plant, GrowCycle, GrowRoom, HarvestBatch)
  - Created validators for Production module (ProductionBatch, ProcessingStep, QualityControl, LabTest)
  - Created validators for Inventory module (StockMovement, StockTransfer, StockAdjustment, StockCount)
  - Created validators for Management module (Product, ProductCategory, Debtor, DebtorCategory, Pricelist)
  - Registered all validators in DI container (Program.cs)
- [x] **Database Migrations** - All 3 module DbContexts migrated
  - Added Npgsql.EntityFrameworkCore.PostgreSQL 9.0.0 (attempted PostgreSQL)
  - Switched to SQL Server due to PostgreSQL compatibility issues
  - Generated InitialCreate migrations for all 3 DbContexts
  - Applied migrations to JASON\SQLDEVED SQL Server instance
  - **Database**: Project420
  - **Server**: JASON\SQLDEVED
  - **Auth**: SQL Server Authentication (sa/ZAQxsw123)
- [x] **Database Schema Created** - 12 tables across 3 modules
  - **Cultivation**: GrowRooms, GrowCycles, Plants, HarvestBatches (4 tables)
  - **Production**: ProductionBatches, ProcessingSteps, QualityControls, LabTests (4 tables)
  - **Inventory**: StockMovements, StockTransfers, StockAdjustments, StockCounts (4 tables)
  - All tables include proper indexes, foreign keys, and audit fields
- [x] **Build Status**: 0 Errors, 9 Warnings (all pre-existing Blazor/async warnings)

**📊 Phase 6 Statistics**:
- **Validators Created**: 32 validators (16 Create, 16 Update)
- **Database Tables**: 12 new tables
- **Indexes Created**: 50+ indexes for performance
- **Foreign Keys**: 8 relationships
- **Migrations**: 6 migration files (2 per DbContext)
- **Troubleshooting**: Resolved PostgreSQL NullReferenceException by switching to SQL Server

**🎯 What Was Achieved**:
Phase 6 completed critical infrastructure for MVP:
1. **Input Validation**: FluentValidation ensures data integrity before hitting the database
2. **Database Schema**: Physical database tables ready for CRUD operations
3. **Seed-to-Sale Foundation**: All traceability entities now have database backing
4. **Compliance Ready**: Audit fields in all tables support SAHPRA/POPIA requirements

**✅ Phase 7A Complete (2025-12-12) - Movement Architecture Foundation**:
- [x] **TransactionType enum** - All 16 transaction types (Sale, Refund, GRV, RTS, Production, Transfer, Adjustment)
- [x] **TransactionDetail entity** - Unified details with HeaderId + TransactionType discriminator
- [x] **Movement entity** - Option A architecture (SOH from movements)
- [x] **MovementDirection enum** - IN/OUT for stock direction
- [x] **SharedDbContext updated** - DbSets for TransactionDetails and Movements
- [x] **Database migrations** - Applied to Project420_Shared database
- [x] **IMovementService interface** - 13 methods for movement operations
- [x] **MovementService implementation** - Full Option A architecture
  - Movement generation from TransactionDetails
  - SOH calculation: SUM(IN) - SUM(OUT)
  - Movement reversal (soft delete)
  - Batch/serial number tracking
  - Historical SOH (as-of-date)
- [x] **DI registration** - ServiceCollectionExtensions + Program.cs
- [x] **Unit tests** - 51 comprehensive tests, 100% passing
- [x] **Database configuration fix** - Created Project420_Shared, aligned connection strings

**📊 Phase 7A Statistics**:
- **New Entities**: 2 (TransactionDetail, Movement)
- **New Enums**: 2 (TransactionType expanded, MovementDirection)
- **Service Methods**: 13 in IMovementService
- **Unit Tests**: 51 tests (all passing)
- **Database Tables**: 2 new (Movements, TransactionDetails in Project420_Shared)
- **Compliance Features**: Batch tracking, serial tracking, movement reasons, audit trail

**🎯 What Was Achieved**:
Phase 7A establishes the Movement Architecture (Option A) foundation:
1. **SOH Never Stored**: SOH = SUM(IN) - SUM(OUT) from Movement ledger
2. **Movement Immutability**: Soft delete only, full audit trail
3. **Transaction Linking**: HeaderId + TransactionType discriminator pattern
4. **Cannabis Compliance**: Batch/serial tracking for seed-to-sale traceability
5. **Comprehensive Testing**: 51 unit tests covering all edge cases

**📋 Phase 7B Planned: Unified Transaction Architecture**:
- [ ] Rename POSTransactionHeader → RetailTransactionHeader
- [ ] Integrate POS with unified TransactionDetails (Shared)
- [ ] Integrate MovementService into TransactionService
- [ ] Update repositories and services
- [ ] Create migrations for table renames

**Detailed Plan**: `docs/roadmap/PHASE-UNIFIED-TRANSACTION-ARCHITECTURE.md`

---

**🚀 Phase 8+ Next Steps** (After Phase 7B):

**Option A: Implement Batch & Serial Number System** (CRITICAL for POC completion):
- [ ] Review reference documents (mvp_modules_seed_traceability.md, system_evolution_roadmap.md, sn_batch_best_practices.md)
- [ ] Design batch numbering system adapted to Project420 architecture
- [ ] Design serial number generation system (internal + external SNs)
- [ ] Implement BatchNumberGeneratorService
- [ ] Implement SerialNumberGeneratorService
- [ ] Create Batch entity and repository
- [ ] Create SerialNumber entity and repository
- [ ] Integrate batch/SN into existing modules (Cultivation, Production, Inventory)
- [ ] Test end-to-end traceability with batch/SN linking

**Option B: Build Repositories & BLL** (Complete Vertical Slices):
- [ ] Create Cultivation repositories (PlantRepository, GrowCycleRepository, etc.)
- [ ] Create Production repositories (ProductionBatchRepository, QualityControlRepository, etc.)
- [ ] Create Inventory repositories (StockMovementRepository, StockTransferRepository, etc.)
- [ ] Create Cultivation BLL services with DTOs
- [ ] Create Production BLL services with DTOs
- [ ] Create Inventory BLL services with DTOs

**Option C: Create Seed Data** (Enable Testing):
- [ ] Create seed data for Cultivation (sample grow rooms, cycles, plants)
- [ ] Create seed data for Production (sample batches, processing steps, lab tests)
- [ ] Create seed data for Inventory (sample stock movements, transfers)

**Option D: UI Development** (Visual Progress):
- [ ] Build Cultivation UI (plant tracking, grow cycle management)
- [ ] Build Production UI (batch processing, QC workflow, lab testing)
- [ ] Build Inventory UI (stock movements, transfers, stock counts)
- [ ] Create seed-to-sale traceability reports

**Option E: Online Ordering API** (E-Commerce MVP - Future Phase):
- [ ] Create ASP.NET Core Web API project (Project420.API.WebApi)
- [ ] Implement Product Catalog API (GET /api/products, /api/products/{id})
- [ ] Build Order Placement API (POST /api/orders with age verification)
- [ ] Develop Customer Authentication (JWT tokens, OAuth 2.0)
- [ ] Integrate Payment Processing (Yoco/PayFast/Ozow for SA market)
- [ ] Add Order Status Tracking (GET /api/orders/{id}/status)
- [ ] Create OnlineOrders Module (Models, DAL, BLL layers)
- [ ] Implement Click & Collect (order online, pickup in-store with ID verification)
- [ ] Add Email Notifications (order confirmation, ready for pickup)
- [ ] Build Compliance Middleware (age verification, audit logging, POPIA)

**⚠️ Online Ordering Legal Compliance Note**:
- **Current Status (2024)**: Commercial cannabis sales NOT yet legal in SA (Cannabis for Private Purposes Act)
- **Expected Timeline**: Commercial regulations anticipated 2026-2027
- **MVP Strategy**: Build "Click & Collect" system (compliant, future-proof for full e-commerce)
- **Required Compliance**:
  - Age verification (18+) at registration AND pickup
  - POPIA data protection (encrypted PII, consent tracking)
  - Purchase limit enforcement
  - Audit trail for all online transactions
  - Delivery age verification (when commercial sales legal)

---

## 💡 SUCCESS METRICS FOR PROJECT420

**Legal Compliance**:
- [ ] All features verified against SA Cannabis Act 2024
- [ ] POPIA compliance audit passed
- [ ] Tax calculations accurate and future-ready
- [ ] Age verification 100% enforced
- [ ] Audit trails complete and immutable

**Technical Quality**:
- [ ] 90%+ unit test coverage
- [ ] All SOLID principles applied
- [ ] Database properly normalized and encrypted
- [ ] Performance targets met (<1s response)
- [ ] Security audit passed

**Documentation**:
- [ ] All compliance requirements documented
- [ ] API documentation complete
- [ ] User guides for legal requirements
- [ ] Developer guides for compliance features

---

**CLAUDE FLOW STATUS**: ✅ **INITIALIZED AND ACTIVE**
**SA GUIDE REFERENCE**: ⚠️ **MANDATORY FOR ALL WORK**
**CODING STRUCTURE TEMPLATE**: ⚠️ **MANDATORY FOR ALL DEVELOPMENT**
**PROJECT420**: ✅ **PHASE 7A COMPLETE - MOVEMENT ARCHITECTURE FOUNDATION IMPLEMENTED**
**COMPLIANCE GATES**: ✅ **ACTIVE AND ENFORCED**
**BUILD STATUS**: ✅ **0 ERRORS (excluding Android SDK)**
**TEST STATUS**: ✅ **275/275 TESTS PASSING (224 existing + 51 MovementService)**
**DATABASE STATUS**: ✅ **Project420_Dev (29 tables) + Project420_Shared (7 tables)**
**VALIDATORS STATUS**: ✅ **32 FLUENTVALIDATION VALIDATORS REGISTERED**
**MODULES STATUS**: ✅ **6 MODULES OPERATIONAL (Management, POS, OnlineOrders, Cultivation, Production, Inventory)**
**MOVEMENT ARCHITECTURE**: ✅ **OPTION A IMPLEMENTED - SOH FROM MOVEMENTS**
**TRACEABILITY STATUS**: ✅ **PLANT → HARVEST → PRODUCTION → INVENTORY → MOVEMENT → SALE CHAIN COMPLETE**
**NEXT PHASE**: 📋 **PHASE 7B - UNIFIED TRANSACTION ARCHITECTURE**

---

*Building compliant, future-proof cannabis management software for South Africa* 🌿
