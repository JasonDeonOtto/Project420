# Unit Testing Guide - Project420
## How to Write and Run Unit Tests

**Last Updated**: 2025-12-05
**Status**: ✅ **Active** - 28 tests passing

---

## 🎯 What Are Unit Tests?

**Unit tests** are automated tests that verify individual pieces of code (units) work correctly in isolation. They:

- Test **one thing** at a time (a single method or function)
- Run **fast** (milliseconds)
- Are **independent** (don't rely on databases, files, or other tests)
- Provide **confidence** that code works as expected
- **Document** how code should behave

---

## 🛠️ Tools We Use

### xUnit
The testing framework that runs our tests.

### Moq
Creates fake (mock) objects so we don't need real databases or services.

### FluentAssertions
Makes test assertions readable and clear.

### In-Memory Database
Provides a fake database for integration tests.

---

## 📁 Test Project Structure

```
Project420.Management.Tests/
├── Helpers/
│   └── TestDbContextFactory.cs       ← Creates fake databases
├── Services/
│   └── StockManagement/
│       └── ProductServiceTests.cs    ← Service tests
├── Validators/
│   └── CustomerRegistrationValidatorTests.cs ← Validation tests
└── Project420.Management.Tests.csproj ← Project file with packages
```

---

## ✅ Current Test Status

```
Test Run Successful!
Total tests: 28
     Passed: 28 ✅
 Total time: 2.5 seconds
```

### Test Breakdown:
- **ProductService Tests**: 12 tests
  - CreateProduct (3 tests)
  - GetProduct (2 tests)
  - Stock Management (5 tests)
  - Inventory Alerts (2 tests)

- **CustomerRegistrationValidator Tests**: 16 tests
  - Age Verification / Cannabis Act (3 tests)
  - Personal Information (3 tests)
  - Contact Information (3 tests)
  - Medical Cannabis / Section 21 (3 tests)
  - POPIA Compliance (2 tests)
  - Credit Management (2 tests)

---

## 📝 How to Write a Unit Test

### The AAA Pattern

Every test follows this structure:

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // ARRANGE: Set up test data and mocks
    var dto = new CreateProductDto { SKU = "TEST001", Name = "Test Product" };

    _mockValidator
        .Setup(v => v.ValidateAsync(dto, default))
        .ReturnsAsync(new ValidationResult());

    // ACT: Call the method being tested
    var result = await _service.CreateProductAsync(dto);

    // ASSERT: Verify the result is correct
    result.Should().NotBeNull();
    result.SKU.Should().Be("TEST001");
}
```

### Test Naming Convention

```
MethodName_Scenario_ExpectedResult
```

**Examples:**
- `CreateProduct_WithValidData_ReturnsProduct`
- `CreateProduct_WithDuplicateSKU_ThrowsException`
- `Validate_Under18_FailsValidation`

---

## 🔨 How to Run Tests

### Option 1: Command Line (Recommended)

```bash
# Navigate to test project
cd tests/Project420.Management.Tests

# Run all tests
dotnet test

# Run with more details
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "FullyQualifiedName~ProductServiceTests"
```

### Option 2: Visual Studio

1. Open Test Explorer: `Test` → `Test Explorer`
2. Click "Run All Tests" button
3. See results in real-time

### Option 3: VS Code

1. Install C# Dev Kit extension
2. Tests appear in sidebar
3. Click play button next to tests

---

## 📚 Test Examples

### Example 1: Testing a Service Method

```csharp
[Fact]
public async Task CreateProduct_WithValidData_ReturnsProduct()
{
    // Arrange
    var dto = new CreateProductDto
    {
        SKU = "PROD001",
        Name = "Blue Dream",
        Price = 250.00m
    };

    // Mock validator to return success
    _mockCreateValidator
        .Setup(v => v.ValidateAsync(dto, default))
        .ReturnsAsync(new ValidationResult());

    // Mock repository to return no duplicates
    _mockRepository
        .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Product, bool>>>()))
        .ReturnsAsync(new List<Product>());

    // Mock repository to return created product
    _mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Product>()))
        .ReturnsAsync((Product p) => p);

    // Act
    var result = await _service.CreateProductAsync(dto);

    // Assert
    result.Should().NotBeNull();
    result.SKU.Should().Be("PROD001");
    result.Name.Should().Be("Blue Dream");

    // Verify repository was called once
    _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
}
```

### Example 2: Testing Exception Throwing

```csharp
[Fact]
public async Task RemoveStock_WithInsufficientStock_ThrowsException()
{
    // Arrange
    var product = new Product
    {
        Id = 1,
        StockOnHand = 10 // Only 10 in stock
    };

    _mockRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(product);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        async () => await _service.RemoveStockAsync(1, 50, "Sale") // Try to remove 50
    );

    exception.Message.Should().Contain("Insufficient stock");
}
```

### Example 3: Testing Validation

```csharp
[Fact]
public async Task Validate_Under18_FailsValidation()
{
    // Arrange
    var dto = CreateValidDto();
    dto.IdNumber = "1501015800087"; // 2015-01-01 (10 years old)

    // Act
    var result = await _validator.ValidateAsync(dto);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e =>
        e.PropertyName == "IdNumber" &&
        e.ErrorMessage.Contains("18"));
}
```

---

## 🎯 What to Test

### ✅ DO Test:

1. **Happy Path** (everything works correctly)
   ```csharp
   CreateProduct_WithValidData_ReturnsProduct
   ```

2. **Error Cases** (things go wrong)
   ```csharp
   CreateProduct_WithDuplicateSKU_ThrowsException
   CreateProduct_WithInvalidData_ThrowsValidationException
   ```

3. **Edge Cases** (boundary conditions)
   ```csharp
   RemoveStock_WithExactAmount_SucceedsWithZeroStock
   AddStock_WithMaxQuantity_DoesNotOverflow
   ```

4. **Business Rules** (Cannabis Act, POPIA compliance)
   ```csharp
   Validate_Under18_FailsValidation
   Validate_NoConsentGiven_FailsValidation
   ```

### ❌ DON'T Test:

- External libraries (EF Core, FluentValidation internals)
- Private methods (test through public methods)
- Simple getters/setters
- Framework code (.NET libraries)

---

## 🔍 Understanding Mocks

### What is a Mock?

A mock is a **fake object** that replaces a real dependency. We use mocks to:
- Avoid hitting real databases
- Control what methods return
- Verify methods were called

### Creating a Mock

```csharp
// Create a mock repository
var _mockRepository = new Mock<IProductRepository>();

// Tell the mock what to return
_mockRepository
    .Setup(r => r.GetByIdAsync(5))
    .ReturnsAsync(new Product { Id = 5, Name = "Test" });

// Use the mock
var result = await _mockRepository.Object.GetByIdAsync(5);
// result.Name == "Test"
```

### Verifying Method Calls

```csharp
// Verify a method was called exactly once
_mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);

// Verify a method was never called
_mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);

// Verify a method was called with specific value
_mockRepository.Verify(r => r.GetByIdAsync(5), Times.Once);
```

---

## 💡 FluentAssertions Cheat Sheet

### Common Assertions

```csharp
// Null checks
result.Should().NotBeNull();
result.Should().BeNull();

// Equality
result.Should().Be(expectedValue);
result.Should().NotBe(unexpectedValue);

// Strings
result.Name.Should().Be("Expected Name");
result.SKU.Should().Contain("TEST");
result.Description.Should().StartWith("Premium");

// Numbers
result.Price.Should().Be(100.00m);
result.StockOnHand.Should().BeGreaterThan(0);
result.StockOnHand.Should().BeLessThanOrEqualTo(1000);
result.ProfitMargin.Should().BeApproximately(33.33m, 0.01m);

// Booleans
result.IsActive.Should().BeTrue();
result.IsDeleted.Should().BeFalse();

// Collections
results.Should().HaveCount(5);
results.Should().NotBeEmpty();
results.Should().Contain(p => p.SKU == "TEST001");

// Exceptions
await Assert.ThrowsAsync<ValidationException>(() => /* code */);
```

---

## 🧪 Writing Your First Test

### Step 1: Create Test Class

```csharp
public class PricelistServiceTests
{
    private readonly Mock<IPricelistRepository> _mockRepository;
    private readonly PricelistService _service;

    public PricelistServiceTests()
    {
        _mockRepository = new Mock<IPricelistRepository>();
        _service = new PricelistService(_mockRepository.Object);
    }

    // Tests go here
}
```

### Step 2: Write Your First Test

```csharp
[Fact]
public async Task GetPricelistById_ExistingPricelist_ReturnsPricelist()
{
    // Arrange
    var pricelistId = 1;
    var pricelist = new RetailPricelist
    {
        Id = pricelistId,
        Name = "Standard Retail",
        IsDefault = true
    };

    _mockRepository
        .Setup(r => r.GetByIdAsync(pricelistId))
        .ReturnsAsync(pricelist);

    // Act
    var result = await _service.GetPricelistByIdAsync(pricelistId);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(pricelistId);
    result.Name.Should().Be("Standard Retail");
}
```

### Step 3: Run the Test

```bash
cd tests/Project420.Management.Tests
dotnet test
```

---

## 📊 Test Coverage Tips

### Aim for 80%+ Coverage

**High Priority (Must Test):**
- Business logic methods
- Validation rules (Cannabis Act, POPIA)
- Stock management operations
- Financial calculations
- Compliance checks

**Medium Priority:**
- CRUD operations
- Search and filter methods
- Mapping between entities and DTOs

**Low Priority:**
- Simple getters/setters
- Constructor initialization
- Framework code

---

## 🐛 Common Testing Mistakes

### 1. Testing Too Much in One Test

❌ **Bad:**
```csharp
[Fact]
public async Task CreateProduct_DoesEverything()
{
    // Tests creation, validation, stock, pricing, all at once
    // Hard to debug when it fails!
}
```

✅ **Good:**
```csharp
[Fact]
public async Task CreateProduct_WithValidData_ReturnsProduct() { }

[Fact]
public async Task CreateProduct_WithInvalidData_ThrowsException() { }

[Fact]
public async Task CreateProduct_WithDuplicateSKU_ThrowsException() { }
```

### 2. Forgetting to Setup Mocks

❌ **Bad:**
```csharp
// No setup - mock returns null!
var result = await _mockRepository.Object.GetByIdAsync(5);
// result is null, test fails
```

✅ **Good:**
```csharp
_mockRepository
    .Setup(r => r.GetByIdAsync(5))
    .ReturnsAsync(new Product { Id = 5 });

var result = await _mockRepository.Object.GetByIdAsync(5);
// result is not null
```

### 3. Tests That Depend on Each Other

❌ **Bad:**
```csharp
[Fact]
public void Test1() { /* Creates product with ID 1 */ }

[Fact]
public void Test2() { /* Assumes product 1 exists */ }
// If Test1 fails, Test2 also fails!
```

✅ **Good:**
```csharp
[Fact]
public void Test1()
{
    // Arrange: Create own test data
    // Act: Test
    // Assert: Verify
}

[Fact]
public void Test2()
{
    // Arrange: Create own test data (independent)
    // Act: Test
    // Assert: Verify
}
```

---

## 🚀 Next Steps

### 1. Write More Service Tests

Create tests for:
- `PricelistService` (12-15 tests)
- `CustomerService` (10-12 tests)

### 2. Write Integration Tests

Test with real database operations:
- Create product → Add to pricelist → Make sale
- Customer registration → Credit check → Purchase

### 3. Add Test Coverage Report

```bash
dotnet add package coverlet.collector
dotnet test --collect:"XPlat Code Coverage"
```

### 4. Set Up Continuous Integration

Run tests automatically on every commit (GitHub Actions, Azure DevOps).

---

## 📚 Resources

### Official Documentation
- **xUnit**: https://xunit.net/
- **Moq**: https://github.com/moq/moq4
- **FluentAssertions**: https://fluentassertions.com/

### Test Patterns
- **AAA Pattern**: Arrange, Act, Assert
- **Given-When-Then**: BDD style testing
- **Test Naming**: Roy Osherove naming standards

---

## 🎉 Summary

You now have:
- ✅ **28 passing tests**
- ✅ **Test infrastructure** set up
- ✅ **Mock examples** for services
- ✅ **Validation examples** for compliance
- ✅ **Helper classes** for test data

**How to Add More Tests:**
1. Copy an existing test file
2. Rename the class
3. Modify the tests for your service
4. Run `dotnet test`

**Golden Rule:**
> Write tests FIRST, then write code. This ensures your code is testable and works correctly from the start.

---

**Last Updated**: 2025-12-05
**Current Test Count**: 28 tests ✅
**Next Goal**: 50+ tests by end of week
