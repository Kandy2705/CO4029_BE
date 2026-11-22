namespace CO4029_BE.API.Contracts.Responses;

/// <summary>Read-only campus data for the mobile app: buildings with their floors and rooms.</summary>
public class CampusMapResponse
{
    public List<CampusBuildingResponse> Buildings { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

public class CampusBuildingResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Content { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<CampusFloorResponse> Floors { get; set; } = new();
}

public class CampusFloorResponse
{
    public string? Id { get; set; }
    public int FloorNumber { get; set; }
    public string? Name { get; set; }
    public string? FloorPlanUrl { get; set; }
    public bool Verified { get; set; }
    public List<RoomResponse> Rooms { get; set; } = new();
}
