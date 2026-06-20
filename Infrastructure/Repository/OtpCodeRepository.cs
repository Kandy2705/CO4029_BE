using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IOtpCodeRepository : IRepository<OtpCode>
    {
        
    }

    public class OtpCodeRepository: Repository<OtpCode>, IOtpCodeRepository
    {
        public OtpCodeRepository(Supabase.Client client) : base(client)
        {
            
        }
    }
}