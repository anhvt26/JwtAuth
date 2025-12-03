using Newtonsoft.Json;
using System.ComponentModel;

namespace JwtAuth.Utilities
{
    public static class SupportExtensions
    {
        public static string ToDescriptionString<T>(this T source)
        {
            var fi = source!.GetType().GetField(source.ToString()!)!;

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            return attributes is { Length: > 0 } ? attributes[0].Description : source.ToString() ?? "";
        }
        public static string ToStringJson(this object? data)
        {
            return JsonConvert.SerializeObject(data, Formatting.None);
        }
        public static T? DeserializeJson<T>(this string? data)
        {
            return !string.IsNullOrWhiteSpace(data) ? JsonConvert.DeserializeObject<T>(data) : default;
        }
    }
}
