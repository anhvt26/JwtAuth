using auth.Config;
using JWT;
using JWT.Builder;
using JwtAuth.ExceptionHandling;
using JwtAuth.Identity.Models;
using JwtAuth.Security.Jwts;
using JwtAuth.Utilities;
using System.Globalization;

namespace JwtAuth.Identity
{
    public static class JwtManager
    {
        private static JwtBuilder GetJwtBuilder(bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var builder = new JwtBuilder()
                .WithAlgorithm(new JWT.Algorithms.HMACSHA256Algorithm())
                .WithJsonSerializer(new JWT.Serializers.JsonNetSerializer())
                .WithUrlEncoder(new JwtBase64UrlEncoder())
                .WithSecret(GlobalSettings.AppSettings.JwtSecret + tokenTimes);

            return mustVerifySignature ? builder.MustVerifySignature()
                                       : builder.WithVerifySignature(false);
        }

        private static string GenerateJwt(
            IEnumerable<KeyValuePair<string, object>> claims,
            JwtTokenType type,
            long lifeTime,
            string audience,
            bool musverifySignature = true,
            DateTime? refreshRootExpireAt = null,
            int tokenTimes = 0)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(lifeTime).ToUnixTimeSeconds();

            var builder = GetJwtBuilder(musverifySignature, tokenTimes)
                .AddClaims(claims)
                .AddClaim("type", (int)type)
                .AddClaim("expireAt", expiresAt)
                .AddClaim("expireIn", lifeTime)
                .AddClaim("jti", Guid.NewGuid().ToString())
                .AddClaim("iss", GlobalSettings.AppSettings.JwtIssuer)
                .AddClaim("aud", audience);

            if (refreshRootExpireAt.HasValue)
                builder = builder.AddClaim("rootExpire",
                    refreshRootExpireAt.Value.ToUniversalTime().ToString("o"));

            return builder.Encode();
        }

        public static string GenerateAccessToken(
            List<KeyValuePair<string, object>> claims,
            string audience,
            bool mustVerifySignature = true,
            int tokenTimes = 0)
        {
            return GenerateJwt(
                claims,
                JwtTokenType.Access,
                JwtToken.AccessTokenLifetime,
                audience,
                mustVerifySignature,
                null,
                tokenTimes
            );
        }

        private static Dictionary<string, object> Claims(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var builder = GetJwtBuilder(mustVerifySignature, tokenTimes);
            return builder.Decode<Dictionary<string, object>>(token);
        }

        public static JwtTokenClaims ClaimTokens(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = Claims(token, mustVerifySignature, tokenTimes);

            // type
            JwtTokenType type = JwtTokenType.Access;
            if (claims.TryGetValue("type", out var tObj))
                type = (JwtTokenType)Convert.ToInt32(tObj);

            // expires
            long expiresAt = claims.TryGetValue("expireAt", out var expObj)
                ? Convert.ToInt64(expObj)
                : 0;

            string jti = claims.TryGetValue("jti", out var jtiObj)
                ? jtiObj?.ToString() ?? ""
                : "";

            string iss = claims.TryGetValue("iss", out var issObj)
                ? issObj?.ToString() ?? ""
                : "";

            string aud = claims.TryGetValue("aud", out var audObj)
                ? audObj?.ToString() ?? ""
                : "";

            // root expire
            DateTime? rootExpire = null;
            if (claims.TryGetValue("rootExpire", out var rootObj) &&
                rootObj is string s &&
                DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt))
            {
                rootExpire = dt;
            }

            // access_token_jti (refresh token only)
            string accessTokenJti = claims.TryGetValue("access_token_jti", out var atjObj)
                ? atjObj?.ToString() ?? ""
                : "";

            // user_token_info
            UserJwtTokenInfo? userTokenInfo = null;
            if (claims.TryGetValue("user_token_info", out var jsonObj) && jsonObj is string jsonStr)
            {
                userTokenInfo = jsonStr.DeserializeJson<UserJwtTokenInfo>();
            }

            return new JwtTokenClaims
            {
                Value = token,
                Type = type,
                ExpiresAt = expiresAt,
                Jti = jti,
                AccessTokenJti = accessTokenJti,
                Issuer = iss,
                Audience = aud,
                UserUuid = userTokenInfo?.UserUuid ?? "",
                RefreshRootExpireAt = rootExpire
            };
        }

        public static string GenerateRefreshToken(
            string accessToken,
            List<KeyValuePair<string, object>> userClaims,
            string audience,
            bool mustVerifySignature = true,
            DateTime? refreshRootExpireAt = null,
            int tokenTimes = 0)
        {
            var accessClaims = ClaimTokens(accessToken, mustVerifySignature, tokenTimes);

            var claims = new List<KeyValuePair<string, object>>
            {
                new("access_token_jti", accessClaims.Jti)
            };

            return GenerateJwt(
                claims.Concat(userClaims),
                JwtTokenType.Refresh,
                JwtToken.RefreshTokenLifetime,
                audience,
                mustVerifySignature,
                refreshRootExpireAt,
                tokenTimes
            );
        }

        public static UserJwtTokenInfo ClaimUserTokenInfo(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = Claims(token, mustVerifySignature, tokenTimes);

            if (claims.TryGetValue("user_token_info", out var json) && json is string s)
                return s.DeserializeJson<UserJwtTokenInfo>() ?? new UserJwtTokenInfo();

            return new UserJwtTokenInfo();
        }

        public static JwtTokenClaims ValidateToken(
            string token,
            string expectedAudience,
            JwtTokenType expectedType,
            bool mustVerifySignature = true,
            int tokenTimes = 0)
        {
            var tokenClaims = ClaimTokens(token, mustVerifySignature, tokenTimes);

            // 1. Kiểu token đúng không?
            if (tokenClaims.Type != expectedType)
                throw new ErrorException(ErrorCode.UN_AUTHORIZED,
                    $"Token type không hợp lệ. Expected: {expectedType}, Actual: {tokenClaims.Type}");

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // 2. Hết hạn chưa?
            if (tokenClaims.ExpiresAt < now)
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token đã hết hạn");

            // 3. Issuer phải đúng
            if (!string.Equals(tokenClaims.Issuer, GlobalSettings.AppSettings.JwtIssuer, StringComparison.Ordinal))
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Issuer không hợp lệ");

            // 4. Audience phải đúng
            if (!string.Equals(tokenClaims.Audience, expectedAudience, StringComparison.Ordinal))
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Audience không hợp lệ");

            // 5. Nếu là refresh token thì phải có access_token_jti
            if (tokenClaims.Type == JwtTokenType.Refresh)
            {
                if (string.IsNullOrWhiteSpace(tokenClaims.AccessTokenJti))
                    throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Refresh token thiếu access_token_jti");
            }

            // 6. Root refresh expired?
            if (tokenClaims.Type == JwtTokenType.Refresh &&
                tokenClaims.RefreshRootExpireAt.HasValue &&
                tokenClaims.RefreshRootExpireAt.Value.ToUniversalTime() < DateTime.UtcNow)
            {
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Refresh root đã hết hạn");
            }
            return tokenClaims;
        }
    }
}
