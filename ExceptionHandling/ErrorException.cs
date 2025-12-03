using JwtAuth.Utilities;
using System.Net;

namespace JwtAuth.ExceptionHandling
{
    public class ErrorException : Exception
    {
        public ErrorCode ErrorCode { get; }

        public int StatusCode { get; }

        public ErrorException(ErrorCode errorCode) : base(errorCode.ToDescriptionString())
        {
            ErrorCode = errorCode;
            StatusCode = DefaultStatusCode();
        }

        public ErrorException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = DefaultStatusCode();
        }

        public int DefaultStatusCode()
        {
            var httpStatusCode = ErrorCode switch
            {
                ErrorCode.UNKNOWN => (int)HttpStatusCode.InternalServerError,
                ErrorCode.BAD_REQUEST => (int)HttpStatusCode.BadRequest,
                ErrorCode.UN_AUTHORIZED => (int)HttpStatusCode.Unauthorized,
                ErrorCode.FORBIDDEN => (int)HttpStatusCode.Forbidden,
                _ => 200
            };
            return httpStatusCode;
        }
    }
}
