using CO4029_BE.Domain.Entities;
using Supabase;
using Supabase.Postgrest;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
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
    }    
}

