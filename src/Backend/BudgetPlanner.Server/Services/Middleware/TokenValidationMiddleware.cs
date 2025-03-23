using BudgetPlanner.Server.AuthModels;
using BudgetPlanner.Server.Services;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetPlanner.Server.Services.Middleware;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMemoryCache cache, IFirebaseAccountManagement tokenService)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        if (!cache.TryGetValue("credential", out Credential? credential) || string.IsNullOrEmpty(credential.IdToken) || !await tokenService.CheckAuthenticatedAsync())
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await _next(context);
    }
}
