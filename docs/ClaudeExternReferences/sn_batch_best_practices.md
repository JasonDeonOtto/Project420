# Best Practices & Requirements for Serial Number (SN) and Batch Number Generation
## Seed-Sale Traceability System (Cannabis)

This document outlines recommended standards for generating batch numbers and serial numbers within a cannabis seed-sale traceability system. The goal is to maintain regulatory compliance, ensure traceability end‑to‑end, and preserve data integrity across production, stock take, transfers, and sale workflows.

---

## 1. Core Design Principles

### **1.1 Uniqueness**
- Every **Batch Number** must be globally unique.
- Every **Internal Serial Number (iSN)** must be unique within the system.
- Every **External Serial Number (xSN)** must be globally unique and non‑guessable.

### **1.2 Traceability**
- SNs should be able to directly reference the Batch they originated from.
- Batches should be traceable upstream (input batches) and downstream (derived batches).
- The system should support seed-to-sale regulatory tracing.

### **1.3 Scalability**
- Must support **millions** of serials.
- Batch numbers should avoid formats that will run out of values (avoid simple numeric sequences).

### **1.4 Non‑Ambiguity**
- Avoid characters that can be confused visually: `0/O`, `1/I/l`.
- Recommended character set: **A‑Z (uppercase) + 2‑9**.

### **1.5 System Safety**
- Must prevent collisions even across distributed systems.
- Allow validation (checksum or encoded structure for detecting tampering/errors).

---

## 2. Batch Type Categories
Your system includes multiple batch types, each requiring traceability:
- **Production Batches** – Created during planting, growing, harvesting, curing, packaging.
- **Stock Take Batches** – Created when performing inventory audits or cycle counts.
- **Transfer Batches** – Created when moving stock between sites or facilities.
- **Sale Batches** – Outbound batches for wholesale or retail customers.

Each batch type should share a consistent format with a different **prefix**.

---

## 3. Batch Number Format
### **Recommended 12–14 Character Batch Number**
Format:
```
TT-YYMMDD-RRRR
```
Where:
- **TT** = Batch Type Code (2 chars)
  - PR = Production
  - ST = Stock Take
  - TR = Transfer
  - SL = Sales
- **YYMMDD** = Creation Date
- **RRRR** = 4‑digit random or sequential alphanumeric counter

Example:
```
PR-250308-A7K3
```

### **Why this works**
- Readable
- Sortable
- Identifies batch origin & creation date
- Very low collision risk
- Works well for regulatory audits

---

## 4. Internal Serial Number (iSN) — 14 Characters
The internal SN must be short, efficient, and easy to index.

### **Recommended 14‑Character Format**
```
BBXXXXYYYYZZZZ
```
Where:
- **BB** = Encoded Batch reference (2 chars, e.g., hash of batch number)
- **XXXX** = Time/sequence component (base‑32 encoded)
- **YYYY** = Random entropy block
- **ZZZZ** = Incrementing per‑batch counter or device ID

### Key requirements
- Must be **fast to generate**.
- Must allow **millions of SNs per batch**.
- Must be **linkable** back to the batch via the first 2 characters (or stored FK).

### Example
```
MP4F92KQ7C3D1X
```

---

## 5. External Serial Number (xSN)
This is the serial placed on packaging and exposed to external customers.
It should contain **human‑readable info** and **encoded details**.

### Inclusions
- Batch Number (full or partial)
- Product Code or Strain Code
- Plant/Seed Type
- Checksum to detect tampering
- Optional QR‑encoded metadata link

### Recommended xSN Format
```
{BatchNumber}-{ItemSeq}-{Checksum}
```
Example:
```
PR-250308-A7K3-004291-X9
```

### Optional Enhanced External Format
A friendlier public-facing version:
```
CANNABIS-SEED | Durban Poison | Batch PR‑250308‑A7K3 | SN 004291 | Hash X9
```

---

## 6. QR-Based External Identifiers
Each external serial may also include a QR code linking to a traceability API.

### QR Payload Example (JSON encoded)
```json
{
  "sn": "PR-250308-A7K3-004291-X9",
  "batch": "PR-250308-A7K3",
  "strain": "Durban Poison",
  "origin": "Farm A",
  "production_date": "2025-03-08"
}
```

---

## 7. Validation Rules
### **SN Requirements**
- Length = 14 characters
- Must validate via checksum or consistent encoding rules
- Must always reference a batch via FK in DB

### **Batch Requirements**
- Must enforce unique constraint on Batch Number
- Must track Batch Type, creation date, status (Open/Closed), and originating facility

### **Collision Prevention**
- Use database unique indexes
- Use random entropy + counter
- Use atomic sequences for batch counters

---

## 8. Database Schema Requirements

### **Batch Table**
- BatchID (PK)
- BatchNumber (Unique)
- BatchType
- CreatedDate
- Facility
- Status

### **Serial Table**
- SerialID (PK)
- InternalSerialNumber (Unique)
- ExternalSerialNumber (Nullable/Unique when used)
- BatchID (FK)
- Status
- CreatedDate

---

## 9. Generation Logic Summary

### **Batch Generation Flow**
1. System identifies batch type.
2. System generates `TT-YYMMDD-RRRR`.
3. Insert into database with uniqueness guaranteed.

### **Internal Serial Generation Flow**
1. Take the batch ID or batch hash.
2. Add time/sequence value.
3. Add random block.
4. Add increment counter.
5. Validate length = 14 chars.
6. Save.

### **External Serial Generation Flow**
1. Retrieve batch.
2. Create sequence per batch.
3. Append checksum.
4. Optionally embed strain/product.
5. Generate QR linking to traceability.

---

## 10. Security Recommendations
- Use **non‑predictable sequences** for external SNs.
- Use checksums or message digests (CRC‑8 or XXHash).
- Rate-limit SN generation to avoid brute force.
- Never expose internal SNs externally.

---

## 11. Regulatory & Compliance Notes
- Keep all batches immutable once closed.
- Maintain full lifecycle logs (creation → transfers → inventory adjustments → sale).
- Serial reuse is forbidden.
- Keep all SN associations auditable.

---

## 12. Summary
This standard provides:
- A uniform, scalable system for batch and serial creation
- A clear link between batches and serials
- A separation between internal system identifiers and external customer-facing ones
- Support for all traceability workflows in the cannabis seed-to-sale pipeline

This structure ensures a professional, auditable, regulatory‑aligned solution suitable for high‑volume commercial environments.

