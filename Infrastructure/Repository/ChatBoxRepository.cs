using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IChatBoxRepository : IRepository<Chatbox>
    {
        Task<IEnumerable<Chatbox>> GetChatboxesByHistoryId(string? historyId);
        Task<IEnumerable<Chatbox>> GetChatboxesByHistoryIds(IEnumerable<string> historyIds);
        Task DeleteByHistoryId(string historyId);
    }
    public class ChatBoxRepository: Repository<Chatbox>, IChatBoxRepository
    {
        public ChatBoxRepository(Supabase.Client client) : base(client)
        {
            
        }

        public async Task<IEnumerable<Chatbox>> GetChatboxesByHistoryId(string? historyId)
        {
            var reponse = await Table.Where(x => x.history_id == historyId)
                                                           .Get();
            return reponse.Models;
        }

        public async Task<IEnumerable<Chatbox>> GetChatboxesByHistoryIds(IEnumerable<string> historyIds)
        {
            var ids = historyIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .Cast<object>()
                .ToList();

            if (ids.Count == 0)
            {
                return [];
            }

            var response = await Client.Postgrest
                .Table<Chatbox>()
                .Filter("history_id", Supabase.Postgrest.Constants.Operator.In, ids)
                .Get();
            return response.Models;
        }

        public async Task DeleteByHistoryId(string historyId)
        {
            await Table.Where(x => x.history_id == historyId)
                       .Delete();
        }
    }
}
