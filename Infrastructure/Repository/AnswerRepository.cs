using CO4029_BE.Domain.Entities;

namespace CO4029_BE.Infrastructure.Repository
{
    public interface IAnswerRepository : IRepository<Answer>
    {
        Task<IEnumerable<Answer>> GetAnswersByUser(string userId);
        Task<IEnumerable<Answer>> GetAnswersByQuestion(string questionId);
    }
    
    public class AnswerRepository: Repository<Answer>, IAnswerRepository
    {
        public AnswerRepository(Supabase.Client client) : base(client)
        {
            
        }

        public async Task<IEnumerable<Answer>> GetAnswersByUser(string userId)
        {
            var reponse = await Table
                .Where(answer => answer.UserID == userId)
                .Get();
            return reponse.Models;
        }

        public async Task<IEnumerable<Answer>> GetAnswersByQuestion(string questionId)
        {
            var response = await Table
                .Where(answer => answer.QuestionID == questionId)
                .Get();
            return response.Models;
        }
    }
}
