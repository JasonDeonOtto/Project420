# Project420 — PoC Identity & Actor Model

> **Status:** Mandatory for PoC Evidence  \
> **Scope:** Proof & Traceability only  \
> **Explicitly NOT:** Security, authentication, or authorization

---

## 1. Purpose

This document defines the **minimal identity model** required for the Project420 PoC.

Its sole purpose is to:
- Attribute actions to an actor
- Support hostile demo questions
- Provide evidential traceability

It intentionally avoids all production-grade authentication concerns.

---

## 2. Core Principle (Rigid)

> **Identity exists to explain actions, not to protect access.**

If identity data does not help answer:
> *"Who caused this?"*

It does not belong in PoC.

---

## 3. Actor Model (Maximum 5 Fields)

Every state-changing action in PoC must capture the following **Actor Context**:

| Field | Required | Description |
|----|----|----|
| `ActorId` | ✅ | Stable identifier for the actor |
| `ActorType` | ✅ | `User` or `System` |
| `ActorRole` | ⚠️ Optional | Logical role (e.g. `Retail`, `Admin`) |
| `Source` | ✅ | Origin of action (POS, Admin, API) |
| `OccurredAtUtc` | ✅ | Timestamp of action |

No additional identity fields are permitted in PoC.

---

## 4. Acceptable Identity Sources (PoC)

Any of the following are acceptable:

- Hardcoded demo identity
- Request header (e.g. `X-Demo-Actor`)
- Dev-only selector in UI
- Test harness injection

**Passwords, tokens, sessions, and MFA are explicitly forbidden.**

---

## 5. Where Actor Context Is Mandatory

Actor Context **must** be persisted on:

- Stock Movements
- Compensating Movements
- Corrections / Adjustments
- Retail Transactions

Actor Context **must not** be optional for these actions.

---

## 6. Evidence Requirements

The PoC must be able to demonstrate:

- Who caused a specific movement
- From which source
- At what time
- In relation to which correlation ID

This may be shown via:
- Minimal UI
- Test output
- Logs

---

## 7. Explicit Non-Goals (PoC Boundary)

The following are intentionally excluded:

- Authentication flows
- Authorization checks
- Permission matrices
- User management
- Identity persistence

Their absence is **not a deficiency**.

---

## 8. Upgrade Path (Forward Reference)

In Prototype:

- `ActorId` maps to authenticated UserId
- `ActorRole` maps to RBAC
- Identity source becomes external (IdP)

No PoC data model changes are required for this upgrade.

---

## 9. Acceptance Rule

The PoC satisfies identity requirements when:

- Every hostile demo question can attribute actions to an actor
- No authentication system exists
- Identity logic cannot block progress

---

## 10. Final Statement

> **The PoC knows who acted — not how they logged in.**

---

**End of PoC Identity & Actor Model**

