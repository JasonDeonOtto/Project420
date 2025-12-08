# Project420 - Agent Assignments & Responsibilities
## Detailed Agent Work Breakdown

**Purpose**: Define which specialized agents should work on each component of the project
**Status**: Active Configuration
**Last Updated**: 2025-12-01

---

## 🤖 Agent Specialization Strategy

This document outlines the division of labor across different specialized agents. Each agent has specific expertise and should be assigned to matching tasks for optimal efficiency and quality.

---

## 📐 Architecture Agent (Architect)

**Specialty**: System design, architecture decisions, database schema design
**Responsible For**:
- Overall system architecture and design patterns
- Database schema design and relationships
- Module interaction and dependency management
- Technology stack decisions
- Performance and scalability planning

### Assigned Tasks:

#### Phase 1: Foundation Design
- [ ] Design complete database schema for all entities
- [ ] Define entity relationships (Customers, Products, Pricelists, Sales, Transactions)
- [ ] Create ER diagrams
- [ ] Design repository pattern interfaces
- [ ] Define service layer contracts
- [ ] Establish data flow between modules

#### Deliverables:
- Database schema diagrams
- Entity relationship models
- Interface definitions for repositories
- Service layer architecture document

**Agent Invocation**: Use when making architectural decisions or designing system-wide components

---

## 💾 Database Agent (Data Engineer)

**Specialty**: EF Core, database migrations, data access patterns
**Responsible For**:
- EF Core DbContext implementation
- Entity configurations and mappings
- Database migrations
- Seed data creation
- Query optimization
- Multi-database support (SQL Server, PostgreSQL, SQLite)

### Assigned Tasks:

#### Shared.Database Layer
- [ ] Implement base DbContext with common configuration
- [ ] Create connection string management
- [ ] Set up multi-database provider support
- [ ] Implement database initialization logic

#### Management Module - DAL
- [ ] Create entity configurations for Customer/Debtor
  - Properties: Id, Name, ContactInfo, TaxNumber, CreditLimit, Balance
- [ ] Create entity configurations for Product
  - Properties: Id, SKU, Name, Description, Category, UnitOfMeasure
- [ ] Create entity configurations for Pricelist
  - Properties: Id, Name, EffectiveDate, ExpiryDate, IsActive
- [ ] Create entity configurations for PricelistItem
  - Properties: Id, PricelistId, ProductId, Price, MinQuantity
- [ ] Implement repositories: CustomerRepository, ProductRepository, PricelistRepository
- [ ] Create initial migration
- [ ] Add seed data

#### Retail.POS Module - DAL
- [ ] Create entity configurations for Sale
  - Properties: Id, SaleDate, CustomerId, TotalAmount, TaxAmount, Status
- [ ] Create entity configurations for SaleItem
  - Properties: Id, SaleId, ProductId, Quantity, UnitPrice, TotalPrice
- [ ] Create entity configurations for Transaction
  - Properties: Id, SaleId, PaymentMethod, Amount, TransactionDate
- [ ] Create entity configurations for Inventory
  - Properties: Id, ProductId, LocationId, QuantityOnHand, ReorderLevel
- [ ] Implement repositories: SaleRepository, TransactionRepository, InventoryRepository
- [ ] Create initial migration
- [ ] Add seed data

#### Deliverables:
- Fully configured EF Core contexts
- All entity configurations
- Repository implementations
- Database migrations
- Seed data scripts

**Agent Invocation**: Use for all database-related work, EF Core setup, and data access layer implementation

---

## 🔧 Business Logic Agent (BLL Developer)

**Specialty**: Business rules, service layer implementation, DTOs
**Responsible For**:
- Business logic implementation
- Service layer development
- DTO creation and mapping
- Validation rules
- Business rule enforcement

### Assigned Tasks:

#### Management Module - BLL
- [ ] Create DTOs for Customer (CustomerDto, CreateCustomerDto, UpdateCustomerDto)
- [ ] Create DTOs for Product (ProductDto, CreateProductDto, UpdateProductDto)
- [ ] Create DTOs for Pricelist (PricelistDto, CreatePricelistDto, UpdatePricelistDto)
- [ ] Implement CustomerService
  - Methods: GetAll, GetById, Create, Update, Delete, GetByTaxNumber, CheckCredit
- [ ] Implement ProductService
  - Methods: GetAll, GetById, Create, Update, Delete, GetByCategory, SearchBySKU
- [ ] Implement PricelistService
  - Methods: GetAll, GetById, Create, Update, Delete, GetActive, GetPriceForProduct
- [ ] Add FluentValidation validators
- [ ] Implement AutoMapper profiles
- [ ] Add business rule validation (credit limits, pricing rules, etc.)

#### Retail.POS Module - BLL
- [ ] Create DTOs for Sale (SaleDto, CreateSaleDto, UpdateSaleDto, SaleItemDto)
- [ ] Create DTOs for Transaction (TransactionDto, ProcessPaymentDto)
- [ ] Create DTOs for Inventory (InventoryDto, StockMovementDto)
- [ ] Implement SaleService
  - Methods: CreateSale, GetSaleById, ProcessSale, CancelSale, GetSalesByDate
- [ ] Implement TransactionService
  - Methods: ProcessPayment, RefundPayment, GetTransactionHistory
- [ ] Implement InventoryService
  - Methods: GetStock, UpdateStock, ReserveStock, ReleaseStock, GetLowStockItems
- [ ] Add FluentValidation validators
- [ ] Implement AutoMapper profiles
- [ ] Add transaction processing logic
- [ ] Add inventory tracking logic

#### Deliverables:
- Complete DTO models
- Service implementations with business logic
- Validation rules
- Mapping configurations

**Agent Invocation**: Use for business logic, service layer, and validation implementation

---

## 🎨 UI/UX Agent (Frontend Developer)

**Specialty**: Blazor components, UI design, user experience
**Responsible For**:
- Blazor component development
- UI layout and design
- User interaction flows
- Responsive design
- Component state management

### Assigned Tasks:

#### Management Module - UI.Blazor
- [ ] Create main navigation and layout
- [ ] Design and implement Customer Management pages:
  - Customer list with search/filter
  - Customer detail view
  - Customer create/edit form
  - Customer credit status display
- [ ] Design and implement Product Management pages:
  - Product catalog with grid/list view
  - Product detail view
  - Product create/edit form
  - Category filtering
- [ ] Design and implement Pricelist Management pages:
  - Pricelist list view
  - Pricelist detail with items
  - Pricelist create/edit form
  - Price comparison view
- [ ] Implement shared components:
  - Data grids with sorting/filtering
  - Form validation displays
  - Loading indicators
  - Error messages

#### Retail.POS Module - UI.Blazor
- [ ] Create POS main interface layout
- [ ] Design and implement POS Transaction page:
  - Product search and selection
  - Shopping cart display
  - Quantity and price adjustments
  - Payment processing interface
  - Receipt preview
- [ ] Design and implement Sales History page:
  - Sales list with filtering
  - Sale detail view
  - Transaction history
  - Refund interface
- [ ] Design and implement Inventory page:
  - Stock levels display
  - Low stock alerts
  - Stock adjustment interface
- [ ] Implement POS-specific components:
  - Numeric keypad
  - Product quick-select grid
  - Payment method selector
  - Receipt printer component

#### Deliverables:
- Blazor components for all pages
- Responsive UI layouts
- User-friendly forms and interfaces
- Shared component library

**Agent Invocation**: Use for all UI development, Blazor components, and frontend work

---

## 🧪 Testing Agent (QA Engineer)

**Specialty**: Unit testing, integration testing, test automation
**Responsible For**:
- Unit test creation
- Integration test development
- Test coverage analysis
- Mocking and test data setup

### Assigned Tasks:

#### Management.Tests
- [ ] Create unit tests for CustomerRepository
- [ ] Create unit tests for ProductRepository
- [ ] Create unit tests for PricelistRepository
- [ ] Create unit tests for CustomerService
- [ ] Create unit tests for ProductService
- [ ] Create unit tests for PricelistService
- [ ] Create integration tests for database operations
- [ ] Mock external dependencies

#### Retail.POS.Tests
- [ ] Create unit tests for SaleRepository
- [ ] Create unit tests for TransactionRepository
- [ ] Create unit tests for InventoryRepository
- [ ] Create unit tests for SaleService
- [ ] Create unit tests for TransactionService
- [ ] Create unit tests for InventoryService
- [ ] Create integration tests for POS workflows
- [ ] Test transaction processing logic

#### Test Coverage Goals:
- DAL: 90%+ coverage
- BLL: 95%+ coverage
- Critical paths: 100% coverage

#### Deliverables:
- Comprehensive test suites
- Test coverage reports
- Integration test scenarios
- Mock data factories

**Agent Invocation**: Use after implementing each component to create corresponding tests

---

## 🔒 Security Agent (Security Specialist)

**Specialty**: Security auditing, authentication, authorization, data protection
**Responsible For**:
- Security vulnerability identification
- Authentication/authorization implementation
- Data encryption and protection
- Input validation and sanitization
- OWASP compliance

### Assigned Tasks:

#### Phase 1: Security Audit (After initial implementation)
- [ ] Review data models for sensitive information handling
- [ ] Audit input validation across all layers
- [ ] Check for SQL injection vulnerabilities
- [ ] Review authentication requirements
- [ ] Plan role-based access control (RBAC)

#### Phase 2: Security Implementation
- [ ] Implement authentication middleware
- [ ] Add authorization policies for different modules
- [ ] Encrypt sensitive data (customer info, transaction details)
- [ ] Implement audit logging
- [ ] Add rate limiting for API endpoints
- [ ] Secure database connection strings

#### Compliance Requirements:
- [ ] South African cannabis regulations compliance check
- [ ] POPIA (Protection of Personal Information Act) compliance
- [ ] PCI DSS for payment processing
- [ ] Data retention policies

#### Deliverables:
- Security audit report
- Authentication/authorization implementation
- Encryption implementation
- Compliance documentation

**Agent Invocation**: Use for security reviews and implementation of security features

---

## 📊 Integration Agent (DevOps/Integration Specialist)

**Specialty**: CI/CD, deployment, system integration
**Responsible For**:
- Build pipeline setup
- Deployment automation
- Environment configuration
- Module integration
- Performance monitoring

### Assigned Tasks:

#### Phase 1: Build & Deployment
- [ ] Set up CI/CD pipeline (GitHub Actions or Azure DevOps)
- [ ] Configure build scripts
- [ ] Set up automated testing in pipeline
- [ ] Create deployment scripts for different environments
- [ ] Configure connection strings per environment

#### Phase 2: Integration
- [ ] Integrate Management and Retail.POS modules
- [ ] Set up shared database or separate databases per module
- [ ] Configure inter-module communication
- [ ] Set up logging and monitoring
- [ ] Configure health checks

#### Phase 3: Performance
- [ ] Set up performance monitoring
- [ ] Configure caching strategies
- [ ] Optimize database queries
- [ ] Load testing
- [ ] Performance tuning

#### Deliverables:
- CI/CD pipelines
- Deployment documentation
- Integration configuration
- Performance reports

**Agent Invocation**: Use for deployment, integration, and DevOps tasks

---

## 📝 Documentation Agent (Technical Writer)

**Specialty**: Documentation, API documentation, user guides
**Responsible For**:
- Technical documentation
- API documentation
- User guides
- Code documentation
- Architecture diagrams

### Assigned Tasks:

#### Phase 1: Technical Documentation
- [ ] Document database schema
- [ ] Create API documentation for services
- [ ] Document repository interfaces
- [ ] Create code comments and XML documentation
- [ ] Document architecture decisions

#### Phase 2: User Documentation
- [ ] Create user guide for Management module
- [ ] Create user guide for POS module
- [ ] Create quick-start guide
- [ ] Create troubleshooting guide
- [ ] Create FAQ document

#### Phase 3: Maintenance Documentation
- [ ] Create deployment guide
- [ ] Document configuration options
- [ ] Create backup/restore procedures
- [ ] Document database maintenance tasks

#### Deliverables:
- Complete technical documentation
- User guides
- API reference documentation
- Deployment and maintenance guides

**Agent Invocation**: Use for all documentation needs throughout the project

---

## 🎯 Coordination Strategy

### Sequential Workflow (Recommended)
1. **Architect** → Design system and database schema
2. **Database Agent** → Implement data layer
3. **BLL Developer** → Implement business logic
4. **Testing Agent** → Create tests for completed components
5. **Frontend Developer** → Build UI
6. **Security Agent** → Audit and secure
7. **Integration Agent** → Deploy and integrate
8. **Technical Writer** → Document

### Parallel Workflow (Where Possible)
- **Database Agent** + **BLL Developer** can work on different modules simultaneously
- **Testing Agent** can create tests as soon as contracts are defined
- **Frontend Developer** can start on UI mockups while backend is being built
- **Documentation Agent** can work continuously alongside development

---

## 📞 Agent Invocation Guidelines

### How to Call Agents

Use the Claude Code Task tool with appropriate agent types:

```
# For architecture and design decisions
Task(subagent_type="general-purpose", description="Design database schema",
     prompt="Design the database schema for Customer and Product entities...")

# For exploration and code understanding
Task(subagent_type="Explore", description="Find EF Core patterns",
     prompt="Explore the codebase for EF Core repository patterns...")
```

### When to Use Each Agent

- **Start of feature**: Architecture Agent
- **Database work**: Database Agent
- **Business logic**: BLL Developer
- **After each component**: Testing Agent
- **UI work**: UI/UX Agent
- **Before deployment**: Security Agent
- **Deployment**: Integration Agent
- **Continuously**: Documentation Agent

---

## ✅ Checklist for Each Module Component

For each new component, ensure these agents are involved:

- [ ] Architect designs the component
- [ ] Database Agent implements data layer (if needed)
- [ ] BLL Developer implements business logic
- [ ] UI/UX Agent creates interface (if needed)
- [ ] Testing Agent creates unit tests
- [ ] Testing Agent creates integration tests
- [ ] Security Agent reviews for vulnerabilities
- [ ] Documentation Agent documents the component
- [ ] Integration Agent deploys to test environment

---

**Remember**: Each agent has specialized skills. Assign tasks to the right agent for best results!

**Last Updated**: 2025-12-01
**Next Review**: After first module completion
