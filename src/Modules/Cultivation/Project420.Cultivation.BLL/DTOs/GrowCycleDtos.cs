namespace Project420.Cultivation.BLL.DTOs;

public class CreateGrowCycleDto
{
    public string CycleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StrainName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? PlannedHarvestDate { get; set; }
    public int? GrowRoomId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateGrowCycleDto
{
    public int Id { get; set; }
    public string CycleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StrainName { get; set; } = string.Empty;
    public DateTime? PlannedHarvestDate { get; set; }
    public int? GrowRoomId { get; set; }
    public int TotalPlantsStarted { get; set; }
    public int PlantsHarvested { get; set; }
    public decimal? TotalWetWeightGrams { get; set; }
    public decimal? TotalDryWeightGrams { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class GrowCycleDto
{
    public int Id { get; set; }
    public string CycleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StrainName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? PlannedHarvestDate { get; set; }
    public DateTime? ActualHarvestDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? GrowRoomId { get; set; }
    public string? GrowRoomName { get; set; }
    public int TotalPlantsStarted { get; set; }
    public int PlantsHarvested { get; set; }
    public decimal? TotalWetWeightGrams { get; set; }
    public decimal? TotalDryWeightGrams { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
