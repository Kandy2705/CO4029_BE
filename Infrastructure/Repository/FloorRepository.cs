using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository;

public interface IFloorRepository : IRepository<Floor>
{
    Task<IEnumerable<Floor>> GetByBuildingIdAsync(string buildingId);
}

public class FloorRepository : Repository<Floor>, IFloorRepository
{
    public FloorRepository(Supabase.Client client) : base(client) { }

    public async Task<IEnumerable<Floor>> GetByBuildingIdAsync(string buildingId)
    {
        var response = await Table
            .Where(floor => floor.BuildingId == buildingId)
            .Get();
        return response.Models.OrderBy(floor => floor.FloorNumber);
    }
}
