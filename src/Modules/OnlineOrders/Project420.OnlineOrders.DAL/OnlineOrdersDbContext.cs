using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Project420.OnlineOrders.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.OnlineOrders.DAL;

/// <summary>
/// Database context for the OnlineOrders module
/// </summary>
/// <remarks>
/// Purpose:
/// - Click & Collect order management
/// - Customer account management with age verification
/// - Online payment processing integration
/// - Pickup verification workflow
/// - Order status tracking and audit trail
///
/// Cannabis Act Compliance:
/// - Age verification at order creation (18+)
/// - Age verification at pickup (CRITICAL - dual verification)
/// - ID document verification records
/// - Audit trail for all verifications
///
/// POPIA Compliance:
/// - All entities inherit from AuditableEntity
/// - Customer consent tracking (required)
/// - Marketing consent (optional)
/// - PII encryption requirements (email, phone, ID number)
/// - 7-year retention for financial/tax records
///
/// Payment Provider Integration:
/// - Yoco (card payments)
/// - PayFast (online payments)
/// - Ozow (instant EFT)
/// - Transaction reference tracking for reconciliation
///
/// Architecture:
/// - Separate DbContext for module independence
/// - Can share database or use separate DB
/// - Links to Management module for product/customer data
/// </remarks>
public class OnlineOrdersDbContext : DbContext
{
    /// <summary>
    /// Constructor that accepts configuration options
    /// </summary>
    public OnlineOrdersDbContext(DbContextOptions<OnlineOrdersDbContext> options) : base(options)
    {
    }

    // ========================================
    // DbSet Properties (OnlineOrders Tables)
    // ========================================

    /// <summary>
    /// Online orders table - Click & Collect orders
    /// </summary>
    /// <remarks>
    /// CRITICAL COMPLIANCE:
    /// - Age verification at order creation
    /// - Age verification at pickup (Cannabis Act requirement)
    /// - VAT calculation (15% SA VAT)
    /// - Payment provider integration
    /// - Pickup location and time tracking
    ///
    /// Future-Proofing:
    /// - Ready for commercial delivery when regulations change (2026-2027)
    /// - Currently supports Click & Collect only
    /// </remarks>
    public DbSet<OnlineOrder> OnlineOrders { get; set; } = null!;

    /// <summary>
    /// Online order items table - Order line items
    /// </summary>
    /// <remarks>
    /// Denormalized product information for historical accuracy:
    /// - ProductSKU, ProductName, BatchNumber
    /// - UnitPrice at time of order (pricing may change)
    /// - THC/CBD percentages at time of order
    ///
    /// Links to:
    /// - OnlineOrder (many-to-one)
    /// - Product data (denormalized for audit trail)
    /// </remarks>
    public DbSet<OnlineOrderItem> OnlineOrderItems { get; set; } = null!;

    /// <summary>
    /// Customer accounts table - Online customer accounts
    /// </summary>
    /// <remarks>
    /// POPIA Compliance:
    /// - Consent to POPIA terms (REQUIRED)
    /// - Consent to marketing (OPTIONAL)
    /// - Consent timestamps tracked
    /// - PII must be encrypted at rest
    ///
    /// Cannabis Act Compliance:
    /// - Age verification (18+ requirement)
    /// - Date of birth validation
    /// - ID document verification
    /// - Multiple verification methods supported
    ///
    /// Security:
    /// - Password hashing (never plain text)
    /// - Account locking after failed attempts
    /// - Email verification workflow
    /// - Password reset with expiring tokens
    /// </remarks>
    public DbSet<CustomerAccount> CustomerAccounts { get; set; } = null!;

    /// <summary>
    /// Payment transactions table - Online payment processing
    /// </summary>
    /// <remarks>
    /// Payment Provider Integration:
    /// - Yoco (card payments)
    /// - PayFast (multiple methods)
    /// - Ozow (instant EFT)
    ///
    /// SARS Compliance:
    /// - All transactions logged for tax purposes
    /// - VAT breakdown tracked
    /// - 5-year retention minimum
    ///
    /// Security:
    /// - Never store full card numbers (PCI-DSS)
    /// - Only store masked card numbers
    /// - Transaction references for reconciliation
    /// </remarks>
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;

    /// <summary>
    /// Pickup confirmations table - In-store pickup verification
    /// </summary>
    /// <remarks>
    /// Cannabis Act CRITICAL Requirement:
    /// - Staff must verify age at pickup (18+)
    /// - ID verification method documented
    /// - Staff member who verified recorded
    /// - Pickup timestamp logged
    ///
    /// Cannot release cannabis products without:
    /// - Valid age verification (18+)
    /// - Matching customer ID
    /// - Order paid in full
    /// </remarks>
    public DbSet<PickupConfirmation> PickupConfirmations { get; set; } = null!;

    /// <summary>
    /// Order status history table - Audit trail for order status changes
    /// </summary>
    /// <remarks>
    /// Immutable audit trail:
    /// - Pending → Confirmed → Processing → Ready → PickedUp
    /// - Cancellation tracking
    /// - User who changed status
    /// - Timestamp of each change
    /// - Optional notes/reason for change
    ///
    /// SAHPRA/POPIA Compliance:
    /// - Complete audit trail required
    /// - Cannot delete history records
    /// - 7-year retention minimum
    /// </remarks>
    public DbSet<OrderStatusHistory> OrderStatusHistory { get; set; } = null!;

    // ========================================
    // OnModelCreating (Fluent API Configuration)
    // ========================================

    /// <summary>
    /// Configures the OnlineOrders database schema using Fluent API
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optional: Use schema for logical separation
        // modelBuilder.HasDefaultSchema("onlineorders");

        // ===========================
        // ONLINE ORDER CONFIGURATION
        // ===========================
        modelBuilder.Entity<OnlineOrder>(entity =>
        {
            // Indexes for performance and uniqueness
            entity.HasIndex(o => o.OrderNumber)
                .IsUnique()
                .HasDatabaseName("IX_OnlineOrders_OrderNumber");

            entity.HasIndex(o => o.CustomerId)
                .HasDatabaseName("IX_OnlineOrders_CustomerId");

            entity.HasIndex(o => o.Status)
                .HasDatabaseName("IX_OnlineOrders_Status");

            entity.HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_OnlineOrders_OrderDate");

            entity.HasIndex(o => o.PickupLocationId)
                .HasDatabaseName("IX_OnlineOrders_PickupLocationId");

            entity.HasIndex(o => o.PreferredPickupDate)
                .HasDatabaseName("IX_OnlineOrders_PreferredPickupDate");

            // Composite index for ready-for-pickup queries
            entity.HasIndex(o => new { o.Status, o.PickupLocationId })
                .HasDatabaseName("IX_OnlineOrders_Status_PickupLocationId");

            // Relationships
            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.PaymentTransactions)
                .WithOne()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(o => o.StatusHistory)
                .WithOne()
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.PickupConfirmation)
                .WithOne()
                .HasForeignKey<PickupConfirmation>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===========================
        // CUSTOMER ACCOUNT CONFIGURATION
        // ===========================
        modelBuilder.Entity<CustomerAccount>(entity =>
        {
            // Indexes for performance and uniqueness
            entity.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("IX_CustomerAccounts_Email");

            entity.HasIndex(c => c.IdNumber)
                .HasDatabaseName("IX_CustomerAccounts_IdNumber");

            entity.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_CustomerAccounts_IsActive");

            entity.HasIndex(c => c.AgeVerified)
                .HasDatabaseName("IX_CustomerAccounts_AgeVerified");

            // Relationships
            entity.HasMany(c => c.Orders)
                .WithOne()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete orders
        });

        // ===========================
        // ONLINE ORDER ITEM CONFIGURATION
        // ===========================
        modelBuilder.Entity<OnlineOrderItem>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(i => i.OrderId)
                .HasDatabaseName("IX_OnlineOrderItems_OrderId");

            entity.HasIndex(i => i.ProductCode)
                .HasDatabaseName("IX_OnlineOrderItems_ProductCode");
        });

        // ===========================
        // PAYMENT TRANSACTION CONFIGURATION
        // ===========================
        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            // Indexes for performance and uniqueness
            entity.HasIndex(p => p.OrderId)
                .HasDatabaseName("IX_PaymentTransactions_OrderId");

            entity.HasIndex(p => p.ProviderTransactionId)
                .HasDatabaseName("IX_PaymentTransactions_ProviderTransactionId");

            entity.HasIndex(p => p.ProviderReference)
                .HasDatabaseName("IX_PaymentTransactions_ProviderReference");

            entity.HasIndex(p => p.Status)
                .HasDatabaseName("IX_PaymentTransactions_Status");

            entity.HasIndex(p => p.Provider)
                .HasDatabaseName("IX_PaymentTransactions_Provider");

            entity.HasIndex(p => p.InitiatedAt)
                .HasDatabaseName("IX_PaymentTransactions_InitiatedAt");
        });

        // ===========================
        // PICKUP CONFIRMATION CONFIGURATION
        // ===========================
        modelBuilder.Entity<PickupConfirmation>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(p => p.OrderId)
                .IsUnique()
                .HasDatabaseName("IX_PickupConfirmations_OrderId");

            entity.HasIndex(p => p.VerifiedByStaffId)
                .HasDatabaseName("IX_PickupConfirmations_VerifiedByStaffId");

            entity.HasIndex(p => p.PickupDate)
                .HasDatabaseName("IX_PickupConfirmations_PickupDate");
        });

        // ===========================
        // ORDER STATUS HISTORY CONFIGURATION
        // ===========================
        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(h => h.OrderId)
                .HasDatabaseName("IX_OrderStatusHistory_OrderId");

            entity.HasIndex(h => h.ChangedAt)
                .HasDatabaseName("IX_OrderStatusHistory_ChangedAt");

            entity.HasIndex(h => h.NewStatus)
                .HasDatabaseName("IX_OrderStatusHistory_NewStatus");

            // Composite index for order status queries
            entity.HasIndex(h => new { h.OrderId, h.ChangedAt })
                .HasDatabaseName("IX_OrderStatusHistory_OrderId_ChangedAt");
        });

        // ===========================
        // GLOBAL QUERY FILTER (Soft Deletes)
        // ===========================
        // Apply soft delete filter to all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    /// <summary>
    /// Creates a soft delete query filter for the entity type
    /// </summary>
    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically populate audit fields
    /// </summary>
    /// <remarks>
    /// POPIA Compliance:
    /// - Automatically sets CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
    /// - Ensures audit trail for all data changes
    /// - DeletedAt and DeletedBy set during soft delete
    ///
    /// Current Implementation:
    /// - CreatedBy/ModifiedBy set to 1 (system user) - TODO: Use actual user ID from auth context
    /// </remarks>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = "System"; // TODO: Get from current user context
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = DateTime.UtcNow;
                entry.Entity.ModifiedBy = "System"; // TODO: Get from current user context
            }

            // Soft delete handling
            if (entry.State == EntityState.Modified && entry.Entity.IsDeleted)
            {
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.DeletedBy = "System"; // TODO: Get from current user context
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
