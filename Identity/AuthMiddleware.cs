using auth.Database;
using JwtAuth.ExceptionHandling;
using JwtAuth.Identity.Jwts;
using JwtAuth.Security.Jwts;
using JwtAuth.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace JwtAuth.Identity
{
    public class AuthMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<AuthMiddleware>>();

            var endpoint = context.GetEndpoint();
            var authAttributes = endpoint?.Metadata.GetOrderedMetadata<AuthorisedAttribute>();
            var requiresAuth = false;
            AuthorisedAttribute? authorisedAttribute = null;
            if (authAttributes is { Count: > 0 })
            {
                authorisedAttribute = authAttributes[^1];
                requiresAuth = authorisedAttribute.Required;
            }
            if (!requiresAuth || authorisedAttribute == null)
            {
                if (ClaimUserFromToken(context, logger, out var user))
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.UserData, user.ToStringJson()),
                        new("UserSpecialPermission", "0")
                    };

                    var identity = new ClaimsIdentity(claims, "Bearer");
                    context.User.AddIdentity(identity);
                }
            }
            else
            {
                await ValidateToken(context, authorisedAttribute, logger);
            }
            await next(context);
        }

        private static Task ValidateToken(HttpContext context, AuthorisedAttribute authAttribute, ILogger<AuthMiddleware> logger)
        {
            var request = context.Request;

            var requestApi = request.Path.ToString();

            // "Bearer + token"
            var requestHeader = request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(requestHeader) || !requestHeader.StartsWith("Bearer "))
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token thiếu hoặc không hợp lệ");

            //token string without "Bearer"
            var token = requestHeader["Bearer ".Length..].Trim();

            UserJwtTokenInfo? userInfo;
            try
            {
                var tokenClaims = JwtManager.ClaimTokens(token, false);
                if (requestApi != "/api/Auth/refresh" && tokenClaims.Type != JwtTokenType.ACCESS ||
                    requestApi == "/api/Auth/refresh" && tokenClaims.Type != JwtTokenType.REFRESH ||
                    !ClaimUserFromToken(context, logger, out userInfo))
                    throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token không hợp lệ");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating token");
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token không hợp lệ");
            }

            var claims = new List<Claim>
            {
                new (ClaimTypes.UserData, userInfo.ToStringJson())
            };

            var identity = new ClaimsIdentity(claims, "Bearer");
            context.User.AddIdentity(identity);
            return Task.CompletedTask;
        }

        private static bool ClaimUserFromToken(HttpContext context, ILogger<AuthMiddleware> logger, [NotNullWhen(true)] out UserJwtTokenInfo? userInfo)
        {
            userInfo = null;
            try
            {
                var auHeader = context.Request.Headers.Authorization.ToString();
                if (string.IsNullOrWhiteSpace(auHeader) || !auHeader.StartsWith("Bearer "))
                    return false;

                var token = auHeader["Bearer ".Length..].Trim();
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                var userTokenInfo = JwtManager.ClaimUserTokenInfo(token, false);
                var tokenTimes = context.RequestServices.GetRequiredService<AppDbContext>().Users
                    .AsNoTracking()
                    .FirstOrDefault(usr => usr.Uuid == userTokenInfo.UserUuid)
                    ?.TokenTimes ?? 0;

                if (!JwtManager.ValidateToken(token, tokenTimes: tokenTimes))
                    return false;

                userInfo = userTokenInfo;
                return true;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error claiming user from token");
                return false;
            }
        }
    }
}
