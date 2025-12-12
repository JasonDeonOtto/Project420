using Microsoft.EntityFrameworkCore;
using Project420.Cultivation.DAL.Repositories.Common;
using Project420.Cultivation.Models.Entities;
using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.DAL.Repositories;

/// <summary>
/// Repository implementation for GrowRoom entity operations.
/// GMP Compliance: Facility and environment tracking.
/// </summary>
public class GrowRoomRepository : Repository<GrowRoom>, IGrowRoomRepository
{
    public GrowRoomRepository(CultivationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowRoom>> GetActiveRoomsAsync()
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoomType)
            .ThenBy(r => r.RoomCode)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowRoom>> GetByRoomTypeAsync(GrowRoomType roomType)
    {
        return await _dbSet
            .Where(r => r.RoomType == roomType && r.IsActive)
            .OrderBy(r => r.RoomCode)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<GrowRoom?> GetByRoomCodeAsync(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<GrowRoom>> GetRoomsWithAvailableCapacityAsync()
    {
        var rooms = await _dbSet
            .Include(r => r.Plants.Where(p => p.IsActive))
            .Where(r => r.IsActive && r.MaxCapacity.HasValue)
            .ToListAsync();

        // Filter rooms with available capacity (client-side evaluation)
        return rooms
            .Where(r => r.Plants.Count(p => p.IsActive) < r.MaxCapacity!.Value)
            .OrderBy(r => r.RoomType)
            .ThenBy(r => r.RoomCode)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<GrowRoom?> GetByIdWithRelatedDataAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Plants.Where(p => p.IsActive))
            .Include(r => r.GrowCycles)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <inheritdoc/>
    public async Task<int> GetCurrentPlantCountAsync(int id)
    {
        var room = await _dbSet
            .Include(r => r.Plants)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
            return 0;

        return room.Plants.Count(p => p.IsActive);
    }

    /// <inheritdoc/>
    public async Task<bool> IsRoomCodeUniqueAsync(string roomCode, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
            return false;

        var query = _dbSet.Where(r => r.RoomCode == roomCode);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
