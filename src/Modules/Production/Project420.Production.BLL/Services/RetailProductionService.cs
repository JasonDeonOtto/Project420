using Microsoft.Extensions.Logging;
using Project420.Production.BLL.DTOs;
using Project420.Production.DAL.Repositories;
using Project420.Production.Models.Entities;
using Project420.Production.Models.Enums;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;

namespace Project420.Production.BLL.Services;

/// <summary>
/// Service implementation for retail production workflows.
/// </summary>
/// <remarks>
/// Phase 10: Production DAL Expansion - Retail Production Service
///
/// Integrations:
/// - MovementService: Creates ProductionInput (OUT) and ProductionOutput (IN) movements
/// - BatchNumberGeneratorService: Generates unique production batch numbers
/// - SerialNumberGeneratorService: Generates serial numbers for packaged units
///
/// SAHPRA GMP Compliance:
/// - Full weight tracking at each step
/// - Environmental condition documentation
/// - Yield calculations for compliance reporting
/// - Audit trail via AuditableEntity
/// </remarks>
public class RetailProductionService : IRetailProductionService
{
    private readonly IProductionBatchRepository _batchRepository;
    private readonly IProcessingStepRepository _stepRepository;
    private readonly IMovementService _movementService;
    private readonly IBatchNumberGeneratorService _batchNumberGenerator;
    private readonly ISerialNumberGeneratorService _serialNumberGenerator;
    private readonly ILogger<RetailProductionService> _logger;

    public RetailProductionService(
        IProductionBatchRepository batchRepository,
        IProcessingStepRepository stepRepository,
        IMovementService movementService,
        IBatchNumberGeneratorService batchNumberGenerator,
        ISerialNumberGeneratorService serialNumberGenerator,
        ILogger<RetailProductionService> logger)
    {
        _batchRepository = batchRepository ?? throw new ArgumentNullException(nameof(batchRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _movementService = movementService ?? throw new ArgumentNullException(nameof(movementService));
        _batchNumberGenerator = batchNumberGenerator ?? throw new ArgumentNullException(nameof(batchNumberGenerator));
        _serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ============================================================
    // PRE-ROLL PRODUCTION WORKFLOW
    // ============================================================

    /// <inheritdoc/>
    public async Task<ProductionBatchResultDto> StartPreRollProductionAsync(StartPreRollProductionDto dto)
    {
        var result = new ProductionBatchResultDto();

        try
        {
            _logger.LogInformation(
                "Starting pre-roll production from source batch {SourceBatch}, strain {Strain}, weight {Weight}g",
                dto.SourceBatchNumber, dto.StrainName, dto.InputWeightGrams);

            // Generate unique batch number using BatchType.Production enum
            var batchNumber = await _batchNumberGenerator.GenerateBatchNumberAsync(
                1, // Default site ID
                BatchType.Production,
                DateTime.UtcNow,
                dto.OperatorId);

            // Create production batch
            var batch = new ProductionBatch
            {
                BatchNumber = batchNumber,
                Name = $"Pre-Roll Production - {dto.StrainName}",
                HarvestBatchNumber = dto.SourceBatchNumber,
                StrainName = dto.StrainName,
                StartingWeightGrams = dto.InputWeightGrams,
                CurrentWeightGrams = dto.InputWeightGrams,
                StartDate = DateTime.UtcNow,
                ProductionBatchStatus = ProductionBatchStatus.InProcess,
                IsActive = true,
                PackageSize = dto.TargetPackageSize,
                Notes = dto.Notes,
                CreatedBy = dto.OperatorId,
                CreatedAt = DateTime.UtcNow
            };

            // AddAsync already calls SaveChangesAsync internally
            await _batchRepository.AddAsync(batch);

            // Create initial milling step
            var millingStep = new ProcessingStep
            {
                ProductionBatchId = batch.Id,
                StepType = ProcessingStepType.Extraction, // Using Extraction for milling
                StepNumber = 1,
                StartTime = DateTime.UtcNow,
                StartWeightGrams = dto.InputWeightGrams,
                Status = "In Progress",
                PerformedBy = dto.OperatorId,
                Notes = "Pre-roll milling step initiated",
                CreatedBy = dto.OperatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _stepRepository.AddAsync(millingStep);

            // Create ProductionInput movement (OUT from inventory)
            await _movementService.CreateMovementAsync(new Movement
            {
                ProductId = 0, // Raw flower doesn't have specific product ID
                ProductSKU = dto.SourceBatchNumber,
                ProductName = $"{dto.StrainName} (Raw Flower)",
                MovementType = "Production Input",
                Direction = MovementDirection.Out,
                Quantity = dto.InputWeightGrams,
                Mass = dto.InputWeightGrams,
                Value = 0, // Cost TBD
                BatchNumber = dto.SourceBatchNumber,
                TransactionType = TransactionType.ProductionInput,
                HeaderId = batch.Id,
                MovementReason = $"Raw material input for pre-roll production batch {batchNumber}",
                TransactionDate = DateTime.UtcNow,
                UserId = dto.OperatorId
            });

            result.Success = true;
            result.BatchId = batch.Id;
            result.BatchNumber = batchNumber;
            result.StartDate = batch.StartDate;
            result.Status = batch.ProductionBatchStatus;

            _logger.LogInformation(
                "Pre-roll production batch {BatchNumber} started successfully. BatchId: {BatchId}",
                batchNumber, batch.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start pre-roll production from source batch {SourceBatch}", dto.SourceBatchNumber);
            result.Success = false;
            result.Errors.Add($"Failed to start production: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProcessingStepResultDto> CompleteMillingStepAsync(int batchId, CompleteStepDto dto)
    {
        return await CompleteProcessingStepAsync(batchId, ProcessingStepType.Extraction, dto);
    }

    /// <inheritdoc/>
    public async Task<ProcessingStepResultDto> CompleteFillingStepAsync(int batchId, CompleteStepDto dto)
    {
        return await CompleteProcessingStepInternalAsync(batchId, ProcessingStepType.Packaging, dto, 2, "Filling");
    }

    /// <inheritdoc/>
    public async Task<ProcessingStepResultDto> CompleteCappingStepAsync(int batchId, CompleteStepDto dto)
    {
        return await CompleteProcessingStepInternalAsync(batchId, ProcessingStepType.Packaging, dto, 3, "Capping");
    }

    // ============================================================
    // PACKAGED FLOWER PRODUCTION WORKFLOW
    // ============================================================

    /// <inheritdoc/>
    public async Task<ProductionBatchResultDto> StartPackagedFlowerProductionAsync(StartPreRollProductionDto dto)
    {
        var result = new ProductionBatchResultDto();

        try
        {
            _logger.LogInformation(
                "Starting packaged flower production from source batch {SourceBatch}, strain {Strain}, weight {Weight}g",
                dto.SourceBatchNumber, dto.StrainName, dto.InputWeightGrams);

            // Generate unique batch number
            var batchNumber = await _batchNumberGenerator.GenerateBatchNumberAsync(
                1,
                BatchType.Production,
                DateTime.UtcNow,
                dto.OperatorId);

            // Create production batch
            var batch = new ProductionBatch
            {
                BatchNumber = batchNumber,
                Name = $"Packaged Flower - {dto.StrainName} ({dto.TargetPackageSize})",
                HarvestBatchNumber = dto.SourceBatchNumber,
                StrainName = dto.StrainName,
                StartingWeightGrams = dto.InputWeightGrams,
                CurrentWeightGrams = dto.InputWeightGrams,
                StartDate = DateTime.UtcNow,
                ProductionBatchStatus = ProductionBatchStatus.InProcess,
                IsActive = true,
                PackageSize = dto.TargetPackageSize,
                Notes = dto.Notes,
                CreatedBy = dto.OperatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _batchRepository.AddAsync(batch);

            // Create initial selection step
            var selectionStep = new ProcessingStep
            {
                ProductionBatchId = batch.Id,
                StepType = ProcessingStepType.QualityControl,
                StepNumber = 1,
                StartTime = DateTime.UtcNow,
                StartWeightGrams = dto.InputWeightGrams,
                Status = "In Progress",
                PerformedBy = dto.OperatorId,
                Notes = "Flower selection step initiated",
                CreatedBy = dto.OperatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _stepRepository.AddAsync(selectionStep);

            // Create ProductionInput movement
            await _movementService.CreateMovementAsync(new Movement
            {
                ProductId = 0,
                ProductSKU = dto.SourceBatchNumber,
                ProductName = $"{dto.StrainName} (Raw Flower)",
                MovementType = "Production Input",
                Direction = MovementDirection.Out,
                Quantity = dto.InputWeightGrams,
                Mass = dto.InputWeightGrams,
                Value = 0,
                BatchNumber = dto.SourceBatchNumber,
                TransactionType = TransactionType.ProductionInput,
                HeaderId = batch.Id,
                MovementReason = $"Raw material input for packaged flower batch {batchNumber}",
                TransactionDate = DateTime.UtcNow,
                UserId = dto.OperatorId
            });

            result.Success = true;
            result.BatchId = batch.Id;
            result.BatchNumber = batchNumber;
            result.StartDate = batch.StartDate;
            result.Status = batch.ProductionBatchStatus;

            _logger.LogInformation(
                "Packaged flower production batch {BatchNumber} started. BatchId: {BatchId}",
                batchNumber, batch.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start packaged flower production from source batch {SourceBatch}", dto.SourceBatchNumber);
            result.Success = false;
            result.Errors.Add($"Failed to start production: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProcessingStepResultDto> CompleteSelectionStepAsync(int batchId, CompleteStepDto dto)
    {
        return await CompleteProcessingStepAsync(batchId, ProcessingStepType.QualityControl, dto);
    }

    /// <inheritdoc/>
    public async Task<ProcessingStepResultDto> CompleteWeighingStepAsync(int batchId, CompleteStepDto dto)
    {
        return await CompleteProcessingStepInternalAsync(batchId, ProcessingStepType.Packaging, dto, 2, "Weighing");
    }

    // ============================================================
    // COMMON PRODUCTION OPERATIONS
    // ============================================================

    /// <inheritdoc/>
    public async Task<PackagingResultDto> PackageAndGenerateSerialNumbersAsync(int batchId, PackageProductionDto dto)
    {
        var result = new PackagingResultDto();

        try
        {
            _logger.LogInformation(
                "Packaging batch {BatchId}: {Units} units at {Size}",
                batchId, dto.UnitsToPackage, dto.PackageSize);

            var batch = await _batchRepository.GetByIdAsync(batchId);
            if (batch == null)
            {
                result.Errors.Add($"Production batch {batchId} not found");
                return result;
            }

            // Generate serial numbers for each unit using bulk generation
            var serialNumbers = new List<string>();
            if (dto.GenerateSerialNumbers)
            {
                var strainCode = GetStrainCodeFromName(batch.StrainName);
                var batchSequence = ExtractBatchSequence(batch.BatchNumber);

                var snResults = await _serialNumberGenerator.GenerateBulkSerialNumbersAsync(
                    count: dto.UnitsToPackage,
                    siteId: dto.SiteId,
                    strainCode: strainCode,
                    batchType: BatchType.Production,
                    productionDate: batch.StartDate,
                    batchSequence: batchSequence,
                    weightGrams: dto.WeightPerUnitGrams,
                    packQty: 1,
                    requestedBy: dto.OperatorId);

                serialNumbers = snResults.Select(sn => sn.FullSerialNumber).ToList();
            }

            // Update batch packaging info
            batch.UnitsPackaged = dto.UnitsToPackage;
            batch.PackageSize = dto.PackageSize;
            batch.PackagingDate = DateTime.UtcNow;
            batch.FinalWeightGrams = dto.UnitsToPackage * dto.WeightPerUnitGrams;
            batch.ModifiedBy = dto.OperatorId;
            batch.ModifiedAt = DateTime.UtcNow;

            await _batchRepository.UpdateAsync(batch);

            // Create packaging step
            var packagingStep = new ProcessingStep
            {
                ProductionBatchId = batchId,
                StepType = ProcessingStepType.Packaging,
                StepNumber = await GetNextStepNumberAsync(batchId),
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                StartWeightGrams = batch.CurrentWeightGrams,
                EndWeightGrams = batch.FinalWeightGrams,
                Status = "Completed",
                PerformedBy = dto.OperatorId,
                Notes = $"Packaged {dto.UnitsToPackage} units of {dto.PackageSize}",
                CreatedBy = dto.OperatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _stepRepository.AddAsync(packagingStep);

            // Create ProductionOutput movement (IN to inventory)
            var totalWeight = dto.UnitsToPackage * dto.WeightPerUnitGrams;

            await _movementService.CreateMovementAsync(new Movement
            {
                ProductId = dto.TargetProductId,
                ProductSKU = $"{batch.BatchNumber}-PKG",
                ProductName = $"{batch.StrainName} {dto.PackageSize}",
                MovementType = "Production Output",
                Direction = MovementDirection.In,
                Quantity = dto.UnitsToPackage,
                Mass = totalWeight,
                Value = 0, // Cost TBD
                BatchNumber = batch.BatchNumber,
                TransactionType = TransactionType.ProductionOutput,
                HeaderId = batchId,
                MovementReason = $"Packaged product output from batch {batch.BatchNumber}",
                TransactionDate = DateTime.UtcNow,
                UserId = dto.OperatorId
            });

            result.Success = true;
            result.UnitsPackaged = dto.UnitsToPackage;
            result.SerialNumbers = serialNumbers;
            result.TotalWeightGrams = totalWeight;
            result.MovementsGenerated = 1;
            result.PackagingDate = DateTime.UtcNow;

            _logger.LogInformation(
                "Packaging complete for batch {BatchId}: {Units} units, {SerialCount} serial numbers generated",
                batchId, dto.UnitsToPackage, serialNumbers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to package batch {BatchId}", batchId);
            result.Errors.Add($"Packaging failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProcessingStepResultDto> CompleteProcessingStepAsync(int batchId, ProcessingStepType stepType, CompleteStepDto dto)
    {
        return await CompleteProcessingStepInternalAsync(batchId, stepType, dto, 0, stepType.ToString());
    }

    private async Task<ProcessingStepResultDto> CompleteProcessingStepInternalAsync(
        int batchId, ProcessingStepType stepType, CompleteStepDto dto, int fixedStepNumber, string stepName)
    {
        var result = new ProcessingStepResultDto { StepType = stepType };

        try
        {
            _logger.LogInformation(
                "Completing {StepName} step for batch {BatchId}, output: {Output}g",
                stepName, batchId, dto.OutputWeightGrams);

            var batch = await _batchRepository.GetByIdWithProcessingStepDataAsync(batchId);
            if (batch == null)
            {
                result.Errors.Add($"Production batch {batchId} not found");
                return result;
            }

            // Find the current in-progress step or create new one
            var currentStep = batch.ProcessingSteps
                .Where(s => s.Status == "In Progress" && (fixedStepNumber == 0 || s.StepNumber == fixedStepNumber))
                .OrderBy(s => s.StepNumber)
                .FirstOrDefault();

            int stepNumber;
            decimal inputWeight;

            if (currentStep != null)
            {
                // Complete existing step
                currentStep.EndTime = DateTime.UtcNow;
                currentStep.EndWeightGrams = dto.OutputWeightGrams;
                currentStep.WasteGrams = dto.WasteWeightGrams;
                currentStep.DurationHours = (decimal)(DateTime.UtcNow - currentStep.StartTime).TotalHours;
                currentStep.Status = "Completed";
                currentStep.Temperature = dto.Temperature;
                currentStep.Humidity = dto.Humidity;
                currentStep.Notes = dto.Notes ?? currentStep.Notes;
                currentStep.ModifiedBy = dto.OperatorId;
                currentStep.ModifiedAt = DateTime.UtcNow;

                stepNumber = currentStep.StepNumber;
                inputWeight = currentStep.StartWeightGrams ?? batch.CurrentWeightGrams ?? batch.StartingWeightGrams;
                await _stepRepository.UpdateAsync(currentStep);
                result.StepId = currentStep.Id;
            }
            else
            {
                // Create and complete new step
                stepNumber = fixedStepNumber > 0 ? fixedStepNumber : await GetNextStepNumberAsync(batchId);
                inputWeight = batch.CurrentWeightGrams ?? batch.StartingWeightGrams;

                var newStep = new ProcessingStep
                {
                    ProductionBatchId = batchId,
                    StepType = stepType,
                    StepNumber = stepNumber,
                    StartTime = DateTime.UtcNow.AddMinutes(-5), // Assume just started
                    EndTime = DateTime.UtcNow,
                    StartWeightGrams = inputWeight,
                    EndWeightGrams = dto.OutputWeightGrams,
                    WasteGrams = dto.WasteWeightGrams,
                    DurationHours = 0,
                    Status = "Completed",
                    Temperature = dto.Temperature,
                    Humidity = dto.Humidity,
                    PerformedBy = dto.OperatorId,
                    Notes = dto.Notes,
                    CreatedBy = dto.OperatorId,
                    CreatedAt = DateTime.UtcNow
                };

                await _stepRepository.AddAsync(newStep);
                result.StepId = newStep.Id;
            }

            // Update batch current weight
            batch.CurrentWeightGrams = dto.OutputWeightGrams;
            batch.WasteWeightGrams = (batch.WasteWeightGrams ?? 0) + dto.WasteWeightGrams;
            batch.ModifiedBy = dto.OperatorId;
            batch.ModifiedAt = DateTime.UtcNow;

            await _batchRepository.UpdateAsync(batch);

            // Calculate yield
            var yieldPercentage = inputWeight > 0 ? (dto.OutputWeightGrams / inputWeight) * 100 : 0;

            result.Success = true;
            result.StepNumber = stepNumber;
            result.OutputWeightGrams = dto.OutputWeightGrams;
            result.WasteWeightGrams = dto.WasteWeightGrams;
            result.YieldPercentage = yieldPercentage;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "{StepName} step completed for batch {BatchId}. Yield: {Yield:F1}%",
                stepName, batchId, yieldPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete {StepName} step for batch {BatchId}", stepName, batchId);
            result.Errors.Add($"Step completion failed: {ex.Message}");
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProductionBatchSummaryDto?> GetBatchSummaryAsync(int batchId)
    {
        var batch = await _batchRepository.GetByIdWithAllRelatedDataAsync(batchId);
        if (batch == null)
            return null;

        return MapToSummaryDto(batch);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductionBatchSummaryDto>> GetActiveBatchesAsync()
    {
        var batches = await _batchRepository.GetActiveBatchesAsync();
        return batches.Select(MapToSummaryDto);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductionBatchSummaryDto>> GetCompletedBatchesAsync(DateTime startDate, DateTime endDate)
    {
        var batches = await _batchRepository.GetCompletedBatchesAsync(startDate, endDate);
        return batches.Select(MapToSummaryDto);
    }

    // ============================================================
    // QUALITY CONTROL & LAB TESTING
    // ============================================================

    /// <inheritdoc/>
    public async Task<bool> RecordQualityControlAsync(int batchId, bool passed, string notes, string operatorId)
    {
        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchId);
            if (batch == null)
                return false;

            batch.QualityControlPassed = passed;
            batch.Notes = string.IsNullOrEmpty(batch.Notes) ? notes : $"{batch.Notes}\nQC: {notes}";
            batch.ModifiedBy = operatorId;
            batch.ModifiedAt = DateTime.UtcNow;

            if (!passed)
            {
                batch.ProductionBatchStatus = ProductionBatchStatus.LabFailed;
            }

            await _batchRepository.UpdateAsync(batch);

            _logger.LogInformation("QC recorded for batch {BatchId}: {Result}", batchId, passed ? "PASSED" : "FAILED");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record QC for batch {BatchId}", batchId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RecordLabTestAsync(int batchId, bool passed, string thcPercentage, string cbdPercentage, string notes)
    {
        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchId);
            if (batch == null)
                return false;

            batch.LabTestPassed = passed;
            batch.THCPercentage = thcPercentage;
            batch.CBDPercentage = cbdPercentage;
            batch.Notes = string.IsNullOrEmpty(batch.Notes) ? notes : $"{batch.Notes}\nLab: {notes}";
            batch.ModifiedAt = DateTime.UtcNow;

            if (!passed)
            {
                batch.ProductionBatchStatus = ProductionBatchStatus.LabFailed;
            }

            await _batchRepository.UpdateAsync(batch);

            _logger.LogInformation("Lab test recorded for batch {BatchId}: {Result}, THC: {THC}, CBD: {CBD}",
                batchId, passed ? "PASSED" : "FAILED", thcPercentage, cbdPercentage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record lab test for batch {BatchId}", batchId);
            return false;
        }
    }

    // ============================================================
    // BATCH STATUS MANAGEMENT
    // ============================================================

    /// <inheritdoc/>
    public async Task<bool> CompleteBatchAsync(int batchId, string operatorId)
    {
        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchId);
            if (batch == null)
                return false;

            batch.ProductionBatchStatus = ProductionBatchStatus.Completed;
            batch.CompletionDate = DateTime.UtcNow;
            batch.ModifiedBy = operatorId;
            batch.ModifiedAt = DateTime.UtcNow;

            await _batchRepository.UpdateAsync(batch);

            _logger.LogInformation("Production batch {BatchId} completed by {Operator}", batchId, operatorId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete batch {BatchId}", batchId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CancelBatchAsync(int batchId, string reason, string operatorId)
    {
        try
        {
            var batch = await _batchRepository.GetByIdAsync(batchId);
            if (batch == null)
                return false;

            batch.ProductionBatchStatus = ProductionBatchStatus.Cancelled;
            batch.IsActive = false;
            batch.Notes = string.IsNullOrEmpty(batch.Notes) ? $"Cancelled: {reason}" : $"{batch.Notes}\nCancelled: {reason}";
            batch.ModifiedBy = operatorId;
            batch.ModifiedAt = DateTime.UtcNow;

            await _batchRepository.UpdateAsync(batch);

            _logger.LogWarning("Production batch {BatchId} cancelled by {Operator}. Reason: {Reason}", batchId, operatorId, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel batch {BatchId}", batchId);
            return false;
        }
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    private async Task<int> GetNextStepNumberAsync(int batchId)
    {
        var batch = await _batchRepository.GetByIdWithProcessingStepDataAsync(batchId);
        if (batch == null || batch.ProcessingSteps.Count == 0)
            return 1;

        return batch.ProcessingSteps.Max(s => s.StepNumber) + 1;
    }

    private ProductionBatchSummaryDto MapToSummaryDto(ProductionBatch batch)
    {
        var yieldPercentage = batch.StartingWeightGrams > 0
            ? ((batch.FinalWeightGrams ?? batch.CurrentWeightGrams ?? 0) / batch.StartingWeightGrams) * 100
            : 0;

        return new ProductionBatchSummaryDto
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            SourceBatchNumber = batch.HarvestBatchNumber,
            StrainName = batch.StrainName,
            StartingWeightGrams = batch.StartingWeightGrams,
            CurrentWeightGrams = batch.CurrentWeightGrams ?? 0,
            FinalWeightGrams = batch.FinalWeightGrams ?? 0,
            TotalWasteGrams = batch.WasteWeightGrams ?? 0,
            OverallYieldPercentage = yieldPercentage,
            TotalStepsCompleted = batch.ProcessingSteps?.Count(s => s.Status == "Completed") ?? 0,
            UnitsPackaged = batch.UnitsPackaged ?? 0,
            PackageSize = batch.PackageSize ?? string.Empty,
            Status = batch.ProductionBatchStatus,
            QualityControlPassed = batch.QualityControlPassed,
            LabTestPassed = batch.LabTestPassed,
            THCPercentage = batch.THCPercentage,
            CBDPercentage = batch.CBDPercentage,
            StartDate = batch.StartDate,
            CompletionDate = batch.CompletionDate,
            Steps = batch.ProcessingSteps?.Select(s => new StepSummaryDto
            {
                StepNumber = s.StepNumber,
                StepType = s.StepType,
                Status = s.Status ?? "Unknown",
                InputWeightGrams = s.StartWeightGrams ?? 0,
                OutputWeightGrams = s.EndWeightGrams ?? 0,
                WasteGrams = s.WasteGrams ?? 0,
                YieldPercentage = s.StartWeightGrams > 0 ? ((s.EndWeightGrams ?? 0) / s.StartWeightGrams.Value) * 100 : 0,
                DurationHours = s.DurationHours ?? 0,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList() ?? new List<StepSummaryDto>()
        };
    }

    /// <summary>
    /// Get strain code from strain name (100-999).
    /// In production, this would lookup from a strain table.
    /// </summary>
    private static int GetStrainCodeFromName(string strainName)
    {
        // Simple hash-based code generation (in production, lookup from strain table)
        // 100-199: Sativa, 200-299: Indica, 300-399: Hybrid, 400-499: CBD
        var hash = Math.Abs(strainName.GetHashCode());
        return 100 + (hash % 400); // Returns 100-499
    }

    /// <summary>
    /// Extract batch sequence number from batch number string.
    /// Batch format: SSTTYYYYMMDDNNNN - last 4 digits are sequence.
    /// </summary>
    private static int ExtractBatchSequence(string batchNumber)
    {
        if (string.IsNullOrEmpty(batchNumber) || batchNumber.Length < 4)
            return 1;

        // Try to parse last 4 characters as sequence
        var sequenceStr = batchNumber[^4..];
        if (int.TryParse(sequenceStr, out int sequence))
            return sequence;

        return 1;
    }
}
