# Blazor Base Form Template Guide

## Overview
This guide provides a standardized base form template for your Blazor project with a custom cannabis green, faded Ferrari red, and dead rose purple theme. The design features flowing, smokey typography and smooth, shaded hard-cornered buttons.

## Color Palette

```css
--cannabis-green: #4a7c59
--cannabis-green-light: #6b9d7a
--cannabis-green-dark: #2d5a3d
--ferrari-red-faded: #c75450
--ferrari-red-hover: #d16a66
--dead-rose-purple: #8b5e83
--dead-rose-purple-light: #a57a9d
--background-dark: #1a1a1a
--background-card: #2d2d2d
--text-primary: #e8e8e8
--text-secondary: #b8b8b8
```

## CSS Styling (wwwroot/css/form-theme.css)

```css
@import url('https://fonts.googleapis.com/css2?family=Shadows+Into+Light&family=Permanent+Marker&family=Satisfy&display=swap');

:root {
    --cannabis-green: #4a7c59;
    --cannabis-green-light: #6b9d7a;
    --cannabis-green-dark: #2d5a3d;
    --ferrari-red-faded: #c75450;
    --ferrari-red-hover: #d16a66;
    --dead-rose-purple: #8b5e83;
    --dead-rose-purple-light: #a57a9d;
    --background-dark: #1a1a1a;
    --background-card: #2d2d2d;
    --text-primary: #e8e8e8;
    --text-secondary: #b8b8b8;
}

/* Flowing, smokey font styling */
.form-title {
    font-family: 'Satisfy', cursive;
    font-size: 2.5rem;
    color: var(--cannabis-green-light);
    text-shadow: 2px 2px 8px rgba(74, 124, 89, 0.6);
    margin-bottom: 1.5rem;
}

.form-label {
    font-family: 'Shadows Into Light', cursive;
    font-size: 1.1rem;
    color: var(--text-primary);
    margin-bottom: 0.5rem;
    display: block;
    letter-spacing: 0.5px;
}

/* Base form container */
.base-form-container {
    background: linear-gradient(135deg, var(--background-card) 0%, #252525 100%);
    border-radius: 0;
    padding: 2.5rem;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
    max-width: 600px;
    margin: 2rem auto;
    border: 1px solid var(--cannabis-green-dark);
}

/* Input fields */
.form-input {
    width: 100%;
    padding: 0.75rem 1rem;
    background-color: #1f1f1f;
    border: 2px solid var(--dead-rose-purple);
    border-radius: 0;
    color: var(--text-primary);
    font-family: 'Shadows Into Light', cursive;
    font-size: 1rem;
    transition: all 0.3s ease;
    margin-bottom: 1.25rem;
}

.form-input:focus {
    outline: none;
    border-color: var(--cannabis-green);
    box-shadow: 0 0 0 3px rgba(74, 124, 89, 0.2);
}

/* Hard-cornered buttons with smooth shading */
.btn-primary-custom {
    background: linear-gradient(180deg, var(--cannabis-green-light) 0%, var(--cannabis-green-dark) 100%);
    border: none;
    border-radius: 0;
    padding: 0.875rem 2rem;
    color: var(--text-primary);
    font-family: 'Permanent Marker', cursive;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 
        0 4px 6px rgba(0, 0, 0, 0.3),
        inset 0 1px 0 rgba(255, 255, 255, 0.1),
        inset 0 -1px 0 rgba(0, 0, 0, 0.3);
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.4);
    letter-spacing: 1px;
}

.btn-primary-custom:hover {
    background: linear-gradient(180deg, var(--cannabis-green) 0%, var(--cannabis-green-dark) 100%);
    box-shadow: 
        0 6px 10px rgba(0, 0, 0, 0.4),
        inset 0 1px 0 rgba(255, 255, 255, 0.15),
        inset 0 -1px 0 rgba(0, 0, 0, 0.4);
    transform: translateY(-2px);
}

.btn-primary-custom:active {
    transform: translateY(0);
    box-shadow: 
        0 2px 4px rgba(0, 0, 0, 0.3),
        inset 0 1px 0 rgba(0, 0, 0, 0.2);
}

.btn-danger-custom {
    background: linear-gradient(180deg, var(--ferrari-red-hover) 0%, var(--ferrari-red-faded) 100%);
    border: none;
    border-radius: 0;
    padding: 0.875rem 2rem;
    color: var(--text-primary);
    font-family: 'Permanent Marker', cursive;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 
        0 4px 6px rgba(0, 0, 0, 0.3),
        inset 0 1px 0 rgba(255, 255, 255, 0.1),
        inset 0 -1px 0 rgba(0, 0, 0, 0.3);
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.4);
    letter-spacing: 1px;
}

.btn-danger-custom:hover {
    background: linear-gradient(180deg, var(--ferrari-red-faded) 0%, #a84440 100%);
    box-shadow: 
        0 6px 10px rgba(0, 0, 0, 0.4),
        inset 0 1px 0 rgba(255, 255, 255, 0.15),
        inset 0 -1px 0 rgba(0, 0, 0, 0.4);
    transform: translateY(-2px);
}

/* Button container */
.form-buttons {
    display: flex;
    gap: 1rem;
    margin-top: 2rem;
    justify-content: flex-end;
}

/* Validation messages */
.validation-message {
    color: var(--ferrari-red-faded);
    font-family: 'Shadows Into Light', cursive;
    font-size: 0.875rem;
    margin-top: -1rem;
    margin-bottom: 0.5rem;
}
```

## Base Form Component (Components/BaseForm.razor)

```razor
@typeparam TModel

<div class="base-form-container">
    <h2 class="form-title">@Title</h2>
    
    <EditForm Model="@Model" OnValidSubmit="@HandleValidSubmit">
        <DataAnnotationsValidator />
        
        @ChildContent
        
        <div class="form-buttons">
            <button type="button" class="btn-danger-custom" @onclick="OnCancel">
                Cancel
            </button>
            <button type="submit" class="btn-primary-custom">
                @SubmitText
            </button>
        </div>
    </EditForm>
</div>

@code {
    [Parameter]
    public TModel? Model { get; set; }
    
    [Parameter]
    public string Title { get; set; } = "Form";
    
    [Parameter]
    public string SubmitText { get; set; } = "Submit";
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public EventCallback<TModel> OnValidSubmit { get; set; }
    
    [Parameter]
    public EventCallback OnCancel { get; set; }
    
    private async Task HandleValidSubmit()
    {
        if (Model != null)
        {
            await OnValidSubmit.InvokeAsync(Model);
        }
    }
}
```

## Example Usage (Pages/ExampleForm.razor)

```razor
@page "/example-form"
@using System.ComponentModel.DataAnnotations

<BaseForm TModel="UserModel" 
          Model="@userModel" 
          Title="User Registration"
          SubmitText="Register"
          OnValidSubmit="@HandleSubmit"
          OnCancel="@HandleCancel">
    
    <div>
        <label class="form-label">Username</label>
        <InputText class="form-input" @bind-Value="userModel.Username" />
        <ValidationMessage For="@(() => userModel.Username)" class="validation-message" />
    </div>
    
    <div>
        <label class="form-label">Email</label>
        <InputText class="form-input" @bind-Value="userModel.Email" type="email" />
        <ValidationMessage For="@(() => userModel.Email)" class="validation-message" />
    </div>
    
    <div>
        <label class="form-label">Password</label>
        <InputText class="form-input" @bind-Value="userModel.Password" type="password" />
        <ValidationMessage For="@(() => userModel.Password)" class="validation-message" />
    </div>

</BaseForm>

@code {
    private UserModel userModel = new();
    
    private async Task HandleSubmit(UserModel model)
    {
        // Handle form submission
        Console.WriteLine($"Submitted: {model.Username}");
    }
    
    private void HandleCancel()
    {
        // Handle cancel action
        Console.WriteLine("Form cancelled");
    }
    
    public class UserModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
```

## Integration Steps

1. Add the CSS file reference to your `wwwroot/index.html` (Blazor WebAssembly) or `Pages/_Host.cshtml` (Blazor Server):
```html
<link href="css/form-theme.css" rel="stylesheet" />
```

2. Create the `BaseForm.razor` component in your `Components` folder

3. Use the base form template throughout your application by passing your model and content

## Customization Tips

- Adjust color variables in the CSS for different variations
- Modify font families for different smokey/flowing effects
- Change button gradients and shadows for different visual styles
- Add additional input types (select, textarea, checkbox) following the same styling pattern

## Cannabis-Themed Icons and Images

### Icon Integration Strategy

Use cannabis-related imagery for buttons and interface elements such as flowers, buds, seeds, and leaves.

### CSS for Icon Buttons

```css
/* Icon button base styles */
.btn-icon {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
}

.btn-icon img {
    width: 24px;
    height: 24px;
    filter: brightness(0) invert(1);
    transition: filter 0.3s ease;
}

.btn-icon:hover img {
    filter: brightness(0) invert(1) drop-shadow(0 0 4px var(--cannabis-green-light));
}

/* Decorative elements */
.form-decoration {
    position: absolute;
    opacity: 0.1;
    pointer-events: none;
}

.form-decoration.top-left {
    top: -20px;
    left: -20px;
    width: 100px;
    height: 100px;
}

.form-decoration.bottom-right {
    bottom: -20px;
    right: -20px;
    width: 120px;
    height: 120px;
    transform: rotate(180deg);
}
```

### SVG Icons (Create in wwwroot/images/)

**cannabis-leaf.svg** - For primary actions, success states:
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
    <path d="M12 2c-.5 0-.9.2-1.2.5L9 5.3c-.3.4-.8.7-1.3.7H5c-.6 0-1 .4-1 1v2.7c0 .5-.3 1-.7 1.3L.5 12.8c-.3.3-.5.7-.5 1.2s.2.9.5 1.2l2.8 1.8c.4.3.7.8.7 1.3V21c0 .6.4 1 1 1h2.7c.5 0 1 .3 1.3.7l1.8 2.8c.3.3.7.5 1.2.5s.9-.2 1.2-.5l1.8-2.8c.3-.4.8-.7 1.3-.7H19c.6 0 1-.4 1-1v-2.7c0-.5.3-1 .7-1.3l2.8-1.8c.3-.3.5-.7.5-1.2s-.2-.9-.5-1.2l-2.8-1.8c-.4-.3-.7-.8-.7-1.3V7c0-.6-.4-1-1-1h-2.7c-.5 0-1-.3-1.3-.7l-1.8-2.8c-.3-.3-.7-.5-1.2-.5z"/>
</svg>
```

**cannabis-bud.svg** - For submit, save actions:
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
    <ellipse cx="12" cy="14" rx="5" ry="7" opacity="0.9"/>
    <ellipse cx="10" cy="12" rx="3" ry="4" opacity="0.8"/>
    <ellipse cx="14" cy="12" rx="3" ry="4" opacity="0.8"/>
    <ellipse cx="12" cy="10" rx="2.5" ry="3" opacity="0.7"/>
    <path d="M11 21v3h2v-3" stroke="currentColor" stroke-width="1.5" fill="none"/>
</svg>
```

**cannabis-seed.svg** - For add, create actions:
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
    <ellipse cx="12" cy="12" rx="4" ry="6"/>
    <path d="M12 6v12M8 9l8 6M8 15l8-6" stroke="currentColor" stroke-width="0.5" opacity="0.5"/>
</svg>
```

**cannabis-flower.svg** - For delete, remove actions:
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
    <circle cx="12" cy="12" r="3"/>
    <circle cx="12" cy="7" r="2" opacity="0.8"/>
    <circle cx="17" cy="9" r="2" opacity="0.8"/>
    <circle cx="19" cy="14" r="2" opacity="0.8"/>
    <circle cx="16" cy="18" r="2" opacity="0.8"/>
    <circle cx="12" cy="19" r="2" opacity="0.8"/>
    <circle cx="8" cy="18" r="2" opacity="0.8"/>
    <circle cx="5" cy="14" r="2" opacity="0.8"/>
    <circle cx="7" cy="9" r="2" opacity="0.8"/>
</svg>
```

### Updated Button Components with Icons

```razor
<!-- Primary button with bud icon -->
<button type="submit" class="btn-primary-custom btn-icon">
    <img src="images/cannabis-bud.svg" alt="" />
    <span>@SubmitText</span>
</button>

<!-- Cancel button with flower icon -->
<button type="button" class="btn-danger-custom btn-icon" @onclick="OnCancel">
    <img src="images/cannabis-flower.svg" alt="" />
    <span>Cancel</span>
</button>

<!-- Add button with seed icon -->
<button type="button" class="btn-primary-custom btn-icon">
    <img src="images/cannabis-seed.svg" alt="" />
    <span>Add New</span>
</button>
```

### Form Container with Decorative Elements

```razor
<div class="base-form-container" style="position: relative;">
    <!-- Decorative cannabis leaf watermarks -->
    <img src="images/cannabis-leaf.svg" class="form-decoration top-left" alt="" />
    <img src="images/cannabis-leaf.svg" class="form-decoration bottom-right" alt="" />
    
    <h2 class="form-title">@Title</h2>
    
    <EditForm Model="@Model" OnValidSubmit="@HandleValidSubmit">
        <!-- Form content -->
    </EditForm>
</div>
```

### Icon Usage Guidelines

1. **Primary Actions** (Submit, Save, Confirm): Use bud icons - represents growth and completion
2. **Secondary Actions** (Add, Create, New): Use seed icons - represents new beginnings
3. **Success States**: Use leaf icons - represents health and success
4. **Destructive Actions** (Delete, Remove, Cancel): Use flower icons - represents end of cycle
5. **Decorative Elements**: Faded leaf/bud patterns as background watermarks

### Loading State Icon

For loading spinners, use a rotating seed animation:

```css
@keyframes spin-seed {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}

.loading-icon {
    animation: spin-seed 2s linear infinite;
    width: 32px;
    height: 32px;
}
```

## Additional Font Options

For more flowing/smokey fonts, consider:
- `Amatic SC` - Hand-drawn, flowing style
- `Dancing Script` - Elegant, cursive flow
- `Pacifico` - Surf-style smoothness
- `Indie Flower` - Casual, handwritten flow

Simply replace font imports in the CSS file to experiment with different styles.