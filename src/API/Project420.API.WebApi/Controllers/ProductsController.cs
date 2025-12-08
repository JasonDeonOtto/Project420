using Microsoft.AspNetCore.Mvc;

namespace Project420.API.WebApi.Controllers;

/// <summary>
/// Product catalog endpoints for online ordering
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all products (paginated)
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="categoryCode">Filter by category</param>
    /// <param name="search">Search by name/description</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? categoryCode = null,
        [FromQuery] string? search = null)
    {
        // TODO: Implement product catalog retrieval
        // - Filter by category
        // - Search by name/description
        // - Include THC/CBD content
        // - Show stock availability
        // - Paginate results
        return Ok(new
        {
            message = "Products endpoint - to be implemented",
            page,
            pageSize,
            categoryCode,
            search
        });
    }

    /// <summary>
    /// Get product details by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id)
    {
        // TODO: Implement product detail retrieval
        // - Return full product details
        // - Include THC/CBD content
        // - Show batch number and lab test date
        // - Display stock availability
        return Ok(new { message = $"Product {id} endpoint - to be implemented" });
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        // TODO: Implement categories retrieval
        return Ok(new { message = "Categories endpoint - to be implemented" });
    }
}
