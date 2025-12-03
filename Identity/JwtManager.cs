using auth.Config;
using JWT;
using JWT.Builder;
using JwtAuth.ExceptionHandling;
using JwtAuth.Identity.Models;
using JwtAuth.Security.Jwts;
using JwtAuth.Utilities;

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
            if (mustVerifySignature)
                builder = builder.MustVerifySignature();
            else
                builder = builder.WithVerifySignature(false);
            return builder;
        }

        private static string GenerateJwt(
            IEnumerable<KeyValuePair<string, object>> claims,
            JwtTokenType type,
            long lifeTime,
            string audience,
            bool musverifySignature = true,
            int tokenTimes = 0)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(lifeTime).ToUnixTimeSeconds();

            var builder = GetJwtBuilder(musverifySignature, tokenTimes)
                .AddClaims(claims)
                .AddClaim("type", type)
                .AddClaim("expireAt", expiresAt)
                .AddClaim("expireIn", lifeTime)
                .AddClaim("jti", Guid.NewGuid().ToString())
                .AddClaim("iss", GlobalSettings.AppSettings.JwtIssuer)
                .AddClaim("aud", audience);

            return builder.Encode();
        }

        public static string GenerateAccessToken(List<KeyValuePair<string, object>> claims, string audience, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            return GenerateJwt(claims, JwtTokenType.ACCESS, JwtToken.AccessTokenLifetime, audience, mustVerifySignature, tokenTimes);
        }

        private static Dictionary<string, object> Claims(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var builder = GetJwtBuilder(mustVerifySignature, tokenTimes);
            return builder.Decode<Dictionary<string, object>>(token);
        }

        public static JwtTokenClaims ClaimTokens(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = Claims(token, mustVerifySignature, tokenTimes);
            var type = (JwtTokenType)int.Parse(claims["type"].ToString()!);
            var expiresAt = long.Parse(claims["expireAt"].ToString() ?? "0");
            var jti = claims["jti"].ToString() ?? "";
            var accessTokenJti = "";
            var iss = claims["iss"].ToString() ?? "";
            var aud = claims["aud"].ToString() ?? "";

            try
            {
                accessTokenJti = claims["access_token_jti"].ToString() ?? "";
            }
            catch {/*ignore*/}

            UserJwtTokenInfo? userTokenInfo = null;
            try
            {
                if (claims.TryGetValue("user_token_info", out var json) && json is string s)
                {
                    userTokenInfo = s.DeserializeJson<UserJwtTokenInfo>();
                }
            }
            catch { /*ignore*/ }

            return new JwtTokenClaims
            {
                Value = token,
                Type = type,
                ExpiresAt = expiresAt,
                Jti = jti,
                AccessTokenJti = accessTokenJti,
                Issuer = iss,
                Audience = aud,
                UserUuid = userTokenInfo?.UserUuid ?? ""
            };
        }

        public static string GenerateRefreshToken(
            string accessToken, 
            List<KeyValuePair<string, object>> userClaims,
            string audience,
            bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = new List<KeyValuePair<string, object>>
            {
                new("access_token_jti", ClaimTokens(accessToken, mustVerifySignature, tokenTimes).Jti)
            };

            return GenerateJwt(claims.Concat(userClaims), JwtTokenType.REFRESH, JwtToken.RefreshTokenLifetime, audience, mustVerifySignature, tokenTimes);
        }
        public static UserJwtTokenInfo ClaimUserTokenInfo(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = Claims(token, mustVerifySignature, tokenTimes);
            var userTokenInfo = claims.FirstOrDefault(c => c.Key == "user_token_info").Value as string ?? "";
            return userTokenInfo.DeserializeJson<UserJwtTokenInfo>() ?? new UserJwtTokenInfo();
        }

        public static JwtTokenClaims ValidateToken(
            string token,
            string expectedAudience,
            JwtTokenType expectedType,
            bool mustVerifySignature = true,
            int tokenTimes = 0)
        {
            var tokenClaims = ClaimTokens(token, mustVerifySignature, tokenTimes);

            if (tokenClaims.Type != expectedType)
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, $"Token type không hợp lệ. Expected: {expectedType}, Actual: {tokenClaims.Type}");

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (tokenClaims.ExpiresAt < now)
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Token đã hết hạn");

            if (!string.Equals(tokenClaims.Issuer, GlobalSettings.AppSettings.JwtIssuer, StringComparison.Ordinal))
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Issuer không hợp lệ");

            if (!string.Equals(tokenClaims.Audience, expectedAudience, StringComparison.Ordinal))
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Audience không hợp lệ");

            if (tokenClaims.Type == JwtTokenType.REFRESH && string.IsNullOrWhiteSpace(tokenClaims.AccessTokenJti))
                throw new ErrorException(ErrorCode.UN_AUTHORIZED, "Refresh token thiếu access_token_jti");

            return tokenClaims;
        }
    }
}
