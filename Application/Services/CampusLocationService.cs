using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class CampusLocationService
{
    private readonly IFloorRepository floorRepository;
    private readonly IRoomRepository roomRepository;
    private readonly IBuildingRepository buildingRepository;
    private readonly IUserRepository userRepository;
    private readonly Client supabaseClient;

    public CampusLocationService(
        IFloorRepository floorRepository,
        IRoomRepository roomRepository,
        IBuildingRepository buildingRepository,
        IUserRepository userRepository,
        Client supabaseClient)
    {
        this.floorRepository = floorRepository;
        this.roomRepository = roomRepository;
        this.buildingRepository = buildingRepository;
        this.userRepository = userRepository;
        this.supabaseClient = supabaseClient;
    }

    private async Task<User> EnsureManager(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        return user;
    }

    public async Task<IEnumerable<FloorResponse>> GetFloors(string buildingId, string accessToken)
    {
        await EnsureManager(accessToken);
        if (await buildingRepository.GetByIdAsync(buildingId) == null)
            throw new KeyNotFoundException("Không tồn tại building");
        var floors = await floorRepository.GetByBuildingIdAsync(buildingId);
        return floors.Select(floor => floor.ToResponse());
    }

    public async Task<FloorResponse> GetFloor(string floorId, string accessToken)
    {
        await EnsureManager(accessToken);
        var floor = await floorRepository.GetByIdAsync(floorId)
            ?? throw new KeyNotFoundException("Không tồn tại floor");
        return floor.ToResponse();
    }

    public async Task<FloorResponse> CreateFloor(string buildingId, CreateFloorRequest request, string accessToken)
    {
        await EnsureManager(accessToken);
        if (await buildingRepository.GetByIdAsync(buildingId) == null)
            throw new KeyNotFoundException("Không tồn tại building");

        var floor = new Floor
        {
            BuildingId = buildingId,
            FloorNumber = request.FloorNumber,
            Name = request.Name?.Trim(),
            FloorPlanUrl = request.FloorPlanUrl?.Trim(),
            SourceUrl = request.SourceUrl?.Trim(),
            Verified = request.Verified
        };
        return (await floorRepository.CreateAsync(floor)).ToResponse();
    }

    public async Task<FloorResponse> UpdateFloor(string floorId, UpdateFloorRequest request, string accessToken)
    {
        await EnsureManager(accessToken);
        var old = await floorRepository.GetByIdAsync(floorId)
            ?? throw new KeyNotFoundException("Không tồn tại floor");
        var updated = new Floor
        {
            Id = floorId,
            BuildingId = old.BuildingId,
            FloorNumber = request.FloorNumber ?? old.FloorNumber,
            Name = request.Name ?? old.Name,
            FloorPlanUrl = request.FloorPlanUrl ?? old.FloorPlanUrl,
            SourceUrl = request.SourceUrl ?? old.SourceUrl,
            Verified = request.Verified ?? old.Verified
        };
        await floorRepository.UpdateAsync(floorId, updated);
        return updated.ToResponse();
    }

    public async Task<bool> DeleteFloor(string floorId, string accessToken)
    {
        await EnsureManager(accessToken);
        if (await floorRepository.GetByIdAsync(floorId) == null)
            throw new KeyNotFoundException("Không tồn tại floor");
        await floorRepository.DeleteAsync(floorId);
        return true;
    }

    public async Task<IEnumerable<RoomResponse>> GetRooms(string floorId, string accessToken)
    {
        await EnsureManager(accessToken);
        if (await floorRepository.GetByIdAsync(floorId) == null)
            throw new KeyNotFoundException("Không tồn tại floor");
        var rooms = await roomRepository.GetByFloorIdAsync(floorId);
        return rooms.Select(room => room.ToResponse());
    }

    public async Task<RoomResponse> GetRoom(string roomId, string accessToken)
    {
        await EnsureManager(accessToken);
        var room = await roomRepository.GetByIdAsync(roomId)
            ?? throw new KeyNotFoundException("Không tồn tại room");
        return room.ToResponse();
    }

    public async Task<RoomResponse> CreateRoom(string floorId, CreateRoomRequest request, string accessToken)
    {
        await EnsureManager(accessToken);
        if (await floorRepository.GetByIdAsync(floorId) == null)
            throw new KeyNotFoundException("Không tồn tại floor");
        if (string.IsNullOrWhiteSpace(request.RoomCode))
            throw new ArgumentException("RoomCode là bắt buộc");

        var room = new Room
        {
            FloorId = floorId,
            RoomCode = request.RoomCode.Trim(),
            Name = request.Name?.Trim(),
            Description = request.Description?.Trim(),
            RoomType = request.RoomType?.Trim(),
            LocalX = request.LocalX,
            LocalY = request.LocalY,
            LocalZ = request.LocalZ,
            SourceUrl = request.SourceUrl?.Trim(),
            Verified = request.Verified
        };
        return (await roomRepository.CreateAsync(room)).ToResponse();
    }

    public async Task<RoomResponse> UpdateRoom(string roomId, UpdateRoomRequest request, string accessToken)
    {
        await EnsureManager(accessToken);
        var old = await roomRepository.GetByIdAsync(roomId)
            ?? throw new KeyNotFoundException("Không tồn tại room");
        var roomCode = request.RoomCode ?? old.RoomCode;
        if (string.IsNullOrWhiteSpace(roomCode))
            throw new ArgumentException("RoomCode là bắt buộc");

        var updated = new Room
        {
            Id = roomId,
            FloorId = old.FloorId,
            RoomCode = roomCode.Trim(),
            Name = request.Name ?? old.Name,
            Description = request.Description ?? old.Description,
            RoomType = request.RoomType ?? old.RoomType,
            LocalX = request.LocalX ?? old.LocalX,
            LocalY = request.LocalY ?? old.LocalY,
            LocalZ = request.LocalZ ?? old.LocalZ,
            SourceUrl = request.SourceUrl ?? old.SourceUrl,
            Verified = request.Verified ?? old.Verified
        };
        await roomRepository.UpdateAsync(roomId, updated);
        return updated.ToResponse();
    }

    public async Task<bool> DeleteRoom(string roomId, string accessToken)
    {
        await EnsureManager(accessToken);
        if (await roomRepository.GetByIdAsync(roomId) == null)
            throw new KeyNotFoundException("Không tồn tại room");
        await roomRepository.DeleteAsync(roomId);
        return true;
    }
}
