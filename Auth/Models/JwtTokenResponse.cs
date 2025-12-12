namespace JwtAuth.Identity.Jwts
{
    public class JwtTokenResponse
    {
        public string AccessToken { get; init; } = "";

        public string RefreshToken { get; init; } = "";

        public string? DeviceUuid { get; set; }

        public DateTime AccessExpiresAt { get; init; }

        public DateTime RefreshExpiresAt { get; init; }

        public DateTime? RefreshRootExpireAt { get; set; }
    }
}
