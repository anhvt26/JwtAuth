using auth.Config;
using JWT;
using JWT.Builder;
using JwtAuth.Identity.Jwts;
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
            bool musverifySignature = true, 
            int tokenTimes = 0)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(lifeTime).ToUnixTimeSeconds();

            var builder = GetJwtBuilder(musverifySignature, tokenTimes)
                .AddClaims(claims)
                .AddClaim("type", type)
                .AddClaim("expireAt", expiresAt)
                .AddClaim("expireIn", lifeTime)
                .AddClaim("jti", Guid.NewGuid().ToString());

            return builder.Encode();
        }

        public static string GenerateAccessToken(List<KeyValuePair<string, object>> claims, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            return GenerateJwt(claims, JwtTokenType.ACCESS, JwtToken.AccessTokenLifetime, mustVerifySignature, tokenTimes);
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
            try
            {
                accessTokenJti = claims["access_token_jti"].ToString() ?? "";
            }
            catch{/*ignore*/}

            return new JwtTokenClaims
            {
                Value = token,
                Type = type,
                ExpiresAt = expiresAt,
                Jti = jti,
                AccessTokenJti = accessTokenJti
            };
        }

        public static string GenerateRefreshToken(string accessToken, List<KeyValuePair<string, object>> userClaims, 
            bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = new List<KeyValuePair<string, object>>
            {
                new("access_token_jti", ClaimTokens(accessToken, mustVerifySignature, tokenTimes).Jti)
            };

            return GenerateJwt(claims, JwtTokenType.REFRESH, JwtToken.RefreshTokenLifetime, mustVerifySignature, tokenTimes);
        }
        public static UserJwtTokenInfo ClaimUserTokenInfo(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var claims = Claims(token, mustVerifySignature, tokenTimes);
            var userTokenInfo = claims.FirstOrDefault(c => c.Key == "user_token_info").Value as string ?? "";
            return userTokenInfo.DeserializeJson<UserJwtTokenInfo>() ?? new UserJwtTokenInfo();
        }

        public static bool ValidateToken(string token, bool mustVerifySignature = true, int tokenTimes = 0)
        {
            var alive = true;
            var tokenClaims = ClaimTokens(token, mustVerifySignature, tokenTimes);
            if (tokenClaims.Type == JwtTokenType.REFRESH)
            {
                var accessTokenJti = Claims(token, mustVerifySignature, tokenTimes)["access_token_jti"].ToString();
                if (string.IsNullOrWhiteSpace(accessTokenJti))
                {
                    alive = false;
                }
            }

            var isExpired = tokenClaims.ExpiresAt < DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();
            if (isExpired)
            {
                alive = false;
            }

            return alive;
        }
    }
}
