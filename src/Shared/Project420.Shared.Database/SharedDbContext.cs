using Microsoft.EntityFrameworkCore;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Shared.Database;

/// <summary>
/// Shared database context for centralized cross-cutting concerns.
/// </summary>
/// <remarks>
/// Purpose:
/// - Centralized error logging across all modules
/// - Centralized audit logging for compliance (POPIA, Cannabis Act)
/// - System-wide configuration and settings (future)
///
/// Architecture:
/// - All modules (Management, Retail POS, etc.) can reference this for logging
/// - Separate database/schema from module-specific data (optional)
/// - Can be same database with different schema, or separate database entirely
///
/// Benefits:
/// - Single source of truth for errors/audits across the system
/// - Easy to query all errors regardless of which module generated them
/// - Compliance: Centralized audit trail for regulatory requirements
/// - Monitoring: Single place to check system health
///
/// Future Enhancements:
/// - SystemConfiguration table (app settings, feature flags)
/// - NotificationQueue table (email, SMS notifications)
/// - ScheduledJob table (background tasks)
/// </remarks>
public class SharedDbContext : DbContext
{
    /// <summary>
    /// Constructor that accepts configuration options (connection string, etc.)
    /// </summary>
    /// <param name="options">Configuration options for the DbContext</param>
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
    {
    }

    // ========================================
    // DbSet Properties (Shared Tables)
    // ========================================

    /// <summary>
    /// Error logs table - Centralized error tracking across all modules
    /// </summary>
    /// <remarks>
    /// All modules log errors here:
    /// - Management module errors
    /// - Retail POS errors
    /// - Shared infrastructure errors
    /// - Database connection errors
    ///
    /// Query examples:
    /// - Get all Critical errors in last 24 hours
    /// - Get errors for specific user
    /// - Get unresolved errors
    /// </remarks>
    public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;

    /// <summary>
    /// Audit logs table - Centralized audit trail for compliance
    /// </summary>
    /// <remarks>
    /// Tracks important operations:
    /// - Security events (login, logout, failed attempts)
    /// - Data access (POPIA compliance - who viewed customer records)
    /// - Business operations (price changes, transaction cancellations)
    /// - Permission changes
    ///
    /// Compliance Requirements:
    /// - POPIA: Track access to personal information (7-year retention)
    /// - Cannabis Act: Track product batch changes, license updates
    /// - SARS: Track price changes for tax compliance
    /// </remarks>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// Station connections table - Multi-tenant routing metadata
    /// </summary>
    /// <remarks>
    /// Maps stations (PCs/terminals) to authorized company databases.
    /// Enables multi-tenant architecture with company selection.
    ///
    /// Usage:
    /// - Station boots → query by hostname → list available companies
    /// - User selects company → decrypt connection string → connect
    /// - Authenticate against selected company's Users table
    ///
    /// Architecture:
    /// - Phase 1: Machine Name identification (Environment.MachineName)
    /// - Phase 4: Hardware ID licensing system
    /// - AES-256 encrypted connection strings (cross-platform)
    /// - User per company model (isolated accounts)
    /// </remarks>
    public DbSet<StationConnection> StationConnections { get; set; } = null!;

    /// <summary>
    /// Transaction number sequences table - Database-backed persistent transaction numbering
    /// </summary>
    /// <remarks>
    /// Purpose:
    /// - Generate unique, continuous transaction numbers (no daily resets)
    /// - User-defined prefixes (letters-only OR numbers-only)
    /// - Persistent across application restarts
    /// - Thread-safe increment operations via database transactions
    ///
    /// Supported Formats:
    /// 1. Invoice/Credit/Retail: {UserPrefix}-{Sequence} (e.g., INV-00001, 1-00001)
    /// 2. Production Batches: YYYYMMDD-{BatchType}-{Sequence}
    /// 3. Serial Numbers: SN-{STRAIN}-{BATCH}-{Sequence}
    ///
    /// Compliance:
    /// - Cannabis Act: Unique batch and serial numbers for traceability
    /// - SARS: Sequential invoice numbering for tax compliance
    /// - POPIA: Audit trail for all transaction number generation
    /// </remarks>
    public DbSet<TransactionNumberSequence> TransactionNumberSequences { get; set; } = null!;

    // ========================================
    // OnModelCreating (Fluent API Configuration)
    // ========================================

    /// <summary>
    /// Configures the shared database schema using Fluent API
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===========================
        // ERRORLOG CONFIGURATION
        // ===========================
        modelBuilder.Entity<ErrorLog>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.OccurredAt)
                .HasDatabaseName("IX_ErrorLogs_OccurredAt");

            entity.HasIndex(e => e.Severity)
                .HasDatabaseName("IX_ErrorLogs_Severity");

            entity.HasIndex(e => e.Source)
                .HasDatabaseName("IX_ErrorLogs_Source");

            entity.HasIndex(e => e.IsResolved)
                .HasDatabaseName("IX_ErrorLogs_IsResolved");

            // Composite index for common queries
            entity.HasIndex(e => new { e.IsResolved, e.Severity, e.OccurredAt })
                .HasDatabaseName("IX_ErrorLogs_Resolved_Severity_Date");

            // Error logs are immutable (no updates after creation)
            // Only allow marking as resolved
        });

        // ===========================
        // AUDITLOG CONFIGURATION
        // ===========================
        modelBuilder.Entity<AuditLog>(entity =>
        {
            // Indexes for performance and compliance queries
            entity.HasIndex(a => a.OccurredAt)
                .HasDatabaseName("IX_AuditLogs_OccurredAt");

            entity.HasIndex(a => a.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            entity.HasIndex(a => a.ActionType)
                .HasDatabaseName("IX_AuditLogs_ActionType");

            entity.HasIndex(a => a.Module)
                .HasDatabaseName("IX_AuditLogs_Module");

            entity.HasIndex(a => new { a.EntityType, a.EntityId })
                .HasDatabaseName("IX_AuditLogs_Entity");

            entity.HasIndex(a => a.Severity)
                .HasDatabaseName("IX_AuditLogs_Severity");

            // Composite index for compliance queries
            entity.HasIndex(a => new { a.EntityType, a.EntityId, a.OccurredAt })
                .HasDatabaseName("IX_AuditLogs_Entity_Date");

            // Audit logs are IMMUTABLE (POPIA compliance requirement)
            // No updates or deletes allowed - only inserts
        });

        // ===========================
        // STATIONCONNECTION CONFIGURATION
        // ===========================
        modelBuilder.Entity<StationConnection>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(sc => sc.HostName)
                .HasDatabaseName("IX_StationConnections_HostName");

            entity.HasIndex(sc => sc.DatabaseName)
                .HasDatabaseName("IX_StationConnections_DatabaseName");

            entity.HasIndex(sc => sc.IsAuthorized)
                .HasDatabaseName("IX_StationConnections_IsAuthorized");

            entity.HasIndex(sc => sc.AccessExpiresAt)
                .HasDatabaseName("IX_StationConnections_AccessExpiresAt");

            // Composite index for common query (hostname + authorized)
            entity.HasIndex(sc => new { sc.HostName, sc.IsAuthorized })
                .HasDatabaseName("IX_StationConnections_HostName_IsAuthorized");

            // Soft delete query filter (POPIA compliance)
            entity.HasQueryFilter(sc => !sc.IsDeleted);

            // ===========================
            // SEED DATA - Example Connections
            // ===========================
            // NOTE: Connection strings are NOT encrypted in seed data (development only)
            // Production: Use encryption service before seeding
            var seedDate = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

            entity.HasData(
                // Example 1: POS-STORE-01 can access Main Store (Project420_Dev)
                new StationConnection
                {
                    Id = 1,
                    HostName = "POS-STORE-01",
                    CompanyName = "Project420 - Main Store",
                    DatabaseName = "Project420_Dev",
                    // TODO Phase 4: Encrypt this connection string using AES-256
                    EncryptedConnectionString = "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
                    IsAuthorized = true,
                    AccessExpiresAt = null, // Permanent access
                    Notes = "Main POS terminal for Store 01",
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                },

                // Example 2: Same station can access Branch Store (multi-company access)
                new StationConnection
                {
                    Id = 2,
                    HostName = "POS-STORE-01",
                    CompanyName = "Project420 - Branch Store",
                    DatabaseName = "Project420_Dev2",
                    // TODO Phase 4: Encrypt this connection string
                    EncryptedConnectionString = "Server=JASON\\SQLDEVED;Database=Project420_Dev2;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
                    IsAuthorized = true,
                    AccessExpiresAt = null,
                    Notes = "Backup access to Branch Store for emergencies",
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                },

                // Example 3: Mobile device with expiring access
                new StationConnection
                {
                    Id = 3,
                    HostName = "MOBILE-REP-01",
                    CompanyName = "Project420 - Main Store",
                    DatabaseName = "Project420_Dev",
                    EncryptedConnectionString = "Server=JASON\\SQLDEVED;Database=Project420_Dev;User Id=sa;Password=ZAQxsw123;TrustServerCertificate=True;",
                    IsAuthorized = true,
                    AccessExpiresAt = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    Notes = "Regional manager tablet - temporary access for Q4 2025",
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM",
                    IsDeleted = false
                }
            );
        });

        // ===========================
        // TRANSACTIONNUMBERSEQUENCE CONFIGURATION
        // ===========================
        modelBuilder.Entity<TransactionNumberSequence>(entity =>
        {
            // Unique constraint on TransactionType (one sequence per type)
            entity.HasIndex(tns => tns.TransactionType)
                .IsUnique()
                .HasDatabaseName("IX_TransactionNumberSequences_TransactionType_Unique");

            // Index for active sequences
            entity.HasIndex(tns => tns.IsActive)
                .HasDatabaseName("IX_TransactionNumberSequences_IsActive");

            // Index for last generated date (finding inactive sequences)
            entity.HasIndex(tns => tns.LastGeneratedAt)
                .HasDatabaseName("IX_TransactionNumberSequences_LastGeneratedAt");

            // Validation: Prefix must be uppercase
            entity.Property(tns => tns.Prefix)
                .HasConversion(
                    v => v.ToUpperInvariant(), // Store uppercase
                    v => v // Read as-is
                );

            // ===========================
            // SEED DATA - Default Sequences
            // ===========================
            var seedDate = new DateTime(2025, 12, 6, 0, 0, 0, DateTimeKind.Utc);

            entity.HasData(
                // Invoice sequence (user-defined prefix: "INV")
                new TransactionNumberSequence
                {
                    SequenceId = 1,
                    TransactionType = TransactionTypeCode.INV,
                    Prefix = "INV",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Sales invoices - formal account invoices",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Credit Note sequence (user-defined prefix: "CRN")
                new TransactionNumberSequence
                {
                    SequenceId = 2,
                    TransactionType = TransactionTypeCode.CRN,
                    Prefix = "CRN",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Credit notes - refunds and returns",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // POS Sale sequence (default: uses TransactionType enum "SALE")
                new TransactionNumberSequence
                {
                    SequenceId = 3,
                    TransactionType = TransactionTypeCode.SALE,
                    Prefix = "", // Empty = uses "SALE" from enum
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Point of Sale transactions - retail sales",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Goods Received Note sequence
                new TransactionNumberSequence
                {
                    SequenceId = 4,
                    TransactionType = TransactionTypeCode.GRV,
                    Prefix = "GRV",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Goods received notes - stock received from suppliers",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Return to Supplier sequence
                new TransactionNumberSequence
                {
                    SequenceId = 5,
                    TransactionType = TransactionTypeCode.RTS,
                    Prefix = "RTS",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Returns to supplier - defective/unwanted stock",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Stock Adjustment sequence
                new TransactionNumberSequence
                {
                    SequenceId = 6,
                    TransactionType = TransactionTypeCode.ADJ,
                    Prefix = "ADJ",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Stock adjustments - internal corrections",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Payment sequence
                new TransactionNumberSequence
                {
                    SequenceId = 7,
                    TransactionType = TransactionTypeCode.PAY,
                    Prefix = "PAY",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Customer payments - cash/card/EFT received",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Quotation sequence
                new TransactionNumberSequence
                {
                    SequenceId = 8,
                    TransactionType = TransactionTypeCode.QTE,
                    Prefix = "QTE",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Sales quotations - non-binding estimates",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Layby sequence
                new TransactionNumberSequence
                {
                    SequenceId = 9,
                    TransactionType = TransactionTypeCode.LAY,
                    Prefix = "LAY",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Layby transactions - deposit for future collection",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                },

                // Stock Transfer sequence
                new TransactionNumberSequence
                {
                    SequenceId = 10,
                    TransactionType = TransactionTypeCode.TRF,
                    Prefix = "TRF",
                    CurrentSequence = 0,
                    StartingSequence = 1,
                    PaddingLength = 5,
                    Description = "Stock transfers - inter-location movements",
                    IsActive = true,
                    CreatedAt = seedDate,
                    CreatedBy = "SYSTEM"
                }
            );
        });
    }

    // NOTE: No SaveChangesAsync override needed here
    // ErrorLog and AuditLog don't inherit from AuditableEntity
    // They ARE the audit system, so they don't need to be audited themselves
}

