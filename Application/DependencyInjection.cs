using CO4029_BE.Application.Services;
using CO4029_BE.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CO4029_BE.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<HistoryService>();
            services.AddScoped<CampusService>();
            services.AddScoped<CampusLocationService>();
            services.AddScoped<ChatboxService>();
            services.AddScoped<QuestionService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<EmailService>();
            services.AddHttpClient<ChatbotApiService>();
            return services;
        }
    }
}
