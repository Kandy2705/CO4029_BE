using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class DashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IAnswerRepository _answerRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly IChatBoxRepository _chatboxRepository;
    private readonly Client _supabaseClient;

    public DashboardService(
        IUserRepository userRepository,
        IBuildingRepository buildingRepository,
        IQuestionRepository questionRepository,
        IAnswerRepository answerRepository,
        ICategoryRepository categoryRepository,
        IHistoryRepository historyRepository,
        IChatBoxRepository chatboxRepository,
        Client supabaseClient)
    {
        _userRepository = userRepository;
        _buildingRepository = buildingRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _categoryRepository = categoryRepository;
        _historyRepository = historyRepository;
        _chatboxRepository = chatboxRepository;
        _supabaseClient = supabaseClient;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(string accessToken)
    {
        await AccessToken.GetAdminUser(accessToken, _supabaseClient, _userRepository);

        var totalUsersTask = _userRepository.CountAsync();
        var activeUsersTask = _userRepository.CountByActiveAsync(true);
        var disabledUsersTask = _userRepository.CountByActiveAsync(false);
        var totalBuildingsTask = _buildingRepository.CountAsync();
        var totalQuestionsTask = _questionRepository.CountAsync();
        var totalAnswersTask = _answerRepository.CountAsync();
        var totalCategoriesTask = _categoryRepository.CountAsync();
        var totalHistoriesTask = _historyRepository.CountAsync();
        var totalChatboxesTask = _chatboxRepository.CountAsync();

        await Task.WhenAll(
            totalUsersTask,
            activeUsersTask,
            disabledUsersTask,
            totalBuildingsTask,
            totalQuestionsTask,
            totalAnswersTask,
            totalCategoriesTask,
            totalHistoriesTask,
            totalChatboxesTask);

        return new DashboardSummaryResponse
        {
            TotalUsers = await totalUsersTask,
            ActiveUsers = await activeUsersTask,
            DisabledUsers = await disabledUsersTask,
            TotalBuildings = await totalBuildingsTask,
            TotalQuestions = await totalQuestionsTask,
            TotalAnswers = await totalAnswersTask,
            TotalCategories = await totalCategoriesTask,
            TotalHistories = await totalHistoriesTask,
            TotalChatboxes = await totalChatboxesTask
        };
    }
}
