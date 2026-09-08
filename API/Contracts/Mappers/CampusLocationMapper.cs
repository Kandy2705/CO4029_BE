using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;

namespace CO4029_BE.API.Contracts.Mappers;

public static class CampusLocationMapper
{
    public static FloorResponse ToResponse(this Floor floor) => new()
    {
        Id = floor.Id,
        BuildingId = floor.BuildingId,
        FloorNumber = floor.FloorNumber,
        Name = floor.Name,
        FloorPlanUrl = floor.FloorPlanUrl,
        SourceUrl = floor.SourceUrl,
        Verified = floor.Verified
    };

    public static RoomResponse ToResponse(this Room room) => new()
    {
        Id = room.Id,
        FloorId = room.FloorId,
        RoomCode = room.RoomCode,
        Name = room.Name,
        Description = room.Description,
        RoomType = room.RoomType,
        LocalX = room.LocalX,
        LocalY = room.LocalY,
        LocalZ = room.LocalZ,
        SourceUrl = room.SourceUrl,
        Verified = room.Verified
    };
}
