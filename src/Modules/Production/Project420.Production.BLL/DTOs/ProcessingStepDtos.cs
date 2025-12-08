using Project420.Production.Models.Enums;

namespace Project420.Production.BLL.DTOs;

public class CreateProcessingStepDto
{
    public int ProductionBatchId { get; set; }
    public ProcessingStepType StepType { get; set; }
    public DateTime StepDate { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? StepDetails { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProcessingStepDto
{
    public int Id { get; set; }
    public ProcessingStepType StepType { get; set; }
    public DateTime StepDate { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? StepDetails { get; set; }
    public string? Notes { get; set; }
}

public class ProcessingStepDto
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public ProcessingStepType StepType { get; set; }
    public DateTime StepDate { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public string? StepDetails { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
