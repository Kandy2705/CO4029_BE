using CO4029_BE.Domain.Entities;
using Supabase.Postgrest;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IOtpCodeRepository : IRepository<OtpCode>
    {
        /// <summary>All codes stored under one key (the <c>email</c> column holds a purpose-scoped key).</summary>
        Task<IEnumerable<OtpCode>> GetByKeyAsync(string key);
    }

    public class OtpCodeRepository: Repository<OtpCode>, IOtpCodeRepository
    {
        public OtpCodeRepository(Supabase.Client client) : base(client)
        {
        }

        public async Task<IEnumerable<OtpCode>> GetByKeyAsync(string key)
        {
            var response = await Table
                .Filter("email", Constants.Operator.Equals, key)
                .Get();
            return response.Models;
        }
    }
}
