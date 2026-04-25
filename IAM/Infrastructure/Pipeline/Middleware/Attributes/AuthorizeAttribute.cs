using Frock_backend.IAM.Application.Internal.QueryServices;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    /**
     * <summary>
     *     This method is called when authorization is required.
     *     It checks if the user is logged in by checking if HttpContext.User is set.
     *     If a user is not signed in then it returns 401-status code.
     * </summary>
     * <param name="context">The authorization filter context</param>
     */
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

        if (allowAnonymous)
        {
            Console.WriteLine(" Skipping authorization");
            return;
        }

        // verify if a user is signed in by checking if HttpContext.User is set
        var user = (int)context.HttpContext.Items["User"];

        // if a user is not signed in, then return 401-status code
        if (user == null) context.Result = new UnauthorizedResult();
    }
}