using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Application.Abstractions;
using CO4029_BE.Infrastructure.Repository;

namespace CO4029_BE.Application.Services;

/// <summary>
/// Campus data for any signed-in user (the management endpoints stay Employee/Admin only). Three queries,
/// grouped in memory: the campus has tens of buildings and rooms, not thousands.
/// </summary>
public class CampusMapService
{
    private readonly IBuildingRepository _buildings;
    private readonly IFloorRepository _floors;
    private readonly IRoomRepository _rooms;
    private readonly ICurrentUserResolver _currentUser;
    private readonly TimeProvider _time;

    public CampusMapService(IBuildingRepository buildings, IFloorRepository floors, IRoomRepository rooms,
        ICurrentUserResolver currentUser, TimeProvider time)
    {
        _buildings = buildings;
        _floors = floors;
        _rooms = rooms;
        _currentUser = currentUser;
        _time = time;
    }

    public async Task<CampusMapResponse> GetMapAsync(string accessToken)
    {
        await _currentUser.ResolveAsync(accessToken);

        var buildings = await _buildings.GetAllAsync();
        var floors = await _floors.GetAllAsync();
        var rooms = await _rooms.GetAllAsync();

        var roomsByFloor = rooms
            .Where(room => room.FloorId != null)
            .GroupBy(room => room.FloorId!)
            .ToDictionary(group => group.Key, group => group
                .OrderBy(room => room.RoomCode, StringComparer.OrdinalIgnoreCase)
                .Select(room => room.ToResponse())
                .ToList());
        var floorsByBuilding = floors
            .Where(floor => floor.BuildingId != null)
            .GroupBy(floor => floor.BuildingId!)
            .ToDictionary(group => group.Key, group => group
                .OrderBy(floor => floor.FloorNumber)
                .Select(floor => new CampusFloorResponse
                {
                    Id = floor.Id,
                    FloorNumber = floor.FloorNumber,
                    Name = floor.Name,
                    FloorPlanUrl = floor.FloorPlanUrl,
                    Verified = floor.Verified,
                    Rooms = floor.Id != null && roomsByFloor.TryGetValue(floor.Id, out var list) ? list : new()
                })
                .ToList());

        return new CampusMapResponse
        {
            GeneratedAt = _time.GetUtcNow().UtcDateTime,
            Buildings = buildings
                .OrderBy(building => building.Name, StringComparer.OrdinalIgnoreCase)
                .Select(building => new CampusBuildingResponse
                {
                    Id = building.ID,
                    Name = building.Name,
                    Content = building.Content,
                    Latitude = building.Latitude,
                    Longitude = building.Longitude,
                    Floors = building.ID != null && floorsByBuilding.TryGetValue(building.ID, out var list)
                        ? list
                        : new()
                })
                .ToList()
        };
    }
}
