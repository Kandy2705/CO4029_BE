using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Utils;
using Microsoft.IdentityModel.Tokens;
using Supabase;

namespace CO4029_BE.Application.Services;

public class QuestionService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly Client supabaseClient;

    public QuestionService(IQuestionRepository questionRepository
        , IAnswerRepository answerRepository
        , ICategoryRepository categoryRepository
        , IUserRepository userRepository
        , Client supabaseClient)
    {
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        this.supabaseClient = supabaseClient;
    }

    public async Task<CategoryReponse> CreateCategory(CreateCategoryRequest categoryRequest, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        Category category = new Category
        {
            Name = categoryRequest.Name,
        };
        var reponse = await _categoryRepository.CreateAsync(category);
        return reponse.ToReponse();
    }

    public async Task<IEnumerable<CategoryReponse>> GetCategories(string accessToken)
    {
        var  user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        var reponse = await _categoryRepository.GetAllAsync();
        return reponse.Select(category => category.ToReponse());
    }

    public async Task<CategoryReponse> GetCategory(string categoryId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }

        var reponse = await _categoryRepository.GetByIdAsync(categoryId);
        return reponse.ToReponse();
    }

    public async Task<CategoryReponse> UpdateCategory(CreateCategoryRequest categoryRequest
                                                    , string categoryId
                                                    , string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        
        var existingCategory = await _categoryRepository.GetByIdAsync(categoryId);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException("Không tìm thấy danh mục này");
        }
        if (!string.IsNullOrWhiteSpace(categoryRequest.Name))
        {
            existingCategory.Name = categoryRequest.Name;
        }
        await _categoryRepository.UpdateAsync(categoryId, existingCategory);
        return existingCategory.ToReponse();
    }

    public async Task<bool> DeleteCategory(string categoryId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        var existingCategory = await _categoryRepository.GetByIdAsync(categoryId);
        if (existingCategory == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi với ID: {categoryId}");
        }
        await _categoryRepository.DeleteAsync(categoryId);
        return true;
    }

    public async Task<QuestionReponse> CreateQuestion(CreateQuestionRequest questionRequest
                                                    , string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);

        Question newQuestion = new Question
        {
            Content = questionRequest.Content,
            Email = questionRequest.Email,
            Name = questionRequest.Name,
            CreateDate = questionRequest.CreateDate,
            CategoryID = questionRequest.CategoryId,
            UserID = user.id
        };
        var reponse =  await _questionRepository.CreateAsync(newQuestion);
        return reponse.ToReponse();
    }

    public async Task<IEnumerable<QuestionReponse>> GetQuestions(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        
        var reponse = await _questionRepository.GetAllAsync();
        return reponse.Select(question => question.ToReponse());
    }

    public async Task<QuestionReponse> GetQuestion(string questionId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        
        var reponse = await _questionRepository.GetByIdAsync(questionId);
        return reponse.ToReponse();
    }

    public async Task<IEnumerable<QuestionReponse>> GetQuestionsByCategory(string categoryId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }

        var reponse = await _questionRepository.GetQuestionsByCategory(categoryId);
        return reponse.Select(question => question.ToReponse());
    }

    public async Task<QuestionReponse> UpdateQuestion(CreateQuestionRequest questionRequest
        , string questionId
        , string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        var existingQuestion = await _questionRepository.GetByIdAsync(questionId);
        if (existingQuestion == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi với ID: {questionId}");
        }
        if (!string.IsNullOrWhiteSpace(questionRequest.Content)) existingQuestion.Content = questionRequest.Content;
        if (!string.IsNullOrWhiteSpace(questionRequest.Email))   existingQuestion.Email = questionRequest.Email;
        if (!string.IsNullOrWhiteSpace(questionRequest.Name))    existingQuestion.Name = questionRequest.Name;
        if (!string.IsNullOrWhiteSpace(questionRequest.CategoryId)) existingQuestion.CategoryID = questionRequest.CategoryId;
        if (!string.IsNullOrWhiteSpace(questionRequest.UserId))     existingQuestion.UserID = questionRequest.UserId;
    
        if (questionRequest.CreateDate.HasValue) 
        {
            existingQuestion.CreateDate = questionRequest.CreateDate.Value;
        }
        await _questionRepository.UpdateAsync(questionId, existingQuestion);
    
        return existingQuestion.ToReponse();
    }

    public async Task<bool> DeleteQuestion(string questionId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        var existingQuestion = await _questionRepository.GetByIdAsync(questionId);
        if (existingQuestion == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy câu hỏi với ID: {questionId}");
        }
        await _questionRepository.DeleteAsync(questionId);
        return true;
    }

    public async Task<AnswerReponse> CreateAnswer(CreateAnswerRequest answerRequest, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }

        Answer newAnswer = new Answer
        {
            Content = answerRequest.Content,
            CreateDate = answerRequest.CreatedDate,
            QuestionID = answerRequest.QuestionId,
            UserID = user.id
        };
        var reponse =  await _answerRepository.CreateAsync(newAnswer);
        return reponse.ToReponse();
    }

    public async Task<IEnumerable<AnswerReponse>> GetAnswers(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        
        var reponse = await _answerRepository.GetAllAsync();
        return reponse.Select(answer => answer.ToReponse());
    }

    public async Task<AnswerReponse> GetAnswer(string answerId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        
        var reponse = await _answerRepository.GetByIdAsync(answerId);
        if (reponse == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy câu trả lời với ID: {answerId}");
        }
        return reponse.ToReponse();
    }

    public async Task<IEnumerable<AnswerReponse>> GetAnswersByUser(string userId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        
        var checkUser =  await _userRepository.GetByIdAsync(userId);
        if (checkUser == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
        }
        var reponse = await _answerRepository.GetAnswersByUser(userId);
        return reponse.Select(answer => answer.ToReponse());
    }

    public async Task<AnswerReponse> UpdateAnswer(CreateAnswerRequest answerRequest, string answerId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        var existingAnswer = await  _answerRepository.GetByIdAsync(answerId);
        if (existingAnswer == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy câu trả lời với ID: {answerId}");
        }
        
        if (!string.IsNullOrWhiteSpace(answerRequest.Content)) existingAnswer.Content = answerRequest.Content;
        if (!string.IsNullOrWhiteSpace(answerRequest.UserId))   existingAnswer.UserID = answerRequest.UserId;
        if (!string.IsNullOrWhiteSpace(answerRequest.QuestionId))    existingAnswer.QuestionID = answerRequest.QuestionId;
        
        if(answerRequest.CreatedDate.HasValue) existingAnswer.CreateDate = answerRequest.CreatedDate;
        
        await _answerRepository.UpdateAsync(answerId, existingAnswer);
        return existingAnswer.ToReponse();
    }

    public async Task<bool> DeleteAnswer(string answerId, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, supabaseClient, _userRepository);
        if (!AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền để sử dụng API này");
        }
        var existingAnswer = await  _answerRepository.GetByIdAsync(answerId);
        if (existingAnswer == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy câu trả lời với ID: {answerId}");
        }
        await _answerRepository.DeleteAsync(answerId);
        return true;
    }
}