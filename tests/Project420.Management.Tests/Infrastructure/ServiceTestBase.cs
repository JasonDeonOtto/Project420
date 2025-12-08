using Moq;
using Project420.Management.DAL;
using Project420.Shared.Core.Compliance.Services;

namespace Project420.Management.Tests.Infrastructure;

/// <summary>
/// Base class for BLL service tests.
/// Provides common mocked dependencies and test infrastructure.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    /// <summary>
    /// In-memory database context for testing.
    /// Automatically created for each test.
    /// </summary>
    protected ManagementDbContext DbContext { get; private set; }

    /// <summary>
    /// Mocked Cannabis Compliance Service.
    /// Configure mock behavior in test methods as needed.
    /// </summary>
    protected Mock<ICannabisComplianceService> MockComplianceService { get; private set; }

    /// <summary>
    /// Unique database name for this test instance.
    /// Ensures test isolation.
    /// </summary>
    protected string DatabaseName { get; private set; }

    /// <summary>
    /// Constructor - creates fresh test infrastructure for each test.
    /// </summary>
    protected ServiceTestBase()
    {
        // Generate unique database name for test isolation
        DatabaseName = Guid.NewGuid().ToString();

        // Create in-memory database context
        DbContext = TestDbContextFactory.CreateManagementDbContext(DatabaseName);

        // Create mock for shared compliance service
        MockComplianceService = new Mock<ICannabisComplianceService>();

        // Setup default mock behaviors
        SetupDefaultMockBehaviors();
    }

    /// <summary>
    /// Setup default mock behaviors that are commonly used across tests.
    /// Override in derived classes to customize.
    /// </summary>
    protected virtual void SetupDefaultMockBehaviors()
    {
        // Default: Extract date of birth from SA ID number
        // SA ID format: YYMMDD-SSSS-C-ZZ (e.g., 900101-5100-0-8-8)
        MockComplianceService
            .Setup(x => x.ExtractDateOfBirth(It.IsAny<string>()))
            .Returns((string idNumber) =>
            {
                // Simple mock: extract date from first 6 digits
                if (string.IsNullOrEmpty(idNumber) || idNumber.Length < 6)
                    return DateTime.Today.AddYears(-25); // Default to 25 years old

                try
                {
                    var year = int.Parse(idNumber.Substring(0, 2));
                    var month = int.Parse(idNumber.Substring(2, 2));
                    var day = int.Parse(idNumber.Substring(4, 2));

                    // Determine century (19xx or 20xx)
                    var fullYear = year >= 0 && year <= 30 ? 2000 + year : 1900 + year;

                    return new DateTime(fullYear, month, day);
                }
                catch
                {
                    return null;
                }
            });

        // Default: Age verification for SA ID (18+ required)
        MockComplianceService
            .Setup(x => x.IsAgeVerified(It.IsAny<string>()))
            .Returns((string idNumber) =>
            {
                var dob = MockComplianceService.Object.ExtractDateOfBirth(idNumber);
                if (!dob.HasValue) return false;

                var age = DateTime.Today.Year - dob.Value.Year;
                if (dob.Value.Date > DateTime.Today.AddYears(-age)) age--;
                return age >= 18;
            });

        // Default: All cannabis products require age verification
        MockComplianceService
            .Setup(x => x.RequiresAgeVerification(It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string? thc, string? cbd) => !string.IsNullOrEmpty(thc) || !string.IsNullOrEmpty(cbd));
    }

    /// <summary>
    /// Clean up test resources.
    /// Disposes database context after each test.
    /// </summary>
    public virtual void Dispose()
    {
        DbContext?.Dispose();
    }
}
