using auth.Config;
using Microsoft.EntityFrameworkCore;

namespace auth.Extensions
{
    public static class DatabaseExtensions
    {
        private static TDbContext GetDbContext<TDbContext>(string connectionString = "") where TDbContext : DbContext
        {
            var connString = string.IsNullOrEmpty(connectionString) ? GlobalSettings.ConnectionString : connectionString;

            Console.WriteLine("Using Connection String: " + connString);

            if (string.IsNullOrEmpty(connString))
            {
                throw new Exception("Connection string empty");
            }

            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString));
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();

            var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
            context.Database.OpenConnection();

            return context;
        }

        public static TResult ExecuteQuery<TResult, TDbContext>(Func<TDbContext, TResult> query)
        where TDbContext : DbContext
        {
            using var context = GetDbContext<TDbContext>();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var result = query(context);
                return result;
            }
            finally
            {
                transaction.Rollback();
            }
        }

        public static TResult ExecuteQuery<TDbContext, TResult>(this TDbContext context, Func<TDbContext, TResult> query)
        where TDbContext : DbContext
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var result = query(context);
                return result;
            }
            finally
            {
                transaction.Rollback();
            }
        }

        public static TResult ExecuteTransaction<TDbContext, TResult>(this TDbContext context,
        Func<TDbContext, TResult> func)
        where TDbContext : DbContext
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                var result = func(context);
                context.SaveChanges();
                transaction.Commit();
                return result;
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
