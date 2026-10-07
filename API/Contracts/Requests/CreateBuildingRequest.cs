namespace CO4029_BE.API.Contracts.Requests;

public class CreateBuildingRequest
{
    public string? Name { get; set; }
    public string? Content { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? UserId { get; set; }
}