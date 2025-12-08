# Project420 - Folder Structure Explained
## Understanding BLL, DAL, and Models

**Created**: 2025-12-01
**Purpose**: Clarify the folder and class structure within each project

---

## 🎯 Key Concept

**BLL, DAL, and Models are PROJECTS (DLLs), not just single classes!**

Each project contains **multiple folders**, and each folder contains **multiple classes**.

---

## 📦 Example: Management Module Structure

### Project420.Management.Models (PROJECT)
```
Project420.Management.Models/
├── Project420.Management.Models.csproj    ← Project file
├── Entities/                              ← FOLDER for database entities
│   ├── Customer.cs                       ← CLASS: Customer entity
│   ├── Product.cs                        ← CLASS: Product entity
│   ├── Pricelist.cs                      ← CLASS: Pricelist entity
│   ├── PricelistItem.cs                  ← CLASS: PricelistItem entity
│   └── ProductCategory.cs                ← CLASS: ProductCategory entity
│
├── DTOs/                                  ← FOLDER for data transfer objects
│   ├── CustomerDto.cs                    ← CLASS: Customer DTO
│   ├── CreateCustomerDto.cs              ← CLASS: Create customer DTO
│   ├── UpdateCustomerDto.cs              ← CLASS: Update customer DTO
│   ├── ProductDto.cs                     ← CLASS: Product DTO
│   ├── CreateProductDto.cs               ← CLASS: Create product DTO
│   ├── PricelistDto.cs                   ← CLASS: Pricelist DTO
│   └── PricelistItemDto.cs               ← CLASS: Pricelist item DTO
│
└── Enums/                                 ← FOLDER for enumerations
    ├── ProductStatus.cs                  ← ENUM: Product statuses
    └── CustomerType.cs                   ← ENUM: Customer types
```

**Purpose of Models Project**:
- Contains all data structures
- Entities = Database tables (what EF Core maps)
- DTOs = Objects sent between UI and BLL (simplified, calculated fields)
- Shared by both DAL and BLL projects

---

### Project420.Management.DAL (PROJECT)
```
Project420.Management.DAL/
├── Project420.Management.DAL.csproj       ← Project file
├── Contexts/                              ← FOLDER for DbContext
│   └── ManagementDbContext.cs            ← CLASS: EF Core DbContext
│
├── Configurations/                        ← FOLDER for entity configurations
│   ├── CustomerConfiguration.cs          ← CLASS: Configure Customer entity
│   ├── ProductConfiguration.cs           ← CLASS: Configure Product entity
│   ├── PricelistConfiguration.cs         ← CLASS: Configure Pricelist entity
│   └── PricelistItemConfiguration.cs     ← CLASS: Configure PricelistItem
│
├── Repositories/                          ← FOLDER for repositories
│   ├── Interfaces/                       ← FOLDER for repository interfaces
│   │   ├── ICustomerRepository.cs       ← INTERFACE
│   │   ├── IProductRepository.cs        ← INTERFACE
│   │   └── IPricelistRepository.cs      ← INTERFACE
│   ├── CustomerRepository.cs             ← CLASS: Customer data access
│   ├── ProductRepository.cs              ← CLASS: Product data access
│   └── PricelistRepository.cs            ← CLASS: Pricelist data access
│
├── UnitOfWork/                            ← FOLDER for unit of work
│   ├── IUnitOfWork.cs                    ← INTERFACE
│   └── UnitOfWork.cs                     ← CLASS: Manages transactions
│
├── Migrations/                            ← FOLDER for EF migrations
│   ├── 20251201_InitialCreate.cs         ← CLASS: First migration
│   └── 20251202_AddPricelists.cs         ← CLASS: Second migration
│
└── Seed/                                  ← FOLDER for seed data
    └── ManagementDataSeeder.cs           ← CLASS: Initial data
```

**Purpose of DAL Project**:
- All database access code
- EF Core DbContext
- Repository classes (CRUD operations)
- Entity configurations (how entities map to tables)
- Database migrations
- NO business logic here!

---

### Project420.Management.BLL (PROJECT)
```
Project420.Management.BLL/
├── Project420.Management.BLL.csproj       ← Project file
│
├── Sales/                                 ← DOMAIN: Sales-related business logic
│   ├── Retail/                           ← SUB-DOMAIN: Retail operations
│   │   ├── DTOs/                         ← FOLDER for Retail DTOs
│   │   │   ├── CreatePricelistDto.cs    ← CLASS: Create pricelist DTO
│   │   │   ├── UpdatePricelistDto.cs    ← CLASS: Update pricelist DTO
│   │   │   ├── PricelistDto.cs          ← CLASS: Pricelist display DTO
│   │   │   ├── CreatePricelistItemDto.cs ← CLASS: Create pricelist item DTO
│   │   │   ├── UpdatePricelistItemDto.cs ← CLASS: Update pricelist item DTO
│   │   │   └── PricelistItemDto.cs      ← CLASS: Pricelist item DTO
│   │   ├── Services/                     ← FOLDER for Retail services
│   │   │   ├── IPricelistService.cs     ← INTERFACE: Pricelist service
│   │   │   └── PricelistService.cs      ← CLASS: Retail pricing logic
│   │   └── Validators/                   ← FOLDER for Retail validators
│   │       ├── CreatePricelistValidator.cs ← CLASS: Validate pricelist creation
│   │       ├── UpdatePricelistValidator.cs ← CLASS: Validate pricelist updates
│   │       ├── CreatePricelistItemValidator.cs ← CLASS: Validate item creation
│   │       └── UpdatePricelistItemValidator.cs ← CLASS: Validate item updates
│   │
│   └── SalesCommon/                      ← SUB-DOMAIN: Shared sales logic
│       ├── DTOs/                         ← FOLDER for common sales DTOs
│       │   └── CustomerRegistrationDto.cs ← CLASS: Customer registration DTO
│       ├── Services/                     ← FOLDER for common sales services
│       │   └── CustomerService.cs       ← CLASS: Customer management logic
│       └── Validators/                   ← FOLDER for common validators
│           └── CustomerRegistrationValidator.cs ← CLASS: Customer validation
│
└── StockManagement/                       ← DOMAIN: Inventory business logic
    ├── DTOs/                             ← FOLDER for Product DTOs
    │   ├── CreateProductDto.cs          ← CLASS: Create product DTO
    │   ├── UpdateProductDto.cs          ← CLASS: Update product DTO
    │   └── ProductDto.cs                ← CLASS: Product display DTO
    ├── Services/                         ← FOLDER for Product services
    │   ├── IProductService.cs           ← INTERFACE: Product service
    │   └── ProductService.cs            ← CLASS: Product catalog logic
    └── Validators/                       ← FOLDER for Product validators
        ├── CreateProductValidator.cs    ← CLASS: Validate product creation
        └── UpdateProductValidator.cs    ← CLASS: Validate product updates
```

**Purpose of BLL Project**:
- Business logic and rules organized by domain (Sales, StockManagement)
- Service classes (orchestrate operations)
- Validation rules (FluentValidation)
- DTOs for data transfer between layers
- Business exceptions (when needed)
- Calls DAL repositories
- **Domain-Driven Structure**: Matches DAL/Models organization for consistency

---

## 🔄 How They Connect

### Dependencies Between Projects

```
UI.Blazor
    ↓ references
   BLL
    ↓ references
   DAL ← references → Models
```

### Example Flow: Get Customer

1. **UI (Blazor page)**:
   ```csharp
   var customer = await CustomerService.GetCustomerByIdAsync(5);
   ```

2. **BLL (Sales/SalesCommon/Services/CustomerService.cs)**:
   ```csharp
   namespace Project420.Management.BLL.Sales.SalesCommon.Services;

   public async Task<CustomerDto> GetCustomerByIdAsync(int id)
   {
       // Call DAL
       var customer = await _customerRepository.GetByIdAsync(id);

       // Map Entity → DTO
       return new CustomerDto
       {
           Id = customer.Id,
           Name = customer.Name,
           AvailableCredit = customer.CreditLimit - customer.CurrentBalance
       };
   }
   ```

3. **DAL (CustomerRepository.cs)**:
   ```csharp
   public async Task<Customer> GetByIdAsync(int id)
   {
       // Use EF Core DbContext
       return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
   }
   ```

4. **Database**: SQL query executes and returns data

5. **Response flows back**: Database → DAL → BLL → UI

---

## 🎨 Real-World Analogy

Think of it like a restaurant:

### Models (Menu Items)
- **Entities**: The actual ingredients in the kitchen (raw data)
- **DTOs**: The plated dishes served to customers (formatted data)

### DAL (Kitchen Storage & Prep)
- **DbContext**: The kitchen itself
- **Repositories**: Chefs who know how to get ingredients and prepare them
- **Configurations**: Recipes for how to store and prepare each ingredient

### BLL (Head Chef & Kitchen Manager)
- **Services**: Head chef who orchestrates the cooking
- **Validators**: Quality control (taste testing, portion sizes)
- **Business Rules**: Restaurant policies (no substitutions, specials, pricing)

### UI (Waiters & Dining Room)
- **Blazor Pages**: Waiters taking orders
- **Components**: The dining room layout
- Orders go: Waiter → Head Chef → Kitchen Chefs → Storage

---

## 📝 Typical Class Examples

### Entity Class (Models/Entities/Customer.cs)
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }

    // EF Core navigation property
    public List<Sale> Sales { get; set; }
}
```

### DTO Class (Models/DTOs/CustomerDto.cs)
```csharp
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public decimal AvailableCredit { get; set; } // Calculated!
}
```

### Repository Class (DAL/Repositories/CustomerRepository.cs)
```csharp
public class CustomerRepository : ICustomerRepository
{
    private readonly ManagementDbContext _context;

    public async Task<Customer> GetByIdAsync(int id)
    {
        return await _context.Customers.FindAsync(id);
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }
}
```

### Service Class (BLL/Services/CustomerService.cs)
```csharp
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);

        // Business logic: Calculate available credit
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            AvailableCredit = customer.CreditLimit - customer.CurrentBalance
        };
    }
}
```

---

## 💡 Why This Structure?

### Separation of Concerns
- **Models**: Pure data structures
- **DAL**: Database operations only
- **BLL**: Business logic only
- **UI**: Display and user interaction only

### Benefits
✅ **Testability**: Can test each layer independently
✅ **Maintainability**: Changes in one layer don't affect others
✅ **Reusability**: Same BLL can serve Blazor, MAUI, API
✅ **Team Work**: Different developers can work on different layers
✅ **Flexibility**: Can swap databases without changing BLL/UI

### Example: Change Database
If you switch from SQL Server to PostgreSQL:
- ✅ Models: No changes
- ⚠️ DAL: Update connection string and provider
- ✅ BLL: No changes
- ✅ UI: No changes

Only DAL configuration changes!

---

## 🔑 Key Takeaways

1. **Projects = DLLs**: BLL, DAL, Models are separate compiled libraries
2. **Folders = Organization**: Group related classes together
3. **Classes = Actual Code**: Each class has a specific job
4. **Entities vs DTOs**: Entities = DB structure, DTOs = API structure
5. **Repositories**: Handle all database queries
6. **Services**: Handle business logic and orchestration
7. **Separation**: Each layer has one responsibility

---

## 📊 What Goes Where?

| What | Where | Example |
|------|-------|---------|
| Database table structure | Models/Entities | `Customer.cs` |
| API request/response | Models/DTOs | `CustomerDto.cs` |
| Database queries | DAL/Repositories | `CustomerRepository.cs` |
| EF Core configuration | DAL/Configurations | `CustomerConfiguration.cs` |
| Business rules | BLL/Services | `CustomerService.cs` |
| Validation | BLL/Validators | `CustomerValidator.cs` |
| UI pages | UI.Blazor/Pages | `CustomerList.razor` |
| UI components | UI.Blazor/Components | `CustomerGrid.razor` |

---

**Now it should be clear**: BLL, DAL, and Models are PROJECTS containing FOLDERS of CLASSES! 🎯
