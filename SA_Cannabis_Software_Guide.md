# South African Cannabis Management Software Development Guide

> **Version:** 1.0 | **Last Updated:** December 2025  
> **Purpose:** Reference guide for developing cannabis management software for the South African market  
> **Disclaimer:** This guide is for informational purposes only and does not constitute legal advice. The regulatory framework is evolving rapidly—always consult qualified legal counsel.

---

## Table of Contents

1. [Legal Framework Overview](#1-legal-framework-overview)
2. [Tax Considerations](#2-tax-considerations)
3. [Essential Features (Must-Have)](#3-essential-features-must-have)
4. [POPIA Data Protection Requirements](#4-popia-data-protection-requirements)
5. [Future-Proof Features (Add Later)](#5-future-proof-features-add-later)
6. [Business Registration Requirements](#6-business-registration-requirements)
7. [Technical Implementation Notes](#7-technical-implementation-notes)
8. [API Integration Points](#8-api-integration-points)
9. [Database Schema Considerations](#9-database-schema-considerations)
10. [Key Contacts and Resources](#10-key-contacts-and-resources)

---

## 1. Legal Framework Overview

### 1.1 Cannabis for Private Purposes Act 2024

**Signed:** 28 May 2024 by President Cyril Ramaphosa

| Provision | Limit |
|-----------|-------|
| Private possession (single adult) | 600g dried cannabis |
| Private possession (2+ adults household) | 1,200g dried cannabis |
| Public possession | 100g dried cannabis |
| Cultivation (per person) | 4 flowering plants |
| Cultivation (per household) | 8 flowering plants |
| Minimum age | 18 years |

**Prohibited Activities:**
- Public consumption
- Commercial sale/distribution (recreational)
- Dealing in cannabis
- Sale to minors

**Permitted Activities:**
- Private use by adults
- Private cultivation within limits
- Possession within prescribed quantities

### 1.2 Commercial Regulatory Timeline

```
Current Status (Dec 2025)
├── Private use: LEGAL (within limits)
├── Commercial recreational sale: ILLEGAL
├── Medical cannabis: LEGAL (with SAHPRA license)
└── Hemp cultivation: LEGAL (with DALRRD permit)

Expected Timeline
├── April 2026: Hemp & Cannabis Commercialisation Policy (Cabinet approval)
├── Mid-2027: Overarching Cannabis Bill to Parliament
└── TBD: Full commercial framework implementation
```

### 1.3 Medical Cannabis Framework (SAHPRA)

**Regulatory Body:** South African Health Products Regulatory Authority (SAHPRA)

**Required Licenses:**
- Section 22C(1)(b) License - Cultivation, manufacture, extraction, testing, import, export, distribution
- Section 22A(9)(a)(i) Permit - Department of Health permit (additional requirement)

**License Details:**
- Validity: 5 years
- Requires GMP compliance (PIC/S guidelines)
- Pre-negotiated off-take agreements often required
- Facility audit before license issuance
- Current status: ~120 licenses issued for export/medical activities

**Patient Access:**
- Section 21 application via doctor to SAHPRA
- For unregistered cannabis medicines
- Individual patient basis only

### 1.4 Hemp Industry Framework (DALRRD)

**Regulatory Body:** Department of Agriculture, Land Reform and Rural Development (DALRRD)

**Legal Basis:** Plant Improvement Act (Act 53 of 1976, amended 2021)

**THC Threshold:** ≤0.2% (proposal to increase to 2% pending)

**Permit Types:**
| Permit Code | Activity |
|-------------|----------|
| HP-GD-005 | Cultivation |
| HP-GD-006 | Cleaning/Processing |
| HP-GD-007 | Export |
| HP-GD-003 | Breeding Research |
| HP-GD-004 | Sale of Material |

**Current Status:** Over 1,408 hemp cultivation permits issued nationally

### 1.5 CBD Products Scheduling

| Schedule | THC Limit | CBD Limit | Daily Dose | Access |
|----------|-----------|-----------|------------|--------|
| Schedule 0 | ≤0.001% | ≤0.0075% | ≤20mg | Over-the-counter |
| Schedule 4 | >0.001% | >0.0075% | N/A | Prescription required |

**Pack Size Limit (Schedule 0):** Maximum 600mg CBD per pack

---

## 2. Tax Considerations

### 2.1 Current Tax Framework

**No cannabis-specific excise tax exists yet.** Design software to accommodate future taxes.

#### Value Added Tax (VAT)
```javascript
const VAT_CONFIG = {
  rate: 0.15, // 15%
  mandatoryThreshold: 1000000, // R1 million annual turnover
  voluntaryThreshold: 50000,   // R50,000 annual turnover
  registrationBody: 'SARS'
};
```

#### Corporate Income Tax
```javascript
const CORPORATE_TAX = {
  standardRate: 0.27, // 27%
  smallBusinessRates: {
    // Progressive rates for qualifying small businesses
    bracket1: { max: 95750, rate: 0 },
    bracket2: { max: 365000, rate: 0.07 },
    bracket3: { max: 550000, rate: 0.21 },
    bracket4: { above: 550000, rate: 0.27 }
  },
  turnoverTaxThreshold: 1000000 // R1 million for micro businesses
};
```

### 2.2 Anticipated Cannabis-Specific Taxes

Design software to support these future tax types:

```javascript
const FUTURE_TAX_TYPES = {
  exciseDuty: {
    // Expected to be introduced, model on tobacco/alcohol
    calculationMethods: ['perGram', 'percentageOfPrice', 'hybrid'],
    reportingFrequency: 'monthly',
    authority: 'SARS'
  },
  provincialLevy: {
    // Potential local government taxes
    varies: true,
    byLocation: true
  },
  cultivationTax: {
    // Possible future tax
    methods: ['perPlant', 'perSquareMeter', 'perHarvest']
  },
  licenseFees: {
    // Annual fees by category
    categories: ['cultivation', 'manufacturing', 'distribution', 'retail']
  }
};
```

### 2.3 Tax Calculation Requirements

```javascript
// Required tax calculation capabilities
const TAX_FEATURES = {
  vat: {
    autoCalculation: true,
    inclusiveExclusiveToggle: true,
    returnDataExport: true,
    format: 'VAT201'
  },
  excise: {
    configurableRates: true,
    productCategoryBased: true,
    weightBasedOption: true,
    valueBasedOption: true,
    sarsFormatExport: true
  },
  multiJurisdiction: {
    locationBasedRules: true,
    separateTracking: true,
    authoritySpecificReports: true
  }
};
```

---

## 3. Essential Features (Must-Have)

### 3.1 License and Permit Management

```javascript
const LICENSE_MODULE = {
  entities: {
    sahpraLicense: {
      type: 'Section22C',
      fields: ['licenseNumber', 'issueDate', 'expiryDate', 'conditions', 'activities'],
      activities: ['cultivation', 'manufacture', 'extraction', 'testing', 'import', 'export', 'distribution'],
      validity: '5 years',
      renewalRequired: true
    },
    dohPermit: {
      type: 'Section22A',
      fields: ['permitNumber', 'issueDate', 'expiryDate', 'conditions'],
      linkedTo: 'sahpraLicense'
    },
    dalrrdHempPermit: {
      types: ['cultivation', 'processing', 'export', 'breeding', 'sale'],
      forms: ['HP-Form-001', 'HP-Form-003', 'HP-Form-005', 'HP-Form-006', 'HP-Form-007'],
      fields: ['permitNumber', 'issueDate', 'expiryDate', 'siteCoordinates', 'activities']
    }
  },
  features: {
    documentStorage: true,      // Secure storage with version control
    expiryAlerts: [90, 60, 30], // Days before expiration
    auditTrail: true,           // Complete activity history
    multiLicense: true,         // Multiple licenses per entity
    renewalWorkflow: true       // Guided renewal process
  }
};
```

### 3.2 Seed-to-Sale Traceability

```javascript
const TRACEABILITY_MODULE = {
  tracking: {
    uniqueIdentifiers: {
      methods: ['RFID', 'barcode', 'QRCode'],
      levels: ['plant', 'batch', 'package', 'product']
    },
    cultivation: {
      fields: ['plantId', 'strain', 'batchNumber', 'plantDate', 'growthStage', 
               'location', 'room', 'section', 'motherPlant', 'cloneSource']
    },
    processing: {
      fields: ['harvestDate', 'wetWeight', 'dryWeight', 'curingStart', 'curingEnd',
               'extractionMethod', 'manufacturingBatch', 'inputMaterials', 'outputProducts']
    },
    labTesting: {
      fields: ['sampleId', 'labName', 'testDate', 'thcContent', 'cbdContent',
               'terpeneProfile', 'contaminants', 'pesticides', 'heavyMetals', 'coaNumber'],
      coaManagement: true
    },
    chainOfCustody: {
      loggedEvents: ['transfer', 'movement', 'handling', 'storage', 'sale'],
      fields: ['timestamp', 'fromLocation', 'toLocation', 'responsibleParty', 'quantity', 'reason']
    },
    wasteTracking: {
      fields: ['wasteType', 'quantity', 'destructionMethod', 'destructionDate', 
               'witnessName', 'witnessSignature', 'photos']
    }
  },
  recall: {
    rapidIdentification: true,
    affectedProductsQuery: true,
    customerNotification: true,
    regulatoryReporting: true
  }
};
```

### 3.3 Point of Sale (POS) System

```javascript
const POS_MODULE = {
  ageVerification: {
    idScanner: true,
    supportedIds: ['SA_ID', 'SA_SmartID', 'Passport', 'DriversLicense'],
    minimumAge: 18,
    verificationLog: true
  },
  purchaseLimits: {
    configurable: true,
    limitTypes: ['daily', 'transaction', 'weekly'],
    alerts: true,
    preventExceedance: true
  },
  taxCalculation: {
    autoVat: true,
    excisePlaceholder: true,
    taxBreakdown: true
  },
  payment: {
    methods: ['cash', 'card', 'eft'],  // Card when banking access improves
    cashManagement: {
      drawerTracking: true,
      dropSafe: true,
      reconciliation: true
    }
  },
  receipts: {
    required: ['businessName', 'vatNumber', 'date', 'items', 'batchNumbers',
               'quantities', 'prices', 'taxBreakdown', 'total', 'paymentMethod'],
    format: ['print', 'email', 'sms']
  },
  medicalPatient: {
    section21Tracking: true,
    prescriptionManagement: true,
    refillAlerts: true,
    authorizationVerification: true
  }
};
```

### 3.4 Inventory Management

```javascript
const INVENTORY_MODULE = {
  stockTracking: {
    realTime: true,
    multiLocation: true,
    autoUpdate: true
  },
  batchManagement: {
    fifo: true,  // First In First Out
    fefo: true,  // First Expired First Out
    expiryMonitoring: true
  },
  productCategories: [
    'flower', 'pre-rolls', 'concentrates', 'edibles', 
    'topicals', 'tinctures', 'capsules', 'seeds', 
    'clones', 'cbd_products', 'accessories'
  ],
  potencyTracking: {
    thcContent: true,
    cbdContent: true,
    perBatch: true,
    limitChecking: true
  },
  alerts: {
    lowStock: { configurable: true, defaultThreshold: 20 },
    expiring: { daysBeforeExpiry: [30, 14, 7] },
    shrinkage: { threshold: 0.02 }  // 2% variance alert
  },
  storageCompliance: {
    temperatureMonitoring: true,
    humidityMonitoring: true,
    securityLogging: true
  }
};
```

### 3.5 Compliance and Reporting

```javascript
const COMPLIANCE_MODULE = {
  regulatoryReports: {
    sahpra: {
      types: ['productionReport', 'inventoryReport', 'salesReport', 'destructionReport'],
      frequency: 'asRequired',
      format: 'SAHPRA_template'
    },
    dalrrd: {
      types: ['cultivationReport', 'thcTestResults', 'harvestNotification', 'exportReport'],
      frequency: 'annual',
      forms: ['Co-oP7', 'Co-oP8']
    },
    sars: {
      types: ['VAT201', 'exciseReturn', 'incomeReturn'],
      frequency: ['monthly', 'biMonthly', 'annual'],
      format: 'eFiling'
    },
    incb: {
      // International Narcotics Control Board
      types: ['productionVolumes', 'exportVolumes', 'importVolumes'],
      frequency: 'annual'
    }
  },
  gmpDocumentation: {
    batchRecords: true,
    deviationReports: true,
    changeControl: true,
    capa: true,  // Corrective and Preventive Actions
    sops: true   // Standard Operating Procedures
  },
  auditLogs: {
    immutable: true,
    userAttribution: true,
    timestamped: true,
    retention: '7 years'
  },
  complianceDashboard: {
    realTimeStatus: true,
    riskIndicators: true,
    alertNotifications: true
  }
};
```

### 3.6 Financial Reporting

```javascript
const FINANCIAL_MODULE = {
  salesReports: {
    periods: ['daily', 'weekly', 'monthly', 'quarterly', 'annual'],
    drillDown: true,
    byProduct: true,
    byCategory: true,
    byLocation: true
  },
  taxReports: {
    vatOutput: true,
    vatInput: true,
    exciseDuty: true,
    taxLiability: true,
    paymentSchedule: true
  },
  costTracking: {
    cogs: true,      // Cost of Goods Sold
    laborCosts: true,
    overhead: true,
    marginAnalysis: true
  },
  accountingIntegration: {
    supported: ['Sage', 'Xero', 'QuickBooks', 'Pastel'],
    exportFormats: ['CSV', 'IIF', 'QBO', 'OFX']
  },
  bbbeeReporting: {
    ownership: true,
    employment: true,
    procurement: true,
    enterpriseDevelopment: true
  }
};
```

---

## 4. POPIA Data Protection Requirements

### 4.1 Overview

**Protection of Personal Information Act (POPIA)**
- Effective: 1 July 2020
- Enforcement: 1 July 2021
- Similar to: EU GDPR

### 4.2 Compliance Requirements

```javascript
const POPIA_COMPLIANCE = {
  dataProcessing: {
    lawfulBasis: ['consent', 'contract', 'legalObligation', 'legitimateInterest', 'vitalInterest', 'publicInterest'],
    purposeLimitation: true,
    dataMinimization: true,
    accuracyObligation: true,
    storageLimitation: true,
    securityMeasures: true
  },
  informationOfficer: {
    required: true,
    registration: 'Information Regulator',
    duties: ['compliance', 'dataSubjectRequests', 'regulatorLiaison', 'policyDevelopment']
  },
  dataSubjectRights: {
    access: true,
    correction: true,
    deletion: true,
    objection: true,
    dataPortability: true,
    responseTimeframe: 'reasonable'  // Not specified like GDPR's 30 days
  },
  breachNotification: {
    required: true,
    notifyRegulator: true,
    notifyDataSubjects: true,
    timeframe: 'as soon as reasonably possible'
  },
  crossBorderTransfer: {
    adequacyRequired: true,
    bindingCorporateRules: true,
    contractualClauses: true
  }
};
```

### 4.3 Penalties

| Violation Type | Maximum Penalty |
|----------------|-----------------|
| Administrative fine | R10 million (~$520,000 USD) |
| Serious violations | Up to 10 years imprisonment |
| Minor violations | Up to 12 months imprisonment |
| Civil liability | Compensation to affected individuals |

### 4.4 Technical Implementation

```javascript
const POPIA_TECHNICAL = {
  encryption: {
    atRest: 'AES-256',
    inTransit: 'TLS 1.3',
    keyManagement: 'required'
  },
  accessControl: {
    rbac: true,  // Role-Based Access Control
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
    policyRequired: true,
    autoPurge: true,
    legalRetentionPeriods: {
      taxRecords: '5 years',
      employmentRecords: '3 years after termination',
      customerRecords: '5 years after last transaction'
    }
  },
  consentManagement: {
    explicitConsent: true,
    withdrawalMechanism: true,
    consentLog: true,
    marketingConsent: 'separate'
  },
  security: {
    regularAssessments: true,
    penetrationTesting: 'annual',
    vulnerabilityScanning: 'continuous',
    incidentResponse: true
  }
};
```

---

## 5. Future-Proof Features (Add Later)

### 5.1 Retail and E-Commerce

```javascript
// Implement when commercial sales permitted (expected ~2027)
const ECOMMERCE_MODULE = {
  onlineOrdering: {
    preOrders: true,
    delivery: true,  // When permitted
    pickup: true
  },
  deliveryManagement: {
    routeOptimization: true,
    driverTracking: true,
    deliveryVerification: true,
    ageVerificationAtDelivery: true
  },
  menuManagement: {
    realTimeInventorySync: true,
    productPhotos: true,
    strainInfo: true,
    labResults: true
  },
  loyaltyProgram: {
    pointsSystem: true,
    rewards: true,
    tieredMembership: true
  },
  thirdPartyIntegration: {
    // If/when operating in SA
    platforms: ['Weedmaps', 'Leafly', 'Dutchie']
  }
};
```

### 5.2 Advanced Cultivation Management

```javascript
const CULTIVATION_ADVANCED = {
  environmentalControls: {
    iotIntegration: true,
    sensors: ['temperature', 'humidity', 'co2', 'light', 'vpd'],
    automation: true,
    alerting: true
  },
  fertigation: {
    nutrientSchedules: true,
    phMonitoring: true,
    ecMonitoring: true,
    automatedDosing: true
  },
  pestManagement: {
    ipmTracking: true,
    treatmentRecords: true,
    preventiveMeasures: true,
    complianceDocumentation: true
  },
  yieldPrediction: {
    historicalAnalysis: true,
    mlPrediction: true,
    harvestPlanning: true
  },
  geneticTracking: {
    strainRegistry: true,
    breedingRecords: true,
    phenotypeDocumentation: true,
    geneticTesting: true
  },
  outdoorCultivation: {
    gpsMapping: true,
    plotManagement: true,
    weatherIntegration: true
  }
};
```

### 5.3 Analytics and Business Intelligence

```javascript
const ANALYTICS_MODULE = {
  salesAnalytics: {
    productPerformance: true,
    customerBehavior: true,
    seasonalTrends: true,
    basketAnalysis: true
  },
  complianceScoring: {
    realTimeScore: true,
    riskIndicators: true,
    trendAnalysis: true
  },
  financialDashboards: {
    cashFlow: true,
    margins: true,
    taxLiability: true,
    projections: true
  },
  predictiveAnalytics: {
    demandForecasting: true,
    inventoryOptimization: true,
    pricingOptimization: true
  }
};
```

### 5.4 Cannabis Club Features

```javascript
// For potential legal cannabis clubs (currently grey area)
const CANNABIS_CLUB_MODULE = {
  memberManagement: {
    verification: true,
    membershipTiers: true,
    accessControl: true
  },
  cultivationAllocation: {
    perMemberAllocation: true,
    collectivePlantCount: true,
    harvestDistribution: true
  },
  cooperativeOwnership: {
    shareTracking: true,
    dividendCalculation: true,
    votingRights: true
  },
  eventManagement: {
    consumptionSpaces: true,
    bookings: true,
    capacityManagement: true
  }
};
```

---

## 6. Business Registration Requirements

### 6.1 Company Registration (CIPC)

```javascript
const CIPC_REGISTRATION = {
  companyTypes: {
    ptyLtd: {
      name: 'Private Company (Pty) Ltd',
      shareholders: { min: 1, max: 50 },
      liability: 'limited',
      recommended: true
    },
    publicCompany: {
      name: 'Public Company (Ltd)',
      shareholders: { min: 1, max: 'unlimited' },
      liability: 'limited'
    },
    npc: {
      name: 'Non-Profit Company',
      purpose: 'public benefit',
      profitDistribution: false
    }
  },
  requirements: {
    validId: 'SA ID or passport (within 3 months)',
    minimumAge: 18,
    address: 'verifiable street and postal address in SA',
    directorEligibility: 'not disqualified under Companies Act'
  },
  fees: {
    registration: 175,  // Via BizPortal
    nameReservation: 50,
    currency: 'ZAR'
  },
  ongoingCompliance: {
    annualReturns: true,
    bbbeeAffidavit: 'if turnover < R10 million'
  },
  portal: 'https://www.bizportal.gov.za/'
};
```

### 6.2 Tax Registration (SARS)

```javascript
const SARS_REGISTRATION = {
  incomeTax: {
    automatic: true,  // Generated after CIPC registration
    required: true
  },
  vat: {
    mandatory: { turnover: 1000000 },  // R1 million
    voluntary: { turnover: 50000 },     // R50,000
    rate: 0.15
  },
  employerTaxes: {
    paye: { required: 'if employing staff' },
    uif: { rate: 0.02, split: '50/50 employer/employee' },
    sdl: { rate: 0.01, threshold: 500000 }  // Skills Development Levy
  },
  coida: {
    name: 'Compensation for Occupational Injuries and Diseases',
    required: 'if employing staff'
  },
  portal: 'https://www.sars.gov.za/'
};
```

### 6.3 Industry-Specific Licenses Summary

| License Type | Authority | Key Requirements | Approx. Cost |
|--------------|-----------|------------------|--------------|
| Medical Cannabis | SAHPRA | GMP facility, pharmacist, security plan | R34,000+ |
| Hemp Cultivation | DALRRD | HP-Form-001, site plan, GPS coordinates | ~R714/hour |
| Manufacturing | SAHPRA + DoH | Section 22C + Section 22A, GMP | R50,000+ |
| CBD Products | SAHPRA | Schedule 0 or 4 compliance, labeling | Varies |

---

## 7. Technical Implementation Notes

### 7.1 Recommended Technology Stack

```javascript
const TECH_STACK = {
  backend: {
    recommended: ['Node.js', 'Python', 'Go'],
    framework: ['Express', 'FastAPI', 'Gin'],
    database: ['PostgreSQL', 'MongoDB'],
    cache: ['Redis'],
    messageQueue: ['RabbitMQ', 'Kafka']
  },
  frontend: {
    web: ['React', 'Vue.js', 'Angular'],
    mobile: ['React Native', 'Flutter'],
    pos: ['Electron', 'Native']
  },
  infrastructure: {
    cloud: ['AWS', 'Azure', 'GCP'],
    containerization: ['Docker', 'Kubernetes'],
    cicd: ['GitHub Actions', 'GitLab CI', 'Jenkins']
  },
  security: {
    authentication: ['OAuth 2.0', 'JWT'],
    encryption: ['AES-256', 'RSA'],
    monitoring: ['SIEM', 'WAF', 'IDS/IPS']
  },
  compliance: {
    dataResidency: 'South Africa preferred',
    backupLocation: 'Consider cross-border implications',
    auditTrail: 'Immutable logging required'
  }
};
```

### 7.2 Development Priorities

```
Phase 1 (Months 1-3): Core Infrastructure
├── Database architecture (POPIA-compliant)
├── Authentication & RBAC
├── License/permit management
├── Basic inventory tracking
└── Audit logging

Phase 2 (Months 4-6): Compliance Features
├── Seed-to-sale traceability
├── SAHPRA/DALRRD reporting
├── Lab testing integration
├── Tax calculation engine
└── Compliance dashboard

Phase 3 (Months 7-9): Operations
├── Point of Sale system
├── Customer management
├── Financial reporting
├── Mobile applications
└── Multi-location support

Phase 4 (Months 10-12): Advanced Features
├── E-commerce preparation
├── Cultivation management
├── Business intelligence
├── API development
└── Government integration (when available)
```

---

## 8. API Integration Points

### 8.1 Required Integrations

```javascript
const INTEGRATIONS = {
  government: {
    sars: {
      description: 'Tax submission',
      format: 'eFiling',
      authentication: 'Digital certificate'
    },
    sahpra: {
      description: 'License management and reporting',
      format: 'Manual/Portal (API when available)',
      authentication: 'Portal credentials'
    },
    dalrrd: {
      description: 'Hemp permit management',
      format: 'Manual submission',
      contact: 'Hemp.PIA@dalrrd.gov.za'
    }
  },
  financial: {
    accounting: {
      platforms: ['Sage', 'Xero', 'QuickBooks', 'Pastel'],
      dataFlow: 'bidirectional'
    },
    banking: {
      // Limited due to cannabis industry restrictions
      methods: ['manual', 'file-based']
    },
    payment: {
      // Prepare for when available
      processors: ['card', 'eft', 'mobile']
    }
  },
  laboratory: {
    labTesting: {
      dataTypes: ['COA', 'potencyResults', 'contaminantTests'],
      formats: ['JSON', 'PDF', 'XML']
    }
  },
  hardware: {
    scanners: ['barcode', 'RFID', 'idDocument'],
    scales: ['certified', 'integrated'],
    printers: ['receipt', 'label'],
    iot: ['temperature', 'humidity', 'co2']
  }
};
```

---

## 9. Database Schema Considerations

### 9.1 Core Entities

```sql
-- Example schema structure (PostgreSQL)

-- Licenses and Permits
CREATE TABLE licenses (
    id UUID PRIMARY KEY,
    entity_id UUID REFERENCES entities(id),
    license_type VARCHAR(50) NOT NULL, -- 'SAHPRA', 'DALRRD_HEMP', 'DOH'
    license_number VARCHAR(100) UNIQUE,
    issue_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL,
    conditions JSONB,
    activities VARCHAR[] NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Plants (Seed-to-Sale)
CREATE TABLE plants (
    id UUID PRIMARY KEY,
    plant_tag VARCHAR(50) UNIQUE NOT NULL,
    batch_id UUID REFERENCES batches(id),
    strain_id UUID REFERENCES strains(id),
    source_type VARCHAR(20) NOT NULL, -- 'seed', 'clone', 'tissue_culture'
    source_id UUID,
    plant_date DATE NOT NULL,
    growth_stage VARCHAR(20) NOT NULL,
    location_id UUID REFERENCES locations(id),
    status VARCHAR(20) NOT NULL,
    destroyed_at TIMESTAMP,
    destruction_reason VARCHAR(255),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Inventory
CREATE TABLE inventory (
    id UUID PRIMARY KEY,
    product_id UUID REFERENCES products(id),
    batch_id UUID REFERENCES batches(id),
    location_id UUID REFERENCES locations(id),
    quantity DECIMAL(10,4) NOT NULL,
    unit VARCHAR(10) NOT NULL,
    thc_content DECIMAL(5,2),
    cbd_content DECIMAL(5,2),
    expiry_date DATE,
    coa_id UUID REFERENCES lab_results(id),
    status VARCHAR(20) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Customers (POPIA-compliant)
CREATE TABLE customers (
    id UUID PRIMARY KEY,
    customer_type VARCHAR(20) NOT NULL, -- 'retail', 'medical', 'wholesale'
    -- Personal info encrypted at rest
    encrypted_data BYTEA NOT NULL, -- Contains name, ID number, contact
    date_of_birth DATE NOT NULL, -- For age verification
    consent_marketing BOOLEAN DEFAULT FALSE,
    consent_date TIMESTAMP,
    medical_authorization_id UUID,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    deleted_at TIMESTAMP -- Soft delete for data retention
);

-- Audit Log (Immutable)
CREATE TABLE audit_log (
    id BIGSERIAL PRIMARY KEY,
    timestamp TIMESTAMP DEFAULT NOW(),
    user_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id UUID NOT NULL,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT
);

-- Create index for audit queries
CREATE INDEX idx_audit_entity ON audit_log(entity_type, entity_id);
CREATE INDEX idx_audit_timestamp ON audit_log(timestamp);
```

### 9.2 Data Retention Policy

```javascript
const DATA_RETENTION = {
  financialRecords: { years: 5, basis: 'Tax Administration Act' },
  employmentRecords: { years: 3, basis: 'after termination' },
  customerRecords: { years: 5, basis: 'after last transaction' },
  auditLogs: { years: 7, basis: 'compliance best practice' },
  licenseDocs: { years: 10, basis: 'regulatory requirement' },
  labResults: { years: 5, basis: 'product liability' },
  salesRecords: { years: 5, basis: 'Tax Administration Act' }
};
```

---

## 10. Key Contacts and Resources

### 10.1 Regulatory Authorities

| Authority | Website | Purpose | Contact |
|-----------|---------|---------|---------|
| SAHPRA | www.sahpra.org.za | Medical cannabis licensing | enquiries@sahpra.org.za |
| DALRRD | www.dalrrd.gov.za | Hemp permits | Hemp.PIA@dalrrd.gov.za |
| SARS | www.sars.gov.za | Tax compliance | 0800 00 7277 |
| CIPC | www.cipc.co.za | Company registration | 086 100 2472 |
| Information Regulator | www.inforegulator.org.za | POPIA compliance | enquiries@inforegulator.org.za |
| DTIC | www.thedtic.gov.za | Cannabis Master Plan | - |

### 10.2 Key Legislation

| Legislation | Reference | Relevance |
|-------------|-----------|-----------|
| Cannabis for Private Purposes Act | Act 7 of 2024 | Private use framework |
| Medicines and Related Substances Act | Act 101 of 1965 | Medical cannabis |
| Plant Improvement Act | Act 53 of 1976 | Hemp regulation |
| Protection of Personal Information Act | Act 4 of 2013 | Data protection |
| Companies Act | Act 71 of 2008 | Business registration |
| Tax Administration Act | Act 28 of 2011 | Tax compliance |

### 10.3 Useful Resources

- **National Cannabis Master Plan**: Contact DTIC for latest version
- **SAHPRA Cannabis Cultivation Guidelines**: www.sahpra.org.za/wp-content/uploads/2020/01/93b0b4262.44_Cannabiscultivation_v2_Nov2019.pdf
- **DALRRD Hemp Guidelines**: www.dalrrd.gov.za (Plant Production section)
- **POPIA Full Text**: www.popia.co.za

---

## Appendix A: Quick Reference Checklists

### A.1 Software Launch Checklist

```markdown
[ ] POPIA compliance audit completed
[ ] Information Officer registered with Information Regulator
[ ] Privacy policy published
[ ] Consent management implemented
[ ] Data encryption at rest and in transit
[ ] Audit logging functional and immutable
[ ] Role-based access control configured
[ ] Backup and disaster recovery tested
[ ] Security assessment completed
[ ] User documentation prepared
[ ] Staff training completed
```

### A.2 Client Onboarding Checklist

```markdown
[ ] CIPC registration confirmed
[ ] SARS registration verified
[ ] Applicable licenses/permits obtained:
    [ ] SAHPRA Section 22C (if medical)
    [ ] DoH Section 22A permit (if medical)
    [ ] DALRRD hemp permit (if hemp)
[ ] B-BBEE status documented
[ ] Information Officer appointed
[ ] Privacy policy accepted
[ ] User roles defined
[ ] Integration requirements identified
```

---

## Appendix B: Glossary

| Term | Definition |
|------|------------|
| B-BBEE | Broad-Based Black Economic Empowerment |
| CBD | Cannabidiol |
| CIPC | Companies and Intellectual Property Commission |
| COA | Certificate of Analysis |
| COIDA | Compensation for Occupational Injuries and Diseases Act |
| DALRRD | Department of Agriculture, Land Reform and Rural Development |
| DTIC | Department of Trade, Industry and Competition |
| EME | Exempted Micro Enterprise (turnover < R10 million) |
| GMP | Good Manufacturing Practice |
| INCB | International Narcotics Control Board |
| PAYE | Pay As You Earn |
| PIC/S | Pharmaceutical Inspection Co-operation Scheme |
| POPIA | Protection of Personal Information Act |
| SAHPRA | South African Health Products Regulatory Authority |
| SARS | South African Revenue Service |
| THC | Tetrahydrocannabinol |
| UIF | Unemployment Insurance Fund |
| VAT | Value Added Tax |

---

*Document Version: 1.0 | December 2025*  
*This guide should be reviewed and updated as regulations evolve.*
