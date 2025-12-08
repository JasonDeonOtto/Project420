namespace Project420.OnlineOrders.BLL.DTOs.Response;

/// <summary>
/// DTO for product catalog listing
/// </summary>
public class ProductCatalogDto
{
    public List<ProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Product DTO
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal PriceInclVAT { get; set; }
    public decimal? ThcContent { get; set; }
    public decimal? CbdContent { get; set; }
    public string? StrainType { get; set; }
    public bool StockAvailable { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? LabTestDate { get; set; }
    public string? BatchNumber { get; set; }
}
