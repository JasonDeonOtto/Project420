namespace Project420.Production.BLL.DTOs;

public class CreateLabTestDto
{
    public int ProductionBatchId { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string? LabCertificateNumber { get; set; }
    public string COANumber { get; set; } = string.Empty;
    public DateTime SampleDate { get; set; }
    public DateTime? ResultsDate { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public string? TestResults { get; set; }
    public string? Notes { get; set; }
}

public class UpdateLabTestDto
{
    public int Id { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string? LabCertificateNumber { get; set; }
    public string COANumber { get; set; } = string.Empty;
    public DateTime SampleDate { get; set; }
    public DateTime? ResultsDate { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public string? TestResults { get; set; }
    public string? Notes { get; set; }
}

public class LabTestDto
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string? LabCertificateNumber { get; set; }
    public string COANumber { get; set; } = string.Empty;
    public DateTime SampleDate { get; set; }
    public DateTime? ResultsDate { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public string? TestResults { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
