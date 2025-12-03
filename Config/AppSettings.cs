namespace JwtAuth.Config
{
    public sealed class AppSettings
    {
        public Database Database { get; init; } = new();

        public string JwtSecret { get; set; } = "fuckkkkkk";

        public string JwtIssuer { get; set; } = "anhvt26";

        public List<string> JwtAudience { get; set; } = ["web", "mobile", "desktop", "service"];

    }
    public class Database
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
