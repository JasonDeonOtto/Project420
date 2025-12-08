using Project420.Cultivation.DAL.Repositories;
using Project420.Cultivation.Models.Entities;

namespace Project420.Cultivation.BLL.Services;

/// <summary>
/// Service for plant business logic.
/// SAHPRA Compliance: Individual plant tracking from seed to sale.
/// </summary>
public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepository;

    public PlantService(IPlantRepository plantRepository)
    {
        _plantRepository = plantRepository;
    }

    public async Task<int> CreatePlantAsync(object dto)
    {
        // TODO: Implement with proper DTO and validation
        throw new NotImplementedException("Will be implemented with DTOs");
    }

    public async Task UpdatePlantAsync(object dto)
    {
        // TODO: Implement with proper DTO and validation
        throw new NotImplementedException("Will be implemented with DTOs");
    }

    public async Task<object?> GetPlantByIdAsync(int id)
    {
        var plant = await _plantRepository.GetByIdAsync(id);
        // TODO: Map to DTO
        return plant;
    }

    public async Task<IEnumerable<object>> GetAllPlantsAsync()
    {
        var plants = await _plantRepository.GetAllAsync();
        // TODO: Map to DTOs
        return plants.Cast<object>();
    }

    public async Task DeactivatePlantAsync(int id)
    {
        await _plantRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<object>> GetPlantsByGrowCycleAsync(int growCycleId)
    {
        var plants = await _plantRepository.FindAsync(p => p.GrowCycleId == growCycleId);
        // TODO: Map to DTOs
        return plants.Cast<object>();
    }

    public async Task<IEnumerable<object>> GetPlantsByStageAsync(string stage)
    {
        // TODO: Parse stage enum
        var plants = await _plantRepository.FindAsync(p => p.CurrentStage.ToString() == stage);
        // TODO: Map to DTOs
        return plants.Cast<object>();
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
}
