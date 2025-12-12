using Microsoft.EntityFrameworkCore;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.DAL;

/// <summary>
/// Database context for the Point of Sale (POS) module.
/// This is the "command center" that manages all database operations for the POS system.
/// </summary>
/// <remarks>
/// POPIA Compliance: This context includes audit trail support and soft delete patterns.
/// Cannabis Compliance: Manages THC/CBD tracking, batch numbers, and age verification data.
/// </remarks>
public class PosDbContext : DbContext
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
            entity.ToTable("Pricelists", t => t.ExcludeFromMigrations());
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        modelBuilder.Entity<PricelistItem>(entity =>
        {
            entity.ToTable("PricelistItem", t => t.ExcludeFromMigrations());
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
        // NOTE: POSTRANSACTIONDETAIL CONFIGURATION REMOVED (Phase 7B)
        // ===========================
        // Transaction details are now stored in SharedDbContext.TransactionDetails
        // See SharedDbContext.OnModelCreating for the unified configuration.

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
