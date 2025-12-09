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
        Access,
        Refresh
    }

    public enum JwtTokenAudience
    {
        Web = 0,
        Mobile,
        Desktop,
        Service
    }
}
