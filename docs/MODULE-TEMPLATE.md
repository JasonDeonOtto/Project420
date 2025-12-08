# Project420 - New Module Template

**Purpose**: Complete scaffolding template for creating new modules in Project420
**Last Updated**: 2025-12-07
**Applies To**: All new module development (Cultivation, Production, Inventory, etc.)

---

## Table of Contents

1. [Module Planning Checklist](#module-planning-checklist)
2. [Project Structure Template](#project-structure-template)
3. [Models Layer Templates](#models-layer-templates)
4. [DAL Layer Templates](#dal-layer-templates)
5. [BLL Layer Templates](#bll-layer-templates)
6. [UI Layer Templates](#ui-layer-templates)
7. [API Layer Templates](#api-layer-templates)
8. [Testing Templates](#testing-templates)
9. [Dependency Injection Registration](#dependency-injection-registration)
10. [Documentation Templates](#documentation-templates)

---

## Module Planning Checklist

Before creating a new module, complete this planning phase:

### Step 1: Define Module Scope

- [ ] **Module Name**: ___________ (e.g., Cultivation, Production, Inventory)
- [ ] **Module Namespace**: Project420._________.Models/DAL/BLL/UI.Blazor
- [ ] **Business Purpose**: What business problem does this solve?
- [ ] **Compliance Requirements**: Which regulations apply? (SAHPRA, DALRRD, POPIA, Cannabis Act)
- [ ] **User Roles**: Who will use this module?

### Step 2: Identify Entities

List all domain entities needed:

1. ____________ (primary entity)
2. ____________ (related entity)
3. ____________ (lookup/reference data)

### Step 3: Define Operations

What operations will users perform?

**CRUD Operations**:
- [ ] Create new _______
- [ ] Read/view _______
- [ ] Update existing _______
- [ ] Delete _______ (soft delete for POPIA)

**Business Operations**:
- [ ] ____________
- [ ] ____________
- [ ] ____________

### Step 4: Identify Integration Points

What other modules/services does this integrate with?

- [ ] **Shared Services**: VAT, Audit Log, Transaction Numbers?
- [ ] **Other Modules**: Management, POS, Online Orders?
- [ ] **External Systems**: Lab testing, accounting, payment processors?

### Step 5: Define Compliance Requirements

- [ ] **SAHPRA GMP**: Seed-to-sale tracking, batch numbers, lab testing?
- [ ] **DALRRD**: Hemp permit tracking, THC testing?
- [ ] **POPIA**: PII encryption, audit trails, 7-year retention?
- [ ] **Cannabis Act**: Age verification, possession limits?
- [ ] **SARS**: Tax calculations, VAT, excise?

---

## Project Structure Template

### Create the following projects:

```
src/Modules/<ModuleName>/
├── Project420.<ModuleName>.Models/
│   ├── Entities/
│   │   └── <EntityName>.cs
│   ├── Enums/
│   │   └── <EnumName>.cs
│   └── Interfaces/
│       └── I<Interface>.cs (if needed)
│
├── Project420.<ModuleName>.DAL/
│   ├── <ModuleName>DbContext.cs
│   ├── Repositories/
│   │   ├── I<Entity>Repository.cs
│   │   └── <Entity>Repository.cs
│   ├── Migrations/
│   └── Configurations/ (optional)
│       └── <Entity>Configuration.cs
│
├── Project420.<ModuleName>.BLL/
│   ├── Services/
│   │   ├── I<Entity>Service.cs
│   │   └── <Entity>Service.cs
│   ├── DTOs/
│   │   ├── Create<Entity>Dto.cs
│   │   ├── Update<Entity>Dto.cs
│   │   └── <Entity>DetailsDto.cs
│   ├── Validators/
│   │   ├── Create<Entity>Validator.cs
│   │   └── Update<Entity>Validator.cs
│   └── Mappings/
│       └── <Module>MappingProfile.cs
│
└── Project420.<ModuleName>.UI.Blazor/
    ├── Components/
    │   ├── Pages/
    │   │   └── <Entity>/
    │   │       ├── <Entity>List.razor
    │   │       ├── <Entity>Create.razor
    │   │       └── <Entity>Edit.razor
    │   └── Shared/
    │       └── <Module>NavMenu.razor
    ├── wwwroot/
    │   ├── css/
    │   └── images/
    └── Program.cs
```

---

## Models Layer Templates

### Entity Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.Models/Entities/<EntityName>.cs`

```csharp
using Project420.Shared.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project420.<ModuleName>.Models.Entities
{
    /// <summary>
    /// Represents a <EntityDescription>.
    /// Complies with [Cannabis Act 2024 / SAHPRA GMP / DALRRD / POPIA] requirements.
    /// </summary>
    public class <EntityName> : AuditableEntity
    {
        /// <summary>
        /// Primary identifier
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// <Property description>
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// <Property description>
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Batch number for traceability (SAHPRA GMP requirement)
        /// </summary>
        [StringLength(100)]
        public string? BatchNumber { get; set; }

        /// <summary>
        /// Date of laboratory testing (Cannabis Act compliance)
        /// </summary>
        public DateTime? LabTestDate { get; set; }

        /// <summary>
        /// Whether entity is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        // public virtual ICollection<<RelatedEntity>>? <RelatedEntities> { get; set; }
        // public virtual <ParentEntity>? <ParentEntity> { get; set; }
        // public int <ParentEntity>Id { get; set; }
    }
}
```

### Enum Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.Models/Enums/<EnumName>.cs`

```csharp
namespace Project420.<ModuleName>.Models.Enums
{
    /// <summary>
    /// <Enum description>
    /// </summary>
    public enum <EnumName>
    {
        /// <summary>
        /// <Value description>
        /// </summary>
        <Value1> = 1,

        /// <summary>
        /// <Value description>
        /// </summary>
        <Value2> = 2,

        /// <summary>
        /// <Value description>
        /// </summary>
        <Value3> = 3
    }
}
```

---

## DAL Layer Templates

### DbContext Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.DAL/<ModuleName>DbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.<ModuleName>.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.<ModuleName>.DAL
{
    /// <summary>
    /// Database context for the <ModuleName> module.
    /// Manages <entities list>.
    /// </summary>
    public class <ModuleName>DbContext : DbContext
    {
        public <ModuleName>DbContext(DbContextOptions<<ModuleName>DbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<<EntityName>> <EntityName>s { get; set; }
        // Add more DbSets here

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(<ModuleName>DbContext).Assembly);

            // Global query filters (soft delete - POPIA compliance)
            modelBuilder.Entity<<EntityName>>().HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            // modelBuilder.Entity<<EntityName>>()
            //     .HasOne(e => e.<ParentEntity>)
            //     .WithMany(p => p.<ChildEntities>)
            //     .HasForeignKey(e => e.<ParentEntity>Id);
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

### Repository Interface Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.DAL/Repositories/I<Entity>Repository.cs`

```csharp
using Project420.<ModuleName>.Models.Entities;

namespace Project420.<ModuleName>.DAL.Repositories
{
    /// <summary>
    /// Repository interface for <EntityName> entity operations.
    /// </summary>
    public interface I<Entity>Repository
    {
        // Basic CRUD
        Task<<Entity>?> GetByIdAsync(int id);
        Task<IEnumerable<<Entity>>> GetAllAsync();
        Task<<Entity>> CreateAsync(<Entity> entity);
        Task<<Entity>> UpdateAsync(<Entity> entity);
        Task DeleteAsync(int id);

        // Business queries (add as needed)
        Task<IEnumerable<<Entity>>> GetActive<Entity>sAsync();
        Task<<Entity>?> GetBy<UniqueProperty>Async(string <uniqueProperty>);
        // Add more business-specific queries here
    }
}
```

### Repository Implementation Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.DAL/Repositories/<Entity>Repository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Project420.<ModuleName>.DAL.Repositories;
using Project420.<ModuleName>.Models.Entities;

namespace Project420.<ModuleName>.DAL.Repositories
{
    /// <summary>
    /// Repository implementation for <EntityName> entity.
    /// Provides data access methods with cannabis compliance checks.
    /// </summary>
    public class <Entity>Repository : I<Entity>Repository
    {
        private readonly <ModuleName>DbContext _context;

        public <Entity>Repository(<ModuleName>DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets a <entity> by ID.
        /// </summary>
        /// <param name="id"><Entity> ID</param>
        /// <returns><Entity> or null if not found</returns>
        public async Task<<Entity>?> GetByIdAsync(int id)
        {
            return await _context.<Entity>s
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Gets all <entities>.
        /// </summary>
        /// <returns>List of all <entities></returns>
        public async Task<IEnumerable<<Entity>>> GetAllAsync()
        {
            return await _context.<Entity>s
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new <entity>.
        /// </summary>
        /// <param name="entity"><Entity> to create</param>
        /// <returns>Created <entity> with ID</returns>
        public async Task<<Entity>> CreateAsync(<Entity> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.<Entity>s.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates an existing <entity>.
        /// </summary>
        /// <param name="entity"><Entity> to update</param>
        /// <returns>Updated <entity></returns>
        public async Task<<Entity>> UpdateAsync(<Entity> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.<Entity>s.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Soft deletes a <entity> (POPIA compliance).
        /// </summary>
        /// <param name="id"><Entity> ID to delete</param>
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.<Entity>s.Remove(entity); // Soft delete via DbContext override
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Gets all active <entities>.
        /// </summary>
        /// <returns>List of active <entities></returns>
        public async Task<IEnumerable<<Entity>>> GetActive<Entity>sAsync()
        {
            return await _context.<Entity>s
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        // Add more business-specific repository methods here
    }
}
```

---

## BLL Layer Templates

### DTO Templates

**Create DTO**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/DTOs/Create<Entity>Dto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Project420.<ModuleName>.BLL.DTOs
{
    /// <summary>
    /// Data transfer object for creating a new <Entity>.
    /// </summary>
    public class Create<Entity>Dto
    {
        /// <summary>
        /// <Property description>
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// <Property description>
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Batch number for traceability (SAHPRA GMP requirement)
        /// </summary>
        [Required(ErrorMessage = "Batch number is required for SAHPRA compliance")]
        [StringLength(100)]
        public string BatchNumber { get; set; } = string.Empty;

        // Add more properties as needed
    }
}
```

**Update DTO**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/DTOs/Update<Entity>Dto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Project420.<ModuleName>.BLL.DTOs
{
    /// <summary>
    /// Data transfer object for updating an existing <Entity>.
    /// </summary>
    public class Update<Entity>Dto
    {
        // Same properties as CreateDto (or subset)
        // Usually does not include CreatedAt, CreatedBy (audit fields)
    }
}
```

**Details DTO**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/DTOs/<Entity>DetailsDto.cs`

```csharp
namespace Project420.<ModuleName>.BLL.DTOs
{
    /// <summary>
    /// Data transfer object for <Entity> details.
    /// </summary>
    public class <Entity>DetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BatchNumber { get; set; }
        public DateTime? LabTestDate { get; set; }
        public bool IsActive { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        // Add more properties as needed
    }
}
```

### AutoMapper Profile Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/Mappings/<Module>MappingProfile.cs`

```csharp
using AutoMapper;
using Project420.<ModuleName>.BLL.DTOs;
using Project420.<ModuleName>.Models.Entities;

namespace Project420.<ModuleName>.BLL.Mappings
{
    /// <summary>
    /// AutoMapper profile for <ModuleName> module.
    /// </summary>
    public class <Module>MappingProfile : Profile
    {
        public <Module>MappingProfile()
        {
            // Entity to DetailsDto
            CreateMap<<Entity>, <Entity>DetailsDto>();

            // CreateDto to Entity
            CreateMap<Create<Entity>Dto, <Entity>>();

            // UpdateDto to Entity
            CreateMap<Update<Entity>Dto, <Entity>>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) // Preserve ID
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Preserve audit fields
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // Add more mappings as needed
        }
    }
}
```

### Service Interface Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/Services/I<Entity>Service.cs`

```csharp
using Project420.<ModuleName>.BLL.DTOs;

namespace Project420.<ModuleName>.BLL.Services
{
    /// <summary>
    /// Service interface for <entity> management operations.
    /// Handles business logic and validation for <entity> management.
    /// </summary>
    public interface I<Entity>Service
    {
        // CRUD operations (using DTOs)
        Task<<Entity>DetailsDto> Create<Entity>Async(Create<Entity>Dto createDto);
        Task<<Entity>DetailsDto?> Get<Entity>ByIdAsync(int id);
        Task<IEnumerable<<Entity>DetailsDto>> GetAll<Entity>sAsync();
        Task<<Entity>DetailsDto> Update<Entity>Async(int id, Update<Entity>Dto updateDto);
        Task Delete<Entity>Async(int id);

        // Business operations (add as needed)
        Task<IEnumerable<<Entity>DetailsDto>> GetActive<Entity>sAsync();
        Task<bool> Is<UniqueProperty>UniqueAsync(string <uniqueProperty>, int? excludeId = null);
        // Add more business methods here
    }
}
```

### Service Implementation Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/Services/<Entity>Service.cs`

```csharp
using AutoMapper;
using FluentValidation;
using Project420.<ModuleName>.BLL.DTOs;
using Project420.<ModuleName>.BLL.Validators;
using Project420.<ModuleName>.DAL.Repositories;
using Project420.<ModuleName>.Models.Entities;

namespace Project420.<ModuleName>.BLL.Services
{
    /// <summary>
    /// Service implementation for <entity> management.
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

            // Map and create
            var entity = _mapper.Map<<Entity>>(createDto);
            var createdEntity = await _repository.CreateAsync(entity);

            // Return DTO
            return _mapper.Map<<Entity>DetailsDto>(createdEntity);
        }

        public async Task<<Entity>DetailsDto?> Get<Entity>ByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<<Entity>DetailsDto>(entity);
        }

        public async Task<IEnumerable<<Entity>DetailsDto>> GetAll<Entity>sAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<<Entity>DetailsDto>>(entities);
        }

        public async Task<<Entity>DetailsDto> Update<Entity>Async(int id, Update<Entity>Dto updateDto)
        {
            // Check existence
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"<Entity> with ID {id} not found.");
            }

            // Validate
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Map and update
            _mapper.Map(updateDto, existingEntity);
            var updatedEntity = await _repository.UpdateAsync(existingEntity);

            // Return DTO
            return _mapper.Map<<Entity>DetailsDto>(updatedEntity);
        }

        public async Task Delete<Entity>Async(int id)
        {
            var existingEntity = await _repository.GetByIdAsync(id);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"<Entity> with ID {id} not found.");
            }

            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<<Entity>DetailsDto>> GetActive<Entity>sAsync()
        {
            var entities = await _repository.GetActive<Entity>sAsync();
            return _mapper.Map<IEnumerable<<Entity>DetailsDto>>(entities);
        }

        // Add more business methods here
    }
}
```

### Validator Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.BLL/Validators/Create<Entity>Validator.cs`

```csharp
using FluentValidation;
using Project420.<ModuleName>.BLL.DTOs;

namespace Project420.<ModuleName>.BLL.Validators
{
    /// <summary>
    /// Validator for <entity> creation.
    /// Enforces cannabis industry compliance requirements.
    /// </summary>
    public class Create<Entity>Validator : AbstractValidator<Create<Entity>Dto>
    {
        public Create<Entity>Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

            // Cannabis-specific validations
            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required for traceability (SAHPRA GMP requirement)")
                .MaximumLength(100).WithMessage("Batch number cannot exceed 100 characters");

            // Add more validation rules here
        }
    }
}
```

---

## UI Layer Templates

### Blazor List Page Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.UI.Blazor/Components/Pages/<Entity>/<Entity>List.razor`

```razor
@page "/<entity-plural>"
@using Project420.<ModuleName>.BLL.DTOs
@using Project420.<ModuleName>.BLL.Services
@inject I<Entity>Service <Entity>Service
@inject NavigationManager Navigation

<PageTitle><Entity> List</PageTitle>

@if (!string.IsNullOrEmpty(successMessage))
{
    <div class="alert-custom alert-success-custom">
        @successMessage
    </div>
}

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert-custom alert-danger-custom">
        @errorMessage
    </div>
}

<div class="base-form-container">
    <h1 class="form-title"><Entity> Management</h1>

    <div style="margin-bottom: 1rem;">
        <button class="btn-secondary-custom btn-icon" @onclick="NavigateToCreate">
            <img src="images/cannabis-seed.svg" alt="" />
            <span>Add New <Entity></span>
        </button>
    </div>

    @if (isLoading)
    {
        <div style="text-align: center;">
            <img src="images/cannabis-seed.svg" class="loading-icon" alt="Loading" />
            <p style="color: var(--text-secondary);">Loading <entities>...</p>
        </div>
    }
    else if (<entities> != null && <entities>.Any())
    {
        <table class="table-custom">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Batch Number</th>
                    <th>Status</th>
                    <th>Created Date</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var <entity> in <entities>)
                {
                    <tr>
                        <td>@<entity>.Name</td>
                        <td>@<entity>.BatchNumber</td>
                        <td>
                            <span class="@(<entity>.IsActive ? "badge-success" : "badge-inactive")">
                                @(<entity>.IsActive ? "Active" : "Inactive")
                            </span>
                        </td>
                        <td>@<entity>.CreatedAt.ToString("yyyy-MM-dd")</td>
                        <td>
                            <button class="btn-primary-custom btn-action btn-icon"
                                    @onclick="() => NavigateToEdit(<entity>.Id)">
                                <img src="images/cannabis-leaf.svg" alt="" />
                                <span>Edit</span>
                            </button>
                            <button class="btn-danger-custom btn-action btn-icon"
                                    @onclick="() => Delete<Entity>(<entity>.Id)">
                                <img src="images/cannabis-flower.svg" alt="" />
                                <span>Delete</span>
                            </button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }
    else
    {
        <p style="color: var(--text-secondary); text-align: center;">No <entities> found.</p>
    }
</div>

@code {
    private List<<Entity>DetailsDto>? <entities>;
    private bool isLoading = true;
    private string? successMessage;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await Load<Entity>sAsync();
    }

    private async Task Load<Entity>sAsync()
    {
        try
        {
            isLoading = true;
            var result = await <Entity>Service.GetAll<Entity>sAsync();
            <entities> = result.ToList();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error loading <entities>: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToCreate()
    {
        Navigation.NavigateTo("/<entity-plural>/create");
    }

    private void NavigateToEdit(int id)
    {
        Navigation.NavigateTo($"/<entity-plural>/edit/{id}");
    }

    private async Task Delete<Entity>(int id)
    {
        if (!await ConfirmDelete())
            return;

        try
        {
            await <Entity>Service.Delete<Entity>Async(id);
            successMessage = "<Entity> deleted successfully.";
            await Load<Entity>sAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error deleting <entity>: {ex.Message}";
        }
    }

    private async Task<bool> ConfirmDelete()
    {
        // TODO: Implement proper confirmation dialog
        return await Task.FromResult(true);
    }
}
```

### Blazor Create Page Template

**File**: `src/Modules/<ModuleName>/Project420.<ModuleName>.UI.Blazor/Components/Pages/<Entity>/<Entity>Create.razor`

```razor
@page "/<entity-plural>/create"
@using Project420.<ModuleName>.BLL.DTOs
@using Project420.<ModuleName>.BLL.Services
@inject I<Entity>Service <Entity>Service
@inject NavigationManager Navigation

<PageTitle>Create <Entity></PageTitle>

@if (!string.IsNullOrEmpty(successMessage))
{
    <div class="alert-custom alert-success-custom">
        @successMessage
    </div>
}

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert-custom alert-danger-custom">
        @errorMessage
    </div>
}

<BaseForm TModel="Create<Entity>Dto"
          Model="@model"
          Title="Create New <Entity>"
          Subtitle="Cannabis Act 2024 / SAHPRA GMP Compliance"
          SubmitText="Create <Entity>"
          OnValidSubmit="@HandleSubmit"
          OnCancel="@HandleCancel"
          IsSubmitting="@isSubmitting">

    <div class="form-section">
        <h3 class="form-section-header">Basic Information</h3>

        <div class="form-field">
            <label class="form-label">Name *</label>
            <InputText class="form-input" @bind-Value="model.Name" />
            <ValidationMessage For="@(() => model.Name)" class="validation-message" />
        </div>

        <div class="form-field">
            <label class="form-label">Description</label>
            <InputTextArea class="form-textarea" @bind-Value="model.Description" rows="4" />
            <ValidationMessage For="@(() => model.Description)" class="validation-message" />
        </div>
    </div>

    <div class="form-section" style="border: 2px solid var(--ferrari-red-faded);">
        <h3 class="form-section-header" style="color: var(--ferrari-red-hover);">
            Compliance Information (SAHPRA GMP)
        </h3>

        <div class="form-field">
            <label class="form-label">Batch Number *</label>
            <InputText class="form-input" @bind-Value="model.BatchNumber" />
            <ValidationMessage For="@(() => model.BatchNumber)" class="validation-message" />
            <small style="color: var(--text-secondary);">Required for seed-to-sale traceability</small>
        </div>

        <!-- Add more compliance fields as needed -->
    </div>

</BaseForm>

@code {
    private Create<Entity>Dto model = new();
    private bool isSubmitting = false;
    private string? successMessage;
    private string? errorMessage;

    private async Task HandleSubmit()
    {
        try
        {
            isSubmitting = true;
            errorMessage = null;

            await <Entity>Service.Create<Entity>Async(model);

            successMessage = "<Entity> created successfully!";

            // Navigate back to list after 2 seconds
            await Task.Delay(2000);
            Navigation.NavigateTo("/<entity-plural>");
        }
        catch (ValidationException ex)
        {
            errorMessage = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            errorMessage = $"Error creating <entity>: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void HandleCancel()
    {
        Navigation.NavigateTo("/<entity-plural>");
    }
}
```

---

## API Layer Templates

### API Controller Template

**File**: `src/API/Project420.API.WebApi/Controllers/<Entity>Controller.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project420.<ModuleName>.BLL.DTOs;
using Project420.<ModuleName>.BLL.Services;

namespace Project420.API.WebApi.Controllers
{
    /// <summary>
    /// API controller for <entity> management.
    /// Provides endpoints for <entity> operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Cannabis Act: Age verification required
    public class <Entity>Controller : ControllerBase
    {
        private readonly I<Entity>Service _service;
        private readonly ILogger<<Entity>Controller> _logger;

        public <Entity>Controller(
            I<Entity>Service service,
            ILogger<<Entity>Controller> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all <entities>.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<<Entity>DetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<<Entity>DetailsDto>>> Get<Entity>s()
        {
            try
            {
                var entities = await _service.GetAll<Entity>sAsync();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving <entities>");
                return StatusCode(500, "An error occurred while retrieving <entities>");
            }
        }

        /// <summary>
        /// Gets a <entity> by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(<Entity>DetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<<Entity>DetailsDto>> Get<Entity>(int id)
        {
            try
            {
                var entity = await _service.Get<Entity>ByIdAsync(id);

                if (entity == null)
                    return NotFound($"<Entity> with ID {id} not found");

                return Ok(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving <entity> {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the <entity>");
            }
        }

        /// <summary>
        /// Creates a new <entity>.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(<Entity>DetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<<Entity>DetailsDto>> Create<Entity>([FromBody] Create<Entity>Dto createDto)
        {
            try
            {
                var entity = await _service.Create<Entity>Async(createDto);
                return CreatedAtAction(nameof(Get<Entity>), new { id = entity.Id }, entity);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating <entity>");
                return StatusCode(500, "An error occurred while creating the <entity>");
            }
        }

        /// <summary>
        /// Updates an existing <entity>.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(<Entity>DetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<<Entity>DetailsDto>> Update<Entity>(int id, [FromBody] Update<Entity>Dto updateDto)
        {
            try
            {
                var entity = await _service.Update<Entity>Async(id, updateDto);
                return Ok(entity);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"<Entity> with ID {id} not found");
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating <entity> {Id}", id);
                return StatusCode(500, "An error occurred while updating the <entity>");
            }
        }

        /// <summary>
        /// Deletes a <entity> (soft delete for POPIA compliance).
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete<Entity>(int id)
        {
            try
            {
                await _service.Delete<Entity>Async(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"<Entity> with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting <entity> {Id}", id);
                return StatusCode(500, "An error occurred while deleting the <entity>");
            }
        }
    }
}
```

---

## Testing Templates

### Service Test Template

**File**: `tests/Project420.<ModuleName>.Tests/Services/<Entity>ServiceTests.cs`

```csharp
using FluentAssertions;
using Moq;
using Project420.<ModuleName>.BLL.DTOs;
using Project420.<ModuleName>.BLL.Services;
using Project420.<ModuleName>.DAL.Repositories;
using Project420.<ModuleName>.Models.Entities;
using Xunit;

namespace Project420.<ModuleName>.Tests.Services
{
    /// <summary>
    /// Unit tests for <Entity>Service.
    /// Tests business logic and cannabis compliance rules.
    /// </summary>
    public class <Entity>ServiceTests
    {
        private readonly Mock<I<Entity>Repository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IValidator<Create<Entity>Dto>> _mockCreateValidator;
        private readonly Mock<IValidator<Update<Entity>Dto>> _mockUpdateValidator;
        private readonly <Entity>Service _service;

        public <Entity>ServiceTests()
        {
            _mockRepository = new Mock<I<Entity>Repository>();
            _mockMapper = new Mock<IMapper>();
            _mockCreateValidator = new Mock<IValidator<Create<Entity>Dto>>();
            _mockUpdateValidator = new Mock<IValidator<Update<Entity>Dto>>();

            _service = new <Entity>Service(
                _mockRepository.Object,
                _mockMapper.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object);
        }

        [Fact]
        public async Task Create<Entity>Async_Valid<Entity>_Returns<Entity>Dto()
        {
            // Arrange
            var createDto = new Create<Entity>Dto
            {
                Name = "Test <Entity>",
                BatchNumber = "BATCH-001"
            };

            var validationResult = new ValidationResult();
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var entity = new <Entity> { Id = 1, Name = "Test <Entity>" };
            _mockMapper.Setup(m => m.Map<<Entity>>(createDto)).Returns(entity);
            _mockRepository.Setup(r => r.CreateAsync(entity)).ReturnsAsync(entity);

            var entityDto = new <Entity>DetailsDto { Id = 1, Name = "Test <Entity>" };
            _mockMapper.Setup(m => m.Map<<Entity>DetailsDto>(entity)).Returns(entityDto);

            // Act
            var result = await _service.Create<Entity>Async(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<<Entity>>()), Times.Once);
        }

        [Fact]
        public async Task Create<Entity>Async_Invalid<Entity>_ThrowsValidationException()
        {
            // Arrange
            var createDto = new Create<Entity>Dto { Name = "" }; // Invalid

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required")
            };
            var validationResult = new ValidationResult(validationErrors);

            _mockCreateValidator
                .Setup(v => v.ValidateAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.Create<Entity>Async(createDto));
        }

        // Add more tests here
    }
}
```

---

## Dependency Injection Registration

### Add to Program.cs

```csharp
// ========================================
// <MODULE NAME> MODULE
// ========================================
// DbContext
builder.Services.AddDbContext<<ModuleName>DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("<ModuleName>Db")));

// Repositories
builder.Services.AddScoped<I<Entity>Repository, <Entity>Repository>();

// Services
builder.Services.AddScoped<I<Entity>Service, <Entity>Service>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Create<Entity>Validator>();

// AutoMapper (if creating new profile)
// builder.Services.AddAutoMapper(typeof(<Module>MappingProfile));
```

### Add to appsettings.json

```json
{
  "ConnectionStrings": {
    "<ModuleName>Db": "Server=localhost\\SQLDEVED;Database=Project420_<ModuleName>;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

---

## Documentation Templates

### Module Documentation

Create: `docs/<MODULE-NAME>-MODULE-ARCHITECTURE.md`

```markdown
# <ModuleName> Module Architecture

**Last Updated**: <Date>
**Status**: <Status>
**Module**: Project420.<ModuleName>

---

## Overview

<Brief description of module purpose>

### Business Purpose

<What business problem does this solve?>

### Compliance Requirements

- **SAHPRA GMP**: <Requirements>
- **DALRRD**: <Requirements>
- **POPIA**: <Requirements>
- **Cannabis Act**: <Requirements>

---

## Entities

### <EntityName>

**Purpose**: <Entity description>

**Key Fields**:
- Field1: Description
- Field2: Description

**Relationships**:
- <ParentEntity> (many-to-one)
- <ChildEntities> (one-to-many)

---

## Business Operations

1. **Create <Entity>**: <Description>
2. **View <Entity>**: <Description>
3. **Update <Entity>**: <Description>
4. **Delete <Entity>**: <Description>

---

## Integration Points

- **Shared Services**: VAT, Audit Log, Transaction Numbers
- **Other Modules**: Management, POS, etc.
- **External Systems**: Lab testing, accounting, etc.

---

## File Structure

<Show project structure>

---

## Implementation Status

- [ ] Models created
- [ ] DAL implemented
- [ ] BLL implemented
- [ ] UI created
- [ ] API endpoints added
- [ ] Tests written
- [ ] Documentation complete

---
```

---

## Quick Start Guide

### Step-by-Step Module Creation

1. **Plan**: Complete Module Planning Checklist
2. **Create Projects**: Use Project Structure Template
3. **Models**: Create entities and enums
4. **DAL**: Create DbContext and repositories
5. **BLL**: Create services, DTOs, validators, AutoMapper profile
6. **UI**: Create Blazor pages
7. **API**: Create API controller (if needed)
8. **DI**: Register in Program.cs
9. **Tests**: Create test project and write tests
10. **Documentation**: Create module documentation

---

**TEMPLATES READY**: ✅
**SCAFFOLDING PATTERN**: Established
**COMPLIANCE**: Built-in
**READY FOR**: Cultivation, Production, Inventory, and all future modules
