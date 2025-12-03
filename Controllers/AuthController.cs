using JwtAuth.Identity;
using JwtAuth.Identity.Models;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers
{
    public class AuthController(IAuthService authService) : BaseController
    {
        [HttpPost("login-web")]
        [Authorised(false)]
        public IActionResult LoginWeb(LoginRequest request)
        {
            var tokenResponse = authService.Login(request, JwtTokenAudience.WEB);
            return Ok(tokenResponse);
        }

        [HttpPost("login-app")]
        [Authorised(false)]
        public IActionResult LoginApp(LoginRequest request)
        {
            var tokenResponse = authService.Login(request, JwtTokenAudience.MOBILE);
            return Ok(tokenResponse);
        }
    }
}
