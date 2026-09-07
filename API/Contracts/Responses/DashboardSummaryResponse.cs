namespace CO4029_BE.API.Contracts.Responses;

public class DashboardSummaryResponse
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int DisabledUsers { get; set; }
    public int TotalBuildings { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalAnswers { get; set; }
    public int TotalCategories { get; set; }
    public int TotalHistories { get; set; }
    public int TotalChatboxes { get; set; }
}
