# Cannabis Theme Conversion Guide

## Overview

This guide shows how to convert your existing Bootstrap-styled forms to use the new cannabis-themed styling with cannabis green (#4a7c59), faded Ferrari red (#c75450), and dead rose purple (#8b5e83) colors.

## Quick Reference

### Color Palette
- **Cannabis Green**: `#4a7c59` (primary actions, success)
- **Cannabis Green Light**: `#6b9d7a` (highlights, hover states)
- **Cannabis Green Dark**: `#2d5a3d` (borders, shadows)
- **Ferrari Red Faded**: `#c75450` (danger actions, errors)
- **Dead Rose Purple**: `#8b5e83` (secondary, accents)
- **Background Dark**: `#1a1a1a` (main background)
- **Background Card**: `#2d2d2d` (container backgrounds)

### Fonts
- **Titles**: 'Satisfy' (flowing, smokey)
- **Labels/Text**: 'Shadows Into Light' (handwritten feel)
- **Buttons**: 'Permanent Marker' (bold, impactful)

## Two Approaches

### Option 1: Use BaseForm Component (Recommended)

**Pros:**
- Automatic themed styling
- Consistent layout
- Built-in submit/cancel handling
- Icons included
- Less code

**When to use:** New forms or forms you're heavily refactoring

### Option 2: Manual Themed Styling

**Pros:**
- Full control over layout
- Use existing form structure
- Gradual migration

**When to use:** Complex existing forms with custom layouts

---

## Option 1: BaseForm Component

### Basic Usage

```razor
@page "/your-page"
@using Your.Models

<BaseForm TModel="YourModel"
          Model="@model"
          Title="Your Form Title"
          Subtitle="Optional subtitle"
          SubmitText="Save"
          OnValidSubmit="@HandleSubmit"
          OnCancel="@HandleCancel"
          IsSubmitting="@isSubmitting">

    <!-- Your form fields here -->
    <div class="form-field">
        <label class="form-label">Field Name</label>
        <InputText class="form-input" @bind-Value="model.Property" />
        <ValidationMessage For="@(() => model.Property)" class="validation-message" />
    </div>

</BaseForm>

@code {
    private YourModel model = new();
    private bool isSubmitting = false;

    private async Task HandleSubmit(YourModel submittedModel)
    {
        isSubmitting = true;
        try
        {
            // Your save logic
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void HandleCancel()
    {
        // Navigate away or reset
    }
}
```

### BaseForm Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Model` | TModel | required | The form model |
| `Title` | string | "Form" | Main form title |
| `Subtitle` | string? | null | Optional subtitle text |
| `SubmitText` | string | "Submit" | Submit button text |
| `SubmittingText` | string | "Submitting..." | Text shown while submitting |
| `CancelText` | string | "Cancel" | Cancel button text |
| `ShowCancelButton` | bool | true | Show/hide cancel button |
| `ShowDecorations` | bool | true | Show/hide cannabis leaf watermarks |
| `UseIcons` | bool | true | Show/hide button icons |
| `IsSubmitting` | bool | false | Controls button disabled state |
| `OnValidSubmit` | EventCallback<TModel> | required | Submit handler |
| `OnCancel` | EventCallback | optional | Cancel handler |

---

## Option 2: Manual Themed Styling

### Class Reference

#### Containers
- `.base-form-container` - Main form wrapper
- `.form-section` - Grouped fields section
- `.form-section-header` - Section title

#### Fields
- `.form-field` - Field wrapper
- `.form-label` - Field label
- `.form-input` - Text input
- `.form-textarea` - Textarea
- `.form-check-custom` - Checkbox wrapper

#### Buttons
- `.btn-primary-custom` - Green primary button
- `.btn-danger-custom` - Red danger button
- `.btn-secondary-custom` - Purple secondary button
- `.btn-icon` - Add to button for icon support
- `.form-buttons` - Button container (flexbox)

#### Alerts
- `.alert-custom` - Base alert
- `.alert-success-custom` - Success message
- `.alert-danger-custom` - Error message
- `.alert-info-custom` - Info message

#### Tables
- `.table-custom` - Themed table for lists

### Manual Conversion Example

**Before (Bootstrap):**
```razor
<div class="container mt-4">
    <h1 class="mb-4">Customer Registration</h1>

    <EditForm Model="@model" OnValidSubmit="@HandleSubmit">
        <div class="card mb-4">
            <div class="card-header bg-primary text-white">
                <h5 class="mb-0">Personal Information</h5>
            </div>
            <div class="card-body">
                <div class="mb-3">
                    <label class="form-label">Full Name</label>
                    <InputText class="form-control" @bind-Value="model.Name" />
                </div>
            </div>
        </div>

        <button type="submit" class="btn btn-primary">Submit</button>
        <button type="button" class="btn btn-secondary">Cancel</button>
    </EditForm>
</div>
```

**After (Themed):**
```razor
<div class="base-form-container">
    <h2 class="form-title">Customer Registration</h2>

    <EditForm Model="@model" OnValidSubmit="@HandleSubmit">
        <div class="form-section">
            <h3 class="form-section-header">Personal Information</h3>

            <div class="form-field">
                <label class="form-label">Full Name</label>
                <InputText class="form-input" @bind-Value="model.Name" />
                <ValidationMessage For="@(() => model.Name)" class="validation-message" />
            </div>
        </div>

        <div class="form-buttons">
            <button type="button" class="btn-danger-custom btn-icon">
                <img src="images/cannabis-flower.svg" alt="" />
                <span>Cancel</span>
            </button>
            <button type="submit" class="btn-primary-custom btn-icon">
                <img src="images/cannabis-bud.svg" alt="" />
                <span>Submit</span>
            </button>
        </div>
    </EditForm>
</div>
```

---

## Converting CustomerRegistration.razor

Here's a partial conversion of your existing CustomerRegistration page:

### Step 1: Replace outer container
```razor
<!-- OLD -->
<div class="container mt-4">
    <h1 class="mb-4">Customer Registration</h1>
    <p class="text-muted mb-4">Register a new customer...</p>

<!-- NEW -->
<div class="base-form-container">
    <h2 class="form-title">Customer Registration</h2>
    <p style="color: var(--text-secondary); font-family: 'Shadows Into Light', cursive;">
        Register a new customer - Cannabis Act &amp; POPIA compliant
    </p>
```

### Step 2: Replace cards with sections
```razor
<!-- OLD -->
<div class="card mb-4">
    <div class="card-header bg-primary text-white">
        <h5 class="mb-0">Personal Information</h5>
    </div>
    <div class="card-body">

<!-- NEW -->
<div class="form-section">
    <h3 class="form-section-header">Personal Information</h3>
```

### Step 3: Replace form fields
```razor
<!-- OLD -->
<div class="col-md-12 mb-3">
    <label for="name" class="form-label">Full Name *</label>
    <InputText id="name" class="form-control" @bind-Value="model.Name" />
</div>

<!-- NEW -->
<div class="form-field">
    <label class="form-label">Full Name *</label>
    <InputText class="form-input" @bind-Value="model.Name" placeholder="Enter full name" />
    <ValidationMessage For="@(() => model.Name)" class="validation-message" />
</div>
```

### Step 4: Replace alerts
```razor
<!-- OLD -->
<div class="alert alert-success alert-dismissible fade show" role="alert">
    @successMessage
</div>

<!-- NEW -->
<div class="alert-custom alert-success-custom">
    @successMessage
</div>
```

### Step 5: Replace buttons
```razor
<!-- OLD -->
<button type="button" class="btn btn-secondary" @onclick="Cancel">
    <i class="bi bi-x-circle"></i> Cancel
</button>
<button type="submit" class="btn btn-primary btn-lg">
    <i class="bi bi-check-circle"></i> Register Customer
</button>

<!-- NEW -->
<div class="form-buttons">
    <button type="button" class="btn-danger-custom btn-icon" @onclick="Cancel">
        <img src="images/cannabis-flower.svg" alt="" />
        <span>Cancel</span>
    </button>
    <button type="submit" class="btn-primary-custom btn-icon" disabled="@isSubmitting">
        @if (isSubmitting)
        {
            <img src="images/cannabis-seed.svg" class="loading-icon" alt="" />
            <span>Registering...</span>
        }
        else
        {
            <img src="images/cannabis-bud.svg" alt="" />
            <span>Register Customer</span>
        }
    </button>
</div>
```

---

## Icon Usage Guide

### Available Icons
- **cannabis-leaf.svg** - Success states, decorations
- **cannabis-bud.svg** - Submit/save actions (completion)
- **cannabis-seed.svg** - Add/create actions, loading spinner
- **cannabis-flower.svg** - Delete/cancel actions (end of cycle)

### Using Icons in Buttons
```razor
<!-- Primary action with bud icon -->
<button class="btn-primary-custom btn-icon">
    <img src="images/cannabis-bud.svg" alt="" />
    <span>Save</span>
</button>

<!-- Delete action with flower icon -->
<button class="btn-danger-custom btn-icon">
    <img src="images/cannabis-flower.svg" alt="" />
    <span>Delete</span>
</button>

<!-- Loading state with spinning seed -->
<button disabled>
    <img src="images/cannabis-seed.svg" class="loading-icon" alt="" />
    <span>Loading...</span>
</button>
```

### Decorative Watermarks
```razor
<div class="base-form-container" style="position: relative;">
    <img src="images/cannabis-leaf.svg" class="form-decoration top-left" alt="" />
    <img src="images/cannabis-leaf.svg" class="form-decoration bottom-right" alt="" />

    <!-- Form content -->
</div>
```

---

## Migration Strategy

### Phase 1: Infrastructure (DONE)
- ✅ Created form-theme.css
- ✅ Created BaseForm component
- ✅ Created SVG icons
- ✅ Updated App.razor

### Phase 2: Simple Forms (Start Here)
Convert simple forms first to get familiar:
1. ProductCategoryForm
2. DebtorCategoryForm
3. PricelistForm

### Phase 3: Complex Forms
Convert larger forms:
1. CustomerRegistration
2. ProductForm
3. Any multi-section forms

### Phase 4: List Pages
Update list/table pages:
1. CustomerList
2. ProductList
3. Use `.table-custom` class

---

## Testing Checklist

After converting a form:
- [ ] All colors match theme (no blue Bootstrap colors)
- [ ] Fonts are flowing/smokey (Satisfy, Shadows Into Light, Permanent Marker)
- [ ] Buttons are hard-cornered with gradients
- [ ] Form validation messages styled correctly
- [ ] Submit button shows loading state properly
- [ ] Form is responsive (test different screen sizes)
- [ ] Icons display correctly (if using)
- [ ] Dark theme works (background is dark)

---

## Tips & Best Practices

1. **Start with BaseForm for new forms** - It's easier and more consistent
2. **Use form-section for grouping** - Replaces Bootstrap cards
3. **Add placeholders to inputs** - They help with the smokey aesthetic
4. **Don't skip validation styling** - Use `.validation-message` class
5. **Test on dark backgrounds** - The theme is designed for dark mode
6. **Keep button text short** - Works better with the Permanent Marker font
7. **Use icons consistently** - Bud = save, Flower = cancel/delete, Seed = add/loading
8. **Gradual migration is OK** - You can mix themed and Bootstrap pages during transition

---

## Common Issues

### Issue: Icons not showing
**Solution:** Check that SVG files are in `wwwroot/images/` and paths are correct

### Issue: Fonts not loading
**Solution:** Ensure form-theme.css is loaded in App.razor and internet connection available (Google Fonts)

### Issue: Colors still Bootstrap blue
**Solution:** Make sure you're using the custom classes (`.btn-primary-custom` not `.btn-primary`)

### Issue: Form too wide on large screens
**Solution:** `.base-form-container` has max-width: 800px. Adjust in CSS if needed.

### Issue: Text hard to read
**Solution:** Theme is designed for dark backgrounds. Ensure parent has dark background.

---

## Example: Full Simple Form

See `Components/Pages/Examples/ThemedFormExample.razor` for a complete working example!

---

## Need Help?

Check these files:
- **CSS Reference**: `wwwroot/css/form-theme.css`
- **Component Source**: `Components/Shared/BaseForm.razor`
- **Working Example**: `Components/Pages/Examples/ThemedFormExample.razor`
- **Original Guide**: `docs/blazor_form_guide.md`
