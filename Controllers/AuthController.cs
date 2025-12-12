using JwtAuth.Identity;
using JwtAuth.Identity.Models;
using JwtAuth.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuth.Controllers
{
    public class AuthController(IAuthService authService) : BaseController
    {
        [HttpPost("login-web")]
        [Authorised(false)]
        public IActionResult LoginWeb(LoginRequest request)
        {
            var tokenResponse = authService.Login(request, JwtTokenAudience.Web);
            return Ok(tokenResponse);
        }

        [HttpPost("login-app")]
        [Authorised(false)]
        public IActionResult LoginApp(LoginRequest request)
        {
            var tokenResponse = authService.Login(request, JwtTokenAudience.Mobile);
            return Ok(tokenResponse);
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var token = HttpContext.Request.Headers.Authorization.ToString()[7..];
            return Ok(authService.RefreshToken(token));
        }

        [HttpPost("log-out-all")]
        public IActionResult LogoutAll()
        {
            authService.LogoutAll(User.GetUserTokenInfo()!);
            return Ok();
        }

        [HttpPost("log-out-locally")]
        public IActionResult LogoutLocally()
        {
            var token = HttpContext.Request.Headers.Authorization.ToString()[7..];
            authService.LogoutLocally(User.GetUserTokenInfo()!, token);
            return Ok();
        }
    }
}
