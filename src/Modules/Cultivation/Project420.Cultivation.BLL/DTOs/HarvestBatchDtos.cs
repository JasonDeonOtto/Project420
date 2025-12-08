namespace Project420.Cultivation.BLL.DTOs;

public class CreateHarvestBatchDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int GrowCycleId { get; set; }
    public string StrainName { get; set; } = string.Empty;
    public DateTime HarvestDate { get; set; }
    public decimal TotalWetWeightGrams { get; set; }
    public int PlantCount { get; set; }
    public string? StorageLocation { get; set; }
    public string? Notes { get; set; }
}

public class UpdateHarvestBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime? DryDate { get; set; }
    public DateTime? CureDate { get; set; }
    public decimal? TotalDryWeightGrams { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public DateTime? LabTestDate { get; set; }
    public string? LabTestCertificateNumber { get; set; }
    public bool? LabTestPassed { get; set; }
    public string? ProcessingStatus { get; set; }
    public bool IsActive { get; set; }
    public string? StorageLocation { get; set; }
    public string? Notes { get; set; }
}

public class HarvestBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int GrowCycleId { get; set; }
    public string? GrowCycleName { get; set; }
    public string StrainName { get; set; } = string.Empty;
    public DateTime HarvestDate { get; set; }
    public DateTime? DryDate { get; set; }
    public DateTime? CureDate { get; set; }
    public decimal TotalWetWeightGrams { get; set; }
    public decimal? TotalDryWeightGrams { get; set; }
    public int PlantCount { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public DateTime? LabTestDate { get; set; }
    public string? LabTestCertificateNumber { get; set; }
    public bool? LabTestPassed { get; set; }
    public string? ProcessingStatus { get; set; }
    public bool IsActive { get; set; }
    public string? StorageLocation { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
