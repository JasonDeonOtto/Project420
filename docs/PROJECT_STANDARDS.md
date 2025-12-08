# Project420 - Development Standards

**Last Updated**: 2025-12-05
**Status**: MANDATORY
**Applies To**: All Blazor UI development in Project420

---

## Cannabis-Themed UI Standards

### MANDATORY: BaseForm Component

**ALL form pages in this project MUST use the BaseForm component with the cannabis-themed styling.**

#### Why This Is Mandatory

1. **Consistent User Experience** - All forms have the same look, feel, and behavior
2. **Cannabis Branding** - Reinforces the cannabis theme throughout the application
3. **Maintainability** - Single source of truth for form styling and behavior
4. **Reduced Code** - Less boilerplate, faster development

#### The Theme

- **Cannabis Green** (#4a7c59) - Primary actions, success states
- **Faded Ferrari Red** (#c75450) - Danger actions, errors
- **Dead Rose Purple** (#8b5e83) - Secondary actions, accents
- **Dark Background** (#1a1a1a / #2d2d2d) - Main backgrounds
- **Flowing Fonts** - Satisfy (titles), Shadows Into Light (labels), Permanent Marker (buttons)
- **Hard-Cornered Buttons** - No border radius, gradient shading
- **Cannabis Icons** - Leaf, bud, seed, and flower SVG icons

---

## Form Development Rules

### Rule 1: ALWAYS Use BaseForm

**DO THIS:**
```razor
<BaseForm TModel="YourModel"
          Model="@model"
          Title="Your Page Title"
          Subtitle="Optional subtitle"
          SubmitText="Save"
          OnValidSubmit="@HandleSubmit"
          OnCancel="@HandleCancel"
          IsSubmitting="@isSubmitting">

    <!-- Your form fields here -->
    <div class="form-section">
        <h3 class="form-section-header">Section Title</h3>

        <div class="form-field">
            <label class="form-label">Field Name</label>
            <InputText class="form-input" @bind-Value="model.Property" />
            <ValidationMessage For="@(() => model.Property)" class="validation-message" />
        </div>
    </div>

</BaseForm>
```

**DON'T DO THIS:**
```razor
<!-- NO! Don't use Bootstrap styling -->
<div class="container">
    <h1>Title</h1>
    <EditForm Model="@model">
        <div class="card">
            <div class="card-body">
                <InputText class="form-control" />
            </div>
        </div>
        <button class="btn btn-primary">Submit</button>
    </EditForm>
</div>
```

### Rule 2: Use Themed CSS Classes

| Element | Class | Usage |
|---------|-------|-------|
| Form Container | `.base-form-container` | Automatic with BaseForm |
| Section | `.form-section` | Group related fields |
| Section Header | `.form-section-header` | Section title |
| Field Wrapper | `.form-field` | Individual field container |
| Label | `.form-label` | Field label |
| Input | `.form-input` | Text inputs |
| Textarea | `.form-textarea` | Multiline text |
| Checkbox | `.form-check-custom` | Checkbox wrapper |
| Validation | `.validation-message` | Error messages |
| Success Alert | `.alert-custom .alert-success-custom` | Success messages |
| Error Alert | `.alert-custom .alert-danger-custom` | Error messages |
| Info Alert | `.alert-custom .alert-info-custom` | Info messages |

### Rule 3: Form Structure Pattern

Every form should follow this structure:

```razor
@* 1. Page directive and usings *@
@page "/your-page"
@using Your.Models

@* 2. Page title *@
<PageTitle>Your Page Title</PageTitle>

@* 3. Alert messages (outside BaseForm) *@
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

@* 4. Loading state (if applicable) *@
@if (isLoading)
{
    <div class="base-form-container" style="text-align: center;">
        <img src="images/cannabis-seed.svg" class="loading-icon" alt="Loading" />
        <p style="color: var(--text-secondary);">Loading...</p>
    </div>
}
else
{
    @* 5. BaseForm with content *@
    <BaseForm TModel="YourModel" Model="@model" ...>
        <!-- Form fields organized into sections -->
    </BaseForm>
}

@* 6. Code block *@
@code {
    // Component code
}
```

### Rule 4: Section Organization

Group related fields into themed sections:

```razor
<!-- Basic section -->
<div class="form-section">
    <h3 class="form-section-header">Section Title</h3>
    <!-- Fields here -->
</div>

<!-- Important/compliance section (red border) -->
<div class="form-section" style="border: 2px solid var(--ferrari-red-faded);">
    <h3 class="form-section-header" style="color: var(--ferrari-red-hover);">
        Important Section
    </h3>
    <!-- Fields here -->
</div>

<!-- Secondary section (purple border) -->
<div class="form-section" style="border: 2px solid var(--dead-rose-purple);">
    <h3 class="form-section-header" style="color: var(--dead-rose-purple-light);">
        Secondary Section
    </h3>
    <!-- Fields here -->
</div>
```

### Rule 5: Grid Layouts

Use CSS Grid for responsive field layouts:

```razor
<!-- Two columns -->
<div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
    <div class="form-field">...</div>
    <div class="form-field">...</div>
</div>

<!-- Three columns -->
<div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 1rem;">
    <div class="form-field">...</div>
    <div class="form-field">...</div>
    <div class="form-field">...</div>
</div>
```

---

## List/Table Pages

### MANDATORY: Themed Table Styling

All list/table pages MUST use `.table-custom` styling:

```razor
<table class="table-custom">
    <thead>
        <tr>
            <th>Column 1</th>
            <th>Column 2</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in items)
        {
            <tr>
                <td>@item.Property1</td>
                <td>@item.Property2</td>
                <td>
                    <button class="btn-primary-custom btn-action btn-icon">
                        <img src="images/cannabis-leaf.svg" alt="" />
                        <span>Edit</span>
                    </button>
                    <button class="btn-danger-custom btn-action btn-icon">
                        <img src="images/cannabis-flower.svg" alt="" />
                        <span>Delete</span>
                    </button>
                </td>
            </tr>
        }
    </tbody>
</table>
```

---

## Button Standards

### Primary Actions (Save, Submit, Confirm)
```razor
<button class="btn-primary-custom btn-icon">
    <img src="images/cannabis-bud.svg" alt="" />
    <span>Save</span>
</button>
```

### Danger Actions (Delete, Cancel, Remove)
```razor
<button class="btn-danger-custom btn-icon">
    <img src="images/cannabis-flower.svg" alt="" />
    <span>Delete</span>
</button>
```

### Secondary Actions (Add, Create)
```razor
<button class="btn-secondary-custom btn-icon">
    <img src="images/cannabis-seed.svg" alt="" />
    <span>Add New</span>
</button>
```

### Loading State
```razor
<button class="btn-primary-custom" disabled>
    <img src="images/cannabis-seed.svg" class="loading-icon" alt="" />
    <span>Processing...</span>
</button>
```

---

## Icon Usage

| Action Type | Icon | Meaning |
|------------|------|---------|
| Primary (Save, Submit) | cannabis-bud.svg | Growth, completion |
| Add/Create | cannabis-seed.svg | New beginning |
| Success | cannabis-leaf.svg | Health, success |
| Delete/Cancel | cannabis-flower.svg | End of cycle |
| Loading | cannabis-seed.svg (spinning) | In progress |

---

## Validation Messages

Always include validation messages below inputs:

```razor
<div class="form-field">
    <label class="form-label">Field Name *</label>
    <InputText class="form-input" @bind-Value="model.Property" />
    <ValidationMessage For="@(() => model.Property)" class="validation-message" />
</div>
```

---

## Code Review Checklist

Before submitting a form page for review, verify:

- [ ] Uses `<BaseForm>` component (mandatory)
- [ ] All inputs use `.form-input` or `.form-textarea` classes
- [ ] All labels use `.form-label` class
- [ ] Fields are wrapped in `.form-field` divs
- [ ] Sections use `.form-section` and `.form-section-header`
- [ ] Buttons use `.btn-primary-custom`, `.btn-danger-custom`, or `.btn-secondary-custom`
- [ ] Icons are included in buttons (`.btn-icon`)
- [ ] Validation messages use `.validation-message` class
- [ ] Success/error alerts use `.alert-custom` classes
- [ ] Loading states show spinning cannabis-seed icon
- [ ] No Bootstrap `.btn`, `.card`, `.form-control` classes used
- [ ] Dark background theme respected (no light backgrounds)
- [ ] Grid layouts used for responsive field arrangement

---

## File Locations

- **CSS**: `wwwroot/css/form-theme.css`
- **BaseForm Component**: `Components/Shared/BaseForm.razor`
- **Icons**: `wwwroot/images/cannabis-*.svg`
- **Examples**: `Components/Pages/Examples/ThemedFormExample.razor`
- **Conversion Guide**: `docs/THEME_CONVERSION_GUIDE.md`

---

## Exceptions

**NONE**. There are no exceptions to these rules. Every form page must follow these standards.

If you believe a specific use case requires an exception, discuss with the team lead before proceeding.

---

## Enforcement

Pull requests that do not follow these standards will be rejected. No exceptions.

All new pages MUST use BaseForm. All existing pages WILL BE converted to use BaseForm.

---

## Questions?

See:
- **Conversion Guide**: `docs/THEME_CONVERSION_GUIDE.md`
- **Example Page**: `Components/Pages/Examples/ThemedFormExample.razor`
- **CSS Reference**: `wwwroot/css/form-theme.css`
- **Original Design**: `docs/blazor_form_guide.md`

---

**COMPLIANCE**: MANDATORY
**EFFECTIVE**: IMMEDIATELY
**NO EXCEPTIONS**
