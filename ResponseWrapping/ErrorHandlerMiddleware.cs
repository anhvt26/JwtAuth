using JwtAuth.Utilities;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Lấy attribute từ endpoint
            var wrapAttributes = context.GetEndpoint()?.Metadata.GetOrderedMetadata<WrapResponseAttribute>() ?? [];
            var needWrap = wrapAttributes is { Count: > 0 } && wrapAttributes[^1].Wrap;

            if (!needWrap)
                throw; // Không wrap → để hệ thống trả lỗi mặc định

            context.Response.ContentType = "application/json";

            var response = ex.HandleException(_logger);

            context.Response.StatusCode =
                response.Error.Exception?.DefaultStatusCode()
                ?? StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
