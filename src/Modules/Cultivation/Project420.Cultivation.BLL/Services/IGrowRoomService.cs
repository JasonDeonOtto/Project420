namespace Project420.Cultivation.BLL.Services;

/// <summary>
/// Service interface for grow room business logic.
/// GMP Compliance: Facility and environment tracking.
/// </summary>
public interface IGrowRoomService
{
    Task<int> CreateGrowRoomAsync(object dto);
    Task UpdateGrowRoomAsync(object dto);
    Task<object?> GetGrowRoomByIdAsync(int id);
    Task<IEnumerable<object>> GetAllGrowRoomsAsync();
    Task DeactivateGrowRoomAsync(int id);
    Task<IEnumerable<object>> GetGrowRoomsByTypeAsync(string roomType);
}
