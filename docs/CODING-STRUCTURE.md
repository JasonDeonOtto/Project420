# Project420 - Coding Structure Template

**Last Updated**: 2025-12-08
**Status**: MANDATORY REFERENCE
**Purpose**: Quick-reference template for all Project420 development
**Compliance**: MUST be used alongside SA Cannabis Guides

---

## 🚨 CRITICAL REQUIREMENT

**BEFORE ANY DEVELOPMENT**: Read and understand **BOTH** compliance guides:
1. `SA_Cannabis_Software_Guide.md` - Software framework & compliance
2. `south-africa-cannabis-cultivation-production-laws-guide.md` - Legal requirements

**This document provides the HOW, the guides provide the WHY.**

---

## 📋 Table of Contents

1. [Project Architecture Overview](#project-architecture-overview)
2. [Module Structure Template](#module-structure-template)
3. [Naming Conventions Quick Reference](#naming-conventions-quick-reference)
4. [Layer Patterns](#layer-patterns)
5. [Compliance Patterns (MANDATORY)](#compliance-patterns-mandatory)
6. [Development Workflow](#development-workflow)
7. [Quality Gates](#quality-gates)

---

## Project Architecture Overview

### 3-Tier Modular Architecture

```
┌─────────────────────────────────────────────────────┐
│  UI LAYER (Blazor/MAUI/API)                        │
│  - User interaction                                 │
│  - Form handling                                    │
│  - Display logic                                    │
├─────────────────────────────────────────────────────┤
│  BLL LAYER (Business Logic)                        │
│  - Services (business operations)                  │
│  - DTOs (data transfer objects)                    │
│  - Validators (FluentValidation)                   │
│  - Business rules & calculations                   │
│  - Cannabis compliance enforcement                 │
├─────────────────────────────────────────────────────┤
│  DAL LAYER (Data Access)                           │
│  - Repositories (CRUD operations)                  │
│  - DbContexts (EF Core)                            │
│  - Migrations                                       │
│  - Entity configurations                            │
├─────────────────────────────────────────────────────┤
│  MODELS LAYER (Domain Entities)                    │
│  - Entities (database tables)                      │
│  - Enums                                            │
│  - Base classes (AuditableEntity)                  │
└─────────────────────────────────────────────────────┘
         ↕
    [Database]
```

### Dependency Flow (NEVER REVERSE!)

```
UI → BLL → DAL → Models
```

### Core Principles

1. **Separation of Concerns**: Each layer has ONE responsibility
2. **Interface-Based**: All services and repositories use interfaces
3. **Dependency Injection**: All dependencies injected via constructor
4. **Modular Design**: Each business domain is a separate module
5. **Compliance First**: POPIA, Cannabis Act, SAHPRA, DALRRD built-in
6. **Shared Infrastructure**: Common services (VAT, Audit, Transaction Numbers)

---

## Module Structure Template

### When Creating a New Module

Use this exact structure for ALL modules:

```
src/Modules/<ModuleName>/
├── Project420.<ModuleName>.Models/
│   ├── Entities/
│   │   └── <EntityName>.cs (inherits AuditableEntity)
│   └── Enums/
│       └── <EnumName>.cs
│
├── Project420.<ModuleName>.DAL/
│   ├── <ModuleName>DbContext.cs
│   ├── <ModuleName>DbContextFactory.cs (for migrations)
│   ├── Repositories/
│   │   ├── I<Entity>Repository.cs
│   │   └── <Entity>Repository.cs
│   ├── Configurations/
│   │   └── <Entity>Configuration.cs
│   └── Migrations/
│       └── (auto-generated)
│
├── Project420.<ModuleName>.BLL/
│   ├── <Domain>/
│   │   ├── DTOs/
│   │   │   ├── Create<Entity>Dto.cs
│   │   │   ├── Update<Entity>Dto.cs
│   │   │   └── <Entity>DetailsDto.cs
│   │   ├── Services/
│   │   │   ├── I<Entity>Service.cs
│   │   │   └── <Entity>Service.cs
│   │   └── Validators/
│   │       ├── Create<Entity>Validator.cs
│   │       └── Update<Entity>Validator.cs
│
└── Project420.<ModuleName>.UI.Blazor/
    ├── Components/
    │   └── Pages/
    │       └── <Domain>/
    │           ├── <Entity>List.razor
    │           ├── <Entity>Create.razor
    │           └── <Entity>Edit.razor
    ├── wwwroot/
    │   └── css/
    └── Program.cs
```

### Example: Retail POS Module

```
src/Modules/Retail/POS/
├── Project420.Retail.POS.Models/
│   ├── Entities/
│   │   ├── POSTransactionHeader.cs
│   │   ├── POSTransactionDetail.cs
│   │   ├── Payment.cs
│   │   ├── Product.cs
│   │   └── Debtor.cs
│   └── Enums/
│       ├── TransactionStatus.cs
│       └── PaymentMethod.cs
│
├── Project420.Retail.POS.DAL/
│   ├── PosDbContext.cs
│   ├── PosDbContextFactory.cs
│   └── Repositories/
│       ├── ITransactionRepository.cs
│       ├── TransactionRepository.cs
│       ├── IPaymentRepository.cs
│       └── PaymentRepository.cs
│
├── Project420.Retail.POS.BLL/
│   ├── Transactions/
│   │   ├── DTOs/
│   │   │   ├── CartItemDto.cs
│   │   │   ├── CheckoutRequestDto.cs
│   │   │   └── CheckoutResultDto.cs
│   │   ├── Services/
│   │   │   ├── ITransactionService.cs
│   │   │   └── TransactionService.cs
│   │   └── Validators/
│   │       └── CheckoutRequestValidator.cs
│
└── Project420.Retail.POS.UI.Blazor/
    └── Components/
        └── Pages/
            └── POSCheckout.razor
```

---

## Naming Conventions Quick Reference

### Projects
```
Format: Project420.<Module>.<Layer>[.<SubLayer>]

Examples:
✅ Project420.Management.Models
✅ Project420.Management.DAL
✅ Project420.Management.BLL
✅ Project420.Retail.POS.Models
✅ Project420.Retail.POS.UI.Blazor
```

### Files & Classes

| Type | Pattern | Example |
|------|---------|---------|
| **Entity** | PascalCase, singular | `Product.cs`, `Customer.cs` |
| **Interface** | `I` + PascalCase | `IProductService.cs`, `IRepository<T>` |
| **Service** | PascalCase + `Service` | `ProductService.cs`, `VATCalculationService.cs` |
| **Repository** | PascalCase + `Repository` | `ProductRepository.cs`, `TransactionRepository.cs` |
| **DTO** | PascalCase + `Dto` | `CreateProductDto.cs`, `ProductDetailsDto.cs` |
| **Validator** | PascalCase + `Validator` | `CreateProductValidator.cs` |
| **DbContext** | PascalCase + `DbContext` | `PosDbContext.cs`, `ManagementDbContext.cs` |
| **Enum** | PascalCase | `TransactionStatus.cs`, `PaymentMethod.cs` |
| **Test Class** | ClassName + `Tests` | `ProductServiceTests.cs` |

### Methods

| Type | Pattern | Example |
|------|---------|---------|
| **CRUD** | `Create/Get/Update/Delete` + Entity | `CreateProduct`, `GetProductById` |
| **Async** | MethodName + `Async` | `GetProductByIdAsync` |
| **Boolean** | `Is/Has/Can/Should` | `IsAgeVerified`, `HasValidLicense` |
| **Validation** | `Validate` + Context | `ValidateAgeRequirement` |
| **Calculation** | `Calculate` + What | `CalculateVAT`, `CalculateLineTotal` |

### Variables

```csharp
// Private fields: _camelCase
private readonly IProductService _productService;

// Properties: PascalCase
public string ProductName { get; set; }

// Local variables: camelCase
var productList = await _productService.GetAllAsync();

// Constants: UPPER_SNAKE_CASE or PascalCase
public const string DEFAULT_CURRENCY = "ZAR";
public const decimal VAT_RATE = 0.15m;
```

---

## Layer Patterns

### 1. Models Layer (Entities)

**MANDATORY**: All entities MUST inherit from `AuditableEntity`

```csharp
using Project420.Shared.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project420.<Module>.Models.Entities
{
    /// <summary>
    /// <Entity purpose and compliance notes>
    /// Complies with: SA Cannabis for Private Purposes Act 2024, SAHPRA GMP, POPIA
    /// </summary>
    public class <EntityName> : AuditableEntity
    {
        /// <summary>
        /// <Property description with compliance notes>
        /// </summary>
        [Required]
        [StringLength(50)]
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// THC percentage content (Cannabis Act compliance)
        /// </summary>
        [StringLength(10)]
        public string? THCPercentage { get; set; }

        /// <summary>
        /// Batch number for traceability (SAHPRA GMP requirement)
        /// </summary>
        [StringLength(100)]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// VAT-inclusive price (SA pricing standard)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        // Navigation Properties
        public virtual ICollection<RelatedEntity>? RelatedEntities { get; set; }
    }
}
```

**Entity Checklist**:
- [ ] Inherits from `AuditableEntity`
- [ ] XML documentation for class and ALL properties
- [ ] Compliance annotations (Cannabis Act, SAHPRA, POPIA)
- [ ] Data annotations for validation
- [ ] Decimal properties use `[Column(TypeName = "decimal(18,2)")]`
- [ ] Nullable reference types (`string?`)
- [ ] Navigation properties marked `virtual`
- [ ] Cannabis-specific fields (THC%, CBD%, BatchNumber, LabTestDate)

### 2. DAL Layer (Repositories)

**Repository Interface**:

```csharp
namespace Project420.<Module>.DAL.Repositories
{
    /// <summary>
    /// Repository interface for <Entity> operations.
    /// </summary>
    public interface I<Entity>Repository
    {
        // Basic CRUD
        Task<<Entity>?> GetByIdAsync(int id);
        Task<IEnumerable<<Entity>>> GetAllAsync();
        Task<<Entity>> CreateAsync(<Entity> entity);
        Task<<Entity>> UpdateAsync(<Entity> entity);
        Task DeleteAsync(int id);

        // Business queries
        Task<IEnumerable<<Entity>>> Get<Condition>Async();
        Task<<Entity>?> GetBy<Property>Async(string value);
    }
}
```

**Repository Implementation**:

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.<Module>.DAL.Repositories;
using Project420.<Module>.Models.Entities;

namespace Project420.<Module>.DAL.Repositories
{
    /// <summary>
    /// Repository implementation for <Entity>.
    /// Provides data access with cannabis compliance checks.
    /// </summary>
    public class <Entity>Repository : I<Entity>Repository
    {
        private readonly <Module>DbContext _context;

        public <Entity>Repository(<Module>DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<<Entity>?> GetByIdAsync(int id)
        {
            return await _context.<Entities>
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<<Entity>>> GetAllAsync()
        {
            return await _context.<Entities>
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<<Entity>> CreateAsync(<Entity> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.<Entities>.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<<Entity>> UpdateAsync(<Entity> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.<Entities>.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.<Entities>.Remove(entity); // Soft delete via DbContext
                await _context.SaveChangesAsync();
            }
        }
    }
}
```

**DbContext Pattern**:

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.<Module>.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.<Module>.DAL
{
    public class <Module>DbContext : DbContext
    {
        public <Module>DbContext(DbContextOptions<<Module>DbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<<Entity>> <Entities> { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(<Module>DbContext).Assembly);

            // Global query filters (soft delete - POPIA)
            modelBuilder.Entity<<Entity>>().HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Override SaveChangesAsync to populate audit fields (POPIA compliance).
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is AuditableEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted));

            foreach (var entityEntry in entries)
            {
                var entity = (AuditableEntity)entityEntry.Entity;

                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = DateTime.UtcNow;
                        entity.CreatedBy = "SYSTEM"; // TODO: Get from auth context
                        break;
                    case EntityState.Modified:
                        entity.ModifiedAt = DateTime.UtcNow;
                        entity.ModifiedBy = "SYSTEM";
                        break;
                    case EntityState.Deleted:
                        // Soft delete (POPIA)
                        entityEntry.State = EntityState.Modified;
                        entity.IsDeleted = true;
                        entity.DeletedAt = DateTime.UtcNow;
                        entity.DeletedBy = "SYSTEM";
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### 3. BLL Layer (Services)

**Service Interface**:

```csharp
namespace Project420.<Module>.BLL.<Domain>.Services
{
    /// <summary>
    /// Service interface for <Entity> management.
    /// Handles business logic and validation for cannabis <entity> management.
    /// </summary>
    public interface I<Entity>Service
    {
        // CRUD operations (using DTOs)
        Task<<Entity>DetailsDto> Create<Entity>Async(Create<Entity>Dto createDto);
        Task<<Entity>DetailsDto?> Get<Entity>ByIdAsync(int id);
        Task<IEnumerable<<Entity>DetailsDto>> GetAll<Entities>Async();
        Task<<Entity>DetailsDto> Update<Entity>Async(int id, Update<Entity>Dto updateDto);
        Task Delete<Entity>Async(int id);

        // Business operations
        Task<IEnumerable<<Entity>DetailsDto>> GetActive<Entities>Async();
        Task<bool> Validate<Condition>Async(...);
    }
}
```

**Service Implementation**:

```csharp
using AutoMapper;
using FluentValidation;
using Project420.<Module>.BLL.<Domain>.DTOs;
using Project420.<Module>.DAL.Repositories;
using Project420.<Module>.Models.Entities;

namespace Project420.<Module>.BLL.<Domain>.Services
{
    /// <summary>
    /// Service implementation for <Entity> management.
    /// Enforces business rules and cannabis compliance requirements.
    /// </summary>
    public class <Entity>Service : I<Entity>Service
    {
        private readonly I<Entity>Repository _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<Create<Entity>Dto> _createValidator;
        private readonly IValidator<Update<Entity>Dto> _updateValidator;

        public <Entity>Service(
            I<Entity>Repository repository,
            IMapper mapper,
            IValidator<Create<Entity>Dto> createValidator,
            IValidator<Update<Entity>Dto> updateValidator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        public async Task<<Entity>DetailsDto> Create<Entity>Async(Create<Entity>Dto createDto)
        {
            // Validate
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Map DTO to entity
            var entity = _mapper.Map<<Entity>>(createDto);

            // Create
            var createdEntity = await _repository.CreateAsync(entity);

            // Map back to DTO and return
            return _mapper.Map<<Entity>DetailsDto>(createdEntity);
        }

        // ... other CRUD methods following same pattern
    }
}
```

**Validator Pattern**:

```csharp
using FluentValidation;
using Project420.<Module>.BLL.<Domain>.DTOs;

namespace Project420.<Module>.BLL.<Domain>.Validators
{
    /// <summary>
    /// Validator for <Entity> creation.
    /// Enforces cannabis industry compliance requirements.
    /// </summary>
    public class Create<Entity>Validator : AbstractValidator<Create<Entity>Dto>
    {
        public Create<Entity>Validator()
        {
            // Basic validation
            RuleFor(x => x.PropertyName)
                .NotEmpty().WithMessage("Property is required")
                .MaximumLength(50).WithMessage("Property cannot exceed 50 characters");

            // Cannabis compliance validation
            RuleFor(x => x.THCPercentage)
                .Matches(@"^\d{1,2}(\.\d{1,2})?%?$")
                .WithMessage("THC percentage must be in format: 15.5 or 15.5%")
                .When(x => !string.IsNullOrWhiteSpace(x.THCPercentage));

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number required (SAHPRA GMP requirement)")
                .MaximumLength(100);

            RuleFor(x => x.LabTestDate)
                .NotNull().WithMessage("Lab test date required (Cannabis Act)")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Cannot be in future");

            // Age verification (if applicable)
            RuleFor(x => x.DateOfBirth)
                .Must(BeAtLeast18YearsOld)
                .WithMessage("Must be 18 or older (Cannabis Act 2024)");
        }

        private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
            return age >= 18;
        }
    }
}
```

---

## Compliance Patterns (MANDATORY)

### 1. Age Verification (Cannabis Act 2024)

```csharp
/// <summary>
/// Validates customer meets minimum age requirement (18+).
/// Cannabis for Private Purposes Act 2024 compliance.
/// </summary>
public bool IsAgeVerified(DateTime dateOfBirth)
{
    const int MINIMUM_AGE = 18;
    var today = DateTime.Today;
    var age = today.Year - dateOfBirth.Year;

    if (dateOfBirth.Date > today.AddYears(-age))
        age--;

    return age >= MINIMUM_AGE;
}
```

### 2. Batch Tracking (SAHPRA GMP)

```csharp
/// <summary>
/// Assigns unique batch number for traceability.
/// SAHPRA GMP compliance: seed-to-sale tracking requirement.
/// </summary>
public string GenerateBatchNumber(string productType, DateTime harvestDate)
{
    var datePart = harvestDate.ToString("yyyyMMdd");
    var typePart = productType.ToUpper().Substring(0, Math.Min(4, productType.Length));
    var sequencePart = GetNextBatchSequence().ToString("D3");

    return $"BATCH-{datePart}-{typePart}-{sequencePart}";
}
```

### 3. Lab Testing Validation (SAHPRA)

```csharp
/// <summary>
/// Validates product has valid Certificate of Analysis (COA).
/// SAHPRA requirement: ISO/IEC 17025 accredited lab testing.
/// </summary>
public ValidationResult ValidateLabTesting(Product product)
{
    var errors = new List<string>();

    if (!product.LabTestDate.HasValue)
        errors.Add("Lab testing date required (SAHPRA compliance)");

    if (product.LabTestDate.HasValue &&
        product.LabTestDate.Value < DateTime.UtcNow.AddMonths(-12))
        errors.Add("Lab testing older than 12 months - retesting required");

    if (string.IsNullOrWhiteSpace(product.THCPercentage) ||
        string.IsNullOrWhiteSpace(product.CBDPercentage))
        errors.Add("THC and CBD content must be documented (Cannabis Act)");

    return new ValidationResult(errors);
}
```

### 4. Audit Trail (POPIA)

```csharp
/// <summary>
/// Creates immutable audit log entry.
/// POPIA compliance: 7-year retention, track all data modifications.
/// </summary>
public async Task CreateAuditLogAsync(
    string userId,
    string action,
    string entityType,
    int entityId,
    object? oldValues = null,
    object? newValues = null)
{
    var auditLog = new AuditLog
    {
        UserId = userId,
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
        NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
        Timestamp = DateTime.UtcNow,
        IpAddress = GetClientIpAddress(),
        UserAgent = GetClientUserAgent()
    };

    await _auditLogRepository.CreateAsync(auditLog);
}
```

### 5. VAT Calculation (SA Tax)

```csharp
/// <summary>
/// Calculates VAT for SA transactions (15% VAT-inclusive).
/// SARS compliance: accurate VAT breakdown required.
/// </summary>
public decimal CalculateVAT(decimal vatInclusiveAmount)
{
    const decimal VAT_RATE = 0.15m;
    const decimal VAT_DIVISOR = 1.15m;

    var vatExclusive = vatInclusiveAmount / VAT_DIVISOR;
    var vat = vatInclusiveAmount - vatExclusive;

    return Math.Round(vat, 2);
}
```

---

## Development Workflow

### Creating a New Feature (Step-by-Step)

```
1. PLAN
   ├── Read BOTH compliance guides (MANDATORY)
   ├── Check if feature requires new module or fits existing
   ├── Identify compliance requirements (Age?, Batch?, Lab Test?)
   └── Plan database changes (migrations)

2. MODELS
   ├── Create/update entities in <Module>.Models/Entities/
   ├── Ensure inherits from AuditableEntity
   ├── Add compliance fields (THC%, BatchNumber, LabTestDate, etc.)
   ├── Add XML documentation with compliance notes
   └── Create enums if needed

3. DAL
   ├── Update DbContext with new DbSet
   ├── Create repository interface (I<Entity>Repository)
   ├── Create repository implementation
   ├── Add entity configuration (Fluent API)
   ├── Create migration: dotnet ef migrations add <Name>
   └── Test migration: dotnet ef database update

4. BLL
   ├── Create DTOs (Create, Update, Details)
   ├── Create service interface (I<Entity>Service)
   ├── Create service implementation
   ├── Create validators (FluentValidation)
   ├── Add AutoMapper mappings
   └── Enforce compliance rules in validators

5. UI (if needed)
   ├── Create Blazor pages/components
   ├── Use BaseForm component (MANDATORY for forms)
   ├── Inject services
   ├── Add age verification UI (if needed)
   └── Display compliance info (batch numbers, test dates)

6. DI REGISTRATION
   ├── Register DbContext in Program.cs
   ├── Register repositories (Scoped)
   ├── Register services (Scoped)
   └── Register validators (from assembly)

7. TESTING
   ├── Write unit tests for service
   ├── Write unit tests for validators
   ├── Test compliance scenarios (age, batch, lab test)
   ├── Verify soft delete (POPIA)
   └── Aim for 70%+ coverage

8. BUILD & VERIFY
   ├── Build solution (0 errors, 0 warnings)
   ├── Run all tests (100% pass rate)
   ├── Test manually in UI
   └── Verify compliance requirements met
```

---

## Quality Gates

### Before Committing Code

✅ **Architecture**:
- [ ] Follows 3-tier structure (UI → BLL → DAL → Models)
- [ ] No circular dependencies
- [ ] Interfaces used for all services/repositories
- [ ] DTOs used (entities never exposed)
- [ ] Dependency injection correctly implemented

✅ **Compliance** (CRITICAL):
- [ ] Both SA Cannabis Guides consulted
- [ ] POPIA data protection implemented (soft delete, audit trails)
- [ ] Cannabis Act requirements met (age verification, batch tracking)
- [ ] SAHPRA GMP standards followed (lab testing, traceability)
- [ ] SA VAT calculation correct (15% VAT-inclusive)
- [ ] Compliance comments in code

✅ **Code Quality**:
- [ ] XML documentation for all public members
- [ ] Naming conventions followed
- [ ] No hard-coded values
- [ ] Async/await used correctly
- [ ] Proper exception handling
- [ ] Logging at appropriate levels

✅ **Testing**:
- [ ] Unit tests written
- [ ] Compliance scenarios tested
- [ ] Test coverage > 70%
- [ ] All tests passing

✅ **Build**:
- [ ] 0 errors
- [ ] 0 warnings
- [ ] Clean build

---

## Quick Reference: Shared Services

### VAT Calculation Service

```csharp
// Inject: IVATCalculationService
await _vatService.CalculateTransactionAsync(details);
```

### Transaction Number Generator

```csharp
// Inject: ITransactionNumberGeneratorService
var transactionNumber = _numberService.GenerateTransactionNumber(TransactionType.Sale);
// Format: SALE-YYYYMMDD-XXX
```

### Audit Log Service

```csharp
// Inject: IAuditLogService
await _auditService.CreateAuditLogAsync(userId, "CREATE", "Product", productId, null, newProduct);
```

### Cannabis Compliance Service

```csharp
// Inject: ICannabisComplianceService
var isCompliant = await _complianceService.ValidateProductComplianceAsync(product);
```

---

## Key Documentation References

### MUST READ (Compliance)
1. **SA_Cannabis_Software_Guide.md** - Legal framework, tax, features, POPIA
2. **south-africa-cannabis-cultivation-production-laws-guide.md** - SAHPRA, DALRRD, GMP

### Technical Standards
3. **docs/CODING-STANDARDS.md** - Complete coding standards (1800+ lines)
4. **docs/PROJECT_STANDARDS.md** - UI/Blazor standards
5. **docs/FOLDER-STRUCTURE-EXPLAINED.md** - Detailed structure explanation

### Architecture & Workflow
6. **docs/ARCHITECTURE.md** - Technical architecture
7. **docs/UNIT-TESTING-GUIDE.md** - Testing patterns

---

## 🚨 CRITICAL REMINDERS

1. **ALWAYS Reference BOTH Compliance Guides** - Non-negotiable
2. **ALWAYS Inherit from AuditableEntity** - POPIA requirement
3. **ALWAYS Use DTOs** - Never expose entities to UI/API
4. **ALWAYS Validate** - FluentValidation required
5. **ALWAYS Document** - XML documentation mandatory
6. **ALWAYS Test** - Unit tests for all services
7. **ALWAYS Check Compliance** - Age, Batch, Lab Test, Audit
8. **ALWAYS Use BaseForm** - For all Blazor forms
9. **ALWAYS Soft Delete** - POPIA data retention
10. **ALWAYS Calculate VAT Correctly** - 15% VAT-inclusive

---

**EFFECTIVE**: IMMEDIATELY
**STATUS**: MANDATORY REFERENCE
**VERSION**: 1.0
**LAST UPDATED**: 2025-12-08

**Remember**: This template provides the structure, the SA Cannabis Guides provide the compliance requirements. Use BOTH together for all development.
