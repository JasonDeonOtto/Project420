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
Project420.Management.Tests/
├── Services/
│   └── StockManagement/
│       └── ProductServiceTests.cs (12 tests)
├── Validators/
│   └── CustomerRegistrationValidatorTests.cs (16 tests)
└── Helpers/
    └── TestDbContextFactory.cs
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

Happy Testing! 🧪
