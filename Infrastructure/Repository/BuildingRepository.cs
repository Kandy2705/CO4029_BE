using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IBuildingRepository : IRepository<Building>
    {
        Task<IEnumerable<Building>> GetBuildingsByUserId(string userID);
    }
    public class BuildingRepository: Repository<Building>, IBuildingRepository
    {
        public BuildingRepository(Supabase.Client client) : base(client)
        {
            
        }

        public async Task<IEnumerable<Building>> GetBuildingsByUserId(string userID)
        {
            var reponse = await Table
                .Where(building => building.UserId == userID)
                .Get();
            return reponse.Models;
        }
    }
}