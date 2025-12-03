namespace auth.Config
{
    public sealed class AppSettings
    {
        public Database Database { get; init; } = new();

        public string JwtSecret { get; set; } = "a-string-secret-at-least-256-bits-l123";

        public string JwtIssuer { get; set; } = "anhvt26";

        public string JwtAudience { get; set; } = "audience";

    }
    public class Database
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
