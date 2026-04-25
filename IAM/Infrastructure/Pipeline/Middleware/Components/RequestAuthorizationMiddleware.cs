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
        Console.WriteLine("Entering InvokeAsync");
        // skip authorization if endpoint is decorated with [AllowAnonymous] attribute
        var endpoint = context.Request.HttpContext.GetEndpoint();
        if (endpoint == null)
        {
            Console.WriteLine("No endpoint found, proceeding with authorization");
        }
        else
        {
            var allowAnonymous = endpoint.Metadata
                .Any(m => m.GetType() == typeof(AllowAnonymousAttribute));
            Console.WriteLine($"Allow Anonymous is {allowAnonymous}");
            if (allowAnonymous)
            {
                Console.WriteLine("Skipping authorization");
                await next(context);
                return;
            }
        }
        Console.WriteLine("Entering authorization");
        // get token from request header
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();


        // if token is null then throw exception
        //if (token == null) throw new Exception("Null or invalid token");

        // validate token
        var userId = await tokenService.ValidateToken(token);

        // if token is invalid then throw exception
        //if (userId == null) throw new Exception("Invalid token");

        // get user by id
       /// var getUserByIdQuery = new GetUserByIdQuery(userId.Value);

        // set user in HttpContext.Items["User"]

        //var user = await userQueryService.Handle(getUserByIdQuery);
        Console.WriteLine("Successful authorization. Updating Context...");
        context.Items["User"] = 1; // user;
        Console.WriteLine("Continuing with Middleware Pipeline");
        // call next middleware
        if (context.Request.Method == HttpMethods.Options)
        {
            Console.WriteLine("Skipping authorization for OPTIONS request");

        }
        await next(context);
        return;

    }
}