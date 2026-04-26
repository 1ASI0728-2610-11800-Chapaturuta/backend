using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute(params Role[] roles) : Attribute, IAuthorizationFilter
{
    public AuthorizeAttribute() : this(Array.Empty<Role>()) { }

    /**
     * <summary>
     *     This method is called when authorization is required.
     *     It checks if the user is logged in by checking if HttpContext.Items["User"] is set.
     *     If a user is not signed in then it returns 401-status code.
     *     If roles are specified and the user does not have the required role, returns 403.
     * </summary>
     * <param name="context">The authorization filter context</param>
     */
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        var user = context.HttpContext.Items["User"] as User;
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (roles.Length > 0 && !roles.Contains(user.Role))
            context.Result = new ObjectResult(new { message = "Forbidden: insufficient role" }) { StatusCode = 403 };
    }
}