using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.DAL;

/// <summary>
/// Database context for the Point of Sale (POS) module.
/// Also serves as the business database context for unified transaction and movement tables.
/// </summary>
/// <remarks>
/// POPIA Compliance: This context includes audit trail support and soft delete patterns.
/// Cannabis Compliance: Manages THC/CBD tracking, batch numbers, and age verification data.
///
/// Architecture:
/// - Implements IBusinessDbContext to allow services in Shared.Database to access business tables
/// - Contains unified tables (TransactionDetails, Movements, SerialNumbers, etc.)
/// - Module-specific tables (RetailTransactionHeaders, Payments, ProductBarcodes)
/// - Referenced master data tables (Products, Debtors, Pricelists) excluded from migrations
/// </remarks>
public class PosDbContext : DbContext, IBusinessDbContext
{
    /// <summary>
    /// Constructor that accepts configuration options (connection string, etc.)
    /// </summary>
    /// <param name="options">Configuration options for the DbContext</param>
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    // ========================================
    // STEP 1: DbSet Properties (Database Tables)
    // ========================================

    // ===========================
    // MASTER DATA (REFERENCED, NOT OWNED)
    // ===========================
    // These tables are OWNED by ManagementDbContext.
    // PosDbContext only references them for navigation properties.
    // They are excluded from PosDbContext migrations.

    /// <summary>
    /// Products table - REFERENCED (owned by ManagementDbContext).
    /// Used for navigation properties in TransactionDetails.
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Debtors (Customers) table - REFERENCED (owned by ManagementDbContext).
    /// Used for navigation properties in TransactionHeaders and Payments.
    /// </summary>
    public DbSet<Debtor> Debtors { get; set; } = null!;

    /// <summary>
    /// Pricelists table - REFERENCED (owned by ManagementDbContext).
    /// Used for navigation properties in TransactionHeaders.
    /// </summary>
    public DbSet<Pricelist> Pricelists { get; set; } = null!;

    // ===========================
    // POS OPERATIONAL DATA (OWNED)
    // ===========================
    // These tables are OWNED and MANAGED by PosDbContext.

    /// <summary>
    /// RetailTransactionHeaders table - Retail invoice/receipt headers (sales, refunds, quotes, account payments).
    /// OWNED by PosDbContext.
    /// RENAMED from POSTransactionHeaders to align with unified transaction architecture (Phase 7B).
    /// </summary>
    public DbSet<RetailTransactionHeader> RetailTransactionHeaders { get; set; } = null!;

    // NOTE: POSTransactionDetails DbSet removed in Phase 7B.
    // Transaction details are now stored in SharedDbContext.TransactionDetails
    // using the unified TransactionDetail entity with HeaderId + TransactionType discriminator.
    // See SharedDbContext.TransactionDetails for the unified table.

    /// <summary>
    /// Payments table - Payment records (cash, card, EFT, etc.) with PCI-DSS compliance.
    /// OWNED by PosDbContext.
    /// </summary>
    public DbSet<Payment> Payments { get; set; } = null!;

    /// <summary>
    /// ProductBarcodes table - Barcode associations for products (standard EAN/UPC + unique serial numbers).
    /// OWNED by PosDbContext.
    /// </summary>
    /// <remarks>
    /// Supports:
    /// - Standard product barcodes (multiple items share one barcode)
    /// - Unique serial numbers (individual item tracking for cannabis compliance)
    /// </remarks>
    public DbSet<ProductBarcode> ProductBarcodes { get; set; } = null!;

    // ===========================
    // UNIFIED TRANSACTION DATA (OWNED)
    // ===========================
    // These tables are shared across all modules but owned by PosDbContext
    // because they require FKs to Products and other business entities.
    // Services in Shared.Database inject PosDbContext to access these.

    /// <summary>
    /// Unified transaction details table - All transaction line items stored here.
    /// </summary>
    /// <remarks>
    /// Movement Architecture (Option A):
    /// - Single table for all transaction types (Sales, GRVs, Refunds, Production, etc.)
    /// - TransactionType enum acts as discriminator to identify header table
    /// - Each detail generates corresponding Movement record(s)
    ///
    /// Benefits:
    /// - Consistent movement generation from all transaction types
    /// - Simplified reporting and queries across transaction types
    /// - Single source for line item data (DRY principle)
    ///
    /// Cannabis Compliance:
    /// - Batch number tracking for seed-to-sale traceability
    /// - Serial number tracking for individual unit traceability
    /// - Full audit trail via AuditableEntity
    /// </remarks>
    public DbSet<TransactionDetail> TransactionDetails { get; set; } = null!;

    /// <summary>
    /// Movement ledger table - Source of truth for SOH calculations.
    /// </summary>
    /// <remarks>
    /// Movement Architecture (Option A):
    /// - SOH = SUM(Quantity WHERE Direction = IN) - SUM(Quantity WHERE Direction = OUT)
    /// - SOH is NEVER stored directly - always calculated from movements
    /// - Movements are immutable once created (soft delete only)
    ///
    /// Relationship:
    /// - Each TransactionDetail generates Movement record(s)
    /// - Movement.DetailId → TransactionDetail.Id
    /// - Movement.HeaderId → Transaction header tables
    ///
    /// Performance:
    /// - Indexed on ProductId + TransactionDate for SOH queries
    /// - Indexed on BatchNumber and SerialNumber for traceability
    /// - Global query filter excludes soft-deleted records
    ///
    /// Cannabis Compliance:
    /// - Full audit trail for SAHPRA/SARS
    /// - Batch/serial tracking for seed-to-sale
    /// - Weight tracking for reconciliation
    /// </remarks>
    public DbSet<Movement> Movements { get; set; } = null!;

    // ===========================
    // BATCH & SERIAL NUMBER SYSTEM (OWNED)
    // ===========================
    // Phase 8: Enterprise-grade batch and serial number generation.
    // These tables require FKs to Products for validation.

    /// <summary>
    /// Batch number sequences table - Tracks daily sequences per site/type/date.
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Generate unique 16-digit batch numbers: SSTTYYYYMMDDNNNN
    /// - Each combination of Site+Type+Date has its own sequence
    /// - Sequences reset daily (NNNN starts at 0001 each day)
    ///
    /// Cannabis Compliance:
    /// - SAHPRA seed-to-sale batch traceability
    /// - Unique batch identification for audit trail
    /// </remarks>
    public DbSet<BatchNumberSequence> BatchNumberSequences { get; set; } = null!;

    /// <summary>
    /// Serial number sequences table - Tracks unit and daily sequences.
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Unit sequences: Per-batch unit numbering (UUUUU in full SN)
    /// - Daily sequences: Per-site daily numbering (NNNNN in short SN)
    ///
    /// Cannabis Compliance:
    /// - Unit-level traceability for SAHPRA requirements
    /// - Supports both full SN (31 digits) and short SN (13 digits)
    /// </remarks>
    public DbSet<SerialNumberSequence> SerialNumberSequences { get; set; } = null!;

    /// <summary>
    /// Serial numbers table - Master record of all generated serial numbers.
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Maps Short SN ↔ Full SN (one-to-one)
    /// - Tracks serial number lifecycle (Created → Assigned → Sold → Destroyed)
    /// - Provides traceability queries by batch, strain, date, status
    ///
    /// Serial Number Formats:
    /// - Full SN (31 digits): All product info embedded (site, strain, batch, unit, weight)
    /// - Short SN (13 digits): Compact format for barcodes
    ///
    /// Cannabis Compliance:
    /// - SAHPRA unit-level seed-to-sale traceability
    /// - Recall management capability
    /// - Destruction documentation
    /// </remarks>
    public DbSet<SerialNumber> SerialNumbers { get; set; } = null!;

    // ========================================
    // STEP 2: OnModelCreating (Fluent API Configuration)
    // ========================================

    /// <summary>
    /// Configures the database schema using Fluent API.
    /// This method is called when the model is being created.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================
        // MASTER DATA TABLES (REFERENCED, NOT OWNED)
        // ========================================
        // These tables are owned by ManagementDbContext.
        // We reference them here for navigation properties but exclude from migrations.

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", t => t.ExcludeFromMigrations());
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<Debtor>(entity =>
        {
            entity.ToTable("Debtors", t => t.ExcludeFromMigrations());
            entity.HasQueryFilter(d => !d.IsDeleted);
        });

        modelBuilder.Entity<Pricelist>(entity =>
        {
            entity.ToTable("RetailPricelists", t => t.ExcludeFromMigrations());
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<PricelistItem>(entity =>
        {
            entity.ToTable("RetailPricelistItems", t => t.ExcludeFromMigrations());
            entity.HasQueryFilter(pi => !pi.IsDeleted);
        });

        // ========================================
        // POS OPERATIONAL TABLES (OWNED)
        // ========================================

        // ===========================
        // RETAILTRANSACTIONHEADER CONFIGURATION
        // ===========================
        modelBuilder.Entity<RetailTransactionHeader>(entity =>
        {
            // Map to RetailTransactionHeaders table (renamed from POSTransactionHeaders)
            entity.ToTable("RetailTransactionHeaders");

            // Indexes for performance
            entity.HasIndex(th => th.TransactionNumber)
                .IsUnique()
                .HasDatabaseName("IX_RetailTransactionHeaders_Number");

            entity.HasIndex(th => th.TransactionDate)
                .HasDatabaseName("IX_RetailTransactionHeaders_Date");

            entity.HasIndex(th => th.DebtorId)
                .HasDatabaseName("IX_RetailTransactionHeaders_DebtorId");

            // NOTE: TransactionDetails relationship removed in Phase 7B.
            // Transaction details are now stored in SharedDbContext.TransactionDetails
            // using HeaderId + TransactionType discriminator pattern.
            // Navigation is handled at the repository level, not via EF navigation property.

            // One-to-many: RetailTransactionHeader -> Payments
            entity.HasMany(th => th.Payments)
                .WithOne(p => p.TransactionHeader)
                .HasForeignKey(p => p.TransactionHeaderId)
                .OnDelete(DeleteBehavior.Restrict); // Don't auto-delete payments

            // Many-to-one: RetailTransactionHeader -> Debtor (optional)
            entity.HasOne(th => th.Debtor)
                .WithMany(d => d.Transactions)
                .HasForeignKey(th => th.DebtorId)
                .OnDelete(DeleteBehavior.Restrict); // Don't delete debtor if transactions exist

            // Many-to-one: RetailTransactionHeader -> Pricelist (optional)
            entity.HasOne(th => th.Pricelist)
                .WithMany()
                .HasForeignKey(th => th.PricelistId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(th => !th.IsDeleted);
        });

        // ===========================
        // NOTE: POSTransactionDetail REPLACED BY UNIFIED TransactionDetail (Phase 7B)
        // ===========================
        // Transaction details are now stored in unified TransactionDetails table
        // with HeaderId + TransactionType discriminator pattern.
        // Configuration is below in UNIFIED TRANSACTION DATA section.

        // ===========================
        // PAYMENT CONFIGURATION
        // ===========================
        modelBuilder.Entity<Payment>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(p => p.TransactionHeaderId)
                .HasDatabaseName("IX_Payments_TransactionHeaderId");

            entity.HasIndex(p => p.DebtorId)
                .HasDatabaseName("IX_Payments_DebtorId");

            entity.HasIndex(p => p.PaymentDate)
                .HasDatabaseName("IX_Payments_PaymentDate");

            // Many-to-one: Payment -> Debtor (optional - for account payments)
            entity.HasOne(p => p.Debtor)
                .WithMany()
                .HasForeignKey(p => p.DebtorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-one: Payment -> TransactionHeader (already configured above)

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===========================
        // PRODUCTBARCODE CONFIGURATION
        // ===========================
        modelBuilder.Entity<ProductBarcode>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(pb => pb.BarcodeValue)
                .IsUnique()
                .HasDatabaseName("IX_ProductBarcodes_BarcodeValue");

            entity.HasIndex(pb => pb.ProductId)
                .HasDatabaseName("IX_ProductBarcodes_ProductId");

            entity.HasIndex(pb => pb.SerialNumber)
                .HasDatabaseName("IX_ProductBarcodes_SerialNumber");

            entity.HasIndex(pb => pb.BatchNumber)
                .HasDatabaseName("IX_ProductBarcodes_BatchNumber");

            entity.HasIndex(pb => pb.IsSold)
                .HasDatabaseName("IX_ProductBarcodes_IsSold");

            // Many-to-one: ProductBarcode -> Product
            entity.HasOne(pb => pb.Product)
                .WithMany()
                .HasForeignKey(pb => pb.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Keep barcode history if product deleted

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(pb => !pb.IsDeleted);
        });

        // ========================================
        // UNIFIED TRANSACTION DATA TABLES (OWNED)
        // ========================================
        // These tables support all modules but are owned by PosDbContext
        // because they require FKs to Products (business database).

        // ===========================
        // TRANSACTIONDETAIL CONFIGURATION (Movement Architecture)
        // ===========================
        modelBuilder.Entity<TransactionDetail>(entity =>
        {
            // Store TransactionType enum as string for readability
            entity.Property(e => e.TransactionType)
                .HasConversion<string>()
                .HasMaxLength(50);

            // Composite index for querying details by header and type (discriminated union pattern)
            entity.HasIndex(e => new { e.HeaderId, e.TransactionType })
                .HasDatabaseName("IX_TransactionDetails_HeaderId_TransactionType");

            // Index on ProductId for inventory queries
            entity.HasIndex(e => e.ProductId)
                .HasDatabaseName("IX_TransactionDetails_ProductId");

            // Filtered index on BatchNumber (only non-null values)
            entity.HasIndex(e => e.BatchNumber)
                .HasFilter("[BatchNumber] IS NOT NULL")
                .HasDatabaseName("IX_TransactionDetails_BatchNumber");

            // Filtered index on SerialNumber (only non-null values)
            entity.HasIndex(e => e.SerialNumber)
                .HasFilter("[SerialNumber] IS NOT NULL")
                .HasDatabaseName("IX_TransactionDetails_SerialNumber");

            // Note: FK to Products is established via ProductId column.
            // Navigation property is not available since entity is in Shared.Core
            // and Product is in POS.Models (to avoid circular dependency).
            // FK constraint can be added at database level if needed.

            // Global query filter for soft delete (POPIA compliance)
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===========================
        // MOVEMENT CONFIGURATION (Movement Architecture)
        // ===========================
        modelBuilder.Entity<Movement>(entity =>
        {
            // Store enums as strings for readability and debugging
            entity.Property(e => e.TransactionType)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Direction)
                .HasConversion<string>()
                .HasMaxLength(10);

            // Performance index for SOH calculation queries
            // SOH = SUM(Quantity WHERE Direction = 'In') - SUM(Quantity WHERE Direction = 'Out')
            entity.HasIndex(e => new { e.ProductId, e.TransactionDate, e.Direction })
                .HasDatabaseName("IX_Movements_ProductId_TransactionDate_Direction");

            // Index for movement history queries
            entity.HasIndex(e => new { e.ProductId, e.TransactionDate })
                .HasDatabaseName("IX_Movements_ProductId_TransactionDate");

            // Index for linking back to transaction
            entity.HasIndex(e => new { e.TransactionType, e.HeaderId })
                .HasDatabaseName("IX_Movements_TransactionType_HeaderId");

            // Filtered index on BatchNumber for traceability queries
            entity.HasIndex(e => e.BatchNumber)
                .HasFilter("[BatchNumber] IS NOT NULL")
                .HasDatabaseName("IX_Movements_BatchNumber");

            // Filtered index on SerialNumber for traceability queries
            entity.HasIndex(e => e.SerialNumber)
                .HasFilter("[SerialNumber] IS NOT NULL")
                .HasDatabaseName("IX_Movements_SerialNumber");

            // Index on LocationId for multi-location inventory
            entity.HasIndex(e => e.LocationId)
                .HasFilter("[LocationId] IS NOT NULL")
                .HasDatabaseName("IX_Movements_LocationId");

            // Note: FK to Products is established via ProductId column.
            // Navigation property is not available since entity is in Shared.Core
            // and Product is in POS.Models (to avoid circular dependency).

            // Global query filter for soft delete (POPIA compliance)
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ========================================
        // BATCH & SERIAL NUMBER TABLES (OWNED)
        // ========================================
        // Phase 8: Enterprise-grade batch and serial number generation.

        // ===========================
        // BATCHNUMBERSEQUENCE CONFIGURATION
        // ===========================
        modelBuilder.Entity<BatchNumberSequence>(entity =>
        {
            // Store BatchType enum as integer (matches enum values: 10, 20, 30, etc.)
            entity.Property(e => e.BatchType)
                .HasConversion<int>();

            // Composite unique constraint: one sequence per site/type/date
            entity.HasIndex(e => new { e.SiteId, e.BatchType, e.BatchDate })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_BatchNumberSequences_SiteId_BatchType_BatchDate_Unique");

            // Index for querying by site
            entity.HasIndex(e => e.SiteId)
                .HasDatabaseName("IX_BatchNumberSequences_SiteId");

            // Index for querying by date (cleanup old sequences)
            entity.HasIndex(e => e.BatchDate)
                .HasDatabaseName("IX_BatchNumberSequences_BatchDate");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===========================
        // SERIALNUMBERSEQUENCE CONFIGURATION
        // ===========================
        modelBuilder.Entity<SerialNumberSequence>(entity =>
        {
            // Store BatchType enum as integer (nullable)
            entity.Property(e => e.BatchType)
                .HasConversion<int?>();

            // Composite index for unit sequences
            entity.HasIndex(e => new { e.SiteId, e.SequenceType, e.ProductionDate, e.BatchType, e.BatchSequence })
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_SerialNumberSequences_Composite");

            // Index for querying by site
            entity.HasIndex(e => e.SiteId)
                .HasDatabaseName("IX_SerialNumberSequences_SiteId");

            // Index for querying by date (cleanup old sequences)
            entity.HasIndex(e => e.ProductionDate)
                .HasDatabaseName("IX_SerialNumberSequences_ProductionDate");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===========================
        // SERIALNUMBER CONFIGURATION
        // ===========================
        modelBuilder.Entity<SerialNumber>(entity =>
        {
            // Store enums as their underlying values
            entity.Property(e => e.BatchType)
                .HasConversion<int>();

            entity.Property(e => e.Status)
                .HasConversion<int>();

            // Unique constraint on FullSerialNumber (primary lookup)
            entity.HasIndex(e => e.FullSerialNumber)
                .IsUnique()
                .HasDatabaseName("IX_SerialNumbers_FullSerialNumber_Unique");

            // Unique constraint on ShortSerialNumber (barcode lookup)
            entity.HasIndex(e => e.ShortSerialNumber)
                .IsUnique()
                .HasDatabaseName("IX_SerialNumbers_ShortSerialNumber_Unique");

            // Index for batch lookups
            entity.HasIndex(e => e.BatchNumber)
                .HasFilter("[BatchNumber] IS NOT NULL")
                .HasDatabaseName("IX_SerialNumbers_BatchNumber");

            // Index for site queries
            entity.HasIndex(e => e.SiteId)
                .HasDatabaseName("IX_SerialNumbers_SiteId");

            // Index for strain queries
            entity.HasIndex(e => e.StrainCode)
                .HasDatabaseName("IX_SerialNumbers_StrainCode");

            // Index for status queries (inventory, sold, etc.)
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_SerialNumbers_Status");

            // Composite index for production queries
            entity.HasIndex(e => new { e.SiteId, e.ProductionDate, e.Status })
                .HasDatabaseName("IX_SerialNumbers_SiteId_ProductionDate_Status");

            // Index for product lookups
            entity.HasIndex(e => e.ProductId)
                .HasFilter("[ProductId] IS NOT NULL")
                .HasDatabaseName("IX_SerialNumbers_ProductId");

            // Note: FK to Products is established via ProductId column (optional).
            // Navigation property is not available since entity is in Shared.Core
            // and Product is in POS.Models (to avoid circular dependency).

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    // ========================================
    // STEP 3: SaveChangesAsync Override (Audit Trail)
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
