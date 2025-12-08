using Project420.Cultivation.Models.Enums;

namespace Project420.Cultivation.BLL.DTOs;

public class CreateGrowRoomDto
{
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GrowRoomType RoomType { get; set; }
    public decimal? SquareMeters { get; set; }
    public int? MaxPlantCapacity { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
}

public class UpdateGrowRoomDto
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GrowRoomType RoomType { get; set; }
    public decimal? SquareMeters { get; set; }
    public int? MaxPlantCapacity { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class GrowRoomDto
{
    public int Id { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public GrowRoomType RoomType { get; set; }
    public decimal? SquareMeters { get; set; }
    public int? MaxPlantCapacity { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
