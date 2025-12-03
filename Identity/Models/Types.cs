namespace JwtAuth.Identity.Jwts
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
}
