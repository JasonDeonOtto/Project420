# Enterprise ERP / Stock Management System
## Core Architecture & Foundations (Reference for AI Generation)

---

## 1. System Goals

- Enterprise-grade scalability (SME → Large Enterprise)
- High data integrity and traceability (audit-first design)
- Modular, domain-driven architecture
- Industry-agnostic ERP core with configurable vertical modules
- Cloud-first, hybrid-capable, on-prem compatible
- Designed for long-term evolution (PoC → Prototype → Production)

---

## 2. Architectural Principles

### 2.1 High-Level Architecture

**Layered / Clean Architecture**

- Presentation Layer
  - Web (Blazor)
  - Mobile (MAUI)
  - API Consumers (3rd-party systems)

- Application Layer
  - Use Cases / Application Services
  - Validation & Orchestration
  - DTOs & Mapping

- Domain Layer
  - Aggregates
  - Entities
  - Value Objects
  - Domain Events
  - Business Rules

- Infrastructure Layer
  - Persistence (SQL Server / PostgreSQL)
  - Messaging (Service Bus / RabbitMQ)
  - File Storage
  - External APIs


### 2.2 Key Architectural Patterns

- CQRS (Command Query Responsibility Segregation)
- Event-driven architecture (for stock movements & audits)
- Domain-Driven Design (DDD)
- Repository + Unit of Work
- Specification pattern for filtering
- Outbox pattern (event consistency)

---

## 3. Core Technical Stack (Reference)

- Backend: .NET (LTS)
- ORM: EF Core (with raw SQL for performance paths)
- Database: SQL Server (primary), PostgreSQL optional
- Frontend: Blazor (Server / WASM Hybrid)
- Auth: OAuth2 / OpenID Connect
- Messaging: Azure Service Bus / RabbitMQ
- Cache: Redis
- Search (optional): ElasticSearch

---

## 4. Cross-Cutting Concerns

### 4.1 Logging & Observability

- Structured logging (JSON)
- Correlation IDs per request
- Centralized log aggregation
- Metrics: stock throughput, transaction latency

### 4.2 Error Handling

- Global exception handling
- Business vs System exceptions
- User-safe error messages

### 4.3 Configuration

- Environment-based config
- Secrets via vault
- Feature flags

---

## 5. Data Design Standards

- Immutable transaction tables
- Soft deletes for master data
- Temporal tables for audit-critical entities
- Explicit foreign keys (no hidden relationships)
- No business logic in triggers

---

## 6. Performance & Scalability

- Read/write separation
- Batch processing for stock movements
- Async processing for heavy workflows
- Horizontal scaling readiness

---

## 7. Environments

- Local Development
- Integration / QA
- UAT
- Production

Each environment must support:
- Database migrations
- Seed data
- Feature toggles

---

## 8. AI Usage Notes (Claude / LLMs)

When generating code:
- Prefer explicit domain models
- Avoid anemic models
- Generate validation at domain level
- Assume high data volumes

---

**This document defines the non-negotiable technical foundation for all ERP modules.**

