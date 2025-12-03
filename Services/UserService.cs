using auth.Database.Entity;
using auth.Repositories;

namespace auth.Services
{
    public interface IUserService
    {
        List<User> GetAll();
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
    }
}
