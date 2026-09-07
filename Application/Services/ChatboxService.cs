using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class ChatboxService
{
    private readonly IChatBoxRepository chatBoxRepository;
    private readonly IHistoryRepository historyRepository;
    private readonly IUserRepository userRepository;
    private readonly ChatbotApiService chatbotApiService;
    private readonly Client _supabaseClient;

    public ChatboxService(IChatBoxRepository chatBoxRepository
                        , IHistoryRepository historyRepository
                        , Client client
                        , IUserRepository userRepository
                        , ChatbotApiService chatbotApiService)
    {
        this.chatBoxRepository = chatBoxRepository;
        this.historyRepository = historyRepository;
        this.userRepository = userRepository;
        _supabaseClient = client;
        this.chatbotApiService = chatbotApiService;
    }

    public async Task<ChatbotApiResponse> CreateChatbox(string accessToken, CreateChatboxRequest request)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);

        var history = await historyRepository.GetByIdAsync(request.History_id!);
        if (history == null)
            throw new KeyNotFoundException("Không tìm thấy History với ID đã cung cấp.");
        EnsureCanReadHistory(user, history);

        var aiResult = await chatbotApiService.AskAsync(request.Content!, request.History_id!, user.id);

        return aiResult;
    }

    public async Task<IEnumerable<ChatboxReponse>> GetAllChatboxes(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);

        IEnumerable<Chatbox> chatboxes;
        if (AuthorizeHelper.AuthorizeForEmployee(user))
        {
            chatboxes = await chatBoxRepository.GetAllAsync();
        }
        else
        {
            var histories = await historyRepository.GetHistorysByUserId(user.id);
            chatboxes = await chatBoxRepository.GetChatboxesByHistoryIds(
                histories.Where(history => history.id != null).Select(history => history.id!));
        }

        return chatboxes.Select(c => ChatboxMapper.ToReponse(c));
    }

    public async Task<ChatboxReponse> GetChatboxById(string accessToken, string chatboxId)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);
        
        var chatbox = await chatBoxRepository.GetByIdAsync(chatboxId);
        if (chatbox == null)
            throw new KeyNotFoundException("Không tìm thấy Chatbox với ID đã cung cấp.");

        var history = await historyRepository.GetByIdAsync(chatbox.history_id!);
        if (history == null)
            throw new KeyNotFoundException("Không tìm thấy History của Chatbox.");
        EnsureCanReadHistory(user, history);
        
        return ChatboxMapper.ToReponse(chatbox);
    }

    public async Task<IEnumerable<ChatboxReponse>> GetAllChatboxesByHistory(string accessToken, string historyId)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);

        var history = await historyRepository.GetByIdAsync(historyId);
        if (history == null)
            throw new KeyNotFoundException("Không tìm thấy History với ID đã cung cấp.");
        EnsureCanReadHistory(user, history);

        var chatboxes = await chatBoxRepository.GetChatboxesByHistoryId(historyId);
        
        return chatboxes.Select(c => ChatboxMapper.ToReponse(c));
    }

    public async Task<bool> DeleteChatbox(string accessToken ,string chatboxId)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);
        
        var chatbox = await chatBoxRepository.GetByIdAsync(chatboxId);

        if (chatbox == null)
        {
            throw new KeyNotFoundException("Không tìm thấy Chatbox với ID đã cung cấp.");
        }
        
        var history = await historyRepository.GetByIdAsync(chatbox.history_id!);

        if (history == null)
        {
            throw new KeyNotFoundException("Không tìm thấy History với ID đã cung cấp.");
        }
        if (user.id != history.user_id && !AuthorizeHelper.AuthorizeForEmployee(user))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền này");
        }
        
        await chatBoxRepository.DeleteAsync(chatbox.id!);

        return true;
    }

    public async Task<bool> DeleteHistory(string accessToken, string historyId)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);

        var history = await historyRepository.GetByIdAsync(historyId);

        if (history == null)
        {
            throw new KeyNotFoundException("Không tìm thấy History với ID đã cung cấp");
        }

        if (user.id != history.user_id && !AuthorizeHelper.AuthorizeForEmployee(user))
            throw new UnauthorizedAccessException("Bạn không có quyền xóa history này");

        await chatBoxRepository.DeleteByHistoryId(historyId);
        
        await historyRepository.DeleteAsync(historyId);

        return true;
    }

    private static void EnsureCanReadHistory(User user, History history)
    {
        if (user.id != history.user_id && !AuthorizeHelper.AuthorizeForEmployee(user))
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập lịch sử trò chuyện này.");
    }
}
