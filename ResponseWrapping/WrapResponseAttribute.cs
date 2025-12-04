using JwtAuth.ExceptionHandling;
using JwtAuth.ResponseWrapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class WrapResponseAttribute : ActionFilterAttribute
{
    public bool Wrap { get; init; }

    public WrapResponseAttribute(bool wrap = true)
    {
        Order = 100;
        Wrap = wrap;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var responseWrappedAttributes = context.Filters
            .OfType<WrapResponseAttribute>()
            .ToList();
        if (!(responseWrappedAttributes.Count > 0 && responseWrappedAttributes[^1].Wrap)) return;

        context.Result = new ObjectResult(new WrappedResponse<object>
        {
            Data = (context.Result as ObjectResult)?.Value,
            Error = new AppError()
        });
    }
}
