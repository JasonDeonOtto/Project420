# Project420 — PoC Hostile Demo Law

> **Status:** Mandatory for PoC Credibility  \
> **Audience:** Buyers, regulators, auditors, senior engineers  \
> **Tone:** Calm, factual, demonstrable

---

## 0. Purpose of This Document

This document defines the **minimum hostile questions** the Project420 PoC **must be able to answer by demonstration**, not explanation.

It exists to:
- Prove factual integrity
- Eliminate trust assumptions
- Differentiate Project420 above typical PoC quality

If an answer requires explanation instead of demonstration, **the PoC fails that question**.

---

## 1. Evidence Standard (Rigid Rule)

For each question below:

- ✅ Acceptable: *"Watch what happens when I do this"*
- ❌ Unacceptable: *"The code is designed to…"*

**Evidence must be:**
- Observable
- Repeatable
- Non-destructive

---

## 2. Core Hostile Questions (Mandatory)

These questions **must** be demonstrable in every PoC review.

---

### 2.1 Immutability of Stock History

**Question**  \
> "What stops someone from editing stock history?"

**Required Demonstration**
- Attempt to UPDATE a stock movement → fails
- Attempt to DELETE a stock movement → fails

**Acceptance Criteria**
- No UI path exists
- No service method succeeds
- Failure is explicit

---

### 2.2 Correction Without Rewriting History

**Question**  \
> "What happens when a mistake is made?"

**Required Demonstration**
- Perform incorrect movement
- Apply compensating movement
- Original movement remains

**Acceptance Criteria**
- Both movements visible
- Net stock resolves correctly

---

### 2.3 Stock State Reconstruction (As-Of)

**Question**  \
> "What stock did you have on a past date?"

**Required Demonstration**
- Execute `GetStockAsOf(date)`
- Show differing results for different dates

**Acceptance Criteria**
- Output derived from movements only

---

### 2.4 Action Traceability (Correlation)

**Question**  \
> "Why did this stock change occur?"

**Required Demonstration**
- Display CorrelationId
- Trace from origin action → movements

**Acceptance Criteria**
- Single ID links cause to effect

---

### 2.5 Retail Cannot Mutate Stock

**Question**  \
> "What stops Retail from cheating stock?"

**Required Demonstration**
- Attempt direct stock mutation from Retail
- Operation fails

**Acceptance Criteria**
- Only MovementService can change stock

---

### 2.6 No Silent Corrections

**Question**  \
> "Can someone fix things quietly?"

**Required Demonstration**
- Apply correction
- Correction is a visible movement

**Acceptance Criteria**
- No hidden state change exists

---

## 3. Secondary Hostile Questions (Recommended)

These elevate the PoC **above average competitors**, without overengineering.

---

### 3.1 Atomicity of Movements

**Question**  \
> "What happens if the system fails mid-operation?"

**Required Demonstration**
- Simulated failure
- No partial movement persisted

---

### 3.2 Batch Lineage Visibility

**Question**  \
> "Where did this batch come from and where did it go?"

**Required Demonstration**
- Show batch-linked movements end-to-end

---

### 3.3 Invalid Action Rejection

**Question**  \
> "What happens when someone tries something invalid or illegal?"

**Required Demonstration**
- Attempt invalid transition
- System rejects with explanation

---

## 4. Explicit Non-Goals (PoC Boundary)

The following are **intentionally out of scope** for PoC:

- Performance optimisation
- Full reporting dashboards
- Complex RBAC
- UX polish
- Multi-tenant behaviour

Their absence **must not** be interpreted as a flaw.

---

## 5. PoC Acceptance Rule

The PoC is considered **credible and complete** when:

- All **Core Hostile Questions** can be demonstrated
- At least **one** Secondary Question is demonstrated

At that point:

> 🔒 **The PoC must be frozen.**

All future enhancements move to **Prototype**.

---

## 6. Final Statement

> **This PoC does not ask to be trusted.**  \
> **It proves what it claims.**

---

**End of PoC Hostile Demo Law**

