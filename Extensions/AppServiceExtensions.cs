using auth.Config;
using Microsoft.EntityFrameworkCore;

namespace auth.Extensions
{
    public static class AppServiceExtensions
    {
        public static void ConfigureDbContext<TDbContext>(this IServiceCollection services, string connectionString)
            where TDbContext : DbContext
        {
            GlobalSettings.ConnectionString = connectionString;
            services.AddDbContext<TDbContext>(o =>
            {
                o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                o.EnableDetailedErrors();
                o.EnableSensitiveDataLogging();
            });
        }
    }
}
