using System.ComponentModel.DataAnnotations;
using Project420.Production.Models.Enums;

namespace Project420.Production.BLL.DTOs;

// ============================================================
// PRE-ROLL PRODUCTION DTOs (Phase 10)
// ============================================================

/// <summary>
/// DTO for starting a pre-roll production batch.
/// </summary>
public class StartPreRollProductionDto
{
    /// <summary>
    /// Source flower batch number (from harvest or previous production).
    /// </summary>
    [Required(ErrorMessage = "Source batch number is required")]
    public string SourceBatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Strain name being processed.
    /// </summary>
    [Required(ErrorMessage = "Strain name is required")]
    [MaxLength(100)]
    public string StrainName { get; set; } = string.Empty;

    /// <summary>
    /// Input weight in grams (flower for milling).
    /// </summary>
    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Input weight must be positive")]
    public decimal InputWeightGrams { get; set; }

    /// <summary>
    /// Target package size (e.g., "0.5g", "1g").
    /// </summary>
    [MaxLength(50)]
    public string? TargetPackageSize { get; set; } = "1g";

    /// <summary>
    /// Expected output quantity (number of pre-rolls).
    /// </summary>
    public int ExpectedOutputQuantity { get; set; }

    /// <summary>
    /// Operator/user starting the production.
    /// </summary>
    [Required]
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// Notes for the production batch.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for completing a processing step.
/// </summary>
public class CompleteStepDto
{
    /// <summary>
    /// Output weight in grams after the step.
    /// </summary>
    [Required]
    [Range(0, 1000000)]
    public decimal OutputWeightGrams { get; set; }

    /// <summary>
    /// Waste/loss weight in grams.
    /// </summary>
    [Range(0, 1000000)]
    public decimal WasteWeightGrams { get; set; }

    /// <summary>
    /// Output quantity (number of units, if applicable).
    /// </summary>
    public int? OutputQuantity { get; set; }

    /// <summary>
    /// Reason for loss/waste (if any).
    /// </summary>
    [MaxLength(500)]
    public string? LossReason { get; set; }

    /// <summary>
    /// Temperature during processing (optional).
    /// </summary>
    [MaxLength(50)]
    public string? Temperature { get; set; }

    /// <summary>
    /// Humidity during processing (optional).
    /// </summary>
    [MaxLength(50)]
    public string? Humidity { get; set; }

    /// <summary>
    /// Operator completing the step.
    /// </summary>
    [Required]
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// Notes about the step.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for packaging step with serial number generation.
/// </summary>
public class PackageProductionDto
{
    /// <summary>
    /// Number of units to package.
    /// </summary>
    [Required]
    [Range(1, 100000)]
    public int UnitsToPackage { get; set; }

    /// <summary>
    /// Package size (e.g., "0.5g", "1g", "3.5g", "7g").
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PackageSize { get; set; } = string.Empty;

    /// <summary>
    /// Target product ID (the retail product being created).
    /// </summary>
    [Required]
    public int TargetProductId { get; set; }

    /// <summary>
    /// Weight per unit in grams.
    /// </summary>
    [Required]
    [Range(0.01, 10000)]
    public decimal WeightPerUnitGrams { get; set; }

    /// <summary>
    /// Whether to generate serial numbers for each unit.
    /// </summary>
    public bool GenerateSerialNumbers { get; set; } = true;

    /// <summary>
    /// Site ID for serial number generation.
    /// </summary>
    public int SiteId { get; set; } = 1;

    /// <summary>
    /// Operator performing the packaging.
    /// </summary>
    [Required]
    public string OperatorId { get; set; } = string.Empty;

    /// <summary>
    /// Notes about the packaging.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

// ============================================================
// RESULT DTOs
// ============================================================

/// <summary>
/// Result of starting a production batch.
/// </summary>
public class ProductionBatchResultDto
{
    public bool Success { get; set; }
    public int? BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public ProductionBatchStatus Status { get; set; }
}

/// <summary>
/// Result of completing a processing step.
/// </summary>
public class ProcessingStepResultDto
{
    public bool Success { get; set; }
    public int? StepId { get; set; }
    public int StepNumber { get; set; }
    public ProcessingStepType StepType { get; set; }
    public decimal OutputWeightGrams { get; set; }
    public decimal WasteWeightGrams { get; set; }
    public decimal YieldPercentage { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Result of packaging step.
/// </summary>
public class PackagingResultDto
{
    public bool Success { get; set; }
    public int UnitsPackaged { get; set; }
    public List<string> SerialNumbers { get; set; } = new();
    public decimal TotalWeightGrams { get; set; }
    public int MovementsGenerated { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime PackagingDate { get; set; }
}

/// <summary>
/// Production batch summary report.
/// </summary>
public class ProductionBatchSummaryDto
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string SourceBatchNumber { get; set; } = string.Empty;
    public string StrainName { get; set; } = string.Empty;
    public decimal StartingWeightGrams { get; set; }
    public decimal CurrentWeightGrams { get; set; }
    public decimal FinalWeightGrams { get; set; }
    public decimal TotalWasteGrams { get; set; }
    public decimal OverallYieldPercentage { get; set; }
    public int TotalStepsCompleted { get; set; }
    public int UnitsPackaged { get; set; }
    public string PackageSize { get; set; } = string.Empty;
    public ProductionBatchStatus Status { get; set; }
    public bool? QualityControlPassed { get; set; }
    public bool? LabTestPassed { get; set; }
    public string? THCPercentage { get; set; }
    public string? CBDPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public List<StepSummaryDto> Steps { get; set; } = new();
}

/// <summary>
/// Processing step summary.
/// </summary>
public class StepSummaryDto
{
    public int StepNumber { get; set; }
    public ProcessingStepType StepType { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal InputWeightGrams { get; set; }
    public decimal OutputWeightGrams { get; set; }
    public decimal WasteGrams { get; set; }
    public decimal YieldPercentage { get; set; }
    public decimal DurationHours { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
