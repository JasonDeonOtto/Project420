using Microsoft.EntityFrameworkCore;
using Project420.Management.DAL;
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
    private readonly SharedDbContext _sharedContext;

    /// <summary>
    /// Creates a new DatabaseSeeder instance.
    /// Note: PosDbContext removed in Phase 7B to break circular dependency.
    /// POS uses the same tables as Management, so no separate seeding required.
    /// </summary>
    public DatabaseSeeder(
        ManagementDbContext managementContext,
        SharedDbContext sharedContext)
    {
        _managementContext = managementContext;
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
        // NOTE: Cultivation, Production, and Inventory seeding available in separate seeder
        // See docs/SEED-DATA-PHASE-5.md for SQL scripts

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

    // NOTE: Seed data for Cultivation, Production, and Inventory modules
    // is available as SQL scripts in docs/SEED-DATA-PHASE-5.md
    // These modules cannot be seeded here due to circular dependency constraints.

    /* COMMENTED OUT - Seed code moved to separate script
    #region Cultivation Database Seeding

    private async Task SeedCultivationDataAsync()
    {
        Console.WriteLine("\n🌱 Seeding Cultivation database...");

        // Check if already seeded
        if (await _cultivationContext.Plants.AnyAsync())
        {
            Console.WriteLine("⏭️  Cultivation data already exists. Skipping...");
            return;
        }

        var now = DateTime.UtcNow;

        // 1. Create Grow Rooms
        var growRooms = new List<GrowRoom>
        {
            new GrowRoom
            {
                RoomCode = "GR-MOTHER-01",
                Name = "Mother Room 1",
                RoomType = GrowRoomType.MotherRoom,
                RoomSizeSquareMeters = 50,
                MaxCapacity = 100,
                TargetTemperature = "22-24°C",
                TargetHumidity = "60-70%",
                LightCycle = "18/6",
                Location = "Building A - Floor 1",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },
            new GrowRoom
            {
                RoomCode = "GR-VEG-01",
                Name = "Vegetative Room 1",
                RoomType = GrowRoomType.VegetativeRoom,
                RoomSizeSquareMeters = 100,
                MaxCapacity = 300,
                TargetTemperature = "22-25°C",
                TargetHumidity = "60-65%",
                LightCycle = "18/6",
                Location = "Building A - Floor 2",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            },
            new GrowRoom
            {
                RoomCode = "GR-FLOWER-01",
                Name = "Flowering Room 1",
                RoomType = GrowRoomType.FloweringRoom,
                RoomSizeSquareMeters = 150,
                MaxCapacity = 250,
                TargetTemperature = "20-24°C",
                TargetHumidity = "45-55%",
                LightCycle = "12/12",
                Location = "Building A - Floor 3",
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = now
            }
        };

        await _cultivationContext.GrowRooms.AddRangeAsync(growRooms);
        await _cultivationContext.SaveChangesAsync();

        // 2. Create Grow Cycle
        var growCycle = new GrowCycle
        {
            CycleCode = "GC-2024-001",
            Name = "Durban Poison - Batch 1",
            StrainName = "Durban Poison",
            StartDate = now.AddDays(-90), // Started 90 days ago
            PlannedHarvestDate = now.AddDays(-10),
            ActualHarvestDate = now.AddDays(-15), // Harvested 15 days ago
            EndDate = now.AddDays(-15),
            GrowRoomId = growRooms[2].Id, // Flowering Room
            TotalPlantsStarted = 20,
            PlantsHarvested = 18, // 2 plants removed (males/issues)
            TotalWetWeightGrams = 7200, // 400g wet per plant average
            TotalDryWeightGrams = 1800, // 25% dry weight (100g per plant)
            IsActive = false, // Cycle complete
            CreatedBy = "System",
            CreatedAt = now.AddDays(-90)
        };

        await _cultivationContext.GrowCycles.AddAsync(growCycle);
        await _cultivationContext.SaveChangesAsync();

        // 3. Create Harvest Batch
        var harvestBatch = new HarvestBatch
        {
            BatchNumber = "HB-2024-001",
            Name = "Durban Poison Harvest - Dec 2024",
            GrowCycleId = growCycle.Id,
            StrainName = "Durban Poison",
            HarvestDate = now.AddDays(-15),
            DryDate = now.AddDays(-8), // 7 days drying
            CureDate = now.AddDays(-1), // 7 days curing (total 14 days)
            TotalWetWeightGrams = 7200,
            TotalDryWeightGrams = 1800,
            PlantCount = 18,
            THCPercentage = "18.5",
            CBDPercentage = "0.8",
            LabTestDate = now.AddDays(-2),
            LabTestCertificateNumber = "LAB-2024-12-001",
            LabTestPassed = true,
            ProcessingStatus = "Completed",
            StorageLocation = "Drying Room - Shelf A",
            IsActive = true,
            CreatedBy = "System",
            CreatedAt = now.AddDays(-15)
        };

        await _cultivationContext.HarvestBatches.AddAsync(harvestBatch);
        await _cultivationContext.SaveChangesAsync();

        // 4. Create Sample Plants
        var plants = new List<Plant>();
        for (int i = 1; i <= 18; i++)
        {
            plants.Add(new Plant
            {
                PlantTag = $"PLT-2024-{i:D3}",
                GrowCycleId = growCycle.Id,
                StrainName = "Durban Poison",
                CurrentStage = PlantStage.Harvested,
                PlantedDate = now.AddDays(-90),
                HarvestDate = now.AddDays(-15),
                CurrentGrowRoomId = growRooms[2].Id, // Flowering Room
                PlantSource = "Clone",
                PlantSex = "Female",
                WetWeightGrams = 400,
                DryWeightGrams = 100,
                HarvestBatchId = harvestBatch.Id,
                HealthStatus = "Excellent",
                IsActive = false, // Harvested
                CreatedBy = "System",
                CreatedAt = now.AddDays(-90)
            });
        }

        await _cultivationContext.Plants.AddRangeAsync(plants);
        await _cultivationContext.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded {growRooms.Count} grow rooms");
        Console.WriteLine($"✅ Seeded 1 grow cycle");
        Console.WriteLine($"✅ Seeded 1 harvest batch");
        Console.WriteLine($"✅ Seeded {plants.Count} plants");
    }

    #endregion

    #region Production Database Seeding

    private async Task SeedProductionDataAsync()
    {
        Console.WriteLine("\n🏭 Seeding Production database...");

        // Check if already seeded
        if (await _productionContext.ProductionBatches.AnyAsync())
        {
            Console.WriteLine("⏭️  Production data already exists. Skipping...");
            return;
        }

        var now = DateTime.UtcNow;

        // 1. Create Production Batch
        var productionBatch = new ProductionBatch
        {
            BatchNumber = "PB-2024-001",
            Name = "Durban Poison - Production Batch 1",
            HarvestBatchNumber = "HB-2024-001", // Links to cultivation
            StrainName = "Durban Poison",
            StartingWeightGrams = 1800, // From harvest
            CurrentWeightGrams = 1440, // After processing (20% loss)
            FinalWeightGrams = 1440,
            WasteWeightGrams = 360, // Stems, trim, etc.
            StartDate = now.AddDays(-10),
            CompletionDate = now.AddDays(-1),
            Status = "Completed",
            QualityControlPassed = true,
            LabTestPassed = true,
            THCPercentage = "18.5",
            CBDPercentage = "0.8",
            UnitsPackaged = 144, // 10g units
            PackageSize = "10g",
            PackagingDate = now.AddDays(-1),
            IsActive = true,
            CreatedBy = "System",
            CreatedAt = now.AddDays(-10)
        };

        await _productionContext.ProductionBatches.AddAsync(productionBatch);
        await _productionContext.SaveChangesAsync();

        // 2. Create Processing Steps
        var processingSteps = new List<ProcessingStep>
        {
            new ProcessingStep
            {
                ProductionBatchId = productionBatch.Id,
                StepType = ProcessingStepType.Drying,
                StepNumber = 1,
                StartTime = now.AddDays(-10),
                EndTime = now.AddDays(-3),
                DurationHours = 168, // 7 days
                StartWeightGrams = 1800,
                EndWeightGrams = 1800,
                Temperature = "18-21°C",
                Humidity = "45-55%",
                Status = "Completed",
                PerformedBy = "Production Team",
                CreatedBy = "System",
                CreatedAt = now.AddDays(-10)
            },
            new ProcessingStep
            {
                ProductionBatchId = productionBatch.Id,
                StepType = ProcessingStepType.Curing,
                StepNumber = 2,
                StartTime = now.AddDays(-3),
                EndTime = now.AddDays(-2),
                DurationHours = 24, // 1 day (fast cure for demo)
                StartWeightGrams = 1800,
                EndWeightGrams = 1800,
                Temperature = "18-21°C",
                Humidity = "58-62%",
                Status = "Completed",
                PerformedBy = "Production Team",
                CreatedBy = "System",
                CreatedAt = now.AddDays(-3)
            },
            new ProcessingStep
            {
                ProductionBatchId = productionBatch.Id,
                StepType = ProcessingStepType.Trimming,
                StepNumber = 3,
                StartTime = now.AddDays(-2),
                EndTime = now.AddDays(-1),
                DurationHours = 8,
                StartWeightGrams = 1800,
                EndWeightGrams = 1440,
                WasteGrams = 360, // Trim waste
                Status = "Completed",
                PerformedBy = "Production Team",
                CreatedBy = "System",
                CreatedAt = now.AddDays(-2)
            },
            new ProcessingStep
            {
                ProductionBatchId = productionBatch.Id,
                StepType = ProcessingStepType.Packaging,
                StepNumber = 4,
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(-1),
                DurationHours = 4,
                StartWeightGrams = 1440,
                EndWeightGrams = 1440,
                Status = "Completed",
                PerformedBy = "Production Team",
                CreatedBy = "System",
                CreatedAt = now.AddDays(-1)
            }
        };

        await _productionContext.ProcessingSteps.AddRangeAsync(processingSteps);
        await _productionContext.SaveChangesAsync();

        // 3. Create Quality Control Checks
        var qualityChecks = new List<QualityControl>
        {
            new QualityControl
            {
                ProductionBatchId = productionBatch.Id,
                CheckType = QualityCheckType.VisualInspection,
                CheckDate = now.AddDays(-2),
                Inspector = "QC Manager",
                Passed = true,
                Results = "No mold, pests, or defects found. Excellent trichome coverage.",
                CreatedBy = "System",
                CreatedAt = now.AddDays(-2)
            },
            new QualityControl
            {
                ProductionBatchId = productionBatch.Id,
                CheckType = QualityCheckType.MoistureTest,
                CheckDate = now.AddDays(-2),
                Inspector = "QC Technician",
                Passed = true,
                Results = "Moisture content: 11.5% (Target: 10-12%)",
                CreatedBy = "System",
                CreatedAt = now.AddDays(-2)
            }
        };

        await _productionContext.QualityControls.AddRangeAsync(qualityChecks);
        await _productionContext.SaveChangesAsync();

        // 4. Create Lab Test
        var labTest = new LabTest
        {
            ProductionBatchId = productionBatch.Id,
            LabName = "SA Cannabis Testing Lab (ISO/IEC 17025 Accredited)",
            LabCertificateNumber = "ISO-17025-2024-SA-001",
            COANumber = "COA-2024-12-001",
            SampleDate = now.AddDays(-3),
            ResultsDate = now.AddDays(-2),
            THCPercentage = 18.5m,
            CBDPercentage = 0.8m,
            TotalCannabinoidsPercentage = 19.8m,
            PesticidesPassed = true,
            HeavyMetalsPassed = true,
            MicrobialPassed = true,
            OverallPass = true,
            COADocumentPath = "/docs/coa/COA-2024-12-001.pdf",
            Notes = "All tests passed. Cleared for sale.",
            CreatedBy = "System",
            CreatedAt = now.AddDays(-3)
        };

        await _productionContext.LabTests.AddAsync(labTest);
        await _productionContext.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded 1 production batch");
        Console.WriteLine($"✅ Seeded {processingSteps.Count} processing steps");
        Console.WriteLine($"✅ Seeded {qualityChecks.Count} quality checks");
        Console.WriteLine($"✅ Seeded 1 lab test (COA)");
    }

    #endregion

    #region Inventory Database Seeding

    private async Task SeedInventoryDataAsync()
    {
        Console.WriteLine("\n📦 Seeding Inventory database...");

        // Check if already seeded
        if (await _inventoryContext.StockMovements.AnyAsync())
        {
            Console.WriteLine("⏭️  Inventory data already exists. Skipping...");
            return;
        }

        var now = DateTime.UtcNow;

        // 1. Create Stock Movement (Goods Received from Production)
        var goodsReceived = new StockMovement
        {
            MovementNumber = "GRV-2024-001",
            MovementType = StockMovementType.GoodsReceived,
            MovementDate = now.AddDays(-1),
            ProductSKU = "CAN-SAT-001",
            ProductName = "Durban Poison",
            BatchNumber = "PB-2024-001", // Links to production
            Quantity = 144, // 144 x 10g units
            WeightGrams = 1440,
            FromLocation = "Production",
            ToLocation = "Warehouse - Section A",
            ReferenceNumber = "PB-2024-001",
            ReferenceType = "ProductionBatch",
            UnitCost = 90.00m,
            TotalValue = 12960.00m, // 144 * R90
            Reason = "Completed production batch received into inventory",
            CreatedBy = "System",
            CreatedAt = now.AddDays(-1)
        };

        await _inventoryContext.StockMovements.AddAsync(goodsReceived);
        await _inventoryContext.SaveChangesAsync();

        // 2. Create Stock Transfer (Warehouse to Retail Store)
        var transfer = new StockTransfer
        {
            TransferNumber = "TRF-2024-001",
            TransferDate = now,
            FromLocation = "Warehouse - Section A",
            ToLocation = "Retail Store - Main Floor",
            Status = "Completed",
            RequestedBy = "Store Manager",
            AuthorizedBy = "Warehouse Manager",
            Notes = "Initial stock for retail floor",
            CreatedBy = "System",
            CreatedAt = now
        };

        await _inventoryContext.StockTransfers.AddAsync(transfer);
        await _inventoryContext.SaveChangesAsync();

        // 3. Create Stock Movement for Transfer
        var transferMovement = new StockMovement
        {
            MovementNumber = "TRF-2024-001-OUT",
            MovementType = StockMovementType.TransferOut,
            MovementDate = now,
            ProductSKU = "CAN-SAT-001",
            ProductName = "Durban Poison",
            BatchNumber = "PB-2024-001",
            Quantity = 50, // Transfer 50 units to retail
            WeightGrams = 500,
            FromLocation = "Warehouse - Section A",
            ToLocation = "Retail Store - Main Floor",
            ReferenceNumber = "TRF-2024-001",
            ReferenceType = "StockTransfer",
            UnitCost = 90.00m,
            TotalValue = 4500.00m,
            CreatedBy = "System",
            CreatedAt = now
        };

        await _inventoryContext.StockMovements.AddAsync(transferMovement);
        await _inventoryContext.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded 2 stock movements (GRV + Transfer)");
        Console.WriteLine($"✅ Seeded 1 stock transfer");
        Console.WriteLine($"✅ Inventory ready for POS sales");
    }

    #endregion
    */
}
