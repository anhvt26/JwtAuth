using auth.Repositories;
using JwtAuth.Database.Entity;
using JwtAuth.ExceptionHandling;
using JwtAuth.Security.Jwts;

namespace auth.Services
{
    public interface IUserService
    {
        List<User> GetAll();
        User GetUser(string uuid, UserJwtTokenInfo? userJwtTokenInfo);
    }
    public class UserService(IBaseRepository baseRepository) : IUserService
    {
        public List<User> GetAll()
        {
            return baseRepository.ExecuteQuery(db =>
            {
                var users = db.Users.ToList();
                return users;
            });
        }

        public User GetUser(string uuid, UserJwtTokenInfo? userJwtTokenInfo)
        {
            return baseRepository.ExecuteTransaction(db =>
            {
                var user = db.Users.FirstOrDefault(u => u.Uuid == uuid) 
                    ?? throw new ErrorException(ErrorCode.NOT_FOUND);
                var isGuest = userJwtTokenInfo == null;
                if (isGuest)
                {
                    user.Count += 1;
                }
                return user;
            });
        }
    }
}
