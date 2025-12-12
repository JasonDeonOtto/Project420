using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository interface for GrowRoom entity operations.
/// GMP Compliance: Facility and environment tracking.
/// </summary>
public interface IGrowRoomRepository : IRepository<GrowRoom>
{
    /// <summary>
    /// Gets all active grow rooms (IsActive = true).
    /// </summary>
    /// <returns>Collection of active grow rooms</returns>
    /// <remarks>
    /// SAHPRA GMP: Only active rooms should be used for cultivation planning.
    /// </remarks>
    Task<IEnumerable<GrowRoom>> GetActiveRoomsAsync();

    /// <summary>
    /// Gets grow rooms filtered by room type.
    /// </summary>
    /// <param name="roomType">Type of grow room (Mother, Clone, Vegetative, Flowering, Drying)</param>
    /// <returns>Collection of rooms matching the specified type</returns>
    /// <remarks>
    /// Used for capacity planning by growth stage.
    /// </remarks>
    Task<IEnumerable<GrowRoom>> GetByRoomTypeAsync(GrowRoomType roomType);

    /// <summary>
    /// Gets a grow room by its unique room code.
    /// </summary>
    /// <param name="roomCode">Room code identifier (e.g., "VEG-01", "FLOWER-A")</param>
    /// <returns>GrowRoom if found, null otherwise</returns>
    /// <remarks>
    /// Room codes are typically unique identifiers for facility layout.
    /// </remarks>
    Task<GrowRoom?> GetByRoomCodeAsync(string roomCode);

    /// <summary>
    /// Gets rooms that have available capacity (current plants less than max capacity).
    /// </summary>
    /// <returns>Collection of rooms with available space</returns>
    /// <remarks>
    /// DALRRD Compliance: Must not exceed licensed plant count per room.
    /// Used for plant placement decisions.
    /// </remarks>
    Task<IEnumerable<GrowRoom>> GetRoomsWithAvailableCapacityAsync();

    /// <summary>
    /// Gets a grow room by ID with related data eagerly loaded.
    /// </summary>
    /// <param name="id">GrowRoom primary key</param>
    /// <returns>GrowRoom with Plants and GrowCycles navigation properties loaded</returns>
    /// <remarks>
    /// Includes: Plants (active only), GrowCycles
    /// Use for detailed room view with occupancy data.
    /// </remarks>
    Task<GrowRoom?> GetByIdWithRelatedDataAsync(int id);

    /// <summary>
    /// Gets the current count of active plants in a specific room.
    /// </summary>
    /// <param name="id">GrowRoom primary key</param>
    /// <returns>Count of active plants currently in the room</returns>
    /// <remarks>
    /// SAHPRA: Used for capacity compliance and room occupancy tracking.
    /// Only counts plants where IsActive = true.
    /// </remarks>
    Task<int> GetCurrentPlantCountAsync(int id);

    /// <summary>
    /// Checks if a room code is unique (not used by another room).
    /// </summary>
    /// <param name="roomCode">Room code to validate</param>
    /// <param name="excludeId">Optional: Exclude this room ID from check (for updates)</param>
    /// <returns>True if unique, false if already exists</returns>
    /// <remarks>
    /// Used for validation before creating/updating rooms.
    /// </remarks>
    Task<bool> IsRoomCodeUniqueAsync(string roomCode, int? excludeId = null);
}
