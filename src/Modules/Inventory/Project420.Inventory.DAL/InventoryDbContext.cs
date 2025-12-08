using Microsoft.EntityFrameworkCore;
using Project420.Inventory.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Inventory.DAL;

/// <summary>
/// Database context for the Inventory module
/// </summary>
/// <remarks>
/// Purpose:
/// - Stock movement tracking (universal ledger)
/// - Stock transfers between locations
/// - Stock adjustments (corrections, waste, shrinkage)
/// - Stock counts (cycle counts, physical inventory)
/// - Links Production → Retail
///
/// SAHPRA/SARS Compliance:
/// - ALL stock movements must be tracked
/// - Batch numbers for seed-to-sale traceability
/// - Weight tracking for cannabis products
/// - Waste must be documented with reason
/// - Inventory reconciliation required
/// </remarks>
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<StockMovement> StockMovements { get; set; } = null!;
    public DbSet<StockTransfer> StockTransfers { get; set; } = null!;
    public DbSet<StockAdjustment> StockAdjustments { get; set; } = null!;
    public DbSet<StockCount> StockCounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // StockMovement
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasIndex(sm => sm.MovementNumber).IsUnique();
            entity.HasIndex(sm => sm.ProductSKU);
            entity.HasIndex(sm => sm.BatchNumber);
            entity.HasIndex(sm => sm.MovementType);
            entity.HasIndex(sm => sm.MovementDate);
            entity.HasIndex(sm => sm.ReferenceNumber);
            entity.HasQueryFilter(sm => !sm.IsDeleted);
        });

        // StockTransfer
        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasIndex(st => st.TransferNumber).IsUnique();
            entity.HasIndex(st => st.TransferDate);
            entity.HasIndex(st => st.Status);
            entity.HasIndex(st => st.FromLocation);
            entity.HasIndex(st => st.ToLocation);
            entity.HasQueryFilter(st => !st.IsDeleted);
        });

        // StockAdjustment
        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasIndex(sa => sa.AdjustmentNumber).IsUnique();
            entity.HasIndex(sa => sa.ProductSKU);
            entity.HasIndex(sa => sa.BatchNumber);
            entity.HasIndex(sa => sa.AdjustmentDate);
            entity.HasQueryFilter(sa => !sa.IsDeleted);
        });

        // StockCount
        modelBuilder.Entity<StockCount>(entity =>
        {
            entity.HasIndex(sc => sc.CountNumber).IsUnique();
            entity.HasIndex(sc => sc.ProductSKU);
            entity.HasIndex(sc => sc.BatchNumber);
            entity.HasIndex(sc => sc.CountDate);
            entity.HasIndex(sc => sc.CountType);
            entity.HasIndex(sc => sc.VarianceInvestigated);
            entity.HasQueryFilter(sc => !sc.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = "system";
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
                entry.Entity.ModifiedBy = "system";
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
