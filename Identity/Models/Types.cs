using System.ComponentModel;

namespace JwtAuth.Identity.Models
{
    public class JwtTokenClaimType
    {
        public const string UserRole = "UserRole";

        public const string UserAction = "UserAction";

        public const string UserSpecialAction = "UserSpecialAction";
    }

    public enum JwtTokenType
    {
        ACCESS,
        REFRESH
    }

    public enum JwtTokenAudience
    {
        WEB = 0,
        MOBILE,
        DESKTOP,
        SERVICE
    }
}
