# Project420 — PoC Capability Mapping & Credibility Gap Analysis

> **Purpose:** Assess current PoC against market & legal expectations, and identify *credibility gaps* (not feature bloat).
>
> **Audience:** Founder, Architects, Claude

---

## 0. How to Read This Document

This is **not** a criticism of the PoC.

The PoC already demonstrates *above-average correctness*. This document exists to:
- Confirm what is already strong
- Identify **small, high-impact gaps** that affect *trust*, not scope
- Prevent unnecessary feature creep

---

## 1. High-Confidence Capabilities (Already Strong)

These areas already meet or exceed market PoC expectations.

---

### 1.1 Stock Movements (Core Engine)

**Status:** ✅ Credible

Evidence (from your PoC work):
- Movement-based stock changes
- Explicit in/out logic
- Batch-aware operations
- No silent stock mutation

Market view:
> "This system understands inventory truth."

No additional features required at PoC.

---

### 1.2 Batch & Traceability Foundations

**Status:** ✅ Credible

- Batch identifiers exist
- Movements reference batches
- Batch lineage is preserved

Market view:
> "This can survive an audit question."

---

### 1.3 Retail as a Consumer (Not Authority)

**Status:** ✅ Correct Direction

- Retail triggers stock changes
- Stock logic does not live in UI
- Returns conceptually exist

Market view:
> "Retail is not corrupting inventory."

Polish not required yet.

---

### 1.4 Legal Intent & Compliance Awareness

**Status:** ✅ Strong Signal

- You are designing *for compliance*, not adding it later
- Immutability is a guiding principle

Market view:
> "This founder understands regulated systems."

This is a major credibility advantage.

---

## 2. Medium-Risk Credibility Gaps (High Leverage Fixes)

These are **not missing features** — they are missing *proof points*.

Addressing these dramatically increases trust with minimal scope increase.

---

### 2.1 Explicit Movement Immutability Proof

**Current State:**
- Movements behave as immutable
- But immutability is not *demonstrable*

**Market Risk:**
> "Can someone edit this record later?"

**Minimal Fix (PoC-Appropriate):**
- Remove update/delete paths entirely
- Add explicit "immutable" language in code & docs
- Optional: DB constraint or soft guard

**Credibility Gain:** High

---

### 2.2 Replay / Reconstruction Demonstration

**Current State:**
- State is *conceptually* derivable
- Replay is not surfaced

**Market Risk:**
> "Can you show me what stock was last month?"

**Minimal Fix:**
- Simple service/function:
  - `RebuildStockState(asOfDate)`
- CLI / admin screen / log output is enough

**Credibility Gain:** Very High

---

### 2.3 Compensating Movements (Visible Example)

**Current State:**
- Corrections likely exist
- But may appear as edits or adjustments

**Market Risk:**
> "What happens when a mistake occurs?"

**Minimal Fix:**
- Explicit compensating movement type
- Demo scenario:
  - Wrong sale → compensating return movement

**Credibility Gain:** High

---

### 2.4 Correlation & Intent Visibility

**Current State:**
- Actions occur
- Intent chain is implicit

**Market Risk:**
> "Why did this happen? Who caused it?"

**Minimal Fix:**
- Add CorrelationId to:
  - Retail action
  - Resulting movements
- Display it in logs or admin view

**Credibility Gain:** Medium–High

---

## 3. Low-Risk Gaps (Can Wait Until Prototype)

These are *nice-to-haves* that do not materially affect PoC trust.

---

### 3.1 Formal Role-Based Access Control

- Basic identity is enough at PoC
- Fine-grained permissions can wait

---

### 3.2 Reporting UX

- CSV / JSON exports sufficient
- Dashboards not required

---

### 3.3 Performance & Scale Signals

- Not expected
- Optimisation talk is a red flag at PoC

---

## 4. PoC Credibility Scorecard

| Area | Status |
|----|----|
| Stock Truth | ✅ Strong |
| Traceability | ✅ Strong |
| Compliance Intent | ✅ Strong |
| Audit Replay | ⚠️ Needs Proof |
| Error Explainability | ⚠️ Partial |
| Market Trust | 🟢 Very Good |

---

## 5. Recommended PoC Action Plan (Minimal Scope)

Do **only** the following before freezing PoC:

1. Add visible immutability enforcement
2. Add one replay / reconstruction example
3. Add one compensating movement example
4. Add correlation IDs end-to-end

Everything else moves to Prototype.

---

## 6. Strategic Truth

> **A PoC succeeds when it answers hostile questions calmly.**
>
> **Yours is very close.**

---

**End of PoC Capability Mapping & Gap Analysis**

