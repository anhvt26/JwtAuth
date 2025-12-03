using auth.Extensions;
using JwtAuth.Database;

namespace auth.Repositories
{
    public interface IBaseRepository
    {
        TResult ExecuteQuery<TResult>(Func<AppDbContext, TResult> func);
        TResult ExecuteTransaction<TResult>(Func<AppDbContext, TResult> func);
    }
    public class BaseRepository(AppDbContext dbContext) : IBaseRepository
    {
        public virtual TResult ExecuteQuery<TResult>(Func<AppDbContext, TResult> func)
        {
            return dbContext.ExecuteQuery(func);
        }

        public virtual TResult ExecuteTransaction<TResult>(Func<AppDbContext, TResult> func)
        {
            return dbContext.ExecuteTransaction(func);
        }
    }
}
