using Microsoft.EntityFrameworkCore;
using Project420.Cultivation.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Cultivation.DAL;

/// <summary>
/// Database context for the Cultivation module
/// </summary>
/// <remarks>
/// Purpose:
/// - Individual plant tracking (SAHPRA Section 22C requirement)
/// - Grow cycle management
/// - Harvest batch tracking
/// - Grow room/location management
/// - Seed-to-sale traceability foundation
///
/// SAHPRA Compliance:
/// - EVERY plant must be tracked from seed/clone to harvest
/// - Growth stage transitions logged
/// - Harvest weights recorded
/// - Plant destruction documented with reason
/// - Links to production module for complete traceability
///
/// DALRRD Hemp Permit Compliance:
/// - Plant count tracking (cannot exceed licensed limit)
/// - THC testing results
/// - Male plant destruction records
///
/// Architecture:
/// - Separate DbContext for module independence
/// - Can share database with other modules or use separate DB
/// - Foreign key references to HarvestBatch link to Production module
///
/// POPIA Compliance:
/// - All entities inherit from AuditableEntity
/// - Automatic audit trail population
/// - 7-year retention for compliance reporting
/// </remarks>
public class CultivationDbContext : DbContext
{
    /// <summary>
    /// Constructor that accepts configuration options
    /// </summary>
    public CultivationDbContext(DbContextOptions<CultivationDbContext> options) : base(options)
    {
    }

    // ========================================
    // DbSet Properties (Cultivation Tables)
    // ========================================

    /// <summary>
    /// Plants table - Individual cannabis plant tracking
    /// </summary>
    /// <remarks>
    /// CRITICAL: SAHPRA requires EVERY plant be uniquely identified and tracked
    /// Each plant must have:
    /// - Unique plant tag (barcode/RFID)
    /// - Growth stage history
    /// - Harvest date and weight
    /// - Destruction records (if applicable)
    ///
    /// This is the foundation of seed-to-sale traceability
    /// </remarks>
    public DbSet<Plant> Plants { get; set; } = null!;

    /// <summary>
    /// Grow cycles table - Cultivation cycle management
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Batch-level cultivation tracking
    /// - Yield calculations and reporting
    /// - Strain-specific grow data
    /// - Timeline and planning
    ///
    /// Links to:
    /// - Plants (one-to-many)
    /// - HarvestBatches (one-to-many)
    /// </remarks>
    public DbSet<GrowCycle> GrowCycles { get; set; } = null!;

    /// <summary>
    /// Harvest batches table - Links cultivation to production
    /// </summary>
    /// <remarks>
    /// CRITICAL TRACEABILITY LINK:
    /// - Bridges Cultivation → Production modules
    /// - Aggregates multiple plants into processing batch
    /// - Tracks wet/dry weight conversion
    /// - Lab testing results (COA)
    /// - Quality control pass/fail
    ///
    /// SAHPRA Requirement:
    /// - Batch numbers must be unique and traceable
    /// - Links back to individual plants via Plant.HarvestBatchId
    /// - Forward links to Production.ProductionBatch via batch number
    /// </remarks>
    public DbSet<HarvestBatch> HarvestBatches { get; set; } = null!;

    /// <summary>
    /// Grow rooms table - Physical growing locations
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Location tracking (GMP requirement)
    /// - Environmental condition monitoring
    /// - Capacity planning
    /// - Security and access control
    ///
    /// GMP Compliance:
    /// - Each room must be identified
    /// - Environmental conditions monitored
    /// - Rooms segregated by growth stage
    /// </remarks>
    public DbSet<GrowRoom> GrowRooms { get; set; } = null!;

    // ========================================
    // OnModelCreating (Fluent API Configuration)
    // ========================================

    /// <summary>
    /// Configures the Cultivation database schema using Fluent API
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: Use schema for logical separation
        // modelBuilder.HasDefaultSchema("cultivation");

        // ===========================
        // PLANT CONFIGURATION
        // ===========================
        modelBuilder.Entity<Plant>(entity =>
        {
            // Indexes for performance and uniqueness
            entity.HasIndex(p => p.PlantTag)
                .IsUnique()
                .HasDatabaseName("IX_Plants_PlantTag");

            entity.HasIndex(p => p.GrowCycleId)
                .HasDatabaseName("IX_Plants_GrowCycleId");

            entity.HasIndex(p => p.CurrentStage)
                .HasDatabaseName("IX_Plants_CurrentStage");

            entity.HasIndex(p => p.HarvestBatchId)
                .HasDatabaseName("IX_Plants_HarvestBatchId");

            entity.HasIndex(p => p.CurrentGrowRoomId)
                .HasDatabaseName("IX_Plants_CurrentGrowRoomId");

            entity.HasIndex(p => p.IsActive)
                .HasDatabaseName("IX_Plants_IsActive");

            // Relationships
            entity.HasOne(p => p.GrowCycle)
                .WithMany(gc => gc.Plants)
                .HasForeignKey(p => p.GrowCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.HarvestBatch)
                .WithMany(hb => hb.Plants)
                .HasForeignKey(p => p.HarvestBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.CurrentGrowRoom)
                .WithMany(gr => gr.Plants)
                .HasForeignKey(p => p.CurrentGrowRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship (mother plant)
            entity.HasOne(p => p.MotherPlant)
                .WithMany()
                .HasForeignKey(p => p.MotherPlantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ===========================
        // GROW CYCLE CONFIGURATION
        // ===========================
        modelBuilder.Entity<GrowCycle>(entity =>
        {
            // Indexes
            entity.HasIndex(gc => gc.CycleCode)
                .IsUnique()
                .HasDatabaseName("IX_GrowCycles_CycleCode");

            entity.HasIndex(gc => gc.StrainName)
                .HasDatabaseName("IX_GrowCycles_StrainName");

            entity.HasIndex(gc => gc.StartDate)
                .HasDatabaseName("IX_GrowCycles_StartDate");

            entity.HasIndex(gc => gc.IsActive)
                .HasDatabaseName("IX_GrowCycles_IsActive");

            // Relationships
            entity.HasOne(gc => gc.GrowRoom)
                .WithMany(gr => gr.GrowCycles)
                .HasForeignKey(gc => gc.GrowRoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete query filter
            entity.HasQueryFilter(gc => !gc.IsDeleted);
        });

        // ===========================
        // HARVEST BATCH CONFIGURATION
        // ===========================
        modelBuilder.Entity<HarvestBatch>(entity =>
        {
            // Indexes
            entity.HasIndex(hb => hb.BatchNumber)
                .IsUnique()
                .HasDatabaseName("IX_HarvestBatches_BatchNumber");

            entity.HasIndex(hb => hb.GrowCycleId)
                .HasDatabaseName("IX_HarvestBatches_GrowCycleId");

            entity.HasIndex(hb => hb.StrainName)
                .HasDatabaseName("IX_HarvestBatches_StrainName");

            entity.HasIndex(hb => hb.HarvestDate)
                .HasDatabaseName("IX_HarvestBatches_HarvestDate");

            entity.HasIndex(hb => hb.IsActive)
                .HasDatabaseName("IX_HarvestBatches_IsActive");

            // Relationships
            entity.HasOne(hb => hb.GrowCycle)
                .WithMany(gc => gc.HarvestBatches)
                .HasForeignKey(hb => hb.GrowCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete query filter
            entity.HasQueryFilter(hb => !hb.IsDeleted);
        });

        // ===========================
        // GROW ROOM CONFIGURATION
        // ===========================
        modelBuilder.Entity<GrowRoom>(entity =>
        {
            // Indexes
            entity.HasIndex(gr => gr.RoomCode)
                .IsUnique()
                .HasDatabaseName("IX_GrowRooms_RoomCode");

            entity.HasIndex(gr => gr.RoomType)
                .HasDatabaseName("IX_GrowRooms_RoomType");

            entity.HasIndex(gr => gr.IsActive)
                .HasDatabaseName("IX_GrowRooms_IsActive");

            // Soft delete query filter
            entity.HasQueryFilter(gr => !gr.IsDeleted);
        });
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically populate audit fields
    /// </summary>
    /// <remarks>
    /// POPIA Compliance:
    /// - Automatically sets CreatedAt, CreatedBy on INSERT
    /// - Automatically sets ModifiedAt, ModifiedBy on UPDATE
    /// - Ensures audit trail for all data changes
    ///
    /// TODO: In production, get current user from authentication context
    /// For now, using "system" as placeholder
    /// </remarks>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = "system"; // TODO: Get from authentication context
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
                entry.Entity.ModifiedBy = "system"; // TODO: Get from authentication context
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
