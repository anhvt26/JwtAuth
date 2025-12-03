using JwtAuth.Identity;
using JwtAuth.Identity.Models;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers
{
    public class AuthController(IAuthService authService) : BaseController
    {
        [HttpPost("login")]
        [Authorised(false)]
        public IActionResult Login(LoginRequest request)
        {
            var tokenResponse = authService.Login(request);
            return Ok(tokenResponse);
        }
    }
}
