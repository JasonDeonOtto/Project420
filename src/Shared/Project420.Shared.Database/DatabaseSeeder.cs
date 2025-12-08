using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL;
using Project420.Retail.POS.DAL;
using Project420.Management.Models.Entities.Sales.Common;
using Project420.Management.Models.Entities.Sales.Retail;
using Project420.Management.Models.Entities.ProductManagement;
using Project420.Management.Models.Entities.SystemAdministration;

namespace Project420.Shared.Database;

/// <summary>
/// Seeds the database with initial test data for development and testing.
/// SEEDING STRATEGY: All seeding happens through ManagementDbContext only.
/// Management and POS share the same database tables, so seeding once populates both.
/// </summary>
public class DatabaseSeeder
{
    private readonly ManagementDbContext _managementContext;
    private readonly PosDbContext _posContext;
    private readonly SharedDbContext _sharedContext;

    public DatabaseSeeder(
        ManagementDbContext managementContext,
        PosDbContext posContext,
        SharedDbContext sharedContext)
    {
        _managementContext = managementContext;
        _posContext = posContext;
        _sharedContext = sharedContext;
    }

    /// <summary>
    /// Seeds all databases with test data.
    /// Safe to run multiple times - checks for existing data first.
    /// </summary>
    public async Task SeedAllAsync()
    {
        Console.WriteLine("🌱 Starting database seeding...");

        await SeedManagementDataAsync();
        await SeedPosDataAsync();
        await SeedSharedDataAsync();

        Console.WriteLine("✅ Database seeding complete!");
    }

    #region Management Database Seeding

    private async Task SeedManagementDataAsync()
    {
        Console.WriteLine("\n📦 Seeding Management database...");

        // Check if already seeded
        if (await _managementContext.Products.AnyAsync())
        {
            Console.WriteLine("⏭️  Management data already exists. Skipping...");
            return;
        }

        var products = CreateSampleProducts();
        var debtors = CreateSampleDebtors();
        var pricelist = CreateDefaultPricelist();

        await _managementContext.Products.AddRangeAsync(products);
        await _managementContext.Debtors.AddRangeAsync(debtors);
        await _managementContext.RetailPricelists.AddAsync(pricelist);
        await _managementContext.SaveChangesAsync();

        // NOTE: Pricelist items will be created from the UI as per user request
        // var pricelistItems = CreatePricelistItems(pricelist.Id, products);
        // await _managementContext.PricelistItems.AddRangeAsync(pricelistItems);
        // await _managementContext.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded {products.Count} products");
        Console.WriteLine($"✅ Seeded {debtors.Count} customers");
        Console.WriteLine($"✅ Seeded 1 pricelist (items will be created from UI)");
        Console.WriteLine("ℹ️  Note: UserProfile seeding requires User table in Shared database");
    }

    private List<Product> CreateSampleProducts()
    {
        var now = DateTime.UtcNow;
        var products = new List<Product>
        {
            // Sativa Strains
            new Product
            {
                SKU = "CAN-SAT-001",
                Name = "Durban Poison",
                Description = "Pure South African sativa. Energizing and uplifting with sweet, earthy aroma.",
                StrainName = "Durban Poison",
                THCPercentage = "18.5",
                CBDPercentage = "0.8",
                BatchNumber = "DP-2025-001",
                LabTestDate = now.AddDays(-10),
                ExpiryDate = now.AddMonths(6),
                Price = 180.00m,
                CostPrice = 90.00m,
                StockOnHand = 250,
                ReorderLevel = 50,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },
            new Product
            {
                SKU = "CAN-SAT-002",
                Name = "Jack Herer",
                Description = "Award-winning sativa. Creative and euphoric effects with pine and spice notes.",
                StrainName = "Jack Herer",
                THCPercentage = "20.2",
                CBDPercentage = "0.5",
                BatchNumber = "JH-2025-001",
                LabTestDate = now.AddDays(-8),
                ExpiryDate = now.AddMonths(6),
                Price = 200.00m,
                CostPrice = 100.00m,
                StockOnHand = 180,
                ReorderLevel = 40,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Indica Strains
            new Product
            {
                SKU = "CAN-IND-001",
                Name = "Northern Lights",
                Description = "Classic indica. Relaxing body high with sweet and spicy aroma.",
                StrainName = "Northern Lights",
                THCPercentage = "19.8",
                CBDPercentage = "1.2",
                BatchNumber = "NL-2025-001",
                LabTestDate = now.AddDays(-12),
                ExpiryDate = now.AddMonths(6),
                Price = 190.00m,
                CostPrice = 95.00m,
                StockOnHand = 200,
                ReorderLevel = 45,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },
            new Product
            {
                SKU = "CAN-IND-002",
                Name = "Granddaddy Purple",
                Description = "Premium indica. Deep relaxation with grape and berry flavors.",
                StrainName = "Granddaddy Purple",
                THCPercentage = "22.5",
                CBDPercentage = "0.6",
                BatchNumber = "GDP-2025-001",
                LabTestDate = now.AddDays(-7),
                ExpiryDate = now.AddMonths(6),
                Price = 220.00m,
                CostPrice = 110.00m,
                StockOnHand = 150,
                ReorderLevel = 35,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Hybrid Strains
            new Product
            {
                SKU = "CAN-HYB-001",
                Name = "Blue Dream",
                Description = "Balanced hybrid. Creative and relaxing with blueberry notes.",
                StrainName = "Blue Dream",
                THCPercentage = "17.5",
                CBDPercentage = "2.0",
                BatchNumber = "BD-2025-001",
                LabTestDate = now.AddDays(-9),
                ExpiryDate = now.AddMonths(6),
                Price = 195.00m,
                CostPrice = 97.50m,
                StockOnHand = 220,
                ReorderLevel = 50,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // CBD Products
            new Product
            {
                SKU = "CAN-CBD-001",
                Name = "Charlotte's Web",
                Description = "High-CBD strain for medical use. Minimal psychoactive effects.",
                StrainName = "Charlotte's Web",
                THCPercentage = "0.9",
                CBDPercentage = "15.5",
                BatchNumber = "CW-2025-001",
                LabTestDate = now.AddDays(-5),
                ExpiryDate = now.AddMonths(6),
                Price = 250.00m,
                CostPrice = 125.00m,
                StockOnHand = 100,
                ReorderLevel = 25,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Pre-rolls
            new Product
            {
                SKU = "CAN-PR-001",
                Name = "Durban Poison Pre-Roll (1g)",
                Description = "Convenient pre-rolled joint. Same quality as our loose flower.",
                StrainName = "Durban Poison",
                THCPercentage = "18.5",
                CBDPercentage = "0.8",
                BatchNumber = "DP-PR-2025-001",
                LabTestDate = now.AddDays(-10),
                ExpiryDate = now.AddMonths(3),
                Price = 50.00m,
                CostPrice = 25.00m,
                StockOnHand = 500,
                ReorderLevel = 100,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Accessories (non-cannabis)
            new Product
            {
                SKU = "ACC-GRN-001",
                Name = "Premium Herb Grinder",
                Description = "4-piece aluminum grinder with kief catcher.",
                StrainName = null,
                THCPercentage = null,
                CBDPercentage = null,
                BatchNumber = null,
                LabTestDate = null,
                ExpiryDate = null,
                Price = 150.00m,
                CostPrice = 60.00m,
                StockOnHand = 75,
                ReorderLevel = 20,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },
            new Product
            {
                SKU = "ACC-PAP-001",
                Name = "Rolling Papers (King Size)",
                Description = "Natural, unbleached rolling papers. 32 leaves per pack.",
                StrainName = null,
                THCPercentage = null,
                CBDPercentage = null,
                BatchNumber = null,
                LabTestDate = null,
                ExpiryDate = null,
                Price = 25.00m,
                CostPrice = 10.00m,
                StockOnHand = 300,
                ReorderLevel = 75,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },
            new Product
            {
                SKU = "ACC-JAR-001",
                Name = "Airtight Storage Jar (250ml)",
                Description = "UV-resistant glass storage jar. Keeps flower fresh.",
                StrainName = null,
                THCPercentage = null,
                CBDPercentage = null,
                BatchNumber = null,
                LabTestDate = null,
                ExpiryDate = null,
                Price = 120.00m,
                CostPrice = 50.00m,
                StockOnHand = 60,
                ReorderLevel = 15,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            }
        };

        return products;
    }

    private List<Debtor> CreateSampleDebtors()
    {
        var now = DateTime.UtcNow;
        var debtors = new List<Debtor>
        {
            // Walk-in customer (minimal info)
            new Debtor
            {
                AccountNumber = "CUST-001",
                Name = "Walk-In Customer",
                CategoryID = 1, // TODO: Create DebtorCategory seeding (1 = Individual, 2 = Business)
                Email = null,
                PhoneNumber = null,
                PhysicalAddress = null,
                IdNumber = null,
                DateOfBirth = null,
                AgeVerified = true,
                AgeVerificationDate = now,
                HasMedicalLicense = false,
                MedicalLicenseNumber = null,
                LicenseExpiryDate = null,
                CreditLimit = 0,
                CurrentBalance = 0,
                PaymentTermsDays = 0,
                Notes = "Walk-in cash customer",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Regular customer with account
            new Debtor
            {
                AccountNumber = "CUST-002",
                Name = "Sipho Mthembu",
                CategoryID = 1, // Individual customer
                Email = "sipho.mthembu@example.co.za",
                PhoneNumber = "+27 82 456 7890",
                PhysicalAddress = "123 Main Road, Durban, KZN, 4001",
                IdNumber = "8505155678089",
                DateOfBirth = new DateTime(1985, 5, 15),
                AgeVerified = true,
                AgeVerificationDate = now.AddMonths(-2),
                HasMedicalLicense = false,
                MedicalLicenseNumber = null,
                LicenseExpiryDate = null,
                CreditLimit = 5000.00m,
                CurrentBalance = 1250.50m,
                PaymentTermsDays = 30,
                Notes = "Regular customer with account",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Medical customer with Section 21 permit
            new Debtor
            {
                AccountNumber = "CUST-003",
                Name = "Dr. Thandi Nkosi",
                CategoryID = 1, // Individual customer (medical)
                Email = "t.nkosi@medmail.co.za",
                PhoneNumber = "+27 83 789 1234",
                PhysicalAddress = "45 Health Street, Cape Town, Western Cape, 8001",
                IdNumber = "7803201234567",
                DateOfBirth = new DateTime(1978, 3, 20),
                AgeVerified = true,
                AgeVerificationDate = now.AddMonths(-6),
                HasMedicalLicense = true,
                MedicalLicenseNumber = "SAHPRA-S21-2024-12345",
                LicenseExpiryDate = now.AddYears(1),
                CreditLimit = 10000.00m,
                CurrentBalance = 3500.00m,
                PaymentTermsDays = 60,
                Notes = "Medical customer with Section 21 permit",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },

            // Business customer (dispensary/clinic)
            new Debtor
            {
                AccountNumber = "CUST-B001",
                Name = "Wellness Clinic JHB",
                CategoryID = 2, // Business customer
                Email = "accounts@wellnessclinic.co.za",
                PhoneNumber = "+27 11 456 7890",
                PhysicalAddress = "789 Business Park, Sandton, Gauteng, 2196",
                IdNumber = null,
                DateOfBirth = null,
                AgeVerified = true,
                AgeVerificationDate = now.AddMonths(-12),
                HasMedicalLicense = true,
                MedicalLicenseNumber = "SAHPRA-CLINIC-2023-98765",
                LicenseExpiryDate = now.AddYears(2),
                CreditLimit = 50000.00m,
                CurrentBalance = 12300.00m,
                PaymentTermsDays = 45,
                Notes = "Business customer - Contact: John van der Merwe",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            }
        };

        return debtors;
    }

    private RetailPricelist CreateDefaultPricelist()
    {
        var now = DateTime.UtcNow;
        return new RetailPricelist
        {
            Name = "Standard Retail",
            Description = "Default retail pricing for walk-in customers",
            Code = "STD-RETAIL",
            EffectiveFrom = now.AddMonths(-1),
            EffectiveTo = null,
            IsDefault = true,
            Priority = 1,
            PricingStrategy = "Fixed",
            PercentageAdjustment = null,
            IsActive = true,
            CreatedBy = "System",
            CreatedAt = now
        };
    }

    private UserProfile CreateTestUserProfile()
    {
        var now = DateTime.UtcNow;
        return new UserProfile
        {
            UserId = 1, // Reference to User.Id (assumed to exist in Shared.Core User table)
            EmployeeNumber = "EMP-0001",
            IdNumber = "8001015800089", // Test SA ID number
            PhoneNumber = "+27 82 000 0000",
            Department = "Management",
            Position = "System Administrator",
            EmploymentStartDate = now.AddYears(-1),
            EmploymentEndDate = null, // Currently employed
            BackgroundCheckStatus = "Cleared",
            BackgroundCheckDate = now.AddMonths(-1),
            BackgroundCheckExpiryDate = now.AddMonths(11),
            ComplianceTrainingDate = now.AddMonths(-1),
            ComplianceTrainingExpiryDate = now.AddMonths(11),
            CreatedBy = "System",
            CreatedAt = now
        };
    }

    #endregion

    #region POS Database Seeding

    private async Task SeedPosDataAsync()
    {
        Console.WriteLine("\n🛒 Seeding POS database...");

        // NOTE: POS and Management use shared data from the same Project420_Dev database
        // No separate seeding needed - POS will reference the same tables
        Console.WriteLine("ℹ️  POS shares data with Management database. No separate seeding required.");
    }

    #endregion

    #region Shared Database Seeding

    private async Task SeedSharedDataAsync()
    {
        Console.WriteLine("\n🔧 Seeding Shared database...");

        // Shared database is for logs and audit trails
        // These are populated at runtime, no seed data needed
        Console.WriteLine("ℹ️  Shared database (logs/audits) populated at runtime. No seed data required.");
    }

    #endregion
}
