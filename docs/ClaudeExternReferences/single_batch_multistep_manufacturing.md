# 🌿 Single-Batch, Multi-Step Manufacturing Model
### Cannabis Production System Specification (FBP + EBP)

---

# 1. Overview
This document specifies the system design for a **single-batch, multi-step manufacturing model** applicable to both:

- **Flower-Based Production (FBP)**: mechanical assembly, pre-rolls, packaged flower.
- **Extraction-Based Processing (EBP)**: chemical extraction, distillates, edibles, vapes.

Key principle: **one batch enters the system, passes through multiple steps, produces WIP and finished goods, all traceable under the same batch lineage.**

---

# 2. Key Concepts

| Term | Definition |
|------|-----------|
| Batch | A single manufacturing run identified by a unique batch number (e.g., BATCH-2025-0001). |
| Step | An internal transformation or processing stage within a batch (e.g., milling, extraction, distillation). |
| WIP | Work-in-progress output at a step. The batch ID remains the same. |
| Finished Goods | Final SKUs produced from the batch (packaged flower, edibles, vape cartridges). |
| Parent/Child Batch | Only created when combining multiple batches or creating a different product type. |
| SN | Serial number assigned to individual finished units, linked to batch and step lineage. |

---

# 3. Batch Life Cycle (Multi-Step)

1. **Batch Creation**
```
BATCH-2025-0001
```
- Assigned when manufacturing run starts
- Linked to input materials (flower, trim, oils, additives)

2. **Step Execution**
- Each step consumes inputs (material, ingredients) and produces WIP
- Step types can include:
  - Milling / Grinding (FBP)
  - Bucking / Trimming (FBP)
  - Decarboxylation (EBP)
  - Extraction (EBP)
  - Filtration / Winterization (EBP)
  - Formulation / Infusion (EBP)
  - Packaging / Assembly (FBP & EBP)

3. **WIP Tracking**
- Record output quantity, losses, yields, potency (if applicable)
- Assign temporary WIP IDs internally, **not new batches**

4. **Finished Goods Assignment**
- After final step, assign SNs to units
- Link finished goods to parent batch and step lineage

5. **Optional Child Batches**
- Only created if:
  - Multiple unrelated batches are combined
  - Product type changes completely (e.g., from crude oil to edibles)

---

# 4. Example: Flower-Based Production (FBP)
```
BATCH-2025-0001
   ↓ Step 1: Milling
   ↓ Step 2: Pre-roll Filling
   ↓ Step 3: Pre-roll Capping
   ↓ Step 4: QC
   ↓ Step 5: Packaging
   → Finished Goods: 500 pre-rolls (SN: SN-PR-2025-0001-001 → SN-PR-2025-0001-500)
```
- No new batch numbers created
- Each step produces WIP tracked under same batch

---

# 5. Example: Extraction-Based Processing (EBP)
```
BATCH-2025-0002
   ↓ Step 1: Decarboxylation
   ↓ Step 2: Extraction (Ethanol/BHO/CO2)
   ↓ Step 3: Filtration / Winterization
   ↓ Step 4: Distillation
   ↓ Step 5: Formulation (Edibles / Vapes)
   ↓ Step 6: Packaging
   → Finished Goods: 100 vape cartridges (SN-EBP-FORM-2025-0002-001 → 100)
```
- Step-level mass balance and potency recorded
- Single batch maintains full traceability

---

# 6. Data Model (Simplified)

## Tables

**Batch**
- BatchID (PK)
- BatchNumber
- ProductType (FBP/EBP)
- InputMaterials (JSON / link table)
- CreationDate
- Status (Active / Complete / Archived)

**Step**
- StepID (PK)
- BatchID (FK)
- StepName
- StepType (Mechanical / Chemical / Formulation / Packaging)
- InputWeight
- OutputWeight
- LossWeight
- PotencyProfile (if EBP)
- Operator
- StartTime
- EndTime
- Status

**WIP / FinishedGoods**
- WIPID / SKUUnitID (PK)
- BatchID (FK)
- StepID (FK)
- Quantity
- SN (if applicable)
- Status (WIP / Finished)
- QualityData

---

# 7. Workflow Logic

1. **Start Batch** → Assign batch number
2. **For each Step:**
   - Consume inputs
   - Produce WIP output
   - Record yields, losses, potency (EBP)
3. **Repeat Steps** until final product is achieved
4. **Assign Finished Goods SNs**
5. **Archive batch** after all outputs are finished

---

# 8. Advantages of Single-Batch Multi-Step Model

- Simplified traceability
- Accurate WIP and yield tracking
- Avoids batch explosion
- Maintains full parent lineage
- Compatible with both FBP and EBP processes
- Supports SN assignment for finished goods without creating multiple batches
- Enables step-level reporting and QC

---

# 9. Notes on System Implementation

- Use step table to capture every transformation
- Link all WIP outputs to batch and step
- Only create new batch when combining unrelated batches or creating a new product type
- Ensure SNs are always linked to batch ID and step
- Maintain mass balance and potency calculations for EBP

---

# 10. Summary
The **single-batch multi-step model** allows your cannabis system to:

- Track flower, trim, extracts, and formulations under a single batch
- Record WIP, step transformations, losses, and yields
- Assign SNs to final units while maintaining traceability
- Handle both **FBP** and **EBP** workflows without creating unnecessary new batches

All steps are internal transformations, producing WIP that flows naturally to the next step until finished goods are produced.

