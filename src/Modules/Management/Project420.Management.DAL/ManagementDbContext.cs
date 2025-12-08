using Microsoft.EntityFrameworkCore;
using Project420.Management.Models.Entities;
using Project420.Management.Models.Entities.Sales.Common;
using Project420.Management.Models.Entities.Sales.Retail;
using Project420.Management.Models.Entities.Sales.Wholesale;
using Project420.Management.Models.Entities.ProductManagement;
using Project420.Management.Models.Entities.SystemAdministration;
using Project420.Shared.Core.Entities;

namespace Project420.Management.DAL;

/// <summary>
/// Database context for the Management module.
/// Handles master data and administrative operations.
/// </summary>
/// <remarks>
/// Purpose:
/// - Customer/Debtor management (master data)
/// - Product catalog management (master data)
/// - Pricelist management (pricing strategies)
/// - User profile/HR management (employee data)
///
/// Architecture:
/// - Separate DbContext from Retail POS (module independence)
/// - Can use same database with different schema (e.g., "management" schema)
/// - Or separate database entirely for data isolation
///
/// Benefits of Separate DbContext:
/// - Module independence: Management can be deployed separately
/// - Schema isolation: Management data separate from operational data
/// - Performance: Optimize each module's database independently
/// - Security: Different connection strings with different permissions
///
/// Cannabis Compliance:
/// - Product management with THC/CBD tracking
/// - License/permit tracking (via customer/employee records)
/// - Audit trails for all master data changes
///
/// POPIA Compliance:
/// - All entities inherit from AuditableEntity (audit trails)
/// - Soft deletes (7-year retention for customer/employee records)
/// - Automatic audit field population via SaveChangesAsync override
/// </remarks>
public class ManagementDbContext : DbContext
{
    /// <summary>
    /// Constructor that accepts configuration options (connection string, etc.)
    /// </summary>
    /// <param name="options">Configuration options for the DbContext</param>
    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
    {
    }

    // ========================================
    // DbSet Properties (Management Tables)
    // ========================================

    /// <summary>
    /// Debtors (Customers) table - Master customer/debtor management
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Customer account management
    /// - Credit management (limits, balances)
    /// - Age verification (18+ for cannabis)
    /// - Medical license tracking (Section 21 permits)
    /// - POPIA-compliant PII storage
    ///
    /// Referenced by:
    /// - Retail POS (transactions, sales)
    /// - Reports and analytics
    /// </remarks>
    public DbSet<Debtor> Debtors { get; set; } = null!;

    /// Referenced by:
    /// Debtors
    /// </remarks>
    public DbSet<DebtorCategory> DebtorCategories { get; set; } = null!;

    /// <summary>
    /// Products table - Master product catalog
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Product catalog management
    /// - Cannabis compliance (THC/CBD, batch tracking)
    /// - Pricing management (base prices)
    /// - Inventory master data
    ///
    /// Referenced by:
    /// - Retail POS (sales transactions)
    /// - Pricelists (pricing strategies)
    /// - Inventory management
    /// </remarks>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Retail pricelists table - Pricing strategy management
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Multiple pricing strategies (retail, wholesale, VIP, medical)
    /// - Promotional pricing
    /// - Time-based pricing
    /// - Customer-specific pricing
    ///
    /// Referenced by:
    /// - Retail POS (applies prices at sale time)
    /// - Customer management (default pricelists)
    /// </remarks>
    public DbSet<RetailPricelist> RetailPricelists { get; set; } = null!;

    /// <summary>
    /// User profiles table - Employee/HR management
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Employee HR data (beyond basic authentication)
    /// - Cannabis license/certification tracking
    /// - Background check compliance
    /// - Department/role management
    ///
    /// Note: Authentication User entity is in Shared.Core
    /// UserProfile extends User with HR/administrative data
    /// </remarks>
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    /// <summary>
    /// Users table - Authentication and user identity (COMPANY-SPECIFIC)
    /// </summary>
    /// <remarks>
    /// Each company database has its own Users table.
    /// Users belong to a specific company.
    /// Authentication happens against this table after company selection.
    ///
    /// Multi-Tenant Architecture:
    /// - User per company (separate accounts for data isolation)
    /// - John at Company A = different user than John at Company B
    /// - Complete security and compliance isolation per company
    ///
    /// Security:
    /// - Passwords are hashed (never plain text)
    /// - Failed login tracking
    /// - Account locking after 5 failed attempts
    ///
    /// POPIA Compliance:
    /// - Email/PII encrypted at rest (production)
    /// - 3-year retention after termination
    /// </remarks>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// User permissions table - Granular permission assignments
    /// </summary>
    /// <remarks>
    /// Company-specific user permissions.
    /// Phase 2: Table exists but not used (permissions hardcoded by role)
    /// Phase 4: Stores user-specific permission grants/denies
    /// </remarks>
    public DbSet<UserPermission> UserPermissions { get; set; } = null!;

    /// <summary>
    /// Retail pricelist items table - Individual product prices within retail pricelists
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Linking products to retail pricelists with specific prices
    /// - Quantity-based pricing (min/max quantities)
    /// - Time-based pricing (effective dates)
    /// </remarks>
    public DbSet<RetailPricelistItem> RetailPricelistItems { get; set; } = null!;

    /// <summary>
    /// Wholesale pricelists table - Wholesale pricing strategy management
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Wholesale customer pricing strategies
    /// - Bulk discount pricing
    /// - Trade/reseller pricing
    /// </remarks>
    public DbSet<WholesalePricelist> WholesalePricelists { get; set; } = null!;

    /// <summary>
    /// Wholesale pricelist items table - Individual product prices within wholesale pricelists
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Linking products to wholesale pricelists with specific prices
    /// - Quantity-based pricing (min/max quantities)
    /// - Bulk discount tiers
    /// </remarks>
    public DbSet<WholesalePricelistItem> WholesalePricelistItems { get; set; } = null!;

    /// <summary>
    /// Product categories table - Cannabis product categorization
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Organizing products by type (Flower, Edibles, Oils, etc.)
    /// - Category-specific rules and compliance
    /// - Inventory management and reporting
    /// </remarks>
    public DbSet<ProductCategory> ProductCategories { get; set; } = null!;

    /// <summary>
    /// Stations table - Terminal configuration (company-specific)
    /// </summary>
    /// <remarks>
    /// Business-specific terminal configuration for THIS company.
    /// Each company configures their own stations independently.
    ///
    /// Phase 1: Basic POS info (Name, Type, Location, Department)
    /// Phase 2: Operational config (DefaultPricelist, OfflineMode, ReceiptTemplate)
    /// Phase 3: Advanced features (ShiftManagement, CashFloat, MaxDiscount)
    ///
    /// Multi-Tenant Context:
    /// - Same physical terminal (POS-STORE-01) has different Station records per company
    /// - StationConnection (Shared DB) determines access
    /// - Station (Business DB) determines configuration
    /// </remarks>
    public DbSet<Station> Stations { get; set; } = null!;

    /// <summary>
    /// Station peripherals table - Peripheral device configuration
    /// </summary>
    /// <remarks>
    /// Devices attached to stations:
    /// - Printers (receipt, label)
    /// - Scanners (barcode, QR)
    /// - Cash drawers
    /// - Card readers
    /// - Scales (weight verification)
    ///
    /// Phase 1: Basic structure (Type, Model, ConnectionType)
    /// Phase 2: Connection details (IP, Port, Settings)
    /// Phase 3: Monitoring (Status, Error logging, Auto-recovery)
    /// </remarks>
    public DbSet<StationPeripheral> StationPeripherals { get; set; } = null!;

    // ========================================
    // OnModelCreating (Fluent API Configuration)
    // ========================================

    /// <summary>
    /// Configures the Management database schema using Fluent API
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: Use schema for logical separation in same database
        // modelBuilder.HasDefaultSchema("management");

        // ===========================
        // DEBTOR CONFIGURATION
        // ===========================
        modelBuilder.Entity<Debtor>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(d => d.AccountNumber)
                .IsUnique()
                .HasDatabaseName("IX_Debtors_AccountNumber");

            entity.HasIndex(d => d.Email)
                .HasDatabaseName("IX_Debtors_Email");

            entity.HasIndex(d => d.DateOfBirth)
                .HasDatabaseName("IX_Debtors_DateOfBirth");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        // ===========================
        // PRODUCT CONFIGURATION
        // ===========================
        modelBuilder.Entity<Product>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(p => p.SKU)
                .IsUnique()
                .HasDatabaseName("IX_Products_SKU");

            entity.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            entity.HasIndex(p => p.BatchNumber)
                .HasDatabaseName("IX_Products_BatchNumber");

            entity.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Products_IsActive");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===========================
        // PRICELIST CONFIGURATION
        // ===========================
        modelBuilder.Entity<RetailPricelist>(entity =>
        {
            // Keep existing table name for backwards compatibility
            entity.ToTable("RetailPricelists");

            // Indexes for performance
            entity.HasIndex(p => p.Code)
                .HasDatabaseName("IX_RetailPricelists_Code");

            entity.HasIndex(p => p.IsDefault)
                .HasDatabaseName("IX_RetailPricelists_IsDefault");

            entity.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_RetailPricelists_IsActive");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===========================
        // USERPROFILE CONFIGURATION
        // ===========================
        modelBuilder.Entity<UserProfile>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(up => up.EmployeeNumber)
                .IsUnique()
                .HasDatabaseName("IX_UserProfiles_EmployeeNumber");

            entity.HasIndex(up => up.UserId)
                .IsUnique()
                .HasDatabaseName("IX_UserProfiles_UserId");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(up => !up.IsDeleted);

            // Note: Relationship to User (in Shared.Core) will be configured
            // when User entity is defined. For now, just UserId foreign key.
        });

        // =====================================
        // RETAIL PRICELISTITEM CONFIGURATION
        // =====================================
        modelBuilder.Entity<RetailPricelistItem>(entity =>
        {
            // Composite index for pricelist + product lookup
            entity.HasIndex(pi => new { pi.RetailPricelistId, pi.ProductId })
                .HasDatabaseName("IX_RetailPricelistItems_PricelistId_ProductId");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(pi => !pi.IsDeleted);

            // Navigation property to RetailPricelist
            entity.HasOne(pi => pi.RetailPricelist)
                .WithMany(p => p.PricelistItems)
                .HasForeignKey(pi => pi.RetailPricelistId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        });

        // ========================================
        // WHOLESALE PRICELISTITEM CONFIGURATION
        // ========================================
        modelBuilder.Entity<WholesalePricelistItem>(entity =>
        {
            // Composite index for pricelist + product lookup
            entity.HasIndex(pi => new { pi.WholesalePricelistId, pi.ProductId })
                .HasDatabaseName("IX_WholesalePricelistItems_PricelistId_ProductId");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(pi => !pi.IsDeleted);

            // Navigation property to WholesalePricelist
            entity.HasOne(pi => pi.WholesalePricelist)
                .WithMany(p => p.WholesalePricelistItems)
                .HasForeignKey(pi => pi.WholesalePricelistId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        });

        // ===========================
        // USER CONFIGURATION
        // ===========================
        modelBuilder.Entity<User>(entity =>
        {
            // Indexes for performance and uniqueness
            entity.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username");

            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            entity.HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_Users_IsActive");

            entity.HasIndex(u => u.Role)
                .HasDatabaseName("IX_Users_Role");

            entity.HasIndex(u => u.LastLoginAt)
                .HasDatabaseName("IX_Users_LastLogin");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(u => !u.IsDeleted);

            // Navigation property configuration (UserPermissions)
            entity.HasMany(u => u.UserPermissions)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship to UserProfile (one-to-one)
            entity.HasOne<UserProfile>()
                .WithOne(up => up.User)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===========================
            // SEED DATA - Default Users
            // ===========================
            // Password for all seed users: "Project420!Pass"
            // In production, these should be changed immediately after first login
            const string seedPasswordHash = "$2a$11$EK5kC8qGqJH5rZvK.YjJGuH7Z6uYF6NjN9XoYdQdH3xRZLqKxhE/G";
            var seedDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

            entity.HasData(
                // 1. SuperAdmin - Full system access
                new User
                {
                    Id = 1,
                    Username = "superadmin",
                    Email = "superadmin@project420.local",
                    PasswordHash = seedPasswordHash,
                    FirstName = "Super",
                    LastName = "Admin",
                    Role = "SuperAdmin",
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    TwoFactorEnabled = false,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                },

                // 2. Admin - Administrative access
                new User
                {
                    Id = 2,
                    Username = "admin",
                    Email = "admin@project420.local",
                    PasswordHash = seedPasswordHash,
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    TwoFactorEnabled = false,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                },

                // 3. Manager - Store management
                new User
                {
                    Id = 3,
                    Username = "manager",
                    Email = "manager@project420.local",
                    PasswordHash = seedPasswordHash,
                    FirstName = "Store",
                    LastName = "Manager",
                    Role = "Manager",
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    TwoFactorEnabled = false,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                },

                // 4. Cashier - POS operations
                new User
                {
                    Id = 4,
                    Username = "cashier",
                    Email = "cashier@project420.local",
                    PasswordHash = seedPasswordHash,
                    FirstName = "POS",
                    LastName = "Cashier",
                    Role = "Cashier",
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    TwoFactorEnabled = false,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                },

                // 5. Inventory User - Inventory management
                new User
                {
                    Id = 5,
                    Username = "inventory",
                    Email = "inventory@project420.local",
                    PasswordHash = seedPasswordHash,
                    FirstName = "Inventory",
                    LastName = "Manager",
                    Role = "Inventory",
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    TwoFactorEnabled = false,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                }
            );
        });

        // ===========================
        // USERPERMISSION CONFIGURATION
        // ===========================
        modelBuilder.Entity<UserPermission>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(up => up.UserId)
                .HasDatabaseName("IX_UserPermissions_UserId");

            entity.HasIndex(up => up.Permission)
                .HasDatabaseName("IX_UserPermissions_Permission");

            entity.HasIndex(up => new { up.UserId, up.Permission })
                .IsUnique()
                .HasDatabaseName("IX_UserPermissions_UserId_Permission");

            entity.HasIndex(up => up.IsActive)
                .HasDatabaseName("IX_UserPermissions_IsActive");

            entity.HasIndex(up => up.ExpiresAt)
                .HasDatabaseName("IX_UserPermissions_ExpiresAt");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(up => !up.IsDeleted);

            // Relationship configuration already defined in User entity
        });

        // ===========================
        // STATION CONFIGURATION
        // ===========================
        modelBuilder.Entity<Station>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(s => s.Name)
                .HasDatabaseName("IX_Stations_Name");

            entity.HasIndex(s => s.StationType)
                .HasDatabaseName("IX_Stations_StationType");

            entity.HasIndex(s => s.Location)
                .HasDatabaseName("IX_Stations_Location");

            entity.HasIndex(s => s.Department)
                .HasDatabaseName("IX_Stations_Department");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(s => !s.IsDeleted);

            // Relationship to peripherals
            entity.HasMany(s => s.Peripherals)
                .WithOne(sp => sp.Station)
                .HasForeignKey(sp => sp.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===========================
        // STATIONPERIPHERAL CONFIGURATION
        // ===========================
        modelBuilder.Entity<StationPeripheral>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(sp => sp.StationId)
                .HasDatabaseName("IX_StationPeripherals_StationId");

            entity.HasIndex(sp => sp.PeripheralType)
                .HasDatabaseName("IX_StationPeripherals_PeripheralType");

            entity.HasIndex(sp => sp.Model)
                .HasDatabaseName("IX_StationPeripherals_Model");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(sp => !sp.IsDeleted);

            // Relationship configuration already defined in Station entity
        });
    }

    // ========================================
    // SaveChangesAsync Override (Audit Trail)
    // ========================================

    /// <summary>
    /// Overrides SaveChangesAsync to automatically populate audit fields.
    /// This ensures POPIA compliance by tracking who created/modified/deleted records and when.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all entities that inherit from AuditableEntity
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Set creation audit fields
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = GetCurrentUser();
                    break;

                case EntityState.Modified:
                    // Set modification audit fields
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = GetCurrentUser();

                    // If soft deleting (IsDeleted changed to true)
                    if (entry.Entity.IsDeleted && entry.OriginalValues.GetValue<bool>(nameof(AuditableEntity.IsDeleted)) == false)
                    {
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.DeletedBy = GetCurrentUser();
                    }
                    break;
            }
        }

        // Call base SaveChangesAsync to persist to database
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the current user for audit trail purposes.
    /// </summary>
    /// <returns>Current user identifier</returns>
    /// <remarks>
    /// TODO: Replace with actual user context from authentication system.
    /// This could come from:
    /// - IHttpContextAccessor for web apps
    /// - IIdentityService for custom auth
    /// - Thread.CurrentPrincipal for legacy apps
    /// </remarks>
    private string GetCurrentUser()
    {
        // TODO: Implement actual user retrieval
        // For now, return SYSTEM as placeholder
        return "SYSTEM";
    }
}
