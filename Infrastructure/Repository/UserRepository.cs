using CO4029_BE.Domain.Entities;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Interfaces;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<(IEnumerable<User> Items, int TotalItems)> GetPagedAsync(
            int offset,
            int limit,
            string? search,
            string? role,
            bool? isActive);
        Task<int> CountByActiveAsync(bool isActive);
    }
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(Supabase.Client client) : base(client)
        {
            
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var response = await Table
                .Filter("email", Constants.Operator.Equals, email)
                .Single();

            return response;
        }

        public async Task<(IEnumerable<User> Items, int TotalItems)> GetPagedAsync(
            int offset,
            int limit,
            string? search,
            string? role,
            bool? isActive)
        {
            var countQuery = ApplyFilters(Client.Postgrest.Table<User>(), search, role, isActive);
            var dataQuery = ApplyFilters(Client.Postgrest.Table<User>(), search, role, isActive);

            var totalItems = await countQuery.Count(Constants.CountType.Exact);
            var response = await dataQuery
                .Order("name", Constants.Ordering.Ascending)
                .Range(offset, offset + limit - 1)
                .Get();

            return (response.Models, totalItems);
        }

        public async Task<int> CountByActiveAsync(bool isActive)
        {
            return await Client.Postgrest
                .Table<User>()
                .Filter("is_active", Constants.Operator.Equals, isActive.ToString().ToLowerInvariant())
                .Count(Constants.CountType.Exact);
        }

        private static IPostgrestTable<User> ApplyFilters(
            IPostgrestTable<User> query,
            string? search,
            string? role,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim()}%";
                query = query.Or([
                    new QueryFilter("name", Constants.Operator.ILike, pattern),
                    new QueryFilter("email", Constants.Operator.ILike, pattern)
                ]);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Filter("role", Constants.Operator.Equals, role);
            }

            if (isActive.HasValue)
            {
                query = query.Filter(
                    "is_active",
                    Constants.Operator.Equals,
                    isActive.Value.ToString().ToLowerInvariant());
            }

            return query;
        }
    }    
}
