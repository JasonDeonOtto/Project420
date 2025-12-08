# Project420 - Cannabis Management System

**Status**: 🟡 Proof of Concept - Initial Setup
**Version**: 0.1.0
**Created**: 2025-12-01
**Location**: `C:\Users\Jason\Documents\Mine\projects\Personal\Project420`

---

## 🌿 Project Overview

Project420 is a comprehensive cannabis industry management system designed for the South African market. The system provides end-to-end management from plantation to retail, with initial focus on **Management** (Customers, Products, Pricelists) and **Retail POS** modules.

### Key Features (Planned)
- 🏪 **Retail Point of Sale**: Fast, intuitive POS system
- 📦 **Product Management**: Complete product catalog with pricing
- 👥 **Customer Management**: Debtor tracking and credit management
- 💰 **Pricelist Management**: Flexible pricing strategies
- 📊 **Inventory Tracking**: Real-time stock management
- 🌱 **Future**: Plantation and production modules

---

## 🎯 Project Goals

- ✅ **Regulatory Compliance**: Align with South African cannabis laws
- ✅ **Cost-Effective**: Affordable solution for small to medium businesses
- ✅ **Scalable**: Grow from single store to enterprise
- ✅ **Secure**: Enterprise-grade security and data protection
- ✅ **User-Friendly**: Intuitive interfaces for all user levels
- ✅ **High Performance**: Fast response times and smooth UX

---

## 🏗️ Architecture

### 3-Tier Modular Architecture
Each module is built with three distinct layers:

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

## 📁 Project Structure

```
Project420/
├── docs/                           # Documentation
│   ├── PROJECT-STATUS.md          # Current status & roadmap
│   ├── AGENT-ASSIGNMENTS.md       # Agent responsibilities
│   └── ARCHITECTURE.md             # Technical architecture
│
├── src/
│   ├── Shared/                     # Shared libraries
│   │   ├── Project420.Shared.Core
│   │   ├── Project420.Shared.Infrastructure
│   │   └── Project420.Shared.Database
│   │
│   └── Modules/
│       ├── Management/             # Customer, Product, Pricelist management
│       │   ├── Project420.Management.Models
│       │   ├── Project420.Management.DAL
│       │   ├── Project420.Management.BLL
│       │   └── Project420.Management.UI.Blazor
│       │
│       └── Retail/
│           └── POS/                # Point of Sale system
│               ├── Project420.Retail.POS.Models
│               ├── Project420.Retail.POS.DAL
│               ├── Project420.Retail.POS.BLL
│               └── Project420.Retail.POS.UI.Blazor
│
└── tests/                          # Unit & Integration tests
    ├── Project420.Management.Tests
    └── Project420.Retail.POS.Tests
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code
- SQL Server / PostgreSQL / SQLite

### Setup

1. **Clone/Open the solution**:
   ```bash
   cd C:\Users\Jason\Documents\Mine\projects\Personal\Project420
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run migrations** (once implemented):
   ```bash
   dotnet ef database update --project src/Modules/Management/Project420.Management.DAL
   ```

5. **Run the application** (once UI is ready):
   ```bash
   dotnet run --project src/Modules/Management/Project420.Management.UI.Blazor
   ```

---

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [PROJECT-STATUS.md](docs/PROJECT-STATUS.md) | Current status, completed work, next steps |
| [AGENT-ASSIGNMENTS.md](docs/AGENT-ASSIGNMENTS.md) | Detailed agent responsibilities and tasks |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | Technical architecture and design decisions |

---

## 🎯 Current Phase: Phase 2 - DbContext & Database Setup

### ✅ Phase 1 Complete (2025-12-02)
**Data Models - Enterprise Grade with Full Compliance**
- 9 entities built with POPIA compliance
- Cannabis-specific tracking (THC/CBD, batch, age verification)
- Flexible pricelist system
- Comprehensive audit trails
- 200+ XML documentation blocks
- Builds successfully with 0 errors

### 🔄 Phase 2 In Progress
**DbContext & Database Setup**
1. Create PosDbContext with Fluent API configuration
2. Install EF Core packages
3. Set up database connection strings
4. Generate and apply EF Core migrations
5. Create seed data for testing

### Focus Areas
1. **Retail POS Module**: Point of sale with full transaction tracking
   - Product management with cannabis compliance
   - Customer management with age verification
   - Flexible pricing via pricelists
   - Complete transaction and payment tracking

2. **Management Module**: (Upcoming) Business management features

See [PROJECT-STATUS.md](docs/PROJECT-STATUS.md) for detailed roadmap.

---

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/Project420.Management.Tests
dotnet test tests/Project420.Retail.POS.Tests
```

---

## 📦 Modules

### Management Module
**Purpose**: Core business entity management
**Features**:
- Customer/Debtor management with credit tracking
- Product catalog with categories
- Flexible pricelist management
- Multi-tier pricing support

### Retail POS Module
**Purpose**: Point of sale and inventory
**Features**:
- Fast POS transaction processing
- Shopping cart with item management
- Multiple payment methods
- Real-time inventory tracking
- Sales history and reporting

### Future Modules
- **Plantation**: Cultivation tracking
- **Production**: Processing and manufacturing
- **Compliance**: Regulatory reporting
- **Analytics**: Business intelligence

---

## 🔐 Security

- Role-based access control (RBAC)
- Data encryption at rest and in transit
- Secure authentication with JWT
- Input validation and sanitization
- Audit logging
- POPIA compliance (South African data protection)

---

## 🤝 Development Workflow

This project uses specialized AI agents for different aspects of development:

- **Architect**: System design and architecture decisions
- **Database Agent**: EF Core, migrations, data access
- **BLL Developer**: Business logic and services
- **UI/UX Agent**: Blazor components and frontend
- **Testing Agent**: Unit and integration tests
- **Security Agent**: Security audits and implementation
- **Integration Agent**: CI/CD and deployment
- **Documentation Agent**: Technical documentation

See [AGENT-ASSIGNMENTS.md](docs/AGENT-ASSIGNMENTS.md) for details.

---

## 📊 Project Status

**Current Status**: Phase 1 Complete - Phase 2 In Progress 🚀

**✅ Phase 1 Complete (Data Models)**:
- [x] AuditableEntity base class (POPIA compliance)
- [x] Product entity (cannabis compliance: THC/CBD, batch tracking)
- [x] Debtor/Customer entity (age verification, medical licenses)
- [x] Pricelist & PricelistItem entities (flexible pricing)
- [x] TransactionHeader & TransactionDetail entities (invoices)
- [x] Payment entity (separate from transaction)
- [x] 3 Enumerations (TransactionStatus, TransactionType, PaymentMethod)
- [x] Comprehensive XML documentation
- [x] Solution builds with 0 errors

**🔄 Phase 2 In Progress (DbContext & Database)**:
- [ ] PosDbContext with Fluent API configuration
- [ ] EF Core packages installation
- [ ] Database connection strings
- [ ] Initial migration
- [ ] Seed data

**Next Up (Phase 3)**:
- [ ] Repository pattern implementation
- [ ] Business logic services
- [ ] Blazor UI components

See [PROJECT-STATUS.md](docs/PROJECT-STATUS.md) for full status.

---

## 🛠️ Development

### Build
```bash
dotnet build
```

### Clean
```bash
dotnet clean
```

### Add Migration
```bash
dotnet ef migrations add InitialCreate --project src/Modules/Management/Project420.Management.DAL
```

### Update Database
```bash
dotnet ef database update --project src/Modules/Management/Project420.Management.DAL
```

---

## 📝 Coding Standards

This project follows:
- ✅ SOLID principles
- ✅ Repository pattern
- ✅ Dependency injection
- ✅ Async/await best practices
- ✅ Comprehensive XML documentation
- ✅ Unit test coverage (target: >90%)

---

## 📞 Support

For issues or questions, refer to the documentation in the `docs/` folder.

---

## 📄 License

Proprietary - All rights reserved

---

**Happy Coding!** 🌿

*Building the future of cannabis management, one line of code at a time.*
