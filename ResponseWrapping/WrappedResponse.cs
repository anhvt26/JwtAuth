using JwtAuth.ExceptionHandling;

namespace JwtAuth.ResponseWrapping
{
    public class WrappedResponse<T>
    {
        public T? Data { get; set; }
        public AppError Error { get; set; } = new();
    }
}
