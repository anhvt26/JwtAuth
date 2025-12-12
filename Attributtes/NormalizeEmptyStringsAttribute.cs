using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections;
using System.Reflection;

namespace JwtAuth.Attributtes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NormalizeEmptyStringsAttribute : ActionFilterAttribute
{
    private readonly bool _enable;

    public NormalizeEmptyStringsAttribute(bool enable = true)
    {
        Order = 50;
        _enable = enable;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Lấy descriptor của action hiện tại
        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            base.OnActionExecuting(context);
            return;
        }

        // Tìm tất cả attribute NormalizeEmptyStringsAttribute gắn trên action
        var methodFilters = descriptor.MethodInfo
            .GetCustomAttributes(typeof(NormalizeEmptyStringsAttribute), true)
            .Cast<NormalizeEmptyStringsAttribute>()
            .ToList();

        // Nếu action có attribute khác với "this", thì bỏ qua thằng ở controller
        if (methodFilters.Count > 0 && !methodFilters.Contains(this))
            return;

        // Chọn cái attribute thực sự được áp dụng
        var effectiveFilter = methodFilters.FirstOrDefault() ?? this;

        if (!effectiveFilter._enable)
        {
            base.OnActionExecuting(context);
            return;
        }

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;
            NormalizeStrings(argument);
        }

        base.OnActionExecuting(context);
    }

    private void NormalizeStrings(object? obj)
    {
        if (obj == null) return;

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .Where(p => p.CanRead && p.CanWrite);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(obj);

            if (value is string str)
            {
                if (str.Trim() == "")
                {
                    prop.SetValue(obj, null);
                }
            }
            else if (value != null)
            {
                var propType = prop.PropertyType;

                if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
                {
                    var enumerable = value as IEnumerable;
                    if (enumerable != null)
                    {
                        foreach (var item in enumerable)
                        {
                            if (item != null && IsComplexType(item.GetType()))
                            {
                                NormalizeStrings(item);
                            }
                        }
                    }
                }
                else if (IsComplexType(propType))
                {
                    NormalizeStrings(value);
                }
            }
        }
    }

    private bool IsComplexType(Type type)
    {
        return type.IsClass
               && type != typeof(string)
               && !type.Namespace!.StartsWith("System");
    }
}
