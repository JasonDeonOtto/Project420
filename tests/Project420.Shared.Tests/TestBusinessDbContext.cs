using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Project420.Shared.Core.Abstractions;
using Project420.Shared.Core.Entities;

namespace Project420.Shared.Tests;

/// <summary>
/// Test implementation of IBusinessDbContext for unit testing.
/// Uses an in-memory database for isolated, fast testing.
/// </summary>
/// <remarks>
/// This context implements IBusinessDbContext and provides all the DbSets
/// needed by the shared services (MovementService, BatchNumberGeneratorService, etc.)
/// without requiring the full PosDbContext and its dependencies.
/// </remarks>
public class TestBusinessDbContext : DbContext, IBusinessDbContext
{
    public TestBusinessDbContext(DbContextOptions<TestBusinessDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Unified transaction details table.
    /// </summary>
    public DbSet<TransactionDetail> TransactionDetails { get; set; } = null!;

    /// <summary>
    /// Movement ledger table.
    /// </summary>
    public DbSet<Movement> Movements { get; set; } = null!;

    /// <summary>
    /// Batch number sequences table.
    /// </summary>
    public DbSet<BatchNumberSequence> BatchNumberSequences { get; set; } = null!;

    /// <summary>
    /// Serial number sequences table.
    /// </summary>
    public DbSet<SerialNumberSequence> SerialNumberSequences { get; set; } = null!;

    /// <summary>
    /// Serial numbers table.
    /// </summary>
    public DbSet<SerialNumber> SerialNumbers { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TransactionDetail
        modelBuilder.Entity<TransactionDetail>(entity =>
        {
            entity.Property(e => e.TransactionType)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasIndex(e => new { e.HeaderId, e.TransactionType });
            entity.HasIndex(e => e.ProductId);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Movement
        modelBuilder.Entity<Movement>(entity =>
        {
            entity.Property(e => e.TransactionType)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.Direction)
                .HasConversion<string>()
                .HasMaxLength(10);

            entity.HasIndex(e => new { e.ProductId, e.TransactionDate, e.Direction });
            entity.HasIndex(e => new { e.TransactionType, e.HeaderId });

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure BatchNumberSequence
        modelBuilder.Entity<BatchNumberSequence>(entity =>
        {
            entity.Property(e => e.BatchType)
                .HasConversion<int>();

            entity.HasIndex(e => new { e.SiteId, e.BatchType, e.BatchDate })
                .IsUnique();

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure SerialNumberSequence
        modelBuilder.Entity<SerialNumberSequence>(entity =>
        {
            entity.Property(e => e.BatchType)
                .HasConversion<int?>();

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure SerialNumber
        modelBuilder.Entity<SerialNumber>(entity =>
        {
            entity.Property(e => e.BatchType)
                .HasConversion<int>();

            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.HasIndex(e => e.FullSerialNumber).IsUnique();
            entity.HasIndex(e => e.ShortSerialNumber).IsUnique();

            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically set audit fields.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy ??= "TEST";
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy ??= "TEST";

                    if (entry.Entity.IsDeleted &&
                        entry.OriginalValues.GetValue<bool>(nameof(AuditableEntity.IsDeleted)) == false)
                    {
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.DeletedBy ??= "TEST";
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
