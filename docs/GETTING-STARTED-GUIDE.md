# Project420 - Getting Started Guide
## Data-First Development Approach - Step by Step

**Created**: 2025-12-01
**Purpose**: Teach the proper development order for building the POS module
**Approach**: Data-First, Bottom-Up

---

## 🎯 The Development Order (Data-First Approach)

When you said "data-first approach," you're absolutely right! Here's the correct order:

```
1. DATA MODELS (Entities)          ← START HERE
   ↓
2. DATABASE (EF Core DbContext)
   ↓
3. DATA ACCESS (Repositories)
   ↓
4. BUSINESS LOGIC (Services)
   ↓
5. USER INTERFACE (Blazor)         ← END HERE
```

**Why this order?**
- You can't save data without entities
- You can't query data without a database
- You can't implement business logic without data access
- You can't build UI without services to call

---

## 📋 Phase 1: Design Your Data Models (START HERE!)

### Step 1.1: Define What Data You Need

Before writing ANY code, answer these questions:

**For POS System:**
- What is a Sale? (Date, Customer, Total, Tax, Status)
- What is a Sale Item? (Product, Quantity, Price)
- What is a Product? (Name, SKU, Price, Stock)
- What is a Transaction? (Payment method, Amount, Date)
- What is Inventory? (Product, Stock levels, Location)

### Step 1.2: Draw Relationships

```
Product (1) ──has many──> (Many) SaleItem
   │                          │
   │                          │
   └────has many──> Inventory │
                              │
                              │
Sale (1) ──has many──> (Many) SaleItem
   │
   │
   └──has many──> Transaction (1 or more payments)
```

### Step 1.3: Create Entity Classes

**Location**: `Project420.Retail.POS.Models/Entities/`

**Start with the simplest entity first**: Product

**Why Product first?**
- No dependencies on other entities
- Everything else needs Products
- Easy to understand and test

---

## 📝 Step-by-Step: Creating Your First Entity

### Step 1: Create Product Entity

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.Models/Entities/Product.cs`

```csharp
namespace Project420.Retail.POS.Models.Entities
{
    public class Product
    {
        // Primary Key
        public int Id { get; set; }

        // Basic Info
        public string SKU { get; set; }          // Stock Keeping Unit (unique code)
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        // Pricing
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }   // What you paid for it

        // Stock
        public int StockOnHand { get; set; }
        public int ReorderLevel { get; set; }    // When to reorder

        // Status
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation Properties (relationships)
        public List<SaleItem> SaleItems { get; set; }
    }
}
```

**What is each property?**
- `Id`: Database primary key (auto-generated)
- `SKU`: Unique product code (barcode)
- `Name`: Product name
- `Price`: Selling price
- `StockOnHand`: How many in stock
- `IsActive`: Is product still being sold?
- `SaleItems`: List of sales containing this product (EF Core relationship)

### Step 2: Create SaleItem Entity

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.Models/Entities/SaleItem.cs`

```csharp
namespace Project420.Retail.POS.Models.Entities
{
    public class SaleItem
    {
        public int Id { get; set; }

        // Foreign Keys (links to other tables)
        public int SaleId { get; set; }          // Which sale?
        public int ProductId { get; set; }       // Which product?

        // Item Details
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // Price at time of sale
        public decimal Subtotal { get; set; }    // Quantity × UnitPrice
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }       // Subtotal + Tax

        // Navigation Properties
        public Sale Sale { get; set; }           // Parent sale
        public Product Product { get; set; }     // Product being sold
    }
}
```

**Why separate Sale and SaleItem?**
- One Sale can have MANY items
- Each item can have different quantities and prices
- Database normalization (proper design)

### Step 3: Create Sale Entity

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.Models/Entities/Sale.cs`

```csharp
namespace Project420.Retail.POS.Models.Entities
{
    public class Sale
    {
        public int Id { get; set; }

        // Sale Info
        public string SaleNumber { get; set; }   // Unique receipt number
        public DateTime SaleDate { get; set; }
        public int? CustomerId { get; set; }     // Optional (walk-in = null)

        // Totals
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Status
        public string Status { get; set; }       // "Pending", "Completed", "Cancelled"
        public int CashierId { get; set; }       // Who made the sale

        // Navigation Properties
        public List<SaleItem> Items { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
```

### Step 4: Create Transaction Entity

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.Models/Entities/Transaction.cs`

```csharp
namespace Project420.Retail.POS.Models.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        // Foreign Key
        public int SaleId { get; set; }

        // Payment Info
        public string PaymentMethod { get; set; } // "Cash", "Card", "EFT"
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }

        // Card/EFT Details
        public string ReferenceNumber { get; set; }
        public bool IsSuccessful { get; set; }

        // Navigation
        public Sale Sale { get; set; }
    }
}
```

---

## 📊 Phase 2: Create DTOs (Data Transfer Objects)

**Location**: `Project420.Retail.POS.Models/DTOs/`

### Why DTOs?

**Entities** = Database structure (has everything, including navigation properties)
**DTOs** = UI/API structure (only what the UI needs, simplified)

**Example difference:**
```csharp
// Entity (database)
public class Product
{
    public int Id { get; set; }
    public string SKU { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public List<SaleItem> SaleItems { get; set; } // EF Core navigation
}

// DTO (UI)
public class ProductDto
{
    public int Id { get; set; }
    public string SKU { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // No SaleItems! UI doesn't need it
    // No CostPrice! Don't expose to cashier
}
```

### Create Product DTOs

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.Models/DTOs/ProductDto.cs`

```csharp
namespace Project420.Retail.POS.Models.DTOs
{
    // For displaying products in POS
    public class ProductDto
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockOnHand { get; set; }
        public bool IsActive { get; set; }
    }

    // For searching products
    public class ProductSearchDto
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockOnHand { get; set; }
    }
}
```

---

## 🗄️ Phase 3: Create Database Context (EF Core)

**Location**: `Project420.Retail.POS.DAL/`

### Step 1: Create DbContext

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.DAL/POSDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.Models.Entities;

namespace Project420.Retail.POS.DAL
{
    public class POSDbContext : DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options)
            : base(options)
        {
        }

        // DbSets = Tables in database
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.HasIndex(e => e.SKU).IsUnique();
            });

            // Configure Sale
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SaleNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.SaleNumber).IsUnique();
            });

            // Configure SaleItem
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasPrecision(18, 2);

                // Relationships
                entity.HasOne(e => e.Sale)
                    .WithMany(s => s.Items)
                    .HasForeignKey(e => e.SaleId);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.SaleItems)
                    .HasForeignKey(e => e.ProductId);
            });

            // Configure Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(e => e.Sale)
                    .WithMany(s => s.Transactions)
                    .HasForeignKey(e => e.SaleId);
            });
        }
    }
}
```

**What does this do?**
- Defines database tables (DbSets)
- Configures columns (max length, required, decimals)
- Defines relationships (foreign keys)
- Creates indexes for performance

---

## 🔧 Phase 4: Create Repositories (Data Access)

**Location**: `Project420.Retail.POS.DAL/Repositories/`

### Create Product Repository

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.DAL/Repositories/ProductRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.Models.Entities;

namespace Project420.Retail.POS.DAL.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetBySkuAsync(string sku);
        Task<List<Product>> GetAllActiveAsync();
        Task<List<Product>> SearchAsync(string searchTerm);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly POSDbContext _context;

        public ProductRepository(POSDbContext context)
        {
            _context = context;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> GetBySkuAsync(string sku)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == sku && p.IsActive);
        }

        public async Task<List<Product>> GetAllActiveAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<List<Product>> SearchAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => p.IsActive &&
                    (p.Name.Contains(searchTerm) ||
                     p.SKU.Contains(searchTerm)))
                .Take(20)
                .ToListAsync();
        }
    }
}
```

**What does repository do?**
- Contains all database queries for Products
- Encapsulates EF Core logic
- Returns entities
- No business logic!

---

## 💼 Phase 5: Create Services (Business Logic)

**Location**: `Project420.Retail.POS.BLL/Services/`

### Create Product Service

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.BLL/Services/ProductService.cs`

```csharp
using Project420.Retail.POS.DAL.Repositories;
using Project420.Retail.POS.Models.DTOs;
using Project420.Retail.POS.Models.Entities;

namespace Project420.Retail.POS.BLL.Services
{
    public interface IProductService
    {
        Task<ProductDto> GetProductBySkuAsync(string sku);
        Task<List<ProductSearchDto>> SearchProductsAsync(string searchTerm);
        Task<bool> CheckStockAvailableAsync(int productId, int quantity);
    }

    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> GetProductBySkuAsync(string sku)
        {
            var product = await _productRepository.GetBySkuAsync(sku);

            if (product == null)
                return null;

            // Map Entity → DTO
            return new ProductDto
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                StockOnHand = product.StockOnHand,
                IsActive = product.IsActive
            };
        }

        public async Task<List<ProductSearchDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _productRepository.SearchAsync(searchTerm);

            // Map list of entities to DTOs
            return products.Select(p => new ProductSearchDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Price = p.Price,
                StockOnHand = p.StockOnHand
            }).ToList();
        }

        public async Task<bool> CheckStockAvailableAsync(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
                return false;

            // Business rule: Check if enough stock
            return product.StockOnHand >= quantity;
        }
    }
}
```

**What does service do?**
- Implements business logic
- Maps Entity ↔ DTO
- Validates business rules
- Calls repositories
- Returns DTOs (not entities!)

---

## 🎨 Phase 6: Create UI (Blazor)

**Location**: `Project420.Retail.POS.UI.Blazor/Pages/`

### Create POS Page

**File**: `src/Modules/Retail/POS/Project420.Retail.POS.UI.Blazor/Pages/POS.razor`

```razor
@page "/pos"
@using Project420.Retail.POS.BLL.Services
@using Project420.Retail.POS.Models.DTOs
@inject IProductService ProductService

<h3>Point of Sale</h3>

<div class="row">
    <div class="col-md-6">
        <h4>Product Search</h4>
        <input type="text" @bind="searchTerm" @bind:event="oninput"
               @onkeyup="SearchProducts" placeholder="Search SKU or Name"
               class="form-control" />

        @if (searchResults != null)
        {
            <ul class="list-group mt-2">
                @foreach (var product in searchResults)
                {
                    <li class="list-group-item d-flex justify-content-between">
                        <span>@product.Name (@product.SKU)</span>
                        <span>R @product.Price.ToString("N2")</span>
                        <button class="btn btn-sm btn-primary"
                                @onclick="() => AddToCart(product)">
                            Add
                        </button>
                    </li>
                }
            </ul>
        }
    </div>

    <div class="col-md-6">
        <h4>Shopping Cart</h4>
        <p>Total: R @cartTotal.ToString("N2")</p>
    </div>
</div>

@code {
    private string searchTerm = "";
    private List<ProductSearchDto> searchResults;
    private decimal cartTotal = 0;

    private async Task SearchProducts()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            searchResults = null;
            return;
        }

        // Call service (BLL)
        searchResults = await ProductService.SearchProductsAsync(searchTerm);
    }

    private void AddToCart(ProductSearchDto product)
    {
        cartTotal += product.Price;
        // TODO: Add to cart list
    }
}
```

**What does UI do?**
- Displays data to user
- Captures user input
- Calls services (not repositories!)
- Works with DTOs (not entities!)

---

## ✅ Summary: The Proper Order

### Start Here:
1. ✅ **Entities** (`Models/Entities/`) - Define your data
2. ✅ **DTOs** (`Models/DTOs/`) - Define your API
3. ✅ **DbContext** (`DAL/`) - Configure database
4. ✅ **Repositories** (`DAL/Repositories/`) - Data access
5. ✅ **Services** (`BLL/Services/`) - Business logic
6. ✅ **UI** (`UI.Blazor/Pages/`) - User interface

### Then:
7. **Add NuGet Packages** (EF Core, etc.)
8. **Configure DI** (Dependency Injection)
9. **Create Migrations** (Database creation)
10. **Seed Data** (Test products)
11. **Run & Test**

---

## 🎯 Next Steps

**Ready to start?** Let's begin with Step 1:

1. Create the Entity classes (Product, Sale, SaleItem, Transaction)
2. Create the DTO classes
3. Create the DbContext
4. Add EF Core packages
5. Create repositories
6. Create services
7. Build the UI

**Want me to create these files step by step, or do you want to try creating them yourself first?**

---

**Key Principle**: Always work **bottom-up** (data → logic → UI), never top-down!
