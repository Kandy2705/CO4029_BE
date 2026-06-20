using CO4029_BE.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Supabase;

namespace CO4029_BE.Infrastructure
{
    public static class DependencyInjection
    {
        public static string supabaseApiKey;
        public static string supabaseUrl;
        public static string supabaseAdminKey;
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            supabaseApiKey = config["supabaseApiKey"];
            supabaseUrl = config["supabaseUrl"];
            supabaseAdminKey = config["SupabaseAdminKey"] ?? supabaseApiKey;
            services.AddSingleton<Supabase.Client>(_ =>
                new Supabase.Client(
                    supabaseUrl,
                    supabaseAdminKey,
                    new SupabaseOptions
                    {
                        AutoRefreshToken = true,
                        AutoConnectRealtime = true
                    }
                )
            );
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IBuildingRepository, BuildingRepository>();
            services.AddScoped<IChatBoxRepository, ChatBoxRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IAnswerRepository, AnswerRepository>();
            return services;
        }
    }
}