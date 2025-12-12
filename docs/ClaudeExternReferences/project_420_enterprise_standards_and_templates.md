# Project420 — Enterprise Standards, Templates & Best Practices

> Comprehensive standards, templates and checklists for an enterprise-grade Project420 implementation.

---

## Table of contents
1. Introduction
2. Architecture & Module Guidelines
3. Coding Standards
4. Project Layout & Naming Conventions
5. Data & Database Standards
6. Security & Compliance
7. API / Integration Standards
8. Testing Strategy & Templates
9. CI/CD, Release & Deployment
10. Observability, Monitoring & SLOs
11. Operational Procedures & Runbooks
12. Templates (Code, PR, Issue, Release Notes)
13. Checklists
14. Appendix: Useful snippets

---

## 1. Introduction
This document defines uniform, auditable, and traceable standards for Project420 — from solution layout and code style to security, testing, and deployment. Use this as the canonical handbook for all teams working on PoC → Prototype → Production builds.

**Scope:** .NET 9 backend, EF Core 9, Blazor UI, MAUI mobile (planned), SQL Server primary DB (Postgres/SQLite supported), xUnit tests, CI/CD pipelines.

**Goals:**
- Consistency across modules
- Compliance (POPIA, SAHPRA, SARS where relevant)
- Secure by default
- Testable and observable
- Scalable and maintainable

---

## 2. Architecture & Module Guidelines
### 2.1 3-Tier Modular Architecture (enforced)
- Presentation: `*.UI.Blazor`, `*.UI.Maui` — minimal logic; only input validation + presentation mapping
- BLL: `*.BLL` — business rules, orchestration, application services, DTOs
- DAL: `*.DAL` — EF Core DbContext(s), repository implementations, migrations
- Shared: `Project420.Shared.*` — cross-cutting concerns (logging, exceptions, utilities, database contracts)

### 2.2 Module boundaries
- Each module owns its domain aggregate roots and persistence tables.
- Inter-module communication: prefer internal module service interfaces and domain events (mediator pattern). When crossing process boundaries use versioned APIs or message bus (RabbitMQ/Kafka) depending on scale.

### 2.3 DDD & Entities
- Define aggregates and boundaries in `*.Models` project.
- Keep entities small — prefer value objects for typed fields (e.g., `Money`, `Weight`, `BatchNumber`).
- All entities derive from `AuditableEntity` (see Appendix).

---

## 3. Coding Standards
### 3.1 General
- Follow Microsoft C# coding conventions with small team exceptions captured in `.editorconfig`.
- Use nullable reference types enabled project-wide.

### 3.2 SOLID & Clean Code
- Single Responsibility per class.
- Prefer composition over inheritance.
- Keep method length < 80 lines where possible.

### 3.3 Formatting & Tooling
- `.editorconfig` enforced on build. Run `dotnet format` in CI.
- Use Roslyn analyzers + StyleCop ruleset. Treat warnings as build warnings; critical issues can be escalated to errors via CI gating.

### 3.4 Exception handling
- Centralized exception handling in Presentation (Blazor) and API middleware.
- DO NOT swallow exceptions; log then map to user-friendly errors.

---

## 4. Project Layout & Naming Conventions
### 4.1 Repository structure
```
src/
├── Shared/
│   ├── Project420.Shared.Core
│   ├── Project420.Shared.Infrastructure
│   └── Project420.Shared.Database
└── Modules/
    ├── Management/
    ├── Retail/
    ├── Cultivation/
    ├── Production/
    └── Inventory/
```

### 4.2 Assembly & Namespace naming
- `Company.Project420.Module.Layer` e.g. `Project420.Inventory.DAL`
- File names match types. One public type per file.

### 4.3 Branching & Git
- Trunk-based or GitFlow? Recommended: **Trunk-based** with short-lived feature branches.
- Branch naming: `feature/<ticket>-short-desc`, `hotfix/<issue>`, `release/<version>`
- Commit messages: Conventional Commits. Example: `feat(inventory): add stock transfer endpoint`.

---

## 5. Data & Database Standards
### 5.1 Schema & Migrations
- One EF Core migration set per bounded context; store migrations in `*.DAL.Migrations`.
- Migrations are the canonical schema changes — never run manual DDL outside migrations in production without an audited change and migration script.
- Migration PRs must include generated SQL review and DB migration plan for production (backups, downtime windows if necessary).

### 5.2 Naming conventions
- Tables: `schema.EntityName` (e.g. `inventory.StockMovements`)
- Columns: `CamelCase` or `snake_case` per DB team preference — choose one and enforce via migration scaffolding. (We recommend `PascalCase` to match EF conventions: `CreatedAt`).
- Primary Keys: `Id` (GUID or bigint — choose per table). Prefer sequential GUIDs for distributed writes or `BIGINT IDENTITY` where ordering is important.

### 5.3 Auditing & Soft Deletes
- Base auditable fields: `Id`, `CreatedBy`, `CreatedAt`, `UpdatedBy`, `UpdatedAt`, `IsDeleted`, `DeletedAt`, `DeletedBy`, `RowVersion` (optimistic concurrency)
- Enforce soft deletes via global query filters in DbContext.

### 5.4 Backups & Retention
- Nightly full backups, hourly differential backups for active DBs.
- Transaction log backups every 15 minutes for high-churn DBs.
- Retention policy: match audit retention (7 years) for compliance data — implement archive DB with reduced pace queries.

### 5.5 Data migrations & archiving
- Use deterministic, idempotent scripts for data migrations.
- Archival strategy: move older data to `Project420_Archive` DB partitioned by year or quarter. Archive process must be reversible and logged.

---

## 6. Security & Compliance
### 6.1 Authentication & Authorization
- Authentication: OAuth 2.0 with JWTs for API and cookie auth for Blazor server-side as needed. Use IdentityServer or Azure AD depending on environment.
- Authorization: RBAC with claims-based checks in BLL. Map roles to least privilege.

### 6.2 Secrets & Keys
- Do NOT store secrets in repo. Use Azure Key Vault / HashiCorp Vault / AWS Secrets Manager.
- Key rotation policy: rotate symmetric keys every 90 days; asymmetric keys/plumbing on schedule defined by security team.

### 6.3 Encryption
- In transit: TLS 1.3 enforced
- At rest: AES-256 via DB encryption or volume encryption
- Field-level encryption for PII (e.g., ID numbers, personal data) where required by POPIA. Use application-layer encryption and protect keys in vault.

### 6.4 Audit Logging & Retention
- Centralized audit service capturing: who, what, when, where, old/new values, reason.
- Retention: 7 years for regulated records (POPIA/SAHPRA/SARS). Ensure storage cost planning.

### 6.5 Compliance
- POPIA controls: data subject rights, consent tracking, purpose limitation, minimization.
- SAHPRA: record retention for cultivation and production events; ensure traceability for seed-to-sale.
- SARS: inventory & tax records retention and traceable movement logs.

### 6.6 Penetration Testing & Vulnerability Scanning
- Annual pen tests on production and after major releases.
- Automated SCA (Software Composition Analysis) scanning in CI, SAST tools, and container image scanning.

---

## 7. API / Integration Standards
### 7.1 API Design
- Use REST with versioning (e.g., `/api/v1/`), or gRPC for internal high-throughput services.
- OpenAPI (Swagger) for all HTTP APIs. Generate client SDKs where helpful.
- Use consistent error envelope:
```json
{ "error": { "code": "InvalidInput", "message": "Validation failed", "details": [...] }}
```

### 7.2 Contracts & DTOs
- DTOs live in `*.Shared` if consumed by multiple modules. Keep DTOs versioned — avoid breaking changes.
- Use AutoMapper profiles per module and map explicitly — avoid `MapAll()` automated mass mapping for critical types.

### 7.3 Integration patterns
- Synchronous: REST/gRPC for request-reply.
- Asynchronous: use message broker for events (e.g., `InventoryAdjusted`, `PlantHarvested`) with at-least-once delivery and idempotency keys.

---

## 8. Testing Strategy & Templates
### 8.1 Test Pyramid
- Unit tests: majority — xUnit + Moq + FluentAssertions
- Integration tests: EF Core in-memory/SQL on CI for contract-level tests
- End-to-end: UI/Blazor automated acceptance tests (Playwright or Selenium)
- Load tests: k6 or JMeter for major flows (POS checkout, batch processing)

### 8.2 Test Data
- Use deterministic fixtures and `Testcontainers` for ephemeral DBs in CI when full DB features are needed.
- Keep test data small, readable, and seeded per test suite.

### 8.3 Coverage & Quality Gates
- Use Coverlet + ReportGenerator to produce coverage reports.
- Set minimum coverage threshold for critical modules (e.g., 70% coverage for BLL).

### 8.4 Test Templates
- `ServiceTestBase` and `RepositoryTestBase` in `Project420.Shared.Tests`.
- Example test arrangement in Appendix.

---

## 9. CI/CD, Release & Deployment
### 9.1 CI Guidelines
- Use GitHub Actions / Azure DevOps / GitLab CI.
- Pipeline stages: `lint` -> `build` -> `unit tests` -> `integration tests` -> `security scans` -> `publish artifacts`.
- PRs: Require passing pipeline, two approvers, and linked ticket.

### 9.2 CD & Environment Promotion
- Environments: `dev`, `qa`, `staging`, `production`.
- Deploy artifacts are immutable (build once, deploy many).
- Blue/Green or Canary deployments for production.

### 9.3 Database Deployment
- Deploy migrations as part of deployment pipeline with pre-checks and dry-run SQL generation.
- Use feature toggles for behaviour changes that require DB transitions.

### 9.4 Release Management
- Semantic versioning `MAJOR.MINOR.PATCH`.
- Release notes must list DB changes, configuration changes, migration scripts and rollback steps.

---

## 10. Observability, Monitoring & SLOs
### 10.1 Logging
- Structured logging (Serilog recommended). Log correlation IDs included in all logs.
- Sensitive data redaction policy — PII never logged in plaintext.

### 10.2 Tracing & Metrics
- Distributed tracing (OpenTelemetry) instrumenting BLL and DAL. Export to backend (Jaeger/Azure Monitor/New Relic)
- Metrics: request rates, error rates, latency percentiles, business metrics (transactions/day)

### 10.3 Alerts & SLOs
- Define SLOs for latency and availability per service.
- Alerting thresholds: for example, 95th percentile latency > X ms for 5 minutes.

---

## 11. Operational Procedures & Runbooks
- Incident response runbook (who to call, severity matrix, rollback steps).
- On-call rotation and escalation.
- Backup restore test schedule (quarterly) with documented RTO/RPO.

---

## 12. Templates (paste into PRs / repos)
### 12.1 .editorconfig (excerpt)
```
root = true
[*.cs]
indent_style = space
indent_size = 4
dotnet_sort_system_directives_first = true
```

### 12.2 PR Template
```
## Summary
- What changed
- Why

## Related tickets
-

## Testing
- Unit tests added/updated
- Manual test steps

## DB Migration
- Migration file: Project420.X.DAL/Migrations/YYYY_AddX.cs
- SQL review notes

## Rollback Plan
-
```

### 12.3 Issue / Bug Template
```
## Steps to reproduce
1.
2.

## Expected

## Actual

## Logs / Stack

## Attachments
```

### 12.4 Release Notes Template
```
# Release v{version}
## Highlights
-
## DB Changes
- Migration: ...
## Breaking Changes
-
## Rollout Plan
-
```

---

## 13. Checklists
### 13.1 PR Checklist
- [ ] Code follows `.editorconfig`
- [ ] Tests added and passing
- [ ] No hard-coded secrets
- [ ] Performance considerations noted
- [ ] Security review (SCA results)
- [ ] DB migration reviewed
- [ ] Update docs/changelog

### 13.2 Release Checklist
- [ ] All tests & scans green
- [ ] Backup taken and verified
- [ ] DB migrations dry-run successful
- [ ] Deployment window booked (if needed)
- [ ] Stakeholders notified

---

## 14. Appendix: Useful snippets
### 14.1 AuditableEntity (C#)
```csharp
public abstract class AuditableEntity
{
    public Guid Id { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

### 14.2 Repository interface (pattern)
```csharp
public interface IRepository<T> where T : AuditableEntity
{
    Task<T?> GetAsync(Guid id);
    Task<IEnumerable<T>> ListAsync(ISpecification<T>? spec = null);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

### 14.3 Example xUnit test skeleton
```csharp
public class ProductServiceTests : ServiceTestBase
{
    [Fact]
    public async Task CreateProduct_Should_Save()
    {
        // arrange
        var svc = CreateService<ProductService>();

        // act
        var result = await svc.CreateAsync(new CreateProductDto { ... });

        // assert
        result.Should().NotBeNull();
    }
}
```

### 14.4 EF Core Migration best practice
- Generate migration: `dotnet ef migrations add <Name> -s src/Project420.Api -p src/Project420.Module.DAL`
- Review generated SQL: `dotnet ef migrations script` and attach to PR.

---

## Final notes & governance
- Keep this document versioned inside the repo (e.g. `docs/standards.md`).
- Review cadence: update and review quarterly or after major platform changes (e.g., .NET update, compliance change).
- Appoint a standards owner per module to govern infra, security and compliance changes.

---

If you want, I can also scaffold:
- `.editorconfig`, `Directory.Build.props`, and `global.json`
- PR/checklist GitHub templates
- Example CI pipeline (GitHub Actions) and deployment YAML
- Example Blazor base theme and shared component library

