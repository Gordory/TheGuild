using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TheGuild.Api.Authorization;

public sealed class GuildAccessDeniedExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not GuildAccessDeniedException accessDenied)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Missing guild permission",
            Detail = accessDenied.Message,
        };

        problem.Extensions["missingPermission"] = accessDenied.MissingPermissionId;

        context.Result = new ObjectResult(problem) { StatusCode = StatusCodes.Status403Forbidden };
        context.ExceptionHandled = true;
    }
}
