using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IQuestionRepository : IRepository<Question>
    {
        Task<IEnumerable<Question>> GetQuestionsByCategory(string categoryId);
        Task<IEnumerable<Question>> GetQuestionsByUser(string userId);
    }
    
    public class QuestionRepository: Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(Supabase.Client client) : base(client)
        {
            
        }

        public async Task<IEnumerable<Question>> GetQuestionsByCategory(string categoryId)
        {
            var reponse = await Table
                .Where(question => question.CategoryID == categoryId)
                .Get();
            return reponse.Models;
        }

        public async Task<IEnumerable<Question>> GetQuestionsByUser(string userId)
        {
            var reponse = await Table
                .Where(question => question.UserID == userId)
                .Get();
            return reponse.Models;
        }
    }
}