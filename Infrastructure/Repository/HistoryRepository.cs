using CO4029_BE.Domain.Entities;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IHistoryRepository : IRepository<History>
    {
        Task<IEnumerable<History>> GetHistorysByUserId(string? user_id);
        Task<History?> GetHistoryByUserId(string? user_id, string history_id);
        Task<(IEnumerable<History> Items, int TotalItems)> GetPagedAsync(
            int offset,
            int limit,
            string? userId,
            DateTime? fromDate,
            DateTime? toDate);
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

        public async Task<(IEnumerable<History> Items, int TotalItems)> GetPagedAsync(
            int offset,
            int limit,
            string? userId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var countQuery = ApplyFilters(Client.Postgrest.Table<History>(), userId, fromDate, toDate);
            var dataQuery = ApplyFilters(Client.Postgrest.Table<History>(), userId, fromDate, toDate);

            var totalItems = await countQuery.Count(Constants.CountType.Exact);
            var response = await dataQuery
                .Order("create_date", Constants.Ordering.Descending)
                .Range(offset, offset + limit - 1)
                .Get();

            return (response.Models, totalItems);
        }

        private static IPostgrestTable<History> ApplyFilters(
            IPostgrestTable<History> query,
            string? userId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Filter("user_id", Constants.Operator.Equals, userId);
            }

            if (fromDate.HasValue)
            {
                query = query.Filter(
                    "create_date",
                    Constants.Operator.GreaterThanOrEqual,
                    fromDate.Value.ToUniversalTime().ToString("O"));
            }

            if (toDate.HasValue)
            {
                query = query.Filter(
                    "create_date",
                    Constants.Operator.LessThanOrEqual,
                    toDate.Value.ToUniversalTime().ToString("O"));
            }

            return query;
        }
    }
}
