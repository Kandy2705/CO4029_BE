using CO4029_BE.Domain.Entities;
using Supabase.Postgrest;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IHistoryRepository : IRepository<History>
    {
        Task<IEnumerable<History>> GetHistorysByUserId(string? user_id);
        Task<History?> GetHistoryByUserId(string? user_id, string history_id);
    }

    public class HistoryRepository: Repository<History>, IHistoryRepository
    {
        public HistoryRepository(Supabase.Client client) : base(client)
        {
            
        }

        public async Task<IEnumerable<History>> GetHistorysByUserId(string? user_id)
        {
            var reponse = await Table
                .Where(x => x.user_id == user_id)
                .Get();
            return reponse.Models;
        }

        public async Task<History?> GetHistoryByUserId(string? user_id, string history_id)
        {
            var reponse = await Table
                .Where(x => x.user_id == user_id && x.id == history_id)
                .Single();
            return reponse;
        }
    }
}