using CO4029_BE.Infrastructure.Repository;
using CO4029_BE.API.Contracts.Mappers;
using CO4029_BE.API.Contracts.Requests;
using CO4029_BE.API.Contracts.Responses;
using CO4029_BE.Domain.Entities;
using CO4029_BE.Utils;
using Supabase;

namespace CO4029_BE.Application.Services;

public class HistoryService
{
    private readonly IHistoryRepository historyRepository;
    private readonly IUserRepository userRepository;
    private readonly Client _supabaseClient;

    public HistoryService(IHistoryRepository historyRepository, IUserRepository userRepository, Client supabaseClient)
    {
        this.userRepository = userRepository;
        this.historyRepository = historyRepository;
        this._supabaseClient = supabaseClient;
    }
    
    public async Task<HistoryReponse> CreateHistory(CreateHistoryRequest createHistoryRequest, string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);

        var history = new History
        {
            header = createHistoryRequest.Header,
            create_date = createHistoryRequest.Create_date,
            user_id = user.id,
        };
        var reponse =  await historyRepository.CreateAsync(history);
        return HistoryMapper.ToReponse(reponse);
    }

    public async Task<IEnumerable<HistoryReponse>> GetHistoryByUserId(string accessToken)
    {
        var user = await AccessToken.GetUser(accessToken, _supabaseClient, userRepository);
        
        var histories = await historyRepository.GetHistorysByUserId(user.id);
        
        return histories.Select(h => HistoryMapper.ToReponse(h));
    }

    public async Task<PagedResponse<HistoryReponse>> GetHistoriesForAdminAsync(
        string accessToken,
        int page,
        int pageSize,
        string? userId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        await AccessToken.GetAdminUser(accessToken, _supabaseClient, userRepository);

        if (page < 1)
            throw new ArgumentException("page phải lớn hơn hoặc bằng 1.");
        if (pageSize is < 1 or > 100)
            throw new ArgumentException("pageSize phải nằm trong khoảng 1 đến 100.");
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            throw new ArgumentException("fromDate không được lớn hơn toDate.");

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var targetUser = await userRepository.GetByIdAsync(userId);
            if (targetUser == null)
                throw new KeyNotFoundException("Không tìm thấy người dùng.");
        }

        var offset = (page - 1) * pageSize;
        var result = await historyRepository.GetPagedAsync(
            offset,
            pageSize,
            userId,
            fromDate,
            toDate);

        return PagedResponse<HistoryReponse>.Create(
            result.Items.Select(history => history.ToReponse()),
            page,
            pageSize,
            result.TotalItems);
    }
}
