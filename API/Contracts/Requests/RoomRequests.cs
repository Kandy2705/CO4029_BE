namespace CO4029_BE.API.Contracts.Requests;

public class CreateRoomRequest
{
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

public class UpdateRoomRequest
{
    public string? RoomCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? RoomType { get; set; }
    public float? LocalX { get; set; }
    public float? LocalY { get; set; }
    public float? LocalZ { get; set; }
    public string? SourceUrl { get; set; }
    public bool? Verified { get; set; }
}
