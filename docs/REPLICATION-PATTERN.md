# Feature Replication Pattern - Customer Registration Template

**Last Updated**: 2025-12-05
**Status**: Customer Registration = ✅ COMPLETE FULL STACK
**Purpose**: Template for replicating features (Supplier, Product, Employee, etc.)

---

## ✅ Customer Registration - Full Stack Completeness Audit

### 1. Domain Layer (Models) ✅ COMPLETE

**Location**: `Project420.Management.Models/Entities/Sales/Common/Debtor.cs`

**What's Complete**:
- ✅ Full entity with all properties (Name, IdNumber, Mobile, Email, etc.)
- ✅ POPIA compliance fields (ConsentGiven, ConsentDate, ConsentPurpose)
- ✅ Medical cannabis fields (MedicalPermitNumber, MedicalPermitExpiryDate)
- ✅ Age verification fields (DateOfBirth, AgeVerified)
- ✅ Account management fields (CreditLimit, CurrentBalance)
- ✅ Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- ✅ Soft delete (IsActive)
- ✅ XML documentation on all properties

**What You Get**:
```csharp
public class Debtor // Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string IdNumber { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    // ... 20+ more properties
}
```

---

### 2. Data Access Layer (DAL) ✅ COMPLETE

**Location**: `Project420.Management.DAL/`

**What's Complete**:

#### 2a. DbContext Configuration ✅
- ✅ `ManagementDbContext.cs` - DbContext with Debtor DbSet
- ✅ Entity configuration (relationships, indexes, constraints)
- ✅ Database connection string setup
- ✅ EF Core migrations created and applied
- ✅ Database created (4 migrations: InitialCreate, Users, Stations, POPIA fields)

#### 2b. Repository Pattern ✅
- ✅ `IRepository<T>` - Generic repository interface
- ✅ `ICustomerRepository` - Customer-specific interface (extends IRepository)
- ✅ `CustomerRepository` - Full implementation with 11 methods:
  - GetByIdAsync()
  - GetAllAsync()
  - AddAsync()
  - UpdateAsync()
  - DeleteAsync() (soft delete)
  - GetByIdNumberAsync()
  - GetByEmailAsync()
  - GetByMobileAsync()
  - SearchCustomersAsync()
  - GetMedicalPatientsAsync()
  - GetExpiringMedicalPermitsAsync()
  - IsAgeVerifiedAsync()

**What You Get**:
```csharp
public interface ICustomerRepository : IRepository<Debtor>
{
    Task<Debtor?> GetByIdNumberAsync(string idNumber);
    Task<Debtor?> GetByEmailAsync(string email);
    Task<IEnumerable<Debtor>> SearchCustomersAsync(string searchTerm);
    // ... 8 more methods
}
```

---

### 3. Business Logic Layer (BLL) ✅ COMPLETE

**Location**: `Project420.Management.BLL/`

**What's Complete**:

#### 3a. DTOs ✅
- ✅ `CustomerRegistrationDto.cs` - Complete DTO with:
  - Personal information (Name, IdNumber, DateOfBirth)
  - Contact information (Mobile, Email, Addresses)
  - Medical cannabis fields (MedicalPermitNumber, etc.)
  - Account/Credit fields (CreditLimit, PaymentTerms)
  - POPIA compliance fields (ConsentGiven, MarketingConsent)
  - Internal notes
  - 130 lines of XML documentation

#### 3b. Services ✅
- ✅ `CustomerService.cs` - Complete business logic service with:
  - RegisterCustomerAsync() - Full registration workflow
  - GetCustomerByIdAsync()
  - GetAllCustomersAsync()
  - SearchCustomersAsync()
  - GetMedicalPatientsAsync()
  - GetExpiringMedicalPermitsAsync()
  - UpdateCustomerAsync()
  - DeactivateCustomerAsync() (soft delete)
  - IsAgeVerifiedAsync()
  - CheckForDuplicatesAsync() (private)
  - ExtractDateOfBirthFromId() (private)

#### 3c. Validators ✅
- ✅ `CustomerRegistrationValidator.cs` - FluentValidation with:
  - Name validation (required, max length)
  - ID number validation (required, format, valid SA ID)
  - Mobile validation (required, format)
  - Email validation (required, email format)
  - Medical permit validation (conditional)
  - POPIA consent validation (required)
  - Age verification (18+ cannabis compliance)

**What You Get**:
```csharp
public class CustomerService
{
    public async Task<Debtor> RegisterCustomerAsync(CustomerRegistrationDto dto)
    {
        // STEP 1: Validate input
        // STEP 2: Check duplicates
        // STEP 3: Extract DOB from ID
        // STEP 4: Map DTO to Entity
        // STEP 5: Save to database
        return registeredCustomer;
    }
    // ... 9 more methods
}
```

---

### 4. Presentation Layer (UI) ✅ COMPLETE

**Location**: `Project420.Management.UI.Blazor/Components/Pages/`

**What's Complete**:

#### 4a. Customer Registration Form ✅
- ✅ `CustomerRegistration.razor` - Full registration form with:
  - Personal information section (Name, ID, DOB)
  - Contact information section (Mobile, Email, Addresses)
  - Medical cannabis section (Permit, Doctor, Expiry)
  - Account/Credit section (Credit limit, Payment terms)
  - POPIA consent section (Data processing, Marketing)
  - Notes section
  - Form validation (FluentValidation integration)
  - Success/error messaging
  - Auto-redirect to customer list after registration

#### 4b. Customer List View ✅
- ✅ `CustomerList.razor` - Full list view with:
  - Data table with all customer fields
  - **POPIA data masking** (ID numbers, mobile, email) ← NEW!
  - Search functionality (by name, email, mobile)
  - Filter count display
  - Medical permit expiry warnings
  - Age verification badges
  - POPIA consent status
  - Navigation to registration
  - View details button (prepared for future detail page)
  - Responsive design
  - Loading/error states

**What You Get**:
```razor
@page "/customer-registration"
@inject CustomerService CustomerService
@inject IValidator<CustomerRegistrationDto> Validator
@inject IDataProtectionService DataProtection

<EditForm Model="@model" OnValidSubmit="@HandleRegistration">
    <!-- 5 sections of form fields -->
</EditForm>
```

---

### 5. Infrastructure ✅ COMPLETE

**Location**: `Project420.Management.UI.Blazor/Program.cs`

**What's Complete**:
- ✅ DbContext registration (SQL Server connection)
- ✅ Repository registration (ICustomerRepository → CustomerRepository)
- ✅ Service registration (CustomerService)
- ✅ Validator registration (CustomerRegistrationValidator)
- ✅ **POPIA Data Protection Service** (IDataProtectionService → PopiaDataMaskingService) ← NEW!
- ✅ Dependency injection configured

**What You Get**:
```csharp
// Database
builder.Services.AddDbContext<ManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// Service
builder.Services.AddScoped<CustomerService>();

// Validator
builder.Services.AddScoped<IValidator<CustomerRegistrationDto>, CustomerRegistrationValidator>();

// POPIA Masking (Singleton - shared across all modules)
builder.Services.AddSingleton<IDataProtectionService, PopiaDataMaskingService>();
```

---

### 6. Shared Services (New!) ✅ COMPLETE

**Location**: `Project420.Shared.Core/Services/DataProtection/`

**What's Complete**:
- ✅ `IDataProtectionService.cs` - Interface with 8 masking methods
- ✅ `PopiaDataMaskingService.cs` - Implementation with:
  - MaskIdNumber() - SA ID masking
  - MaskMobile() - Cell phone masking
  - MaskEmail() - Email masking
  - MaskAddress() - Address masking
  - MaskBankAccount() - Bank account masking
  - MaskMedicalPermit() - Medical license masking
  - MaskFinancialAmount() - Financial data masking
  - UnmaskWithAuditAsync() - Future audit trail support

---

## 🎯 What's NOT Complete (Future Enhancements)

These are NOT required for the pattern to work, but nice to have:

- ⚠️ Customer details page (View button exists, page doesn't)
- ⚠️ Customer edit functionality (UpdateAsync exists in service, no UI)
- ⚠️ Customer deactivate UI (DeactivateAsync exists in service, no UI)
- ⚠️ Seed data for testing (optional)
- ⚠️ Unit tests (optional but recommended)
- ⚠️ Role-based authorization (UnmaskWithAuditAsync prepared, not implemented)

---

## 📋 REPLICATION PATTERN - Step-by-Step

Use this pattern to create Supplier, Product, Employee, or any other feature.

### Step 1: Domain Layer (Models)

**Create Entity**:
```csharp
// Project420.Management.Models/Entities/[Category]/[EntityName].cs
public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string IdNumber { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public string BankAccount { get; set; }

    // Add entity-specific fields
    public string TaxNumber { get; set; }
    public bool IsPreferredSupplier { get; set; }

    // POPIA compliance (copy from Debtor)
    public bool ConsentGiven { get; set; }
    public DateTime? ConsentDate { get; set; }

    // Audit fields (copy from Debtor)
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
}
```

**Checklist**:
- [ ] Create entity class
- [ ] Add all required properties
- [ ] Include POPIA compliance fields
- [ ] Include audit fields
- [ ] Add XML documentation
- [ ] Add soft delete (IsActive)

---

### Step 2: Data Access Layer (DAL)

**2a. Create Repository Interface**:
```csharp
// Project420.Management.DAL/Repositories/ISupplierRepository.cs
public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByIdNumberAsync(string idNumber);
    Task<Supplier?> GetByEmailAsync(string email);
    Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm);
    Task<IEnumerable<Supplier>> GetPreferredSuppliersAsync();
}
```

**2b. Create Repository Implementation**:
```csharp
// Project420.Management.DAL/Repositories/SupplierRepository.cs
public class SupplierRepository : ISupplierRepository
{
    private readonly ManagementDbContext _context;

    // Implement IRepository<T> methods (copy from CustomerRepository)
    // Implement ISupplierRepository methods
}
```

**2c. Update DbContext**:
```csharp
// Project420.Management.DAL/ManagementDbContext.cs
public class ManagementDbContext : DbContext
{
    public DbSet<Debtor> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; } // ← ADD THIS

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Supplier entity
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.IdNumber).IsUnique();
            // ... more configuration
        });
    }
}
```

**2d. Create Migration**:
```bash
cd Project420.Management.DAL
dotnet ef migrations add AddSuppliers
dotnet ef database update
```

**Checklist**:
- [ ] Create ISupplierRepository (extend IRepository<T>)
- [ ] Create SupplierRepository implementation
- [ ] Add DbSet<Supplier> to DbContext
- [ ] Configure entity in OnModelCreating
- [ ] Create and apply migration
- [ ] Verify database table created

---

### Step 3: Business Logic Layer (BLL)

**3a. Create DTO**:
```csharp
// Project420.Management.BLL/DTOs/SupplierRegistrationDto.cs
public class SupplierRegistrationDto
{
    // Copy structure from CustomerRegistrationDto
    public string Name { get; set; }
    public string IdNumber { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }

    // Supplier-specific fields
    public string BankAccount { get; set; }
    public string TaxNumber { get; set; }

    // POPIA (copy from Customer)
    public bool ConsentGiven { get; set; }
    public bool MarketingConsent { get; set; }
}
```

**3b. Create Validator**:
```csharp
// Project420.Management.BLL/Validators/SupplierRegistrationValidator.cs
public class SupplierRegistrationValidator : AbstractValidator<SupplierRegistrationDto>
{
    public SupplierRegistrationValidator()
    {
        // Copy validation rules from CustomerRegistrationValidator
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IdNumber).NotEmpty().Matches(@"^\d{13}$");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        // Add supplier-specific validation
        RuleFor(x => x.BankAccount).NotEmpty().When(x => !string.IsNullOrEmpty(x.BankAccount));
        RuleFor(x => x.ConsentGiven).Equal(true).WithMessage("POPIA consent required");
    }
}
```

**3c. Create Service**:
```csharp
// Project420.Management.BLL/Services/SupplierService.cs
public class SupplierService
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IValidator<SupplierRegistrationDto> _validator;

    // Copy method structure from CustomerService
    public async Task<Supplier> RegisterSupplierAsync(SupplierRegistrationDto dto)
    {
        // STEP 1: Validate
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // STEP 2: Check duplicates
        await CheckForDuplicatesAsync(dto);

        // STEP 3: Map DTO to Entity
        var supplier = new Supplier
        {
            Name = dto.Name,
            IdNumber = dto.IdNumber,
            Mobile = dto.Mobile,
            Email = dto.Email,
            BankAccount = dto.BankAccount,
            ConsentGiven = dto.ConsentGiven,
            ConsentDate = DateTime.UtcNow,
            IsActive = true
        };

        // STEP 4: Save
        return await _supplierRepository.AddAsync(supplier);
    }

    public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        => await _supplierRepository.GetAllAsync();

    // Add more methods as needed
}
```

**Checklist**:
- [ ] Create SupplierRegistrationDto
- [ ] Create SupplierRegistrationValidator
- [ ] Create SupplierService
- [ ] Implement RegisterSupplierAsync()
- [ ] Implement GetAllSuppliersAsync()
- [ ] Add XML documentation

---

### Step 4: Presentation Layer (UI)

**4a. Create Registration Form**:
```razor
@* Project420.Management.UI.Blazor/Components/Pages/SupplierRegistration.razor *@
@page "/supplier-registration"
@using Project420.Management.BLL.Services
@using Project420.Management.BLL.DTOs
@using FluentValidation
@inject SupplierService SupplierService
@inject IValidator<SupplierRegistrationDto> Validator
@inject NavigationManager Navigation
@rendermode InteractiveServer

<PageTitle>Supplier Registration</PageTitle>

<div class="container mt-4">
    <h1 class="mb-4">Supplier Registration</h1>

    <!-- Copy success/error message handling from CustomerRegistration -->

    <EditForm Model="@model" OnValidSubmit="@HandleRegistration">
        <!-- Copy section structure from CustomerRegistration -->

        <!-- Personal Information -->
        <div class="card mb-4">
            <div class="card-header bg-primary text-white">
                <h5 class="mb-0">Personal Information</h5>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Name *</label>
                        <InputText @bind-Value="model.Name" class="form-control" />
                    </div>
                    <!-- Add more fields -->
                </div>
            </div>
        </div>

        <!-- POPIA Consent (copy from Customer) -->
        <div class="card mb-4">
            <div class="card-header bg-warning">
                <h5 class="mb-0">POPIA Consent (Required)</h5>
            </div>
            <div class="card-body">
                <div class="form-check mb-3">
                    <InputCheckbox @bind-Value="model.ConsentGiven" class="form-check-input" id="consentGiven" />
                    <label class="form-check-label" for="consentGiven">
                        I consent to data processing
                    </label>
                </div>
            </div>
        </div>

        <button type="submit" class="btn btn-primary">Register Supplier</button>
    </EditForm>
</div>

@code {
    private SupplierRegistrationDto model = new();
    private string successMessage = string.Empty;
    private string errorMessage = string.Empty;

    private async Task HandleRegistration()
    {
        try
        {
            var supplier = await SupplierService.RegisterSupplierAsync(model);
            successMessage = $"Supplier {supplier.Name} registered successfully!";
            model = new(); // Reset form

            // Optional: Navigate to list
            await Task.Delay(2000);
            Navigation.NavigateTo("/suppliers");
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }
}
```

**4b. Create List View**:
```razor
@* Project420.Management.UI.Blazor/Components/Pages/SupplierList.razor *@
@page "/suppliers"
@using Project420.Management.BLL.Services
@using Project420.Management.Models.Entities
@using Project420.Shared.Core.Services.DataProtection
@inject SupplierService SupplierService
@inject NavigationManager Navigation
@inject IDataProtectionService DataProtection
@rendermode InteractiveServer

<PageTitle>Supplier List</PageTitle>

<div class="container-fluid mt-4">
    <h1>Supplier List</h1>

    <!-- Copy table structure from CustomerList -->
    <table class="table table-hover">
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>ID Number</th>
                <th>Mobile</th>
                <th>Email</th>
                <th>Bank Account</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var supplier in suppliers)
            {
                <tr>
                    <td>@supplier.Id</td>
                    <td>@supplier.Name</td>
                    <td>@DataProtection.MaskIdNumber(supplier.IdNumber)</td>
                    <td>@DataProtection.MaskMobile(supplier.Mobile)</td>
                    <td>@DataProtection.MaskEmail(supplier.Email)</td>
                    <td>@DataProtection.MaskBankAccount(supplier.BankAccount)</td>
                    <td>
                        <button class="btn btn-sm btn-info">View</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>

@code {
    private List<Supplier> suppliers = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await SupplierService.GetAllSuppliersAsync();
        suppliers = result.ToList();
    }
}
```

**Checklist**:
- [ ] Create SupplierRegistration.razor (copy from CustomerRegistration)
- [ ] Create SupplierList.razor (copy from CustomerList)
- [ ] Inject IDataProtectionService for masking
- [ ] Update form fields for supplier-specific data
- [ ] Add navigation links
- [ ] Test form submission
- [ ] Test list display with masked data

---

### Step 5: Infrastructure (DI Registration)

**Update Program.cs**:
```csharp
// Project420.Management.UI.Blazor/Program.cs

// Repository
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

// Service
builder.Services.AddScoped<SupplierService>();

// Validator
builder.Services.AddScoped<IValidator<SupplierRegistrationDto>, SupplierRegistrationValidator>();

// POPIA service already registered globally (Singleton) ✅
```

**Checklist**:
- [ ] Register ISupplierRepository
- [ ] Register SupplierService
- [ ] Register SupplierRegistrationValidator
- [ ] Build and verify no errors

---

## 🚀 Quick Start Checklist

For **any new feature** (Supplier, Product, Employee):

### Planning Phase
- [ ] Define entity properties (copy from Debtor template)
- [ ] Identify unique business rules
- [ ] Determine required validations
- [ ] List CRUD operations needed

### Development Phase (5 Steps)
- [ ] **Step 1**: Create entity in Models project
- [ ] **Step 2**: Create repository (interface + implementation) + migration
- [ ] **Step 3**: Create DTO + Validator + Service
- [ ] **Step 4**: Create UI (Registration form + List view)
- [ ] **Step 5**: Register in DI container

### Testing Phase
- [ ] Build solution (0 errors)
- [ ] Test registration form
- [ ] Test list view with POPIA masking
- [ ] Test validation rules
- [ ] Test database persistence

---

## 📊 Time Estimate Per Feature

Using this pattern, you should be able to replicate a full feature in:

| Phase | Time | Effort |
|-------|------|--------|
| **Entity creation** | 15 min | Copy Debtor, adjust fields |
| **Repository** | 20 min | Copy CustomerRepository, adjust methods |
| **Migration** | 5 min | dotnet ef migrations add + update |
| **DTO + Validator** | 15 min | Copy CustomerRegistrationDto/Validator |
| **Service** | 20 min | Copy CustomerService, adjust logic |
| **UI - Registration** | 30 min | Copy CustomerRegistration, adjust form |
| **UI - List** | 20 min | Copy CustomerList, adjust columns |
| **DI Registration** | 5 min | Add 3 lines to Program.cs |
| **Testing** | 15 min | Form + list + validation |
| **TOTAL** | **~2.5 hours** | Per feature |

---

## ✅ Summary

**Customer Registration is 100% complete** and ready to use as a template:

✅ **Domain**: Debtor entity with 25+ properties
✅ **DAL**: Repository with 11 methods + 4 migrations
✅ **BLL**: Service (10 methods) + Validator + DTO
✅ **UI**: Registration form + List view + POPIA masking
✅ **Infrastructure**: Full DI registration
✅ **Shared**: POPIA data masking service

**You can now replicate this pattern for**:
- Supplier registration/management
- Product management
- Employee management
- Any other entity

**Just follow the 5-step pattern** → Save Claude resources! 🎯

---

**Next Feature to Build**: Supplier? Product? Your choice!
