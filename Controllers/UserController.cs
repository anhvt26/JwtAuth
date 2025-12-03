using auth.Services;
using JwtAuth.Identity;
using JwtAuth.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtAuth.Controllers
{
    public class UserController(IUserService userService) : BaseController
    {
        [HttpPost("get-all")]
        [Authorised(false)]
        public IActionResult GetAll()
        { 
            var users = userService.GetAll();
            return Ok(users);
        }

        [HttpPost("get-user")]
        [Authorised(false)]
        public IActionResult GetUser(string uuid)
        {
            return Ok(userService.GetUser(uuid, User.GetUserTokenInfo()));
        }
    }
}
