namespace CO4029_BE.API.Contracts.Responses;

public class FloorResponse
{
    public string? Id { get; set; }
    public string? BuildingId { get; set; }
    public int FloorNumber { get; set; }
    public string? Name { get; set; }
    public string? FloorPlanUrl { get; set; }
    public string? SourceUrl { get; set; }
    public bool Verified { get; set; }
}
