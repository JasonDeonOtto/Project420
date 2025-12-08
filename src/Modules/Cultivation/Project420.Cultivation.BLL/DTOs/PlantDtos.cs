using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.BLL.DTOs;

/// <summary>
/// DTO for creating a new plant record.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public class CreatePlantDto
{
    public string PlantTag { get; set; } = string.Empty;
    public int GrowCycleId { get; set; }
    public string? StrainName { get; set; }
    public PlantStage CurrentStage { get; set; } = PlantStage.Seed;
    public DateTime PlantedDate { get; set; }
    public int? CurrentGrowRoomId { get; set; }
    public string? PlantSource { get; set; } // "Seed" or "Clone"
    public int? MotherPlantId { get; set; }
    public string? PlantSex { get; set; } // "Male", "Female", "Hermaphrodite", "Unknown"
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing plant record.
/// </summary>
public class UpdatePlantDto
{
    public int Id { get; set; }
    public string PlantTag { get; set; } = string.Empty;
    public PlantStage CurrentStage { get; set; }
    public DateTime? StageStartDate { get; set; }
    public int? CurrentGrowRoomId { get; set; }
    public string? PlantSex { get; set; }
    public string? HealthStatus { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for displaying plant information.
/// </summary>
public class PlantDto
{
    public int Id { get; set; }
    public string PlantTag { get; set; } = string.Empty;
    public int GrowCycleId { get; set; }
    public string? GrowCycleName { get; set; }
    public string? StrainName { get; set; }
    public PlantStage CurrentStage { get; set; }
    public DateTime? StageStartDate { get; set; }
    public DateTime PlantedDate { get; set; }
    public DateTime? HarvestDate { get; set; }
    public int? CurrentGrowRoomId { get; set; }
    public string? CurrentGrowRoomName { get; set; }
    public string? PlantSource { get; set; }
    public int? MotherPlantId { get; set; }
    public string? PlantSex { get; set; }
    public decimal? WetWeightGrams { get; set; }
    public decimal? DryWeightGrams { get; set; }
    public int? HarvestBatchId { get; set; }
    public string? HealthStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime? DestroyedDate { get; set; }
    public string? DestructionReason { get; set; }
    public decimal? WasteWeightGrams { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
