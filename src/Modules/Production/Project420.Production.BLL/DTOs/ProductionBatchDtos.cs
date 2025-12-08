namespace Project420.Production.BLL.DTOs;

public class CreateProductionBatchDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string HarvestBatchNumber { get; set; } = string.Empty;
    public string StrainName { get; set; } = string.Empty;
    public decimal StartingWeightGrams { get; set; }
    public DateTime StartDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProductionBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal? CurrentWeightGrams { get; set; }
    public decimal? FinalWeightGrams { get; set; }
    public decimal? WasteWeightGrams { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Status { get; set; }
    public bool? QualityControlPassed { get; set; }
    public bool? LabTestPassed { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public int? UnitsPackaged { get; set; }
    public string? PackageSize { get; set; }
    public DateTime? PackagingDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class ProductionBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string HarvestBatchNumber { get; set; } = string.Empty;
    public string StrainName { get; set; } = string.Empty;
    public decimal StartingWeightGrams { get; set; }
    public decimal? CurrentWeightGrams { get; set; }
    public decimal? FinalWeightGrams { get; set; }
    public decimal? WasteWeightGrams { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? Status { get; set; }
    public bool? QualityControlPassed { get; set; }
    public bool? LabTestPassed { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public int? UnitsPackaged { get; set; }
    public string? PackageSize { get; set; }
    public DateTime? PackagingDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
