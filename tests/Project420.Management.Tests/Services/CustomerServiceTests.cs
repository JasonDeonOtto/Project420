using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Management.BLL.Sales.SalesCommon.Services;
using Project420.Management.DAL.Repositories.Sales.SalesCommon;
using Project420.Management.Models.Entities.Sales.Common;
using Project420.Management.Tests.Infrastructure;

namespace Project420.Management.Tests.Services;

/// <summary>
/// Comprehensive unit tests for CustomerService.
/// Tests registration, validation, age verification, and POPIA compliance.
/// CRITICAL: Cannabis Act requires 18+ age verification for all customers.
/// </summary>
public class CustomerServiceTests : ServiceTestBase
{
    private readonly Mock<IDebtorRepository> _mockCustomerRepository;
    private readonly Mock<IValidator<CustomerRegistrationDto>> _mockValidator;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        // Create mocks
        _mockCustomerRepository = new Mock<IDebtorRepository>();
        _mockValidator = new Mock<IValidator<CustomerRegistrationDto>>();

        // Create service with mocked dependencies
        _customerService = new CustomerService(
            _mockCustomerRepository.Object,
            _mockValidator.Object,
            MockComplianceService.Object);

        // Setup default valid validation result
        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<CustomerRegistrationDto>(), default))
            .ReturnsAsync(new ValidationResult());
    }

    #region RegisterCustomerAsync Tests

    [Fact]
    public async Task RegisterCustomerAsync_ValidData_ReturnsRegisteredCustomer()
    {
        // Arrange: Valid customer registration (18+ years old)
        var dto = new CustomerRegistrationDto
        {
            Name = "John Doe",
            IdNumber = "900101-5100-0-88", // Born 1990-01-01 (34 years old)
            Mobile = "0821234567",
            Email = "john.doe@example.com",
            PhysicalAddress = "123 Main St, Cape Town",
            ConsentGiven = true,
            ConsentPurpose = "Customer account management"
        };

        // Mock: No duplicate customers found
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(dto.IdNumber)).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(dto.Email)).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByMobileAsync(dto.Mobile)).ReturnsAsync((Debtor?)null);

        // Mock: Repository saves customer
        _mockCustomerRepository
            .Setup(x => x.AddAsync(It.IsAny<Debtor>()))
            .ReturnsAsync((Debtor debtor) =>
            {
                debtor.Id = 1; // Simulate database ID assignment
                return debtor;
            });

        // Act
        var result = await _customerService.RegisterCustomerAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(dto.Name);
        result.IdNumber.Should().Be(dto.IdNumber);
        result.Mobile.Should().Be(dto.Mobile);
        result.Email.Should().Be(dto.Email);
        result.ConsentGiven.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.CurrentBalance.Should().Be(0); // New customer starts with 0 balance
        result.ConsentDate.Should().NotBeNull();
        result.DateOfBirth.Should().Be(new DateTime(1990, 1, 1));

        // Verify repository was called
        _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Debtor>()), Times.Once);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange: Invalid data
        var dto = new CustomerRegistrationDto
        {
            Name = "", // Invalid: empty name
            IdNumber = "invalid",
            Mobile = "invalid",
            Email = "invalid"
        };

        // Mock: Validation fails
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("IdNumber", "Invalid SA ID number"),
            new ValidationFailure("Mobile", "Invalid mobile number"),
            new ValidationFailure("Email", "Invalid email address")
        };
        _mockValidator
            .Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act
        Func<Task> act = async () => await _customerService.RegisterCustomerAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name*IdNumber*Mobile*Email*");

        // Verify repository was NOT called
        _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Debtor>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCustomerAsync_DuplicateIdNumber_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CustomerRegistrationDto
        {
            Name = "Jane Doe",
            IdNumber = "950505-5200-0-77",
            Mobile = "0829876543",
            Email = "jane.doe@example.com",
            ConsentGiven = true
        };

        // Mock: Existing customer with same ID number
        var existingCustomer = new Debtor
        {
            Id = 10,
            IdNumber = dto.IdNumber,
            Name = "Existing Customer"
        };
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(dto.IdNumber)).ReturnsAsync(existingCustomer);

        // Act
        Func<Task> act = async () => await _customerService.RegisterCustomerAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{dto.IdNumber}*already exists*");

        // Verify repository was NOT called
        _mockCustomerRepository.Verify(x => x.AddAsync(It.IsAny<Debtor>()), Times.Never);
    }

    [Fact]
    public async Task RegisterCustomerAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CustomerRegistrationDto
        {
            Name = "Jane Doe",
            IdNumber = "950505-5200-0-77",
            Mobile = "0829876543",
            Email = "duplicate@example.com",
            ConsentGiven = true
        };

        // Mock: No duplicate ID number
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(dto.IdNumber)).ReturnsAsync((Debtor?)null);

        // Mock: Existing customer with same email
        var existingCustomer = new Debtor
        {
            Id = 10,
            Email = dto.Email,
            Name = "Existing Customer"
        };
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(dto.Email)).ReturnsAsync(existingCustomer);

        // Act
        Func<Task> act = async () => await _customerService.RegisterCustomerAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{dto.Email}*already exists*");
    }

    [Fact]
    public async Task RegisterCustomerAsync_DuplicateMobile_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CustomerRegistrationDto
        {
            Name = "Jane Doe",
            IdNumber = "950505-5200-0-77",
            Mobile = "0829876543",
            Email = "jane@example.com",
            ConsentGiven = true
        };

        // Mock: No duplicate ID number or email
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(dto.IdNumber)).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(dto.Email)).ReturnsAsync((Debtor?)null);

        // Mock: Existing customer with same mobile
        var existingCustomer = new Debtor
        {
            Id = 10,
            Mobile = dto.Mobile,
            Name = "Existing Customer"
        };
        _mockCustomerRepository.Setup(x => x.GetByMobileAsync(dto.Mobile)).ReturnsAsync(existingCustomer);

        // Act
        Func<Task> act = async () => await _customerService.RegisterCustomerAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{dto.Mobile}*already exists*");
    }

    [Fact]
    public async Task RegisterCustomerAsync_MedicalPatient_StoresPermitInformation()
    {
        // Arrange: Medical cannabis patient (Section 21 permit)
        var dto = new CustomerRegistrationDto
        {
            Name = "Medical Patient",
            IdNumber = "850815-5100-0-99",
            Mobile = "0823456789",
            Email = "patient@example.com",
            MedicalPermitNumber = "MP-2024-001",
            MedicalPermitExpiryDate = DateTime.Today.AddYears(1),
            PrescribingDoctor = "Dr. Smith",
            ConsentGiven = true
        };

        // Mock: No duplicates
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByMobileAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);

        // Mock: Repository saves customer
        _mockCustomerRepository
            .Setup(x => x.AddAsync(It.IsAny<Debtor>()))
            .ReturnsAsync((Debtor debtor) =>
            {
                debtor.Id = 2;
                return debtor;
            });

        // Act
        var result = await _customerService.RegisterCustomerAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.MedicalPermitNumber.Should().Be(dto.MedicalPermitNumber);
        result.MedicalPermitExpiryDate.Should().Be(dto.MedicalPermitExpiryDate);
        result.PrescribingDoctor.Should().Be(dto.PrescribingDoctor);
    }

    [Fact]
    public async Task RegisterCustomerAsync_POPIAConsent_StoresConsentInformation()
    {
        // Arrange: Customer gives POPIA consent
        var dto = new CustomerRegistrationDto
        {
            Name = "Consent Customer",
            IdNumber = "920303-5300-0-44",
            Mobile = "0824567890",
            Email = "consent@example.com",
            ConsentGiven = true,
            MarketingConsent = true,
            ConsentPurpose = "Customer account and marketing"
        };

        // Mock: No duplicates
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByMobileAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);

        // Mock: Repository saves customer
        _mockCustomerRepository
            .Setup(x => x.AddAsync(It.IsAny<Debtor>()))
            .ReturnsAsync((Debtor debtor) =>
            {
                debtor.Id = 3;
                return debtor;
            });

        // Act
        var result = await _customerService.RegisterCustomerAsync(dto);

        // Assert
        result.ConsentGiven.Should().BeTrue();
        result.MarketingConsent.Should().BeTrue();
        result.ConsentDate.Should().NotBeNull();
        result.ConsentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.ConsentPurpose.Should().Be(dto.ConsentPurpose);
    }

    [Fact]
    public async Task RegisterCustomerAsync_CreditCustomer_StoresCreditInformation()
    {
        // Arrange: Account customer with credit
        var dto = new CustomerRegistrationDto
        {
            Name = "Credit Customer",
            IdNumber = "880707-5100-0-22",
            Mobile = "0825678901",
            Email = "credit@example.com",
            CreditLimit = 10000.00m,
            PaymentTerms = 30, // Net 30
            ConsentGiven = true
        };

        // Mock: No duplicates
        _mockCustomerRepository.Setup(x => x.GetByIdNumberAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);
        _mockCustomerRepository.Setup(x => x.GetByMobileAsync(It.IsAny<string>())).ReturnsAsync((Debtor?)null);

        // Mock: Repository saves customer
        _mockCustomerRepository
            .Setup(x => x.AddAsync(It.IsAny<Debtor>()))
            .ReturnsAsync((Debtor debtor) =>
            {
                debtor.Id = 4;
                return debtor;
            });

        // Act
        var result = await _customerService.RegisterCustomerAsync(dto);

        // Assert
        result.CreditLimit.Should().Be(10000.00m);
        result.PaymentTerms.Should().Be(30);
        result.CurrentBalance.Should().Be(0); // New customer starts at 0
    }

    #endregion

    #region GetCustomerByIdAsync Tests

    [Fact]
    public async Task GetCustomerByIdAsync_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var customerId = 5;
        var expectedCustomer = new Debtor
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com"
        };

        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId)).ReturnsAsync(expectedCustomer);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.Name.Should().Be(expectedCustomer.Name);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_NonExistingCustomer_ReturnsNull()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId)).ReturnsAsync((Debtor?)null);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllCustomersAsync Tests

    [Fact]
    public async Task GetAllCustomersAsync_ReturnsAllCustomers()
    {
        // Arrange
        var customers = new List<Debtor>
        {
            new Debtor { Id = 1, Name = "Customer 1" },
            new Debtor { Id = 2, Name = "Customer 2" },
            new Debtor { Id = 3, Name = "Customer 3" }
        };

        _mockCustomerRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Id == 1 && c.Name == "Customer 1");
    }

    #endregion

    #region SearchCustomersAsync Tests

    [Fact]
    public async Task SearchCustomersAsync_ValidSearchTerm_ReturnsMatchingCustomers()
    {
        // Arrange
        var searchTerm = "john";
        var matchingCustomers = new List<Debtor>
        {
            new Debtor { Id = 1, Name = "John Doe", Email = "john@example.com" },
            new Debtor { Id = 2, Name = "Johnny Smith", Email = "johnny@example.com" }
        };

        _mockCustomerRepository.Setup(x => x.SearchCustomersAsync(searchTerm)).ReturnsAsync(matchingCustomers);

        // Act
        var result = await _customerService.SearchCustomersAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "John Doe");
        result.Should().Contain(c => c.Name == "Johnny Smith");
    }

    [Fact]
    public async Task SearchCustomersAsync_EmptySearchTerm_ReturnsEmptyList()
    {
        // Act
        var result = await _customerService.SearchCustomersAsync("");

        // Assert
        result.Should().BeEmpty();
        _mockCustomerRepository.Verify(x => x.SearchCustomersAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchCustomersAsync_WhitespaceSearchTerm_ReturnsEmptyList()
    {
        // Act
        var result = await _customerService.SearchCustomersAsync("   ");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMedicalPatientsAsync Tests

    [Fact]
    public async Task GetMedicalPatientsAsync_ReturnsMedicalPatients()
    {
        // Arrange
        var medicalPatients = new List<Debtor>
        {
            new Debtor
            {
                Id = 1,
                Name = "Patient 1",
                MedicalPermitNumber = "MP-001",
                MedicalPermitExpiryDate = DateTime.Today.AddMonths(6)
            },
            new Debtor
            {
                Id = 2,
                Name = "Patient 2",
                MedicalPermitNumber = "MP-002",
                MedicalPermitExpiryDate = DateTime.Today.AddYears(1)
            }
        };

        _mockCustomerRepository.Setup(x => x.GetMedicalPatientsAsync()).ReturnsAsync(medicalPatients);

        // Act
        var result = await _customerService.GetMedicalPatientsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => !string.IsNullOrEmpty(p.MedicalPermitNumber));
    }

    #endregion

    #region GetExpiringMedicalPermitsAsync Tests

    [Fact]
    public async Task GetExpiringMedicalPermitsAsync_ReturnsExpiringPermits()
    {
        // Arrange
        var daysUntilExpiry = 30;
        var expiringPatients = new List<Debtor>
        {
            new Debtor
            {
                Id = 1,
                Name = "Expiring Patient",
                MedicalPermitNumber = "MP-003",
                MedicalPermitExpiryDate = DateTime.Today.AddDays(15)
            }
        };

        _mockCustomerRepository.Setup(x => x.GetExpiringMedicalPermitsAsync(daysUntilExpiry)).ReturnsAsync(expiringPatients);

        // Act
        var result = await _customerService.GetExpiringMedicalPermitsAsync(daysUntilExpiry);

        // Assert
        result.Should().HaveCount(1);
        result.First().MedicalPermitExpiryDate.Should().BeBefore(DateTime.Today.AddDays(daysUntilExpiry));
    }

    #endregion

    #region UpdateCustomerAsync Tests

    [Fact]
    public async Task UpdateCustomerAsync_ValidCustomer_UpdatesSuccessfully()
    {
        // Arrange
        var customer = new Debtor
        {
            Id = 1,
            Name = "Updated Customer",
            Email = "updated@example.com"
        };

        _mockCustomerRepository.Setup(x => x.UpdateAsync(customer)).Returns(Task.CompletedTask);

        // Act
        await _customerService.UpdateCustomerAsync(customer);

        // Assert
        _mockCustomerRepository.Verify(x => x.UpdateAsync(customer), Times.Once);
    }

    #endregion

    #region DeactivateCustomerAsync Tests

    [Fact]
    public async Task DeactivateCustomerAsync_ValidId_DeactivatesCustomer()
    {
        // Arrange: POPIA requires soft delete (retention for 7 years)
        var customerId = 10;
        _mockCustomerRepository.Setup(x => x.DeleteAsync(customerId)).Returns(Task.CompletedTask);

        // Act
        await _customerService.DeactivateCustomerAsync(customerId);

        // Assert
        _mockCustomerRepository.Verify(x => x.DeleteAsync(customerId), Times.Once);
    }

    #endregion

    #region IsAgeVerifiedAsync Tests

    [Fact]
    public async Task IsAgeVerifiedAsync_ValidAge18Plus_ReturnsTrue()
    {
        // Arrange: ID number for someone 25 years old
        var idNumber = "990101-5100-0-88";
        _mockCustomerRepository.Setup(x => x.IsAgeVerifiedAsync(idNumber)).ReturnsAsync(true);

        // Act
        var result = await _customerService.IsAgeVerifiedAsync(idNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAgeVerifiedAsync_UnderAge18_ReturnsFalse()
    {
        // Arrange: ID number for someone under 18
        var idNumber = "100101-5100-0-88"; // Born 2010 (14 years old)
        _mockCustomerRepository.Setup(x => x.IsAgeVerifiedAsync(idNumber)).ReturnsAsync(false);

        // Act
        var result = await _customerService.IsAgeVerifiedAsync(idNumber);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
