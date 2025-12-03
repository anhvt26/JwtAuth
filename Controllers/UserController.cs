using auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        [HttpPost]
        public IActionResult GetAll()
        { 
            var users = userService.GetAll();
            return Ok(users);
        }
    }
}
