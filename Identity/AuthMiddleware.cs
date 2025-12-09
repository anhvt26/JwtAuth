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
        var requiresAuth = authAttr?.Required ?? false;

        var allowedAudiences = endpoint?.Metadata
            .GetMetadata<AllowedAudiencesAttribute>()
            ?.Audiences
            ?? Array.Empty<string>();

        if (!requiresAuth)
        {
            if (TryBuildUser(context, logger, allowedAudiences, out var user))
                AttachPrincipal(context, user);
        }
        else
        {
            var user = ValidateAndGetUser(context, logger, allowedAudiences);
            AttachPrincipal(context, user);
        }

        await next(context);
    }

    // Attach user info as ClaimsIdentity
    private static void AttachPrincipal(HttpContext context, UserJwtTokenInfo user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.UserData, user.ToStringJson())
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User.AddIdentity(identity);
    }

    // OPTIONAL AUTH
    private static bool TryBuildUser(HttpContext context, ILogger logger, string[] allowedAudiences, out UserJwtTokenInfo userInfo)
    {
        userInfo = null!;
        try
        {
            var token = ExtractBearerToken(context);
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Step 1: decode without signature (ONLY to read minimal fields)
            var unsafeClaims = JwtManager.ClaimTokens(token, mustVerifySignature: false);

            // Audience không khớp → bỏ qua
            if (!allowedAudiences.Contains(unsafeClaims.Audience))
                return false;

            // Token type phải là Access → Refresh không được dùng để auth
            if (unsafeClaims.Type != JwtTokenType.Access)
                return false;

            // Step 2: lấy tokenTimes từ DB (dùng để verify signature lần 2)
            var db = context.RequestServices.GetRequiredService<AppDbContext>();
            var tokenTimes = db.Users.AsNoTracking()
                                     .Where(u => u.Uuid == unsafeClaims.UserUuid)
                                     .Select(u => u.TokenTimes)
                                     .FirstOrDefault();

            // Step 3: validate FULL (verify signature + expiry + issuer + type + audience)
            var fullClaims = JwtManager.ValidateToken(
                token,
                expectedAudience: unsafeClaims.Audience,
                expectedType: JwtTokenType.Access,
                mustVerifySignature: true,
                tokenTimes: tokenTimes
            );

            // Step 4: lấy user info
            userInfo = JwtManager.ClaimUserTokenInfo(token, mustVerifySignature: true, tokenTimes: tokenTimes);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in optional auth.");
            return false;
        }
    }

    // REQUIRED AUTH
    private static UserJwtTokenInfo ValidateAndGetUser(HttpContext context, ILogger logger, string[] allowedAudiences)
    {
        var token = ExtractBearerToken(context);
        if (string.IsNullOrWhiteSpace(token))
            throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token thiếu");

        // Decode không verify để đọc audience
        var unsafeClaims = JwtManager.ClaimTokens(token, false);

        if (!allowedAudiences.Contains(unsafeClaims.Audience))
            throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Audience không hợp lệ");

        // Refresh token không được dùng để truy cập API
        if (unsafeClaims.Type != JwtTokenType.Access)
            throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Chỉ chấp nhận Access Token");

        // Lấy tokenTimes
        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        var tokenTimes = db.Users.AsNoTracking()
                                 .Where(u => u.Uuid == unsafeClaims.UserUuid)
                                 .Select(u => u.TokenTimes)
                                 .FirstOrDefault();

        // Validate FULL
        JwtManager.ValidateToken(
            token,
            expectedAudience: unsafeClaims.Audience,
            expectedType: JwtTokenType.Access,
            mustVerifySignature: true,
            tokenTimes: tokenTimes
        );

        // Decode final, must verify signature
        return JwtManager.ClaimUserTokenInfo(token, mustVerifySignature: true, tokenTimes: tokenTimes);
    }

    private static string ExtractBearerToken(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return "";
        return header["Bearer ".Length..].Trim();
    }
}

