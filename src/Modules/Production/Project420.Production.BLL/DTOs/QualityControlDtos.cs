using Project420.Production.Models.Enums;

namespace Project420.Production.BLL.DTOs;

public class CreateQualityControlDto
{
    public int ProductionBatchId { get; set; }
    public QualityCheckType CheckType { get; set; }
    public DateTime CheckDate { get; set; }
    public string Inspector { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? TestResults { get; set; }
    public string? Notes { get; set; }
}

public class UpdateQualityControlDto
{
    public int Id { get; set; }
    public QualityCheckType CheckType { get; set; }
    public DateTime CheckDate { get; set; }
    public string Inspector { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? TestResults { get; set; }
    public string? Notes { get; set; }
}

public class QualityControlDto
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public QualityCheckType CheckType { get; set; }
    public DateTime CheckDate { get; set; }
    public string Inspector { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? TestResults { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
