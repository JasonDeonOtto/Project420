using Microsoft.EntityFrameworkCore;
using Project420.Production.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Production.DAL;

/// <summary>
/// Database context for the Production module
/// </summary>
/// <remarks>
/// Purpose:
/// - Production batch processing workflow
/// - Processing step tracking (drying, curing, trimming, packaging)
/// - Quality control checkpoints
/// - Laboratory testing (COA) management
/// - Links Cultivation (HarvestBatch) → Inventory
///
/// SAHPRA GMP Compliance:
/// - All processing steps must be documented
/// - Quality control checks required at critical points
/// - Lab testing (COA) mandatory before release to inventory
/// - Batch traceability from harvest to retail
///
/// This module bridges cultivation and retail sales
/// </remarks>
public class ProductionDbContext : DbContext
{
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options) : base(options)
    {
    }

    public DbSet<ProductionBatch> ProductionBatches { get; set; } = null!;
    public DbSet<ProcessingStep> ProcessingSteps { get; set; } = null!;
    public DbSet<QualityControl> QualityControls { get; set; } = null!;
    public DbSet<LabTest> LabTests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProductionBatch
        modelBuilder.Entity<ProductionBatch>(entity =>
        {
            entity.HasIndex(pb => pb.BatchNumber).IsUnique();
            entity.HasIndex(pb => pb.HarvestBatchNumber);
            entity.HasIndex(pb => pb.StrainName);
            entity.HasIndex(pb => pb.Status);
            entity.HasIndex(pb => pb.IsActive);
            entity.HasQueryFilter(pb => !pb.IsDeleted);
        });

        // ProcessingStep
        modelBuilder.Entity<ProcessingStep>(entity =>
        {
            entity.HasIndex(ps => ps.ProductionBatchId);
            entity.HasIndex(ps => ps.StepType);
            entity.HasIndex(ps => ps.Status);

            entity.HasOne(ps => ps.ProductionBatch)
                .WithMany(pb => pb.ProcessingSteps)
                .HasForeignKey(ps => ps.ProductionBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(ps => !ps.IsDeleted);
        });

        // QualityControl
        modelBuilder.Entity<QualityControl>(entity =>
        {
            entity.HasIndex(qc => qc.ProductionBatchId);
            entity.HasIndex(qc => qc.CheckType);
            entity.HasIndex(qc => qc.Passed);

            entity.HasOne(qc => qc.ProductionBatch)
                .WithMany(pb => pb.QualityControls)
                .HasForeignKey(qc => qc.ProductionBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(qc => !qc.IsDeleted);
        });

        // LabTest
        modelBuilder.Entity<LabTest>(entity =>
        {
            entity.HasIndex(lt => lt.ProductionBatchId);
            entity.HasIndex(lt => lt.COANumber).IsUnique();
            entity.HasIndex(lt => lt.OverallPass);

            entity.HasOne(lt => lt.ProductionBatch)
                .WithMany(pb => pb.LabTests)
                .HasForeignKey(lt => lt.ProductionBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(lt => !lt.IsDeleted);
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
