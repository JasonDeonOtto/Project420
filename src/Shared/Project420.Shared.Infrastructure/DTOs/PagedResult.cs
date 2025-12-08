namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Generic wrapper for paginated query results
/// </summary>
/// <typeparam name="T">Type of items in the result set</typeparam>
/// <remarks>
/// Used for all paginated queries across the application.
/// Provides consistent pagination metadata.
///
/// Example Usage:
/// <code>
/// var result = new PagedResult<TransactionDto>
/// {
///     Items = transactions,
///     TotalCount = 1250,
///     PageNumber = 2,
///     PageSize = 50
/// };
/// // Automatically calculates: TotalPages = 25, HasNextPage = true, etc.
/// </code>
/// </remarks>
public class PagedResult<T>
{
    /// <summary>
    /// The items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    // ========================================
    // CALCULATED PROPERTIES
    // ========================================

    /// <summary>
    /// Total number of pages
    /// </summary>
    /// <remarks>
    /// Automatically calculated from TotalCount and PageSize.
    /// Example: 1250 items / 50 per page = 25 pages
    /// </remarks>
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Index of first item on current page (1-based)
    /// </summary>
    /// <remarks>
    /// Example: Page 2, Size 50 → FirstItemIndex = 51
    /// </remarks>
    public int FirstItemIndex => TotalCount == 0
        ? 0
        : ((PageNumber - 1) * PageSize) + 1;

    /// <summary>
    /// Index of last item on current page (1-based)
    /// </summary>
    /// <remarks>
    /// Example: Page 2, Size 50, Total 1250 → LastItemIndex = 100
    /// </remarks>
    public int LastItemIndex => Math.Min(FirstItemIndex + Items.Count - 1, TotalCount);

    /// <summary>
    /// Helper property for UI display
    /// </summary>
    /// <example>
    /// "Showing 51-100 of 1,250 results"
    /// </example>
    public string DisplayText => TotalCount == 0
        ? "No results found"
        : $"Showing {FirstItemIndex:N0}-{LastItemIndex:N0} of {TotalCount:N0} results";
}
