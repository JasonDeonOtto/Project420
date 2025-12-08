# Project420 - System Architecture
## Cannabis Management System - Technical Architecture

**Version**: 1.0
**Last Updated**: 2025-12-01
**Status**: Initial Design

---

## 🏗️ High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│  ┌──────────────────────┐         ┌──────────────────────┐     │
│  │   Blazor Web Apps    │         │     MAUI Mobile      │     │
│  │  - Management UI     │         │  - Management App    │     │
│  │  - Retail POS UI     │         │  - POS Mobile App    │     │
│  └──────────────────────┘         └──────────────────────┘     │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                     BUSINESS LOGIC LAYER                         │
│  ┌─────────────────────┐         ┌─────────────────────┐       │
│  │   Management.BLL    │         │  Retail.POS.BLL     │       │
│  │  - CustomerService  │         │  - SaleService      │       │
│  │  - ProductService   │         │  - TransactionSvc   │       │
│  │  - PricelistService │         │  - InventoryService │       │
│  └─────────────────────┘         └─────────────────────┘       │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                      DATA ACCESS LAYER                           │
│  ┌─────────────────────┐         ┌─────────────────────┐       │
│  │   Management.DAL    │         │  Retail.POS.DAL     │       │
│  │  - Repositories     │         │  - Repositories     │       │
│  │  - EF DbContext     │         │  - EF DbContext     │       │
│  └─────────────────────┘         └─────────────────────┘       │
└─────────────────────────────────────────────────────────────────┘
                              ↕
┌─────────────────────────────────────────────────────────────────┐
│                         DATABASE LAYER                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ SQL Server / │  │  PostgreSQL  │  │   SQLite     │          │
│  │  Azure SQL   │  │  (Business)  │  │   (Local)    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```
Project420/
│
├── Project420.sln                           # Solution file
│
├── docs/                                    # Documentation
│   ├── PROJECT-STATUS.md                   # Work tracking
│   ├── AGENT-ASSIGNMENTS.md                # Agent responsibilities
│   └── ARCHITECTURE.md                      # This file
│
├── src/
│   ├── Shared/                              # Shared libraries
│   │   ├── Project420.Shared.Core/
│   │   │   ├── Interfaces/                  # Common interfaces
│   │   │   │   ├── IRepository.cs
│   │   │   │   ├── IService.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── DTOs/                        # Common DTOs
│   │   │   ├── Enums/                       # Common enumerations
│   │   │   └── Constants/                   # Application constants
│   │   │
│   │   ├── Project420.Shared.Infrastructure/
│   │   │   ├── Logging/                     # Logging utilities
│   │   │   ├── Validation/                  # Validation helpers
│   │   │   ├── Mapping/                     # AutoMapper profiles
│   │   │   └── Extensions/                  # Extension methods
│   │   │
│   │   └── Project420.Shared.Database/
│   │       ├── BaseDbContext.cs             # Base EF Core context
│   │       ├── ConnectionFactory.cs         # Multi-DB support
│   │       └── Migrations/                  # Shared migrations
│   │
│   └── Modules/
│       ├── Management/                      # Management Module
│       │   ├── Project420.Management.Models/
│       │   │   ├── Entities/
│       │   │   │   ├── Customer.cs
│       │   │   │   ├── Product.cs
│       │   │   │   ├── Pricelist.cs
│       │   │   │   └── PricelistItem.cs
│       │   │   └── DTOs/
│       │   │       ├── CustomerDto.cs
│       │   │       ├── ProductDto.cs
│       │   │       └── PricelistDto.cs
│       │   │
│       │   ├── Project420.Management.DAL/
│       │   │   ├── Contexts/
│       │   │   │   └── ManagementDbContext.cs
│       │   │   ├── Configurations/
│       │   │   │   ├── CustomerConfiguration.cs
│       │   │   │   ├── ProductConfiguration.cs
│       │   │   │   └── PricelistConfiguration.cs
│       │   │   ├── Repositories/
│       │   │   │   ├── CustomerRepository.cs
│       │   │   │   ├── ProductRepository.cs
│       │   │   │   └── PricelistRepository.cs
│       │   │   └── UnitOfWork.cs
│       │   │
│       │   ├── Project420.Management.BLL/
│       │   │   ├── Services/
│       │   │   │   ├── CustomerService.cs
│       │   │   │   ├── ProductService.cs
│       │   │   │   └── PricelistService.cs
│       │   │   ├── Validators/
│       │   │   │   ├── CustomerValidator.cs
│       │   │   │   ├── ProductValidator.cs
│       │   │   │   └── PricelistValidator.cs
│       │   │   └── Mapping/
│       │   │       └── MappingProfile.cs
│       │   │
│       │   ├── Project420.Management.UI.Blazor/
│       │   │   ├── Pages/
│       │   │   │   ├── Customers/
│       │   │   │   │   ├── CustomerList.razor
│       │   │   │   │   ├── CustomerDetail.razor
│       │   │   │   │   └── CustomerForm.razor
│       │   │   │   ├── Products/
│       │   │   │   │   ├── ProductCatalog.razor
│       │   │   │   │   ├── ProductDetail.razor
│       │   │   │   │   └── ProductForm.razor
│       │   │   │   └── Pricelists/
│       │   │   │       ├── PricelistList.razor
│       │   │   │       ├── PricelistDetail.razor
│       │   │   │       └── PricelistForm.razor
│       │   │   ├── Components/
│       │   │   │   ├── Shared/
│       │   │   │   └── Forms/
│       │   │   └── Services/
│       │   │
│       │   └── Project420.Management.UI.Maui/
│       │       └── (To be created after MAUI workload install)
│       │
│       └── Retail/
│           └── POS/                         # Point of Sale Module
│               ├── Project420.Retail.POS.Models/
│               │   ├── Entities/
│               │   │   ├── Sale.cs
│               │   │   ├── SaleItem.cs
│               │   │   ├── Transaction.cs
│               │   │   └── Inventory.cs
│               │   └── DTOs/
│               │       ├── SaleDto.cs
│               │       ├── TransactionDto.cs
│               │       └── InventoryDto.cs
│               │
│               ├── Project420.Retail.POS.DAL/
│               │   ├── Contexts/
│               │   │   └── POSDbContext.cs
│               │   ├── Configurations/
│               │   │   ├── SaleConfiguration.cs
│               │   │   ├── SaleItemConfiguration.cs
│               │   │   ├── TransactionConfiguration.cs
│               │   │   └── InventoryConfiguration.cs
│               │   ├── Repositories/
│               │   │   ├── SaleRepository.cs
│               │   │   ├── TransactionRepository.cs
│               │   │   └── InventoryRepository.cs
│               │   └── UnitOfWork.cs
│               │
│               ├── Project420.Retail.POS.BLL/
│               │   ├── Services/
│               │   │   ├── SaleService.cs
│               │   │   ├── TransactionService.cs
│               │   │   └── InventoryService.cs
│               │   ├── Validators/
│               │   │   ├── SaleValidator.cs
│               │   │   ├── TransactionValidator.cs
│               │   │   └── InventoryValidator.cs
│               │   └── Mapping/
│               │       └── MappingProfile.cs
│               │
│               ├── Project420.Retail.POS.UI.Blazor/
│               │   ├── Pages/
│               │   │   ├── POS/
│               │   │   │   ├── POSMain.razor
│               │   │   │   └── POSCheckout.razor
│               │   │   ├── Sales/
│               │   │   │   ├── SalesHistory.razor
│               │   │   │   └── SaleDetail.razor
│               │   │   └── Inventory/
│               │   │       ├── StockLevels.razor
│               │   │       └── StockAdjustment.razor
│               │   ├── Components/
│               │   │   ├── POS/
│               │   │   │   ├── ProductSearch.razor
│               │   │   │   ├── ShoppingCart.razor
│               │   │   │   └── PaymentPanel.razor
│               │   │   └── Shared/
│               │   └── Services/
│               │
│               └── Project420.Retail.POS.UI.Maui/
│                   └── (To be created after MAUI workload install)
│
└── tests/
    ├── Project420.Management.Tests/
    │   ├── Repositories/                    # DAL tests
    │   ├── Services/                        # BLL tests
    │   └── Integration/                     # Integration tests
    │
    └── Project420.Retail.POS.Tests/
        ├── Repositories/                    # DAL tests
        ├── Services/                        # BLL tests
        └── Integration/                     # Integration tests
```

---

## 🎯 3-Tier Architecture per Module

Each module follows strict 3-tier separation:

### 1. Presentation Layer (UI)
- **Technology**: Blazor Server/WebAssembly, MAUI
- **Responsibility**: User interface, user input, data display
- **Dependencies**: References BLL only (NOT DAL)
- **Communication**: Calls service methods via dependency injection

### 2. Business Logic Layer (BLL)
- **Technology**: C# class libraries
- **Responsibility**: Business rules, validation, orchestration
- **Dependencies**: References Models and DAL
- **Components**:
  - Services (business operations)
  - Validators (FluentValidation)
  - Mapping profiles (AutoMapper)
  - DTOs (data transfer objects)

### 3. Data Access Layer (DAL)
- **Technology**: EF Core, Repository pattern
- **Responsibility**: Database operations, data persistence
- **Dependencies**: References Models and Shared.Database
- **Components**:
  - DbContext (EF Core context)
  - Repositories (data access)
  - Entity configurations (Fluent API)
  - Migrations

---

## 🗄️ Database Design Strategy

### Database Options

#### Option 1: Single Database (Recommended for POC)
```
Project420_Database
├── Management Schema
│   ├── Customers
│   ├── Products
│   ├── Pricelists
│   └── PricelistItems
└── RetailPOS Schema
    ├── Sales
    ├── SaleItems
    ├── Transactions
    └── Inventory
```

#### Option 2: Separate Databases (Production)
```
Management_Database          POS_Database
├── Customers               ├── Sales
├── Products                ├── SaleItems
├── Pricelists             ├── Transactions
└── PricelistItems         └── Inventory
```

### Database Provider Support
- **Primary**: SQL Server / Azure SQL Database
- **Alternative**: PostgreSQL
- **Local/Offline**: SQLite (for MAUI mobile apps)

### Connection String Management
```json
{
  "ConnectionStrings": {
    "ManagementDb": "Server=...;Database=Management;",
    "POSDb": "Server=...;Database=POS;",
    "LocalDb": "Data Source=local.db"
  }
}
```

---

## 🔗 Module Communication

### Inter-Module Dependencies

```
Management Module
       ↓ (Product data)
Retail.POS Module
```

**Flow**:
1. Management module maintains product catalog
2. POS module reads product data for sales
3. POS module updates inventory independently
4. Shared customer data between modules

### Communication Methods
- **Direct DB**: Shared database access (POC)
- **API Layer**: Future microservices architecture
- **Message Queue**: Event-driven communication (future)

---

## 📦 Technology Stack

### Backend
- **.NET 9**: Application framework
- **EF Core 9**: ORM and data access
- **FluentValidation**: Input validation
- **AutoMapper**: Object mapping
- **Serilog**: Logging

### Frontend
- **Blazor Server/WebAssembly**: Web UI
- **MAUI**: Mobile/desktop apps
- **MudBlazor / Radzen**: UI component libraries (to be decided)

### Database
- **SQL Server**: Primary business database
- **PostgreSQL**: Alternative business database
- **SQLite**: Local/mobile database

### Testing
- **xUnit**: Unit testing framework
- **Moq**: Mocking library
- **FluentAssertions**: Test assertions
- **Respawn**: Database reset for integration tests

### DevOps
- **Git**: Version control
- **GitHub Actions / Azure DevOps**: CI/CD
- **Docker**: Containerization (future)

---

## 🔐 Security Architecture

### Authentication & Authorization
- **ASP.NET Core Identity**: User management
- **JWT Tokens**: API authentication
- **Role-Based Access Control (RBAC)**: Authorization

### Data Protection
- **Encryption at Rest**: Sensitive data encrypted in database
- **Encryption in Transit**: HTTPS/TLS for all communications
- **Key Management**: Azure Key Vault or similar

### Compliance
- **POPIA**: South African data protection laws
- **Cannabis Regulations**: Industry-specific compliance
- **PCI DSS**: Payment card industry standards (for POS)

---

## 📊 Data Flow Example: POS Sale

```
1. User scans/selects product in POS UI
   ↓
2. UI calls SaleService.AddItemToSale(productId, quantity)
   ↓
3. SaleService validates business rules:
   - Check inventory availability
   - Apply correct pricelist
   - Calculate taxes
   ↓
4. SaleService calls ProductRepository.GetById(productId)
   ↓
5. SaleService calls PricelistRepository.GetPriceForProduct(productId)
   ↓
6. SaleService updates sale total
   ↓
7. User completes payment
   ↓
8. UI calls SaleService.CompleteSale(saleId, paymentMethod)
   ↓
9. SaleService creates Transaction record
   ↓
10. SaleService updates Inventory (reduces stock)
    ↓
11. TransactionRepository saves Transaction
    ↓
12. InventoryRepository updates stock levels
    ↓
13. Success response to UI
    ↓
14. UI displays receipt
```

---

## 🚀 Deployment Architecture

### Development Environment
- Local SQL Server / SQLite
- IIS Express / Kestrel
- Visual Studio 2022

### Production Environment (Future)
```
┌──────────────────┐
│   Load Balancer  │
└────────┬─────────┘
         │
    ┌────┴────┐
    │         │
┌───▼──┐  ┌──▼───┐
│ Web  │  │ Web  │  (Blazor apps)
│ App1 │  │ App2 │
└───┬──┘  └──┬───┘
    │         │
    └────┬────┘
         │
    ┌────▼─────┐
    │ Database │  (SQL Server / PostgreSQL)
    │ Cluster  │
    └──────────┘
```

---

## 📈 Scalability Considerations

### Current POC
- Monolithic architecture
- Single database
- In-process services

### Future Scaling
- **Horizontal Scaling**: Multiple web app instances
- **Database Scaling**: Read replicas, sharding
- **Caching**: Redis for frequently accessed data
- **CDN**: Static content delivery
- **Microservices**: Break into independent services

---

## 🎨 Design Patterns Used

### Repository Pattern
- Abstracts data access
- Enables unit testing with mocks
- Centralized data access logic

### Unit of Work Pattern
- Manages transactions across multiple repositories
- Ensures data consistency

### Service Layer Pattern
- Encapsulates business logic
- Provides transaction boundaries
- Coordinates between repositories

### DTO Pattern
- Decouples domain models from API contracts
- Controls data exposure
- Enables versioning

### Dependency Injection
- Promotes loose coupling
- Enables testability
- Simplifies configuration

---

## 📝 Naming Conventions

### Projects
- `Project420.[Module].[Layer].[Technology]`
- Examples: `Project420.Management.DAL`, `Project420.Retail.POS.UI.Blazor`

### Namespaces
- Follow project structure
- Example: `Project420.Management.BLL.Services`

### Files
- Entity: `Customer.cs`
- DTO: `CustomerDto.cs`
- Repository: `CustomerRepository.cs`
- Service: `CustomerService.cs`
- Validator: `CustomerValidator.cs`

---

## 🔄 Development Workflow

1. **Design Phase**: Architect designs entity models and relationships
2. **DAL Phase**: Database agent implements EF Core entities and repositories
3. **BLL Phase**: BLL developer creates services and business logic
4. **Testing Phase**: QA creates unit and integration tests
5. **UI Phase**: Frontend developer builds Blazor components
6. **Security Phase**: Security agent audits and secures
7. **Integration Phase**: DevOps integrates and deploys
8. **Documentation Phase**: Technical writer documents everything

---

## ✅ Architecture Principles

- ✅ **Separation of Concerns**: Clear layer boundaries
- ✅ **DRY (Don't Repeat Yourself)**: Shared libraries for common code
- ✅ **SOLID Principles**: Applied throughout
- ✅ **Testability**: Every layer is unit-testable
- ✅ **Scalability**: Designed for growth
- ✅ **Security First**: Security built-in, not bolted-on
- ✅ **Performance**: Async/await, caching, optimization
- ✅ **Maintainability**: Clear structure, good documentation

---

**Last Updated**: 2025-12-01
**Next Review**: After POC completion
