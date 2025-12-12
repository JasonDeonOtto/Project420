# Project420 — Auxiliary Files Bundle

This document contains all auxiliary files recommended for enterprise-grade development across Project420.

---

## 1. `.editorconfig`
```ini
root = true

# C# files
[*.cs]
indent_style = space
indent_size = 4
charset = utf-8-bom
end_of_line = crlf
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = true

# Require explicit types
csharp_style_var_for_built_in_types = false
csharp_style_var_when_type_is_apparent = false
csharp_style_var_elsewhere = false

# Naming rules
dotnet_naming_rule.private_fields.should_have_underscore_prefix.severity = warning
```

---

## 2. `Directory.Build.props`
```xml
<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Update="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Update="FluentValidation" Version="Latest" />
    <PackageReference Update="AutoMapper" Version="Latest" />
  </ItemGroup>
</Project>
```

---

## 3. `global.json`
```json
{
  "sdk": {
    "version": "9.0.100",
    "rollForward": "latestMajor"
  }
}
```

---

## 4. GitHub Templates
### 4.1 Pull Request Template — `.github/pull_request_template.md`
```md
# Pull Request Summary
Describe the purpose and scope of this PR.

## Changes
-

## Tests
- [ ] Unit tests added/updated
- [ ] Integration tests run

## Database
- [ ] Migration included (if needed)
- [ ] SQL reviewed

## Documentation
- [ ] Updated related docs

## Checklist
- [ ] No secrets committed
- [ ] Code follows standards
```

### 4.2 Issue Template — `.github/ISSUE_TEMPLATE/bug_report.md`
```md
---
title: "Bug Report"
labels: bug
---

## Description

## Steps to Reproduce
1.
2.
3.

## Expected Behavior

## Actual Behavior

## Screenshots / Logs
```

### 4.3 Feature Request Template — `.github/ISSUE_TEMPLATE/feature_request.md`
```md
---
title: "Feature Request"
labels: enhancement
---

## Summary

## Why It's Needed

## Proposed Solution

## Dependencies / Considerations
```

---

## 5. GitHub Actions CI Pipeline — `.github/workflows/ci.yml`
```yaml
name: Project420 CI

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.0.x

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Test
        run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage"

      - name: Coverage Report
        uses: danielpalme/ReportGenerator-GitHub-Action@v5
        with:
          reports: "**/coverage.cobertura.xml"
          targetdir: "coverage-report"
```

---

## 6. Standard `appsettings.json` Template
```json
{
  "ConnectionStrings": {
    "Default": "Server=.\\SQLDEVED;Database=Project420;Trusted_Connection=True;Encrypt=True;"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  "Jwt": {
    "Authority": "https://identity.provider/",
    "Audience": "project420-api",
    "RequireHttps": true
  },

  "Audit": {
    "RetentionYears": 7
  }
}
```

---

## 7. Base Blazor UI Theme (CSS Extract)
```css
:root {
  --p420-primary: #0a7f2e;
  --p420-secondary: #1f1f1f;
  --p420-accent: #e0ffe9;
  --p420-border: #dcdcdc;
}

body {
  font-family: "Inter", sans-serif;
  background: #fafafa;
  color: #222;
}

.card {
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid var(--p420-border);
  background: white;
}
```

---

## 8. Base Service Template
```csharp
public abstract class ServiceBase
{
    protected readonly ILogger Logger;

    protected ServiceBase(ILogger logger)
    {
        Logger = logger;
    }
}
```

---

## 9. Standard README Template
```md
# Project420 Module — {ModuleName}

## Overview
Describe module purpose.

## Architecture
- Layered structure (UI/BLL/DAL)
- Dependencies

## Running
```sh
dotnet run --project src/Project420.{ModuleName}.UI.Blazor
```

## Testing
```sh
dotnet test
```
```

---

## 10. Suggested Folder Structure for Docs
```
docs/
├── standards.md
├── auxiliary-files.md
├── api/
├── ui/
├── db/
└── security/
```

---

If you want, I can automatically generate:
- A **full Blazor base component library** (buttons, cards, tables, forms)
- A **full CI/CD pipeline including CD** (staging + production)
- A **scripted DB migration runbook**
- A **developer onboarding manual**

