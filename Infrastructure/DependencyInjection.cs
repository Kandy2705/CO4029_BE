using CO4029_BE.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Supabase;

namespace CO4029_BE.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration config)
        {
            var supabaseUrl = FirstConfigured(
                config["Supabase:Url"],
                config["supabaseUrl"]);
            var supabaseKey = FirstConfigured(
                config["Supabase:ServiceRoleKey"],
                config["Supabase:PublishableKey"],
                config["SupabaseAdminKey"],
                config["supabaseApiKey"]);

            if (supabaseUrl == null || supabaseKey == null)
            {
                throw new InvalidOperationException(
                    "Supabase configuration is missing. Configure Supabase__Url and either Supabase__ServiceRoleKey or Supabase__PublishableKey.");
            }

            services.AddSingleton<Supabase.Client>(_ =>
                new Supabase.Client(
                    supabaseUrl,
                    supabaseKey,
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
            services.AddScoped<IFloorRepository, FloorRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IChatBoxRepository, ChatBoxRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<IAnswerRepository, AnswerRepository>();
            return services;
        }

        private static string? FirstConfigured(params string?[] values)
            => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
