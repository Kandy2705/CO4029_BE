using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository;

public interface IRoomRepository : IRepository<Room>
{
    Task<IEnumerable<Room>> GetByFloorIdAsync(string floorId);
}

public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(Supabase.Client client) : base(client) { }

    public async Task<IEnumerable<Room>> GetByFloorIdAsync(string floorId)
    {
        var response = await Table
            .Where(room => room.FloorId == floorId)
            .Get();
        return response.Models.OrderBy(room => room.RoomCode);
    }
}
