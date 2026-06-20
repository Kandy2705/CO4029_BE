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
}