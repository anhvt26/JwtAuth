using JwtAuth.ExceptionHandling;
using JwtAuth.ResponseWrapping;
using JwtAuth.Security.Jwts;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Security.Claims;

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

        public static UserJwtTokenInfo? GetUserTokenInfo(this ClaimsPrincipal user)
        {
            var userInfo = user.FindFirst(ClaimTypes.UserData)?.Value;
            return userInfo?.DeserializeJson<UserJwtTokenInfo>();
        }

        public static WrappedResponse<object> HandleException(this Exception exception, ILogger logger)
        {
            WrappedResponse<object> response;

            switch (exception)
            {
                case ErrorException appException:
                    logger.LogError(appException, "Application error occurred: {Message}", appException.Message);
                    response = new WrappedResponse<object>
                    {
                        Error = new AppError(appException)
                    };
                    break;

                default:
                    logger.LogError(exception, "Unhandled exception occurred.");
                    response = new WrappedResponse<object>
                    {
                        Error = new AppError(new ErrorException(ErrorCode.OTHER))
                    };
                    break;
            }

            response.Error.Trace = exception.StackTrace ?? "";
            return response;
        }
    }
}
