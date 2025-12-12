using JwtAuth.Identity.Models;
using System.Text.Json.Serialization;

namespace JwtAuth.Security.Jwts
{
    public class JwtToken // original model for tokens
    {
        [JsonIgnore]
        public static int AccessTokenLifetime  = 1 * 24 * 60 * 60; 

        [JsonIgnore]
        public static int RefreshTokenLifetime = 7 * 24 * 60 * 60;

        [JsonIgnore]
        public static int RefreshRootTokenLifetime = 30 * 24 * 60 * 60;

        public string AccessToken { get; init; } = "";

        public string RefreshToken { get; init; } = "";

        public DateTime AccessExpiresAt { get; init; }

        public DateTime RefreshExpiresAt { get; init; }
    }
    public class JwtTokenClaims //claims from token
    {
        public string UserUuid { get; set; } = "";

        public string Value { get; set; } = "";
        
        public JwtTokenType Type { get; set; }
        
        public long ExpiresAt { get; set; }
        
        public string Jti { get; set; } = "";
        
        public string AccessTokenJti { get; set; } = "";

        public string Issuer { get; set; } = "";

        public string Audience { get; set; } = "";

        public string? DeviceUuid { get; set; }

        public int IsRevoked { get; set; }

        public DateTime? RefreshRootExpireAt { get; set; }
    }

    public class UserJwtTokenInfo // user info stored in token, key: "user_token_info"
    {
        public string UserUuid { get; set; } = "";
        
        public string UserName { get; set; } = "Unknown User";
        
        public List<string> UserRoles { get; set; } = [];
        
        public int TokenTimes { get; set; }
        
        public int AccountType { get; set; }
        
        public string PhoneNumber { get; set; } = "";

        public string? DeviceUuid { get; set; }

        public DateTime? RefreshRootExpireAt { get; set; }
    }
}
