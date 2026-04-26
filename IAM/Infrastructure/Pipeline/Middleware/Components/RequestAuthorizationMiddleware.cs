using Frock_backend.IAM.Application.Internal.OutboundServices;
using Frock_backend.IAM.Domain.Model.Queries;
using Frock_backend.IAM.Domain.Services;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;

namespace Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Components;

public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        // OPTIONS requests pass through without auth
        if (context.Request.Method == HttpMethods.Options)
        {
            await next(context);
            return;
        }

        // skip authorization if endpoint is decorated with [AllowAnonymous] attribute
        var endpoint = context.Request.HttpContext.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.Any(m => m.GetType() == typeof(AllowAnonymousAttribute)) ?? false;
        if (allowAnonymous)
        {
            await next(context);
            return;
        }

        // get token from request header
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        // validate token and set user in context
        var userId = await tokenService.ValidateToken(token);

        if (userId != null)
        {
            var user = await userQueryService.Handle(new GetUserByIdQuery(userId.Value));
            context.Items["User"] = user;
        }

        await next(context);
    }
}