namespace CO4029_BE.API.Contracts.Responses;

public class BuildingReponse
{
    public String? Id { get; set; }
    public string? Name { get; set; }
    public string? Content { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}