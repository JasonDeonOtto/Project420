using Project420.Cultivation.DAL.Repositories;

namespace Project420.Cultivation.BLL.Services;

public class GrowRoomService : IGrowRoomService
{
    private readonly IGrowRoomRepository _repository;

    public GrowRoomService(IGrowRoomRepository repository)
    {
        _repository = repository;
    }

    public Task<int> CreateGrowRoomAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public Task UpdateGrowRoomAsync(object dto) => throw new NotImplementedException("Will be implemented with DTOs");
    public async Task<object?> GetGrowRoomByIdAsync(int id) => await _repository.GetByIdAsync(id);
    public async Task<IEnumerable<object>> GetAllGrowRoomsAsync() => (await _repository.GetAllAsync()).Cast<object>();
    public async Task DeactivateGrowRoomAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<IEnumerable<object>> GetGrowRoomsByTypeAsync(string roomType)
    {
        var rooms = await _repository.FindAsync(r => r.RoomType.ToString() == roomType);
        return rooms.Cast<object>();
    }
}
