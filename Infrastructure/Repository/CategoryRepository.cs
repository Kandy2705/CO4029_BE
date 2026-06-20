using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        
    }

    public class CategoryRepository: Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(Supabase.Client client) : base(client)
        {
            
        }
    }
}