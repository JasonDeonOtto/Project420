# Project420 Unit Tests

**Status**: ✅ 28 tests passing
**Last Run**: 2025-12-05

---

## Quick Start

### Run All Tests
```bash
cd tests/Project420.Management.Tests
dotnet test
```

### Run with Details
```bash
dotnet test --verbosity normal
```

### Run Specific Tests
```bash
# Run only ProductService tests
dotnet test --filter "ProductServiceTests"

# Run only validation tests
dotnet test --filter "ValidatorTests"
```

### Watch Mode (Auto-run on file changes)
```bash
dotnet watch test
```

---

## Test Structure

```
tests/
├── Project420.Management.Tests/
│   ├── Services/
│   │   └── StockManagement/
│   │       └── ProductServiceTests.cs (12 tests)
│   ├── Validators/
│   │   └── CustomerRegistrationValidatorTests.cs (16 tests)
│   └── Helpers/
│       └── TestDbContextFactory.cs
├── Project420.ProofRunner/          ⭐ NEW: Evidence Runner
│   ├── Program.cs
│   ├── Evidence/
│   │   ├── ImmutabilityEvidence.cs
│   │   ├── CompensationEvidence.cs
│   │   ├── ReplayEvidence.cs
│   │   └── TraceabilityEvidence.cs
│   └── Infrastructure/
│       └── EvidenceDbContextFactory.cs
```

---

## Current Test Coverage

| Component | Tests | Status |
|-----------|-------|--------|
| ProductService | 12 | ✅ Passing |
| CustomerRegistrationValidator | 16 | ✅ Passing |
| **Total** | **28** | **✅ All Passing** |

---

## Adding New Tests

1. Create a new file in appropriate folder:
   ```
   Services/Sales/PricelistServiceTests.cs
   ```

2. Copy structure from existing test file

3. Write tests following AAA pattern:
   ```csharp
   [Fact]
   public async Task MethodName_Scenario_ExpectedResult()
   {
       // Arrange
       // Act
       // Assert
   }
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

---

## Documentation

See `../docs/UNIT-TESTING-GUIDE.md` for comprehensive guide including:
- How to write tests
- Mocking examples
- Common patterns
- Best practices

---

## Test Results (Latest Run)

```
Test Run Successful.
Total tests: 28
     Passed: 28 ✅
 Total time: 2.5 seconds
```

**Cannabis Compliance Tests**: ✅ Age verification working
**POPIA Compliance Tests**: ✅ Consent validation working
**Business Logic Tests**: ✅ Stock management working

---

## ⭐ Evidence Runner (Client Demonstrations)

### Purpose
**Single-command proof** for hostile client demonstrations. Shows business rules work correctly in plain English.

**This is NOT unit tests** - it's a client-facing demonstration tool.

### How to Run

#### Quick Demo (Console Output)
```bash
cd tests/Project420.ProofRunner
dotnet run
```

**Output:**
```
--------------------------------
Project420 Evidence Check
--------------------------------
✔ Stock history is immutable
✔ Corrections create compensating movements
✔ Stock can be reconstructed as-of a date
✔ All movements are traceable to an actor

Evidence Status: PASS
--------------------------------
```

### What It Proves

| Evidence | Business Rule Demonstrated |
|----------|----------------------------|
| **Immutability** | Stock movements cannot be modified after creation |
| **Compensation** | Errors are corrected with new entries, not by editing history |
| **Replay** | Stock levels can be calculated as-of any historical date |
| **Traceability** | All movements trace back to an actor (audit compliance) |

### When to Use

**Use Evidence Runner when:**
- Client asks hostile question: "How do you ensure data isn't tampered with?"
- Compliance demo: "Show me your audit trail works"
- Live proof: Run it during the meeting (takes <1 second)

**Use Unit Tests when:**
- Development and debugging
- CI/CD pipelines
- Technical code review

### Client Demo Script

**Client asks**: "What stops someone from just changing the stock records?"

**You demonstrate**:
```bash
cd tests/Project420.ProofRunner
dotnet run
```

**You show**: Console output proving:
1. ✔ History is write-once (immutable)
2. ✔ Corrections require compensating entries
3. ✔ Full audit trail to reconstruct history
4. ✔ All changes traced to users

**Result**: Client sees executable proof, not promises.

---

Happy Testing! 🧪
