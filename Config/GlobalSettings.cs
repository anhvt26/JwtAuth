namespace auth.Config
{
    public class GlobalSettings
    {
        public static AppSettings AppSettings { get; private set; } = null!;

        public static void IncludeConfig(AppSettings? appSetting)
        {
            AppSettings = appSetting ?? throw new ArgumentNullException(nameof(appSetting), "AppSettings is null");
        }
        public static string ConnectionString { get; set; } = string.Empty;
    }
}
