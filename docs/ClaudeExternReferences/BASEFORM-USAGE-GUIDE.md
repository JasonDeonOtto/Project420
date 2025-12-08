# BaseForm Usage Guide
**Project**: Project420
**Component**: BaseForm.razor (Clean Professional Version)
**Status**: USER OVERRIDE - Replaces cannabis-themed version
**Last Updated**: 2025-12-05

---

## Overview

The **BaseForm** component is a clean, professional, reusable form wrapper for Blazor applications. It provides:
- ✅ Standard Bootstrap 5 styling
- ✅ Built-in validation support
- ✅ Success/Error message display
- ✅ Automatic button state management (loading, disabled)
- ✅ Customizable button text and behavior
- ✅ Optional additional buttons
- ✅ Clean, enterprise-ready appearance

**User Override Applied**: This version replaces the cannabis-themed BaseForm per user request.

---

## Basic Usage

### Minimal Example

```razor
@page "/customer/create"
@using Project420.Management.BLL.DTOs

<BaseForm TModel="CustomerRegistrationDto"
          Model="@model"
          Title="New Customer Registration"
          OnValidSubmit="@HandleSubmit"
          OnCancel="@HandleCancel"
          IsSubmitting="@isSubmitting">

    <div class="mb-3">
        <label class="form-label">Customer Name *</label>
        <InputText class="form-control" @bind-Value="model.Name" />
        <ValidationMessage For="@(() => model.Name)" />
    </div>

    <div class="mb-3">
        <label class="form-label">Email *</label>
        <InputText class="form-control" @bind-Value="model.Email" />
        <ValidationMessage For="@(() => model.Email)" />
    </div>

</BaseForm>

@code {
    private CustomerRegistrationDto model = new();
    private bool isSubmitting = false;

    private async Task HandleSubmit(CustomerRegistrationDto dto)
    {
        isSubmitting = true;
        try
        {
            await CustomerService.CreateAsync(dto);
            NavigationManager.NavigateTo("/customers");
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void HandleCancel()
    {
        NavigationManager.NavigateTo("/customers");
    }
}
```

---

## Component Parameters

### Required Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `TModel` | Generic Type | The model type for the form |
| `Model` | TModel | The model instance to bind |
| `OnValidSubmit` | EventCallback<TModel> | Callback when form submits with valid data |

### Optional Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | string | `""` | Form title displayed at top |
| `Subtitle` | string | `null` | Optional subtitle/description |
| `SubmitText` | string | `"Save"` | Text for submit button |
| `SubmittingText` | string | `"Saving..."` | Text shown while submitting |
| `CancelText` | string | `"Cancel"` | Text for cancel button |
| `ShowCancelButton` | bool | `true` | Whether to show cancel button |
| `ShowValidationSummary` | bool | `true` | Whether to show validation summary |
| `IsSubmitting` | bool | `false` | Whether form is currently submitting |
| `SuccessMessage` | string | `null` | Success message to display |
| `ErrorMessage` | string | `null` | Error message to display |
| `AdditionalButtons` | RenderFragment | `null` | Custom buttons to add |
| `OnInvalidSubmit` | EventCallback | `null` | Callback when form submits invalid |
| `OnCancel` | EventCallback | `null` | Callback when cancel clicked |

---

## Advanced Usage

### With Success/Error Messages

```razor
<BaseForm TModel="ProductDto"
          Model="@model"
          Title="Edit Product"
          SuccessMessage="@successMessage"
          ErrorMessage="@errorMessage"
          OnValidSubmit="@HandleSubmit"
          IsSubmitting="@isSubmitting">

    <!-- Form fields -->

</BaseForm>

@code {
    private string? successMessage;
    private string? errorMessage;

    private async Task HandleSubmit(ProductDto dto)
    {
        isSubmitting = true;
        try
        {
            await ProductService.UpdateAsync(dto);
            successMessage = "Product updated successfully!";
        }
        catch (Exception ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
```

### With Additional Custom Buttons

```razor
<BaseForm TModel="ProductDto"
          Model="@model"
          Title="Product Management"
          OnValidSubmit="@HandleSave"
          IsSubmitting="@isSubmitting">

    <AdditionalButtons>
        <button type="button"
                class="btn btn-info"
                @onclick="HandleSaveAndNew"
                disabled="@isSubmitting">
            <i class="bi bi-plus-circle"></i> Save & New
        </button>

        <button type="button"
                class="btn btn-warning"
                @onclick="HandleDuplicate"
                disabled="@isSubmitting">
            <i class="bi bi-files"></i> Duplicate
        </button>
    </AdditionalButtons>

    <!-- Form fields -->

</BaseForm>

@code {
    private async Task HandleSaveAndNew(ProductDto dto)
    {
        await ProductService.CreateAsync(dto);
        model = new ProductDto(); // Reset form
    }

    private void HandleDuplicate()
    {
        var copy = model with { SKU = $"{model.SKU}-COPY" };
        model = copy;
    }
}
```

### With Programmatic Message Control

```razor
<BaseForm @ref="formRef"
          TModel="CustomerDto"
          Model="@model"
          Title="Customer Management"
          OnValidSubmit="@HandleSubmit">

    <!-- Form fields -->

</BaseForm>

@code {
    private BaseForm<CustomerDto>? formRef;

    private async Task HandleSubmit(CustomerDto dto)
    {
        try
        {
            await CustomerService.UpdateAsync(dto);
            formRef?.SetSuccessMessage("Customer saved successfully!");
        }
        catch (ValidationException ex)
        {
            formRef?.SetErrorMessage(ex.Message);
            foreach (var error in ex.Errors)
            {
                formRef?.AddValidationError(error);
            }
        }
    }

    private void ClearAllMessages()
    {
        formRef?.ClearMessages();
    }
}
```

---

## Form Field Layout Patterns

### Single Column Layout

```razor
<BaseForm TModel="MyModel" Model="@model" OnValidSubmit="@HandleSubmit">

    <div class="mb-3">
        <label class="form-label">Field 1</label>
        <InputText class="form-control" @bind-Value="model.Field1" />
        <ValidationMessage For="@(() => model.Field1)" />
    </div>

    <div class="mb-3">
        <label class="form-label">Field 2</label>
        <InputText class="form-control" @bind-Value="model.Field2" />
        <ValidationMessage For="@(() => model.Field2)" />
    </div>

</BaseForm>
```

### Two Column Layout

```razor
<BaseForm TModel="MyModel" Model="@model" OnValidSubmit="@HandleSubmit">

    <div class="row">
        <div class="col-md-6 mb-3">
            <label class="form-label">First Name</label>
            <InputText class="form-control" @bind-Value="model.FirstName" />
            <ValidationMessage For="@(() => model.FirstName)" />
        </div>

        <div class="col-md-6 mb-3">
            <label class="form-label">Last Name</label>
            <InputText class="form-control" @bind-Value="model.LastName" />
            <ValidationMessage For="@(() => model.LastName)" />
        </div>
    </div>

</BaseForm>
```

### Grouped Sections

```razor
<BaseForm TModel="MyModel" Model="@model" OnValidSubmit="@HandleSubmit">

    <!-- Basic Information -->
    <h5 class="border-bottom pb-2 mb-3">Basic Information</h5>
    <div class="row">
        <div class="col-md-6 mb-3">
            <label class="form-label">Name</label>
            <InputText class="form-control" @bind-Value="model.Name" />
            <ValidationMessage For="@(() => model.Name)" />
        </div>
        <div class="col-md-6 mb-3">
            <label class="form-label">Email</label>
            <InputText class="form-control" @bind-Value="model.Email" />
            <ValidationMessage For="@(() => model.Email)" />
        </div>
    </div>

    <!-- Contact Information -->
    <h5 class="border-bottom pb-2 mb-3 mt-4">Contact Information</h5>
    <div class="row">
        <div class="col-md-6 mb-3">
            <label class="form-label">Phone</label>
            <InputText class="form-control" @bind-Value="model.Phone" />
            <ValidationMessage For="@(() => model.Phone)" />
        </div>
        <div class="col-md-6 mb-3">
            <label class="form-label">Mobile</label>
            <InputText class="form-control" @bind-Value="model.Mobile" />
            <ValidationMessage For="@(() => model.Mobile)" />
        </div>
    </div>

</BaseForm>
```

---

## Input Types

### Text Input
```razor
<div class="mb-3">
    <label class="form-label">Name *</label>
    <InputText class="form-control" @bind-Value="model.Name" />
    <ValidationMessage For="@(() => model.Name)" />
</div>
```

### Number Input
```razor
<div class="mb-3">
    <label class="form-label">Price *</label>
    <InputNumber class="form-control" @bind-Value="model.Price" />
    <ValidationMessage For="@(() => model.Price)" />
</div>
```

### Date Input
```razor
<div class="mb-3">
    <label class="form-label">Date of Birth *</label>
    <InputDate class="form-control" @bind-Value="model.DateOfBirth" />
    <ValidationMessage For="@(() => model.DateOfBirth)" />
</div>
```

### Select/Dropdown
```razor
<div class="mb-3">
    <label class="form-label">Category *</label>
    <InputSelect class="form-control" @bind-Value="model.CategoryId">
        <option value="">-- Select Category --</option>
        @foreach (var category in categories)
        {
            <option value="@category.Id">@category.Name</option>
        }
    </InputSelect>
    <ValidationMessage For="@(() => model.CategoryId)" />
</div>
```

### Checkbox
```razor
<div class="mb-3 form-check">
    <InputCheckbox class="form-check-input" @bind-Value="model.IsActive" id="isActive" />
    <label class="form-check-label" for="isActive">Active</label>
    <ValidationMessage For="@(() => model.IsActive)" />
</div>
```

### Textarea
```razor
<div class="mb-3">
    <label class="form-label">Description</label>
    <InputTextArea class="form-control" rows="4" @bind-Value="model.Description" />
    <ValidationMessage For="@(() => model.Description)" />
</div>
```

---

## Public Methods

Access these methods via component reference (`@ref`):

### SetSuccessMessage(string message)
Display a success message alert.
```csharp
formRef?.SetSuccessMessage("Operation completed successfully!");
```

### SetErrorMessage(string message)
Display an error message alert.
```csharp
formRef?.SetErrorMessage("An error occurred. Please try again.");
```

### ClearMessages()
Clear all success/error messages and validation errors.
```csharp
formRef?.ClearMessages();
```

### AddValidationError(string error)
Add a custom validation error to the summary.
```csharp
formRef?.AddValidationError("Custom validation failed");
```

---

## Bootstrap Icons (Optional)

The BaseForm uses Bootstrap Icons for button icons. If not already included, add to your layout:

```html
<!-- In App.razor or _Host.cshtml -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css">
```

---

## Styling

The BaseForm uses standard Bootstrap 5 classes. Customize appearance using:

### Custom CSS
```css
/* Custom form styling */
.form-label {
    font-weight: 600;
    color: #333;
}

.border-bottom {
    border-color: #007bff !important;
}

.btn-primary {
    background-color: #28a745;
    border-color: #28a745;
}
```

---

## Migration from Cannabis-Themed BaseForm

If you're migrating from the old cannabis-themed BaseForm:

### Parameter Changes

| Old Parameter | New Parameter | Notes |
|--------------|---------------|-------|
| `ShowDecorations` | Removed | No decorations in clean version |
| `UseIcons` | Removed | Icons always shown (Bootstrap Icons) |
| All others | Same | Compatible parameters |

### Visual Changes
- ❌ No cannabis leaf decorations
- ❌ No custom fonts (Satisfy, Shadows Into Light, etc.)
- ❌ No custom color scheme
- ✅ Standard Bootstrap 5 styling
- ✅ Professional enterprise appearance
- ✅ Bootstrap Icons for buttons

### Code Changes Required
Remove these parameters if present:
```razor
<!-- OLD (Remove) -->
ShowDecorations="false"
UseIcons="true"

<!-- NEW (Not needed) -->
<!-- Parameters removed -->
```

---

## Complete Working Example

```razor
@page "/products/create"
@using Project420.Management.BLL.DTOs
@inject IProductService ProductService
@inject NavigationManager NavigationManager

<PageTitle>Create Product</PageTitle>

<BaseForm @ref="formRef"
          TModel="ProductCreateDto"
          Model="@model"
          Title="Create New Product"
          Subtitle="Add a new cannabis product to the inventory"
          SubmitText="Create Product"
          OnValidSubmit="@HandleSubmit"
          OnCancel="@HandleCancel"
          IsSubmitting="@isSubmitting">

    <!-- Basic Information -->
    <h5 class="border-bottom pb-2 mb-3">Basic Information</h5>

    <div class="row">
        <div class="col-md-6 mb-3">
            <label class="form-label">SKU *</label>
            <InputText class="form-control" @bind-Value="model.SKU" placeholder="e.g., BD-001" />
            <ValidationMessage For="@(() => model.SKU)" />
        </div>

        <div class="col-md-6 mb-3">
            <label class="form-label">Product Name *</label>
            <InputText class="form-control" @bind-Value="model.Name" placeholder="e.g., Blue Dream" />
            <ValidationMessage For="@(() => model.Name)" />
        </div>
    </div>

    <div class="mb-3">
        <label class="form-label">Description</label>
        <InputTextArea class="form-control" rows="3" @bind-Value="model.Description" />
        <ValidationMessage For="@(() => model.Description)" />
    </div>

    <!-- Pricing -->
    <h5 class="border-bottom pb-2 mb-3 mt-4">Pricing</h5>

    <div class="row">
        <div class="col-md-6 mb-3">
            <label class="form-label">Selling Price (incl. VAT) *</label>
            <InputNumber class="form-control" @bind-Value="model.Price" />
            <ValidationMessage For="@(() => model.Price)" />
        </div>

        <div class="col-md-6 mb-3">
            <label class="form-label">Cost Price *</label>
            <InputNumber class="form-control" @bind-Value="model.CostPrice" />
            <ValidationMessage For="@(() => model.CostPrice)" />
        </div>
    </div>

    <!-- Cannabis Compliance -->
    <h5 class="border-bottom pb-2 mb-3 mt-4">Cannabis Compliance</h5>

    <div class="row">
        <div class="col-md-4 mb-3">
            <label class="form-label">THC % *</label>
            <InputText class="form-control" @bind-Value="model.THCPercentage" placeholder="e.g., 18.5" />
            <ValidationMessage For="@(() => model.THCPercentage)" />
        </div>

        <div class="col-md-4 mb-3">
            <label class="form-label">CBD % *</label>
            <InputText class="form-control" @bind-Value="model.CBDPercentage" placeholder="e.g., 0.5" />
            <ValidationMessage For="@(() => model.CBDPercentage)" />
        </div>

        <div class="col-md-4 mb-3">
            <label class="form-label">Strain Name</label>
            <InputText class="form-control" @bind-Value="model.StrainName" />
            <ValidationMessage For="@(() => model.StrainName)" />
        </div>
    </div>

    <div class="row">
        <div class="col-md-6 mb-3">
            <label class="form-label">Batch Number *</label>
            <InputText class="form-control" @bind-Value="model.BatchNumber" placeholder="e.g., BD-2024-001" />
            <ValidationMessage For="@(() => model.BatchNumber)" />
        </div>

        <div class="col-md-6 mb-3">
            <label class="form-label">Lab Test Date</label>
            <InputDate class="form-control" @bind-Value="model.LabTestDate" />
            <ValidationMessage For="@(() => model.LabTestDate)" />
        </div>
    </div>

    <!-- Status -->
    <div class="mb-3 form-check">
        <InputCheckbox class="form-check-input" @bind-Value="model.IsActive" id="isActive" />
        <label class="form-check-label" for="isActive">Product Active</label>
    </div>

</BaseForm>

@code {
    private BaseForm<ProductCreateDto>? formRef;
    private ProductCreateDto model = new();
    private bool isSubmitting = false;

    private async Task HandleSubmit(ProductCreateDto dto)
    {
        isSubmitting = true;
        try
        {
            await ProductService.CreateAsync(dto);
            formRef?.SetSuccessMessage("Product created successfully!");

            // Wait 2 seconds then navigate
            await Task.Delay(2000);
            NavigationManager.NavigateTo("/products");
        }
        catch (Exception ex)
        {
            formRef?.SetErrorMessage($"Error creating product: {ex.Message}");
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void HandleCancel()
    {
        NavigationManager.NavigateTo("/products");
    }
}
```

---

## Best Practices

1. **Always provide validation messages** for required fields
2. **Use responsive column layouts** (Bootstrap grid) for multi-column forms
3. **Group related fields** with section headers
4. **Handle errors gracefully** with try-catch and error messages
5. **Provide user feedback** with success messages
6. **Disable submit button** while submitting (IsSubmitting = true)
7. **Use placeholder text** to guide users
8. **Mark required fields** with asterisk (*) in labels

---

## Support

For issues or questions:
- See existing form pages in Management.UI.Blazor for examples
- Check PROJECT_STANDARDS.md (note: cannabis theme overridden)
- Review Bootstrap 5 documentation for styling options

---

**Version**: 2.0 (Clean Professional)
**Status**: Active (User Override Approved)
**Compatibility**: .NET 9.0, Blazor, Bootstrap 5
