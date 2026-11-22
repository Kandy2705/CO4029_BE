using CO4029_BE.Application.Abstractions;
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
            services.AddMemoryCache();
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<AccountService>();
            services.AddScoped<OtpService>();
            services.AddScoped<HistoryService>();
            services.AddScoped<CampusService>();
            services.AddScoped<CampusLocationService>();
            services.AddScoped<CampusMapService>();
            services.AddScoped<ChatboxService>();
            services.AddScoped<QuestionService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<EmailService>();
            services.AddScoped<IOtpMailer>(provider => provider.GetRequiredService<EmailService>());
            services.AddHttpClient<ChatbotApiService>();
            return services;
        }
    }
}
