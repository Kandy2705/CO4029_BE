namespace CO4029_BE.API.Contracts.Responses;

public class RoomResponse
{
    public string? Id { get; set; }
    public string? FloorId { get; set; }
    public string? RoomCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? RoomType { get; set; }
    public float? LocalX { get; set; }
    public float? LocalY { get; set; }
    public float? LocalZ { get; set; }
    public string? SourceUrl { get; set; }
    public bool Verified { get; set; }
}
