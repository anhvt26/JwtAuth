using JwtAuth.Database;
using JwtAuth.ExceptionHandling;
using JwtAuth.Identity;
using JwtAuth.Identity.Models;
using JwtAuth.Security.Jwts;
using JwtAuth.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class AuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<AuthMiddleware>>();

        var endpoint = context.GetEndpoint();
        var authAttr = endpoint?.Metadata.GetMetadata<AuthorisedAttribute>();
        bool requiresAuth = authAttr?.Required ?? false;

        var allowedAudiencesAttr = endpoint?.Metadata.GetMetadata<AllowedAudiencesAttribute>();
        string[] allowedAudiences = allowedAudiencesAttr?.Audiences!; 

        if (!requiresAuth)
        {
            if (TryBuildUser(context, logger, allowedAudiences, out var user))
            {
                AttachPrincipal(context, user);
            }
        }
        else
        {
            var user = ValidateAndGetUser(context, logger, allowedAudiences);
            AttachPrincipal(context, user);
        }

        await next(context);
    }

    private static void AttachPrincipal(HttpContext context, UserJwtTokenInfo user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.UserData, user.ToStringJson())
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User.AddIdentity(identity);
    }

    private static bool TryBuildUser(HttpContext context, ILogger logger, string[] allowedAudiences, out UserJwtTokenInfo userInfo)
    {
        userInfo = null!;
        try
        {
            var token = ExtractBearerToken(context);
            if (string.IsNullOrWhiteSpace(token)) return false;

            var tokenClaims = JwtManager.ClaimTokens(token, false);
            if (!allowedAudiences.Contains(tokenClaims.Audience)) return false;

            var db = context.RequestServices.GetRequiredService<AppDbContext>();
            var tokenTimes = db.Users.AsNoTracking()
                                     .Where(u => u.Uuid == tokenClaims.UserUuid)
                                     .Select(u => u.TokenTimes)
                                     .FirstOrDefault();

            if (JwtManager.ValidateToken(token, tokenClaims.Audience, tokenClaims.Type, tokenTimes: tokenTimes) == null)
                return false;

            userInfo = JwtManager.ClaimUserTokenInfo(token, false);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error claiming user from token");
            return false;
        }
    }

    private static UserJwtTokenInfo ValidateAndGetUser(HttpContext context, ILogger logger, string[] allowedAudiences)
    {
        var token = ExtractBearerToken(context);

        if (string.IsNullOrWhiteSpace(token))
            throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token thiếu hoặc không hợp lệ");

        var tokenClaims = JwtManager.ClaimTokens(token, false);

        if (!allowedAudiences.Contains(tokenClaims.Audience))
            throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Audience không hợp lệ");

        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        var tokenTimes = db.Users.AsNoTracking()
                                 .Where(u => u.Uuid == tokenClaims.UserUuid)
                                 .Select(u => u.TokenTimes)
                                 .FirstOrDefault();

        if (JwtManager.ValidateToken(token, tokenClaims.Audience, tokenClaims.Type, tokenTimes: tokenTimes) == null)
            throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token không hợp lệ");

        return JwtManager.ClaimUserTokenInfo(token, false);
    }

    private static string ExtractBearerToken(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer "))
            return "";
        return header["Bearer ".Length..].Trim();
    }
}
