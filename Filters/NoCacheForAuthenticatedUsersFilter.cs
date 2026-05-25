using Microsoft.AspNetCore.Mvc.Filters;

namespace VibraUrbana.Filters;

public class NoCacheForAuthenticatedUsersFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var response = context.HttpContext.Response;
        response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
    }
}
