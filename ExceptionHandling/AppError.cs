using Microsoft.OpenApi.Extensions;
using System.ComponentModel;

namespace JwtAuth.ExceptionHandling
{
    public class AppError
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string? Trace { get; set; }

        public readonly ErrorException? Exception;

        public AppError()
        {
            Code = ErrorCode.SUCCESS.GetHashCode();
            Message = ErrorCode.SUCCESS.GetAttributeOfType<DescriptionAttribute>().Description;
        }

        public AppError(ErrorException exception)
        {
            Code = exception.ErrorCode.GetHashCode();
            Message = string.IsNullOrEmpty(exception.Message) ? exception.ErrorCode.GetAttributeOfType<DescriptionAttribute>().Description : exception.Message;
            Exception = exception;
        }

        public AppError(string message)
        {
            Code = ErrorCode.OTHER.GetHashCode();
            Message = message;
        }
    }
}
