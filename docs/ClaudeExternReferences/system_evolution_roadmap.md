# System Evolution Roadmap
## PoC (Current) → Prototype (Next) → Production Build (Final)

This document describes the progressive development of the cannabis seed–sale traceability system across three distinct phases: **Proof of Concept**, **Prototype**, and **Production Build**.

---

# 🌱 Phase 1 — Proof of Concept (PoC)
### Goal
Demonstrate feasibility of batch numbering, serial generation, simple traceability, and basic inventory logic.

### Included Modules
#### **1. Batch Numbering System**
- Production, Stock Take, Transfer, and Sales batches
- 12–14 character batch format
- Basic CRUD
- Batch lifecycle: Open → Closed

#### **2. Serial Number Generation**
- Internal 14-character SN generator
- SN ↔ Batch linking
- Collision-free generation rules
- Simple SN validation

#### **3. Basic Inventory Tracking**
- Add/Reduce stock
- Inventory per batch
- Adjustment reasons

#### **4. Basic Traceability**
- Batch history
- Serial history
- Simple traceability report

#### **5. Minimal UI (Blazor)**
- Simple navigation
- Minimal styling

#### **6. Local/Single Database Only**
- EF Core models
- Direct SQL Server access

---

# 🧪 Phase 2 — Prototype (Next)
### Goal
Deliver a fully functional, internally usable system with improved UX, multi-facility workflows, and expanded traceability.

## Prototype Module Set

### **1. Authentication & Authorization**
- User accounts
- Roles & permissions
- Basic audit logging

### **2. UI Upgrade**
- Unified theme
- Shared base form components
- Better navigation & layout

### **3. Enhanced Batch Management**
- Full metadata (strain, facility, documents)
- Batch split/merge (optional)
- Attachment support (COA, lab tests)

### **4. Advanced Serial Number Layer**
- Bulk SN generation tools
- External SN support with checksum
- QR code generation
- Full SN status lifecycle

### **5. Expanded Inventory Module**
- Multi-location & multi-facility support
- Stock freeze
- Variance approval workflow

### **6. Transfers v2**
- Dispatch & Receive steps
- Transfer logs
- Printable transfer slips

### **7. Sales Module v2**
- Sales orders
- SN assignment to orders
- Customer list
- Packing slips

### **8. Reporting & Dashboards**
- Inventory dashboard
- Serial usage report
- Batch cycle analytics
- Movement logs

### **9. Settings & Configuration**
- Batch type configuration
- SN generator settings
- Facility/location management

### **10. Internal API Layer**
- Token-based auth
- For mobile scan apps or external tools

### **11. Prototype Security**
- Password reset
- Improved audit trails

### **Data Architecture Improvements**
- Referential integrity
- Stored procedures for heavy ops
- Background workers for bulk SN tasks

---

# 🚀 Phase 3 — Production Build
### Goal
Deliver a commercial-grade, secure, scalable, regulatory-compliant seed-to-sale traceability platform.

## Production-Grade Modules & Enhancements

### **1. Compliance Engine**
- Immutable audit ledger
- Regulatory reporting
- Seed-to-sale lineage
- Exportable audit bundles (PDF/CSV)

### **2. Enterprise Security**
- MFA
- OAuth2/SSO
- Permission system enhancements
- Row-level security (multi-tenant)
- Encryption of sensitive fields

### **3. Cloud-Ready, Scalable Architecture**
- Containerized Blazor + API apps
- Horizontal scaling
- SQL Server read replicas
- Cloud storage for files
- Zero-downtime deployments

### **4. High-Volume Serial Engine**
- SN generation worker cluster
- Distributed ID generator
- Queue-based bulk operations
- High-speed indexing

### **5. Production Inventory Engine**
- Real-time stock ledger
- FIFO/LIFO/batch rotations
- Negative stock prevention
- Bulk data import/export

### **6. Public QR Validation Portal**
- Public-facing authenticity checker
- QR scanning API
- Product/batch metadata display

### **7. QC & Compliance Modules**
- Germination tests
- Lab results upload
- Document management (certificates, permits)
- Quality status per batch

### **8. Packaging & Printing Suite**
- Label template builder
- SN labels
- Batch labels
- Transfer/shipping labels

### **9. Advanced Analytics & BI**
- Sales analysis
- Inventory aging
- Batch performance metrics
- Forecasting models

### **10. Full Multi-Tenant Mode**
- Database isolation (schema or DB-per-tenant)
- Tenant onboarding
- Subscription billing integration

### **11. Disaster Recovery & Backup Strategy**
- Automated backups
- Point-in-time restore (PITR)
- Region failover for uptime

---

# 📌 Summary Overview

| Phase | Focus | Use Case |
|------|--------|----------|
| **PoC** | Prove core batch + SN logic works | Internal testing only |
| **Prototype** | Usable system with full workflows | Internal + pilot customers |
| **Production** | Scalable, secure, regulatory-compliant platform | Commercial SaaS |

---

This roadmap provides a clear, structured progression from a simple technical demo to a fully operational enterprise-grade traceability platform.

