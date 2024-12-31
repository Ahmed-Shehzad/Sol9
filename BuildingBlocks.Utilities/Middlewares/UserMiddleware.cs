using BuildingBlocks.Contracts.Services.Users;
using BuildingBlocks.Utilities.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Utilities.Middlewares;

public class UserMiddleware(IUserService userService) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("X-UserId", out var userId))
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new NotFoundException("Invalid or unauthorized user.");
            }

            if (Guid.TryParse(userId, out var userIdGuidValue))
            {
                if (!userIdGuidValue.Equals(Guid.Empty))
                {
                    userService.SetUserId(new Ulid(userIdGuidValue));
                }
            }
            else if (Ulid.TryParse(userId, out var userIdValue))
            {
                userService.SetUserId(userIdValue);
            }
        }

        await next(context);
    }
}