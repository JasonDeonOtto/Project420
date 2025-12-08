# Project420 - Comprehensive Coding Standards

**Last Updated**: 2025-12-07
**Status**: MANDATORY
**Applies To**: ALL development in Project420 (Models, DAL, BLL, UI, API, Tests)

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Naming Conventions](#naming-conventions)
3. [Project Structure](#project-structure)
4. [Models Layer Standards](#models-layer-standards)
5. [DAL (Data Access Layer) Standards](#dal-data-access-layer-standards)
6. [BLL (Business Logic Layer) Standards](#bll-business-logic-layer-standards)
7. [UI Layer Standards](#ui-layer-standards)
8. [API Layer Standards](#api-layer-standards)
9. [Dependency Injection Patterns](#dependency-injection-patterns)
10. [Testing Standards](#testing-standards)
11. [Cannabis Compliance Patterns](#cannabis-compliance-patterns)
12. [Error Handling & Logging](#error-handling--logging)
13. [Validation Patterns](#validation-patterns)
14. [Code Review Checklist](#code-review-checklist)

---

## Architecture Overview

### 3-Tier Modular Architecture

Project420 follows a **strict 3-tier architecture** with **modular separation**:

```
┌────────────────────────────────────────────────────────────┐
│  PRESENTATION LAYER (UI)                                   │
│  ├── Blazor Server/WebAssembly (UI.Blazor)                │
│  ├── MAUI Mobile (UI.Maui) - Future                       │
│  └── ASP.NET Core Web API (API.WebApi)                    │
├────────────────────────────────────────────────────────────┤
│  BUSINESS LOGIC LAYER (BLL)                                │
│  ├── Services (IService, ServiceImpl)                     │
│  ├── DTOs (Data Transfer Objects)                         │
│  ├── Validators (FluentValidation)                        │
│  └── Business Rules & Calculations                        │
├────────────────────────────────────────────────────────────┤
│  DATA ACCESS LAYER (DAL)                                   │
│  ├── DbContexts (EF Core)                                 │
│  ├── Repositories (IRepository, RepositoryImpl)           │
│  ├── Migrations                                           │
│  └── Database Configuration                               │
├────────────────────────────────────────────────────────────┤
│  MODELS LAYER (Entities)                                   │
│  ├── Domain Entities                                      │
│  ├── Enumerations                                         │
│  ├── Base Classes (AuditableEntity)                       │
│  └── Interfaces                                           │
└────────────────────────────────────────────────────────────┘
         ↕
    [Database]
```

### Key Principles

1. **Separation of Concerns**: Each layer has a single, well-defined responsibility
2. **Dependency Flow**: UI → BLL → DAL → Models (never reverse)
3. **Interface Segregation**: All services and repositories use interfaces
4. **Dependency Injection**: All dependencies injected via constructor
5. **Modular Design**: Each business domain is a separate module
6. **Shared Infrastructure**: Common services in Shared projects

---

## Naming Conventions

### Projects

```
Format: Project420.<Module>.<Layer>[.<SubLayer>]

Examples:
- Project420.Shared.Core              (Shared models, enums, interfaces)
- Project420.Shared.Infrastructure    (Shared services like VAT, audit)
- Project420.Management.Models        (Management module entities)
- Project420.Management.DAL           (Management module data access)
- Project420.Management.BLL           (Management module business logic)
- Project420.Management.UI.Blazor     (Management module Blazor UI)
- Project420.Retail.POS.Models        (POS module entities)
- Project420.Retail.POS.DAL           (POS data access)
- Project420.Retail.POS.BLL           (POS business logic)
- Project420.Retail.POS.UI.Blazor     (POS Blazor UI)
- Project420.API.WebApi               (REST API)
```

### Files & Classes

| Type | Naming Pattern | Example |
|------|---------------|---------|
| **Entity** | PascalCase, singular noun | `Product.cs`, `Debtor.cs` |
| **Interface** | `I` + PascalCase | `IProductService.cs`, `IRepository<T>` |
| **Service** | PascalCase + `Service` | `ProductService.cs`, `VATCalculationService.cs` |
| **Repository** | PascalCase + `Repository` | `ProductRepository.cs`, `TransactionRepository.cs` |
| **DTO** | PascalCase + `Dto` | `CreateProductDto.cs`, `CheckoutRequestDto.cs` |
| **Validator** | PascalCase + `Validator` | `CreateProductValidator.cs`, `CustomerRegistrationValidator.cs` |
| **DbContext** | PascalCase + `DbContext` | `PosDbContext.cs`, `ManagementDbContext.cs` |
| **Enum** | PascalCase, plural or singular | `TransactionStatus.cs`, `PaymentMethod.cs` |
| **Test Class** | ClassName + `Tests` | `ProductServiceTests.cs`, `VATCalculationServiceTests.cs` |

### Methods

| Type | Naming Pattern | Example |
|------|---------------|---------|
| **CRUD Operations** | `Create`, `Get`, `Update`, `Delete` + Entity | `CreateProduct`, `GetProductById`, `UpdateProduct`, `DeleteProduct` |
| **Async Methods** | MethodName + `Async` | `GetProductByIdAsync`, `CreateCustomerAsync` |
| **Boolean Queries** | `Is`, `Has`, `Can`, `Should` | `IsAgeVerified`, `HasValidLicense`, `CanProcessRefund` |
| **Validation** | `Validate` + Context | `ValidateAgeRequirement`, `ValidateStockAvailability` |
| **Calculation** | `Calculate` + What | `CalculateVAT`, `CalculateLineTotal` |

### Variables

```csharp
// Private fields: _camelCase
private readonly IProductService _productService;
private string _cachedValue;

// Properties: PascalCase
public string ProductName { get; set; }
public decimal Price { get; set; }

// Local variables: camelCase
var productList = await _productService.GetAllAsync();
decimal totalAmount = CalculateTotal(items);

// Constants: UPPER_SNAKE_CASE or PascalCase
public const string DEFAULT_CURRENCY = "ZAR";
public const decimal VAT_RATE = 0.15m;

// Parameters: camelCase
public async Task<Product> GetProductById(int productId)
{
    // ...
}
```

---

## Project Structure

### Module Organization

Each module follows this structure:

```
Modules/
└── <ModuleName>/
    ├── <ModuleName>/                          # Logical grouping
    │   ├── Project420.<ModuleName>.Models/    # Entities, Enums
    │   ├── Project420.<ModuleName>.DAL/       # Data Access
    │   ├── Project420.<ModuleName>.BLL/       # Business Logic
    │   └── Project420.<ModuleName>.UI.Blazor/ # Blazor UI
    │
    └── Submodules (if applicable)/
        └── <SubmoduleName>/
            ├── Project420.<Module>.<Submodule>.Models/
            ├── Project420.<Module>.<Submodule>.DAL/
            ├── Project420.<Module>.<Submodule>.BLL/
            └── Project420.<Module>.<Submodule>.UI.Blazor/

Example:
Modules/
├── Management/
│   ├── Project420.Management.Models/
│   ├── Project420.Management.DAL/
│   ├── Project420.Management.BLL/
│   └── Project420.Management.UI.Blazor/
│
└── Retail/
    └── POS/
        ├── Project420.Retail.POS.Models/
        ├── Project420.Retail.POS.DAL/
        ├── Project420.Retail.POS.BLL/
        └── Project420.Retail.POS.UI.Blazor/
```

### Folder Structure Within Projects

#### Models Project

```
Project420.<Module>.Models/
├── Entities/
│   ├── Product.cs
│   ├── Debtor.cs
│   └── Pricelist.cs
├── Enums/
│   ├── TransactionStatus.cs
│   ├── PaymentMethod.cs
│   └── DebtorCategory.cs
└── Interfaces/
    └── ICustomEntity.cs (if needed)
```

#### DAL Project

```
Project420.<Module>.DAL/
├── <Module>DbContext.cs
├── Repositories/
│   ├── ProductRepository.cs
│   ├── IProductRepository.cs
│   ├── CustomerRepository.cs
│   └── ICustomerRepository.cs
├── Migrations/
│   ├── 20251201_InitialCreate.cs
│   └── 20251205_AddProductBarcodes.cs
└── Configurations/ (EF Core fluent configurations)
    ├── ProductConfiguration.cs
    └── DebtorConfiguration.cs
```

#### BLL Project

```
Project420.<Module>.BLL/
├── Services/
│   ├── CustomerService.cs
│   ├── ICustomerService.cs
│   ├── ProductService.cs
│   └── IProductService.cs
├── DTOs/
│   ├── CreateProductDto.cs
│   ├── UpdateProductDto.cs
│   └── ProductDetailsDto.cs
└── Validators/
    ├── CreateProductValidator.cs
    ├── UpdateProductValidator.cs
    └── CustomerRegistrationValidator.cs
```

#### UI.Blazor Project

```
Project420.<Module>.UI.Blazor/
├── Components/
│   ├── Pages/
│   │   ├── Products/
│   │   │   ├── ProductList.razor
│   │   │   ├── ProductCreate.razor
│   │   │   └── ProductEdit.razor
│   │   └── Customers/
│   │       ├── CustomerList.razor
│   │       └── CustomerCreate.razor
│   └── Shared/
│       └── ModuleNavMenu.razor
├── wwwroot/
│   ├── css/
│   │   └── module-styles.css
│   └── images/
└── Program.cs (DI registration)
```

---

## Models Layer Standards

### Base Entity Pattern

**MANDATORY**: All entities MUST inherit from `AuditableEntity` for POPIA compliance:

```csharp
using Project420.Shared.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project420.Management.Models.Entities
{
    /// <summary>
    /// Represents a product in the cannabis inventory system.
    /// Complies with SA Cannabis for Private Purposes Act 2024 and SAHPRA GMP standards.
    /// </summary>
    public class Product : AuditableEntity
    {
        /// <summary>
        /// Stock Keeping Unit - unique product identifier
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Product name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Product description
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// THC percentage content (Cannabis Act compliance)
        /// Format: "15.5%" or "15.5"
        /// </summary>
        [StringLength(10)]
        public string? THCPercentage { get; set; }

        /// <summary>
        /// CBD percentage content
        /// Format: "0.5%" or "0.5"
        /// </summary>
        [StringLength(10)]
        public string? CBDPercentage { get; set; }

        /// <summary>
        /// Batch number for traceability (SAHPRA GMP requirement)
        /// </summary>
        [StringLength(100)]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Cannabis strain name
        /// </summary>
        [StringLength(100)]
        public string? StrainName { get; set; }

        /// <summary>
        /// Date of laboratory testing (COA date)
        /// </summary>
        public DateTime? LabTestDate { get; set; }

        /// <summary>
        /// Product expiry date
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// VAT-inclusive price (SA pricing standard)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal Price { get; set; }

        /// <summary>
        /// Cost price for profit calculations
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Current stock on hand
        /// </summary>
        public int StockOnHand { get; set; }

        /// <summary>
        /// Minimum stock level before reorder alert
        /// </summary>
        public int ReorderLevel { get; set; } = 10;

        /// <summary>
        /// Whether product is active for sale
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<POSTransactionDetail>? TransactionDetails { get; set; }
    }
}
```

### Entity Standards Checklist

- [ ] Inherits from `AuditableEntity`
- [ ] XML documentation for class and ALL public properties
- [ ] Compliance annotations in comments (Cannabis Act, SAHPRA, POPIA)
- [ ] Data annotations for validation (`[Required]`, `[StringLength]`, `[Range]`)
- [ ] Decimal properties use `[Column(TypeName = "decimal(18,2)")]`
- [ ] Nullable reference types used correctly (`string?`)
- [ ] Navigation properties marked as `virtual`
- [ ] Default values set where appropriate
- [ ] Cannabis-specific fields included (THC%, CBD%, BatchNumber, LabTestDate)

### Enum Standards

```csharp
namespace Project420.Shared.Core.Enums
{
    /// <summary>
    /// Payment methods supported by the POS system.
    /// Complies with SA banking regulations and cannabis industry restrictions.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Cash payment (most common in cannabis industry due to banking restrictions)
        /// </summary>
        Cash = 1,

        /// <summary>
        /// Credit/debit card payment (when banking access available)
        /// </summary>
        Card = 2,

        /// <summary>
        /// Electronic Funds Transfer
        /// </summary>
        EFT = 3,

        /// <summary>
        /// Mobile payment (e.g., SnapScan, Zapper, PayFast)
        /// </summary>
        MobilePayment = 4,

        /// <summary>
        /// Account payment for wholesale/credit customers
        /// </summary>
        OnAccount = 5,

        /// <summary>
        /// Voucher or gift certificate
        /// </summary>
        Voucher = 6
    }
}
```

---

## DAL (Data Access Layer) Standards

### DbContext Pattern

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.Models.Entities;

namespace Project420.Retail.POS.DAL
{
    /// <summary>
    /// Database context for the Point of Sale module.
    /// Manages transactions, payments, and related entities.
    /// </summary>
    public class PosDbContext : DbContext
    {
        public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<POSTransactionHeader> POSTransactionHeaders { get; set; }
        public DbSet<POSTransactionDetail> POSTransactionDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Debtor> Debtors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PosDbContext).Assembly);

            // Global query filters (soft delete - POPIA compliance)
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Debtor>().HasQueryFilter(d => !d.IsDeleted);
        }

        /// <summary>
        /// Override SaveChangesAsync to populate audit fields.
        /// POPIA compliance: track all data modifications.
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
                        entity.ModifiedBy = "SYSTEM"; // TODO: Get from auth context
                        break;

                    case EntityState.Deleted:
                        // Soft delete (POPIA requirement)
                        entityEntry.State = EntityState.Modified;
                        entity.IsDeleted = true;
                        entity.DeletedAt = DateTime.UtcNow;
                        entity.DeletedBy = "SYSTEM"; // TODO: Get from auth context
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### Repository Pattern

**Interface:**

```csharp
namespace Project420.Management.DAL.Repositories
{
    /// <summary>
    /// Repository interface for Product entity operations.
    /// </summary>
    public interface IProductRepository
    {
        // Basic CRUD
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(int id);

        // Business queries
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<Product?> GetBySkuAsync(string sku);
        Task<IEnumerable<Product>> GetByBatchNumberAsync(string batchNumber);
        Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysFromNow);
    }
}
```

**Implementation:**

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL.Repositories;
using Project420.Management.Models.Entities;

namespace Project420.Management.DAL.Repositories
{
    /// <summary>
    /// Repository implementation for Product entity.
    /// Provides data access methods with cannabis compliance checks.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ManagementDbContext _context;

        public ProductRepository(ManagementDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product or null if not found</returns>
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Gets all products (active and inactive).
        /// </summary>
        /// <returns>List of all products</returns>
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">Product to create</param>
        /// <returns>Created product with ID</returns>
        public async Task<Product> CreateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="product">Product to update</param>
        /// <returns>Updated product</returns>
        public async Task<Product> UpdateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        /// <summary>
        /// Soft deletes a product (POPIA compliance).
        /// </summary>
        /// <param name="id">Product ID to delete</param>
        public async Task DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product); // Soft delete via DbContext override
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets all active products.
        /// </summary>
        /// <returns>List of active products</returns>
        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Gets products with stock below reorder level.
        /// </summary>
        /// <returns>List of low stock products</returns>
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive && p.StockOnHand <= p.ReorderLevel)
                .OrderBy(p => p.StockOnHand)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a product by SKU.
        /// </summary>
        /// <param name="sku">Stock Keeping Unit</param>
        /// <returns>Product or null if not found</returns>
        public async Task<Product?> GetBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU cannot be null or empty", nameof(sku));

            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        /// <summary>
        /// Gets products by batch number (SAHPRA compliance - batch tracking).
        /// </summary>
        /// <param name="batchNumber">Batch number</param>
        /// <returns>List of products in batch</returns>
        public async Task<IEnumerable<Product>> GetByBatchNumberAsync(string batchNumber)
        {
            if (string.IsNullOrWhiteSpace(batchNumber))
                throw new ArgumentException("Batch number cannot be null or empty", nameof(batchNumber));

            return await _context.Products
                .Where(p => p.BatchNumber == batchNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Gets products expiring within specified days (Cannabis Act compliance).
        /// </summary>
        /// <param name="daysFromNow">Number of days from today</param>
        /// <returns>List of expiring products</returns>
        public async Task<IEnumerable<Product>> GetExpiringProductsAsync(int daysFromNow)
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysFromNow);

            return await _context.Products
                .Where(p => p.IsActive
                    && p.ExpiryDate.HasValue
                    && p.ExpiryDate <= expiryDate)
                .OrderBy(p => p.ExpiryDate)
                .ToListAsync();
        }
    }
}
```

### Repository Standards Checklist

- [ ] Interface-based design (`IRepository`)
- [ ] XML documentation for ALL public methods
- [ ] Async/await for all database operations
- [ ] Null checking for parameters
- [ ] Dependency injection via constructor
- [ ] Basic CRUD methods included
- [ ] Business-specific query methods
- [ ] Cannabis compliance methods (batch tracking, expiry)
- [ ] Soft delete support (POPIA)
- [ ] Proper exception handling

---

## BLL (Business Logic Layer) Standards

### Service Pattern

**Interface:**

```csharp
namespace Project420.Management.BLL.Services
{
    /// <summary>
    /// Service interface for product management operations.
    /// Handles business logic and validation for cannabis product management.
    /// </summary>
    public interface IProductService
    {
        // CRUD operations (using DTOs)
        Task<ProductDetailsDto> CreateProductAsync(CreateProductDto createDto);
        Task<ProductDetailsDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDetailsDto>> GetAllProductsAsync();
        Task<ProductDetailsDto> UpdateProductAsync(int id, UpdateProductDto updateDto);
        Task DeleteProductAsync(int id);

        // Business operations
        Task<IEnumerable<ProductDetailsDto>> GetActiveProductsAsync();
        Task<IEnumerable<ProductDetailsDto>> GetLowStockProductsAsync();
        Task<ProductDetailsDto?> GetProductBySkuAsync(string sku);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null);
        Task<bool> HasSufficientStockAsync(int productId, int quantity);
        Task UpdateStockAsync(int productId, int quantityChange);
    }
}
```

**Implementation:**

```csharp
using AutoMapper;
using FluentValidation;
using Project420.Management.BLL.DTOs;
using Project420.Management.BLL.Validators;
using Project420.Management.DAL.Repositories;
using Project420.Management.Models.Entities;

namespace Project420.Management.BLL.Services
{
    /// <summary>
    /// Service implementation for product management.
    /// Enforces business rules and cannabis compliance requirements.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateProductDto> _createValidator;
        private readonly IValidator<UpdateProductDto> _updateValidator;

        public ProductService(
            IProductRepository productRepository,
            IMapper mapper,
            IValidator<CreateProductDto> createValidator,
            IValidator<UpdateProductDto> updateValidator)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        }

        /// <summary>
        /// Creates a new product with validation.
        /// </summary>
        /// <param name="createDto">Product creation data</param>
        /// <returns>Created product details</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public async Task<ProductDetailsDto> CreateProductAsync(CreateProductDto createDto)
        {
            // Validate DTO
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Map DTO to entity
            var product = _mapper.Map<Product>(createDto);

            // Create product
            var createdProduct = await _productRepository.CreateAsync(product);

            // Map back to DTO and return
            return _mapper.Map<ProductDetailsDto>(createdProduct);
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details or null if not found</returns>
        public async Task<ProductDetailsDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductDetailsDto>(product);
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>List of all product details</returns>
        public async Task<IEnumerable<ProductDetailsDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDetailsDto>>(products);
        }

        /// <summary>
        /// Updates an existing product with validation.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateDto">Product update data</param>
        /// <returns>Updated product details</returns>
        /// <exception cref="KeyNotFoundException">Thrown when product not found</exception>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        public async Task<ProductDetailsDto> UpdateProductAsync(int id, UpdateProductDto updateDto)
        {
            // Check if product exists
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            // Validate DTO
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Map DTO to entity (preserving ID and audit fields)
            _mapper.Map(updateDto, existingProduct);

            // Update product
            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);

            // Map back to DTO and return
            return _mapper.Map<ProductDetailsDto>(updatedProduct);
        }

        /// <summary>
        /// Soft deletes a product (POPIA compliance).
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <exception cref="KeyNotFoundException">Thrown when product not found</exception>
        public async Task DeleteProductAsync(int id)
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            await _productRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Gets all active products.
        /// </summary>
        /// <returns>List of active product details</returns>
        public async Task<IEnumerable<ProductDetailsDto>> GetActiveProductsAsync()
        {
            var products = await _productRepository.GetActiveProductsAsync();
            return _mapper.Map<IEnumerable<ProductDetailsDto>>(products);
        }

        /// <summary>
        /// Gets products with low stock levels.
        /// </summary>
        /// <returns>List of low stock product details</returns>
        public async Task<IEnumerable<ProductDetailsDto>> GetLowStockProductsAsync()
        {
            var products = await _productRepository.GetLowStockProductsAsync();
            return _mapper.Map<IEnumerable<ProductDetailsDto>>(products);
        }

        /// <summary>
        /// Gets a product by SKU.
        /// </summary>
        /// <param name="sku">Stock Keeping Unit</param>
        /// <returns>Product details or null if not found</returns>
        public async Task<ProductDetailsDto?> GetProductBySkuAsync(string sku)
        {
            var product = await _productRepository.GetBySkuAsync(sku);
            return product == null ? null : _mapper.Map<ProductDetailsDto>(product);
        }

        /// <summary>
        /// Checks if a SKU is unique in the system.
        /// </summary>
        /// <param name="sku">SKU to check</param>
        /// <param name="excludeId">Product ID to exclude from check (for updates)</param>
        /// <returns>True if SKU is unique, false otherwise</returns>
        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null)
        {
            var existingProduct = await _productRepository.GetBySkuAsync(sku);

            if (existingProduct == null)
                return true;

            if (excludeId.HasValue && existingProduct.Id == excludeId.Value)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a product has sufficient stock for a transaction.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="quantity">Quantity required</param>
        /// <returns>True if sufficient stock available, false otherwise</returns>
        public async Task<bool> HasSufficientStockAsync(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product != null && product.StockOnHand >= quantity;
        }

        /// <summary>
        /// Updates stock level for a product.
        /// Positive value increases stock, negative value decreases stock.
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="quantityChange">Quantity to add or subtract</param>
        /// <exception cref="KeyNotFoundException">Thrown when product not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when stock would go negative</exception>
        public async Task UpdateStockAsync(int productId, int quantityChange)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
            }

            var newStock = product.StockOnHand + quantityChange;
            if (newStock < 0)
            {
                throw new InvalidOperationException($"Insufficient stock. Current: {product.StockOnHand}, Requested change: {quantityChange}");
            }

            product.StockOnHand = newStock;
            await _productRepository.UpdateAsync(product);
        }
    }
}
```

### Service Standards Checklist

- [ ] Interface-based design (`IService`)
- [ ] XML documentation for ALL public methods
- [ ] Async/await for all operations
- [ ] Constructor dependency injection
- [ ] Null checking for injected dependencies
- [ ] FluentValidation integration
- [ ] AutoMapper for DTO mapping
- [ ] DTOs used for input/output (never expose entities)
- [ ] Business logic centralized in service
- [ ] Proper exception handling with meaningful messages
- [ ] Cannabis compliance business rules enforced

---

## UI Layer Standards

See **`docs/PROJECT_STANDARDS.md`** for complete UI/Blazor standards including:

- BaseForm component usage (MANDATORY)
- Cannabis-themed styling
- Form structure patterns
- Button standards
- Icon usage
- Validation message display
- Alert patterns

**Key Rule**: ALL Blazor forms MUST use `<BaseForm>` component with cannabis theming.

---

## API Layer Standards

### Controller Pattern

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project420.Management.BLL.DTOs;
using Project420.Management.BLL.Services;

namespace Project420.API.WebApi.Controllers
{
    /// <summary>
    /// API controller for product management.
    /// Provides endpoints for cannabis product catalog operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Cannabis Act: Age verification required
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all active products.
        /// </summary>
        /// <returns>List of product details</returns>
        /// <response code="200">Returns the list of products</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductDetailsDto>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, "An error occurred while retrieving products");
            }
        }

        /// <summary>
        /// Gets a product by ID.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        /// <response code="200">Returns the product</response>
        /// <response code="404">Product not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDetailsDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound($"Product with ID {id} not found");

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="createDto">Product creation data</param>
        /// <returns>Created product details</returns>
        /// <response code="201">Product created successfully</response>
        /// <response code="400">Invalid product data</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDetailsDto>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "An error occurred while creating the product");
            }
        }

        /// <summary>
        /// Updates an existing product.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateDto">Product update data</param>
        /// <returns>Updated product details</returns>
        /// <response code="200">Product updated successfully</response>
        /// <response code="404">Product not found</response>
        /// <response code="400">Invalid product data</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDetailsDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateDto);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found");
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, "An error occurred while updating the product");
            }
        }

        /// <summary>
        /// Deletes a product (soft delete for POPIA compliance).
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>No content</returns>
        /// <response code="204">Product deleted successfully</response>
        /// <response code="404">Product not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Product with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product");
            }
        }
    }
}
```

### API Standards Checklist

- [ ] XML documentation for controller and ALL actions
- [ ] `[ApiController]` attribute on controller
- [ ] Proper route configuration (`[Route("api/[controller]")]`)
- [ ] Authorization attributes (`[Authorize]` for cannabis compliance)
- [ ] HTTP verb attributes (`[HttpGet]`, `[HttpPost]`, etc.)
- [ ] `ProducesResponseType` for all possible responses
- [ ] DTOs for request/response (never expose entities)
- [ ] Proper status codes (200, 201, 204, 400, 404, 500)
- [ ] Comprehensive error handling with logging
- [ ] `CreatedAtAction` for POST endpoints
- [ ] Consistent response formats

---

## Dependency Injection Patterns

### Program.cs Registration

```csharp
using Project420.Management.BLL.Services;
using Project420.Management.DAL;
using Project420.Management.DAL.Repositories;
using Project420.Shared.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// ========================================
// DBCONTEXTS
// ========================================
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ManagementDb")));

builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PosDb")));

builder.Services.AddDbContext<SharedDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SharedDb")));

// ========================================
// SHARED SERVICES (Universal)
// ========================================
builder.Services.AddScoped<IVATCalculationService, VATCalculationService>();
builder.Services.AddSingleton<ITransactionNumberGeneratorService, TransactionNumberGeneratorService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

// ========================================
// MANAGEMENT MODULE
// ========================================
// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IPricelistRepository, PricelistRepository>();

// Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPricelistService, PricelistService>();

// ========================================
// RETAIL POS MODULE
// ========================================
// Repositories
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Services
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();
builder.Services.AddScoped<IRefundService, RefundService>();

// ========================================
// AUTOMAPPER
// ========================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ========================================
// FLUENTVALIDATION
// ========================================
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

// ========================================
// BUILD APP
// ========================================
var app = builder.Build();

// Configure the HTTP request pipeline
app.Run();
```

### Service Lifetime Guidelines

| Service Type | Lifetime | Reason |
|--------------|----------|--------|
| **DbContext** | `Scoped` | Per-request database connection |
| **Repositories** | `Scoped` | Tied to DbContext lifetime |
| **Business Services** | `Scoped` | Tied to repositories and DbContext |
| **Validators** | `Scoped` | Stateless but may have dependencies |
| **Stateless Utilities** | `Singleton` | Thread-safe, no state (e.g., TransactionNumberGeneratorService) |
| **Logging** | `Singleton` | Built-in service |
| **Configuration** | `Singleton` | Built-in service |

---

## Testing Standards

### Test File Organization

```
tests/
└── Project420.<Module>.Tests/
    ├── Services/
    │   ├── ProductServiceTests.cs
    │   ├── CustomerServiceTests.cs
    │   └── VATCalculationServiceTests.cs
    ├── Validators/
    │   ├── CreateProductValidatorTests.cs
    │   └── UpdateProductValidatorTests.cs
    ├── Repositories/
    │   └── ProductRepositoryTests.cs (integration tests)
    └── Infrastructure/
        ├── ServiceTestBase.cs
        ├── RepositoryTestBase.cs
        └── TestDbContextFactory.cs
```

### Test Class Template

```csharp
using FluentAssertions;
using Moq;
using Project420.Management.BLL.Services;
using Project420.Management.DAL.Repositories;
using Project420.Management.Models.Entities;
using Xunit;

namespace Project420.Management.Tests.Services
{
    /// <summary>
    /// Unit tests for ProductService.
    /// Tests business logic and cannabis compliance rules.
    /// </summary>
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<CreateProductDto>> _mockCreateValidator;
        private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateValidator;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            // Arrange: Set up mocks
            _mockRepository = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockCreateValidator = new Mock<IValidator<CreateProductDto>>();
            _mockUpdateValidator = new Mock<IValidator<UpdateProductDto>>();

            // Create service with mocks
            _service = new ProductService(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object);
        }

        [Fact]
        public async Task CreateProductAsync_ValidProduct_ReturnsProductDto()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                SKU = "TEST001",
                Name = "Test Cannabis Flower",
                Price = 100.00m,
                CostPrice = 50.00m,
                THCPercentage = "15.5",
                CBDPercentage = "0.5",
                BatchNumber = "BATCH-001"
            };

            var validationResult = new ValidationResult(); // Empty = valid
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var product = new Product { Id = 1, SKU = "TEST001", Name = "Test Cannabis Flower" };
            _mockMapper.Setup(m => m.Map<Product>(createDto)).Returns(product);
            _mockRepository.Setup(r => r.CreateAsync(product)).ReturnsAsync(product);

            var productDto = new ProductDetailsDto { Id = 1, SKU = "TEST001", Name = "Test Cannabis Flower" };
            _mockMapper.Setup(m => m.Map<ProductDetailsDto>(product)).Returns(productDto);

            // Act
            var result = await _service.CreateProductAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.SKU.Should().Be("TEST001");
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_InvalidProduct_ThrowsValidationException()
        {
            // Arrange
            var createDto = new CreateProductDto { SKU = "", Name = "" }; // Invalid

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("SKU", "SKU is required"),
                new ValidationFailure("Name", "Name is required")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CreateProductAsync(createDto));
        }

        [Fact]
        public async Task HasSufficientStockAsync_SufficientStock_ReturnsTrue()
        {
            // Arrange
            var product = new Product { Id = 1, StockOnHand = 100 };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _service.HasSufficientStockAsync(1, 50);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasSufficientStockAsync_InsufficientStock_ReturnsFalse()
        {
            // Arrange
            var product = new Product { Id = 1, StockOnHand = 10 };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _service.HasSufficientStockAsync(1, 50);

            // Assert
            result.Should().BeFalse();
        }
    }
}
```

### Testing Standards Checklist

- [ ] xUnit test framework
- [ ] Moq for mocking
- [ ] FluentAssertions for assertions
- [ ] Test class name = `ClassName` + `Tests`
- [ ] Test method name = `MethodName_Scenario_ExpectedResult`
- [ ] Arrange-Act-Assert (AAA) pattern
- [ ] XML documentation for test class
- [ ] Each test focuses on ONE behavior
- [ ] Mock all dependencies
- [ ] Test both happy path and error cases
- [ ] Test cannabis compliance scenarios
- [ ] Verify method calls with `Times.Once`, `Times.Never`
- [ ] Use meaningful test data (realistic cannabis products)

---

## Cannabis Compliance Patterns

### Age Verification Pattern

```csharp
/// <summary>
/// Validates that a customer meets the minimum age requirement (18+).
/// Cannabis for Private Purposes Act 2024 compliance.
/// </summary>
/// <param name="dateOfBirth">Customer's date of birth</param>
/// <returns>True if customer is 18 or older</returns>
public bool IsAgeVerified(DateTime dateOfBirth)
{
    const int MINIMUM_AGE = 18;
    var today = DateTime.Today;
    var age = today.Year - dateOfBirth.Year;

    // Adjust if birthday hasn't occurred this year
    if (dateOfBirth.Date > today.AddYears(-age))
        age--;

    return age >= MINIMUM_AGE;
}
```

### Batch Tracking Pattern

```csharp
/// <summary>
/// Assigns a unique batch number to a product for traceability.
/// SAHPRA GMP compliance: seed-to-sale tracking requirement.
/// </summary>
/// <param name="productType">Type of cannabis product</param>
/// <param name="harvestDate">Date of harvest</param>
/// <returns>Unique batch number in format: BATCH-YYYYMMDD-TYPE-XXX</returns>
public string GenerateBatchNumber(string productType, DateTime harvestDate)
{
    var datePart = harvestDate.ToString("yyyyMMdd");
    var typePart = productType.ToUpper().Substring(0, Math.Min(4, productType.Length));
    var sequencePart = GetNextBatchSequence().ToString("D3");

    return $"BATCH-{datePart}-{typePart}-{sequencePart}";
}
```

### Lab Test Validation Pattern

```csharp
/// <summary>
/// Validates that a product has a valid Certificate of Analysis (COA).
/// SAHPRA requirement: all batches must be tested by ISO/IEC 17025 accredited lab.
/// </summary>
/// <param name="product">Product to validate</param>
/// <returns>Validation result with errors if any</returns>
public ValidationResult ValidateLabTesting(Product product)
{
    var errors = new List<string>();

    // Lab test date required
    if (!product.LabTestDate.HasValue)
    {
        errors.Add("Laboratory testing date is required (SAHPRA compliance)");
    }

    // Lab test must be recent (within last 12 months)
    if (product.LabTestDate.HasValue &&
        product.LabTestDate.Value < DateTime.UtcNow.AddMonths(-12))
    {
        errors.Add("Laboratory testing is older than 12 months - retesting required");
    }

    // THC/CBD content must be documented
    if (string.IsNullOrWhiteSpace(product.THCPercentage) ||
        string.IsNullOrWhiteSpace(product.CBDPercentage))
    {
        errors.Add("THC and CBD content must be documented (Cannabis Act compliance)");
    }

    return new ValidationResult(errors);
}
```

### Audit Trail Pattern

```csharp
/// <summary>
/// Creates an immutable audit log entry.
/// POPIA compliance: 7-year retention, track all data access/modifications.
/// </summary>
/// <param name="userId">User performing action</param>
/// <param name="action">Action performed</param>
/// <param name="entityType">Type of entity</param>
/// <param name="entityId">Entity ID</param>
/// <param name="oldValues">Previous values (for updates)</param>
/// <param name="newValues">New values</param>
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

---

## Error Handling & Logging

### Exception Handling Pattern

```csharp
public async Task<ProductDetailsDto> CreateProductAsync(CreateProductDto createDto)
{
    try
    {
        _logger.LogInformation("Creating product: {SKU}", createDto.SKU);

        // Validate
        var validationResult = await _createValidator.ValidateAsync(createDto);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Product validation failed: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            throw new ValidationException(validationResult.Errors);
        }

        // Business logic
        var product = _mapper.Map<Product>(createDto);
        var createdProduct = await _productRepository.CreateAsync(product);

        _logger.LogInformation("Product created successfully: {ProductId}, SKU: {SKU}",
            createdProduct.Id, createdProduct.SKU);

        return _mapper.Map<ProductDetailsDto>(createdProduct);
    }
    catch (ValidationException)
    {
        // Re-throw validation exceptions (handled by controller)
        throw;
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error creating product: {SKU}", createDto.SKU);
        throw new InvalidOperationException("Failed to create product due to database error", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating product: {SKU}", createDto.SKU);
        throw;
    }
}
```

### Logging Levels

| Level | Usage | Example |
|-------|-------|---------|
| **Trace** | Very detailed diagnostic info | Method entry/exit, variable values |
| **Debug** | Detailed diagnostic info | SQL queries, calculation steps |
| **Information** | General informational messages | Product created, customer registered |
| **Warning** | Potentially harmful situations | Validation failures, low stock alerts |
| **Error** | Error events | Database failures, null references |
| **Critical** | Very severe error events | Application crash, data corruption |

---

## Validation Patterns

### FluentValidation Example

```csharp
using FluentValidation;
using Project420.Management.BLL.DTOs;
using Project420.Management.BLL.Services;

namespace Project420.Management.BLL.Validators
{
    /// <summary>
    /// Validator for product creation.
    /// Enforces cannabis industry compliance requirements.
    /// </summary>
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        private readonly IProductService _productService;

        public CreateProductValidator(IProductService productService)
        {
            _productService = productService;

            // SKU validation
            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
                .Matches("^[A-Z0-9-]+$").WithMessage("SKU must contain only uppercase letters, numbers, and hyphens")
                .MustAsync(BeUniqueSku).WithMessage("SKU already exists");

            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            // Price validation
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0")
                .LessThan(999999.99m).WithMessage("Price is unrealistically high");

            // Cost price validation
            RuleFor(x => x.CostPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Cost price cannot be negative")
                .LessThan(x => x.Price).WithMessage("Cost price should be less than selling price");

            // Cannabis-specific validations
            RuleFor(x => x.THCPercentage)
                .Matches(@"^\d{1,2}(\.\d{1,2})?%?$").WithMessage("THC percentage must be in format: 15.5 or 15.5%")
                .When(x => !string.IsNullOrWhiteSpace(x.THCPercentage));

            RuleFor(x => x.CBDPercentage)
                .Matches(@"^\d{1,2}(\.\d{1,2})?%?$").WithMessage("CBD percentage must be in format: 0.5 or 0.5%")
                .When(x => !string.IsNullOrWhiteSpace(x.CBDPercentage));

            // Batch number (SAHPRA compliance)
            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required for traceability (SAHPRA GMP requirement)")
                .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters");

            // Lab test date (Cannabis Act compliance)
            RuleFor(x => x.LabTestDate)
                .NotNull().WithMessage("Lab test date is required (Cannabis Act compliance)")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Lab test date cannot be in the future");

            // Expiry date validation
            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.Today).WithMessage("Expiry date must be in the future")
                .When(x => x.ExpiryDate.HasValue);

            // Stock validation
            RuleFor(x => x.StockOnHand)
                .GreaterThanOrEqualTo(0).WithMessage("Stock on hand cannot be negative");

            RuleFor(x => x.ReorderLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative");
        }

        /// <summary>
        /// Async validation to check SKU uniqueness.
        /// </summary>
        private async Task<bool> BeUniqueSku(string sku, CancellationToken cancellationToken)
        {
            return await _productService.IsSkuUniqueAsync(sku);
        }
    }
}
```

---

## Code Review Checklist

### General

- [ ] Code follows naming conventions
- [ ] XML documentation for all public members
- [ ] No commented-out code
- [ ] No hard-coded values (use configuration)
- [ ] No magic numbers (use named constants)
- [ ] Async/await used correctly
- [ ] Null checking where appropriate
- [ ] Proper exception handling
- [ ] Logging at appropriate levels

### Architecture

- [ ] Follows 3-tier architecture (UI → BLL → DAL → Models)
- [ ] No circular dependencies
- [ ] Dependency injection used correctly
- [ ] Interfaces used for services and repositories
- [ ] DTOs used for data transfer (entities never exposed)
- [ ] Proper separation of concerns

### Cannabis Compliance

- [ ] Age verification implemented (18+)
- [ ] Batch tracking included (SAHPRA)
- [ ] Lab testing requirements met
- [ ] Audit trails created (POPIA)
- [ ] THC/CBD content tracked
- [ ] Expiry dates monitored
- [ ] Soft delete used (POPIA data retention)
- [ ] Compliance comments in code

### Testing

- [ ] Unit tests included
- [ ] Test coverage > 70%
- [ ] Happy path tested
- [ ] Error cases tested
- [ ] Compliance scenarios tested
- [ ] Mock dependencies correctly

### Performance

- [ ] Database queries optimized
- [ ] No N+1 query problems
- [ ] Async operations used
- [ ] Large result sets paginated
- [ ] Caching considered where appropriate

---

## Enforcement

**These standards are MANDATORY for ALL Project420 development.**

- Pull requests MUST follow these standards
- Code reviews WILL enforce compliance
- Non-compliant code WILL BE rejected

---

## Questions & Support

**Reference Documents:**
- **UI Standards**: `docs/PROJECT_STANDARDS.md`
- **Architecture**: `docs/ARCHITECTURE.md`
- **Testing Guide**: `docs/UNIT-TESTING-GUIDE.md`
- **Compliance**: `SA_Cannabis_Software_Guide.md`, `south-africa-cannabis-cultivation-production-laws-guide.md`

---

**EFFECTIVE**: IMMEDIATELY
**STATUS**: MANDATORY
**VERSION**: 1.0
**LAST UPDATED**: 2025-12-07
