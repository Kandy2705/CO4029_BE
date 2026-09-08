namespace CO4029_BE.API.Contracts.Requests;

public class CreateFloorRequest
{
    public int FloorNumber { get; set; }
    public string? Name { get; set; }
    public string? FloorPlanUrl { get; set; }
    public string? SourceUrl { get; set; }
    public bool Verified { get; set; }
}

public class UpdateFloorRequest
{
    public int? FloorNumber { get; set; }
    public string? Name { get; set; }
    public string? FloorPlanUrl { get; set; }
    public string? SourceUrl { get; set; }
    public bool? Verified { get; set; }
}
