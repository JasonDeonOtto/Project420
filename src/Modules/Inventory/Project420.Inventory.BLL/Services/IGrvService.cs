using Project420.Inventory.BLL.DTOs;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service interface for Goods Received Voucher (GRV) operations.
/// Integrates with MovementService for stock tracking.
/// </summary>
/// <remarks>
/// Phase 12: Purchasing Workflow
///
/// Movement Architecture (Option A):
/// - GRV creates IN movements when approved
/// - TransactionDetails stores line items with TransactionType.GRV
/// - MovementService.GenerateMovementsAsync() creates stock movements
///
/// Workflow:
/// 1. Create GRV (Draft status)
/// 2. Add line items (products being received)
/// 3. Assign batch numbers (cannabis compliance)
/// 4. Verify lab certificates (cannabis compliance)
/// 5. Submit for approval
/// 6. Approve → Creates TransactionDetails → Generates IN movements
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - All received stock must have batch numbers
/// - Lab test certificates required for cannabis products
/// - Full audit trail for seed-to-sale traceability
/// </remarks>
public interface IGrvService
{
    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Create a new GRV.
    /// </summary>
    /// <param name="dto">GRV creation data including line items</param>
    /// <returns>Created GRV with generated GRV number</returns>
    Task<GrvDto> CreateGrvAsync(CreateGrvDto dto);

    /// <summary>
    /// Get a GRV by ID.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <returns>GRV DTO if found, null otherwise</returns>
    Task<GrvDto?> GetGrvByIdAsync(int grvId);

    /// <summary>
    /// Get a GRV by GRV number.
    /// </summary>
    /// <param name="grvNumber">GRV number</param>
    /// <returns>GRV DTO if found, null otherwise</returns>
    Task<GrvDto?> GetGrvByNumberAsync(string grvNumber);

    /// <summary>
    /// Get all GRVs with optional filtering.
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>List of GRVs matching criteria</returns>
    Task<List<GrvDto>> GetGrvsAsync(GrvFilterDto? filter = null);

    /// <summary>
    /// Get pending GRVs (awaiting approval).
    /// </summary>
    /// <returns>List of GRVs pending approval</returns>
    Task<List<GrvDto>> GetPendingGrvsAsync();

    // ============================================================
    // LINE ITEM OPERATIONS
    // ============================================================

    /// <summary>
    /// Add a line item to an existing draft GRV.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <param name="line">Line item to add</param>
    /// <returns>Updated GRV</returns>
    Task<GrvDto> AddLineAsync(int grvId, GrvLineDto line);

    /// <summary>
    /// Update a line item on a draft GRV.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <param name="lineIndex">Line index (0-based)</param>
    /// <param name="line">Updated line data</param>
    /// <returns>Updated GRV</returns>
    Task<GrvDto> UpdateLineAsync(int grvId, int lineIndex, GrvLineDto line);

    /// <summary>
    /// Remove a line item from a draft GRV.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <param name="lineIndex">Line index (0-based)</param>
    /// <returns>Updated GRV</returns>
    Task<GrvDto> RemoveLineAsync(int grvId, int lineIndex);

    // ============================================================
    // BATCH NUMBER OPERATIONS (Cannabis Compliance)
    // ============================================================

    /// <summary>
    /// Assign batch numbers to GRV lines.
    /// </summary>
    /// <param name="dto">Batch number assignments</param>
    /// <returns>Updated GRV</returns>
    Task<GrvDto> AssignBatchNumbersAsync(AssignBatchNumbersDto dto);

    /// <summary>
    /// Auto-generate batch numbers for all lines without batch numbers.
    /// Uses BatchNumberGeneratorService.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <returns>Updated GRV with batch numbers assigned</returns>
    Task<GrvDto> AutoGenerateBatchNumbersAsync(int grvId);

    // ============================================================
    // STATUS WORKFLOW
    // ============================================================

    /// <summary>
    /// Submit a draft GRV for approval.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <returns>Updated GRV with PendingApproval status</returns>
    Task<GrvDto> SubmitForApprovalAsync(int grvId);

    /// <summary>
    /// Approve a GRV and generate stock movements.
    /// This is the key integration point with MovementService.
    /// </summary>
    /// <param name="dto">Approval details</param>
    /// <returns>Approval result including movement count</returns>
    Task<GrvApprovalResult> ApproveGrvAsync(ApproveGrvDto dto);

    /// <summary>
    /// Reject a GRV.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <param name="reason">Rejection reason</param>
    /// <param name="rejectedBy">User rejecting</param>
    /// <returns>Updated GRV with Rejected status</returns>
    Task<GrvDto> RejectGrvAsync(int grvId, string reason, string rejectedBy);

    /// <summary>
    /// Cancel a draft or pending GRV.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancelledBy">User cancelling</param>
    /// <returns>Updated GRV with Cancelled status</returns>
    Task<GrvDto> CancelGrvAsync(int grvId, string reason, string cancelledBy);

    // ============================================================
    // VALIDATION
    // ============================================================

    /// <summary>
    /// Validate a GRV is ready for approval.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    Task<List<string>> ValidateForApprovalAsync(int grvId);

    /// <summary>
    /// Check if all lines have batch numbers assigned.
    /// </summary>
    /// <param name="grvId">GRV ID</param>
    /// <returns>True if all lines have batch numbers</returns>
    Task<bool> AllBatchNumbersAssignedAsync(int grvId);

    // ============================================================
    // REPORTING
    // ============================================================

    /// <summary>
    /// Get GRV summary statistics.
    /// </summary>
    /// <param name="dateFrom">Start date</param>
    /// <param name="dateTo">End date</param>
    /// <returns>Summary statistics</returns>
    Task<GrvSummaryDto> GetGrvSummaryAsync(DateTime dateFrom, DateTime dateTo);

    /// <summary>
    /// Get GRVs by supplier.
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>List of GRVs for the supplier</returns>
    Task<List<GrvDto>> GetGrvsBySupplierAsync(int supplierId);
}

/// <summary>
/// GRV summary statistics.
/// </summary>
public class GrvSummaryDto
{
    /// <summary>Total number of GRVs.</summary>
    public int TotalGrvs { get; set; }

    /// <summary>Number of draft GRVs.</summary>
    public int DraftCount { get; set; }

    /// <summary>Number of pending GRVs.</summary>
    public int PendingCount { get; set; }

    /// <summary>Number of approved GRVs.</summary>
    public int ApprovedCount { get; set; }

    /// <summary>Number of completed GRVs.</summary>
    public int CompletedCount { get; set; }

    /// <summary>Total value of completed GRVs.</summary>
    public decimal TotalValueCompleted { get; set; }

    /// <summary>Total items received (completed GRVs).</summary>
    public decimal TotalQuantityReceived { get; set; }

    /// <summary>Date range start.</summary>
    public DateTime DateFrom { get; set; }

    /// <summary>Date range end.</summary>
    public DateTime DateTo { get; set; }
}
