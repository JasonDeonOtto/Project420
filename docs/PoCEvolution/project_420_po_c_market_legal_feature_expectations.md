# Project420 — PoC Feature Expectations (Market, Legal & Functional)

> **Scope:** Proof of Concept Phase  \
> **Audience:** Founder, Architects, Claude  \
> **Purpose:** Define what the market and regulators *expect* a PoC to already demonstrate for a regulated, traceability-driven system.

---

## 0. Why This Document Exists

In regulated domains (stocked, traceable, controlled goods), a **PoC is not a demo**.

The market expects a PoC to already prove:
- Legal survivability
- Traceability correctness
- Foundational operational realism

This document defines the **minimum credible PoC feature set** for Project420.

---

## 1. Market Expectations for a PoC (Reality Check)

Buyers, partners, and regulators expect a PoC to:

- Demonstrate **end-to-end traceability**
- Prove **non-bypassable controls**
- Show **audit-ready data**
- Reflect **real operational flows**, not mockups

A PoC that only shows UI or CRUD is **not taken seriously** in this market.

---

## 2. Legal & Regulatory Expectations (Baseline)

Regardless of jurisdiction, regulated stock systems are expected to prove:

### 2.1 Traceability

- Batch-level traceability
- Serial-level traceability where applicable
- Forward and backward lineage

> Regulators care more about *reconstruction* than dashboards.

---

### 2.2 Immutability

- Historical records cannot be edited
- Corrections are additive
- Deletions are forbidden

---

### 2.3 Audit Readiness

- Clear who / what / when / why
- Deterministic replay capability
- Evidence-grade records

---

### 2.4 Separation of Concerns

- Operational actions ≠ records of truth
- UI cannot be the authority

---

## 3. Core PoC Modules (Expected by Market)

### 3.1 Stock Movements (Mandatory)

The PoC MUST demonstrate:

- Append-only stock movement ledger
- Explicit movement types
- Signed quantities
- Batch & serial enforcement
- Compensating corrections

This is **non-negotiable**.

---

### 3.2 Inventory State Reconstruction

- Stock levels derived from movements
- Ability to replay history
- Ability to answer:
  - "What did we have on date X?"

---

### 3.3 Retail (Market Entry)

Expected PoC retail capability:

- Simple POS flow
- Sale creates movements
- Returns create compensating movements
- Receipt references movement IDs

Retail polish is NOT required.

---

### 3.4 Product & Batch Management

- Product master data
- Batch creation & tracking
- Batch attributes (origin, dates, compliance flags)

---

## 4. Compliance-Oriented Features

### 4.1 Compliance Flags

- Items carry compliance attributes
- Movements validate against them

---

### 4.2 Restricted Actions

- Prevent illegal movement types
- Prevent invalid transitions

Failure must be explicit.

---

## 5. Data Integrity Expectations

The PoC must prove:

- No silent failures
- No hidden state
- No derived values stored as truth

---

## 6. Reporting Expectations (PoC Level)

PoC reporting is expected to show:

- Movement history per item / batch
- Current stock snapshot (derived)
- Simple audit export (CSV / JSON)

Advanced BI is NOT expected.

---

## 7. Security & Access Control (PoC Grade)

Expected:

- User identity
- Role-based access
- Action attribution

Not expected:

- Advanced IAM
- External SSO

---

## 8. What the Market Does NOT Expect at PoC

Explicitly NOT required:

- Performance optimization
- Multi-tenant scaling
- Full regulatory certification
- Complex integrations
- Mobile apps

Attempting these early is a red flag.

---

## 9. Credibility Signals

A PoC gains credibility when it can:

- Explain *why* a transaction failed
- Replay historical state
- Survive hostile questioning
- Demonstrate legal intent

---

## 10. PoC Success Criteria

The PoC is considered successful if:

- Core flows are legally sound
- Traceability is provable
- Stock cannot be corrupted
- Retail cannot bypass rules

---

## 11. Final Market Truth

> **The market does not buy features.**  \
> **It buys trust.**  \
> **Trust is built on traceability and correctness.**

---

**End of PoC Feature Expectations**

