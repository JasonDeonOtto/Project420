using FluentValidation;
using Project420.Cultivation.BLL.DTOs;
using Project420.Cultivation.DAL.Repositories;
using Project420.Cultivation.Models.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.BLL.Services;

/// <summary>
/// Service for plant business logic.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;
    private readonly IValidator<CreatePlantDto> _createValidator;
    private readonly IValidator<UpdatePlantDto> _updateValidator;

    public PlantService(
        IPlantRepository plantRepository,
        IValidator<CreatePlantDto> createValidator,
        IValidator<UpdatePlantDto> updateValidator)
    {
        _plantRepository = plantRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<int> CreatePlantAsync(CreatePlantDto dto)
    {
        // Validate input
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var plant = new Plant
        {
            PlantTag = dto.PlantTag,
            GrowCycleId = dto.GrowCycleId,
            StrainName = dto.StrainName,
            CurrentStage = dto.CurrentStage,
            StageStartDate = DateTime.UtcNow,
            PlantedDate = dto.PlantedDate,
            CurrentGrowRoomId = dto.CurrentGrowRoomId,
            PlantSource = dto.PlantSource,
            MotherPlantId = dto.MotherPlantId,
            PlantSex = dto.PlantSex,
            Notes = dto.Notes,
            IsActive = true,
            CreatedBy = "System", // TODO: Get from user context
            ModifiedBy = "System"
        };

        var createdPlant = await _plantRepository.AddAsync(plant);
        return createdPlant.Id;
    }

    public async Task UpdatePlantAsync(UpdatePlantDto dto)
    {
        // Validate input
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var plant = await _plantRepository.GetByIdAsync(dto.Id);
        if (plant == null)
        {
            throw new InvalidOperationException($"Plant with ID {dto.Id} not found");
        }

        plant.PlantTag = dto.PlantTag;
        plant.CurrentStage = dto.CurrentStage;
        plant.StageStartDate = dto.StageStartDate ?? plant.StageStartDate;
        plant.CurrentGrowRoomId = dto.CurrentGrowRoomId;
        plant.PlantSex = dto.PlantSex;
        plant.HealthStatus = dto.HealthStatus;
        plant.Notes = dto.Notes;
        plant.IsActive = dto.IsActive;
        plant.ModifiedAt = DateTime.UtcNow;
        plant.ModifiedBy = "System"; // TODO: Get from user context

        await _plantRepository.UpdateAsync(plant);
    }

    public async Task<PlantDto?> GetPlantByIdAsync(int id)
    {
        var plant = await _plantRepository.GetByIdAsync(id);
        return plant != null ? MapToDto(plant) : null;
    }

    public async Task<IEnumerable<PlantDto>> GetAllPlantsAsync()
    {
        var plants = await _plantRepository.GetAllAsync();
        return plants.Select(MapToDto);
    }

    public async Task DeactivatePlantAsync(int id)
    {
        await _plantRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<PlantDto>> GetPlantsByGrowCycleAsync(int growCycleId)
    {
        var plants = await _plantRepository.FindAsync(p => p.GrowCycleId == growCycleId);
        return plants.Select(MapToDto);
    }

    public async Task<IEnumerable<PlantDto>> GetPlantsByStageAsync(PlantStage stage)
    {
        var plants = await _plantRepository.FindAsync(p => p.CurrentStage == stage);
        return plants.Select(MapToDto);
    }

    public async Task RecordHarvestAsync(int plantId, DateTime harvestDate, decimal wetWeightGrams, decimal? dryWeightGrams = null)
    {
        var plant = await _plantRepository.GetByIdAsync(plantId);
        if (plant == null)
        {
            throw new InvalidOperationException($"Plant with ID {plantId} not found");
        }

        plant.HarvestDate = harvestDate;
        plant.WetWeightGrams = wetWeightGrams;
        plant.DryWeightGrams = dryWeightGrams;
        plant.IsActive = false; // Plant is no longer actively growing

        await _plantRepository.UpdateAsync(plant);
    }

    public async Task RecordDestructionAsync(int plantId, DateTime destructionDate, string reason, decimal wasteWeightGrams)
    {
        var plant = await _plantRepository.GetByIdAsync(plantId);
        if (plant == null)
        {
            throw new InvalidOperationException($"Plant with ID {plantId} not found");
        }

        plant.DestroyedDate = destructionDate;
        plant.DestructionReason = reason;
        plant.WasteWeightGrams = wasteWeightGrams;
        plant.IsActive = false;

        await _plantRepository.UpdateAsync(plant);
    }

    private PlantDto MapToDto(Plant plant)
    {
        return new PlantDto
        {
            Id = plant.Id,
            PlantTag = plant.PlantTag,
            GrowCycleId = plant.GrowCycleId,
            StrainName = plant.StrainName,
            CurrentStage = plant.CurrentStage,
            StageStartDate = plant.StageStartDate,
            PlantedDate = plant.PlantedDate,
            HarvestDate = plant.HarvestDate,
            CurrentGrowRoomId = plant.CurrentGrowRoomId,
            PlantSource = plant.PlantSource,
            MotherPlantId = plant.MotherPlantId,
            PlantSex = plant.PlantSex,
            WetWeightGrams = plant.WetWeightGrams,
            DryWeightGrams = plant.DryWeightGrams,
            HarvestBatchId = plant.HarvestBatchId,
            HealthStatus = plant.HealthStatus,
            Notes = plant.Notes,
            DestroyedDate = plant.DestroyedDate,
            DestructionReason = plant.DestructionReason,
            WasteWeightGrams = plant.WasteWeightGrams,
            IsActive = plant.IsActive,
            CreatedAt = plant.CreatedAt,
            CreatedBy = plant.CreatedBy
        };
    }
}
