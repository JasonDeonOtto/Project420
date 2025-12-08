# Claude Prompt: Professional Blazor Theme + Base Form Component (Cannabis-Green Theme)

**ROLE:** You are a senior Blazor architect and UX/UI engineer.  
**GOAL:** Generate production-ready, accessible, modern Blazor component code that implements a **global theme** + a **BaseFormComponent** for a Blazor Server or WebAssembly project using **Entity Framework Core**.

---

## 🎨 THEME REQUIREMENTS

### Primary Colour Palette
- **Primary Green (Cannabis Theme):** `#1FAF5B`
- **Dark Green Variant:** `#128644`
- **Light Green Variant:** `#D5F5E3`
- **Neutral Grey:** `#F3F3F3`
- **Accent Charcoal:** `#1E1E1E`
- **White:** `#FFFFFF`

> Do NOT use red/pink/purple tones unless required for error messages.

### Global Style Rules
- Page backgrounds: **very light grey (`#F3F3F3`)**
- DataGrid / Tables: **White background with black text**
- Buttons: **Squared edges (~4px radius), modern Windows‑11 style**
- Font: **Inter, Segoe UI, or Roboto**
- Always maintain WCAG AA contrast
- Spacing: **8px scale (4, 8, 12, 16, 24)**
- No text run-over
- Scrollbars: **green accented**
- Navbar: **primary green with white text**

### Dark Mode
Do NOT implement dark mode.

---

## 📦 OUTPUT EXPECTATIONS

### 1. `theme.css`
Global stylesheet defining:
- Colours
- Fonts
- Buttons
- Spacing system
- Scrollbars
- Navbar theme
- Resets unwanted default Blazor styling

### 2. Base Form Component
Files:
- `Components/Base/BaseFormComponent.razor`
- `Components/Base/BaseFormComponent.razor.cs`

Features:
- `<EditForm>` wrapper
- Title bar
- Automatic spacing
- Toolbar (Save / Cancel)
- Themed buttons
- Auto-grid alignment
- Works with cascading theme parameters

### 3. Example Usage
`Pages/Products/ProductEdit.razor`  
Demonstrating how to use the BaseFormComponent.

### 4. Integration Instructions
- Add CSS to `index.html` or `_Host.cshtml`
- Register component namespace globally
- Apply new navbar theme to layout file

---

## 🧪 STYLE GUIDANCE
- Clean indentation and formatting
- Predictable design system
- Avoid neon/flashy effects
- Avoid bulky margins
- Avoid pill/rounded buttons

---

## 🚫 AVOID
- Rounded or pill-shaped buttons
- Neon colours
- Purple/pink/red tones except errors
- Overly complex CSS

---

## 🎯 FINAL DELIVERABLE
Provide all code fully generated and ready to paste into a Blazor project.
