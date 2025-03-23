using BudgetPlanner.Server.AuthModels;
using BudgetPlanner.Server.Services;
using BudgetPlanner.Server.Services.Middleware;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BudgetPlanner.Server.Endpoints;

public static class AccountManagementEndpoint
{
    public static IEndpointRouteBuilder MapAccountManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("account").WithMetadata(new AllowAnonymousAttribute());

        group.MapPost("/login", LoginAsync);
        group.MapPost("/register", RegisterUser);
        group.MapPost("/logout", LogoutAsync);
        group.MapGet("/checkToken", CheckAuthenticatedAsync);
        group.MapGet("/userInfo", GetFirebaseUser);

        return app;
    }

    private static async Task<Results<Ok<UserDTO>, BadRequest<string>>> RegisterUser(IFirebaseAccountManagement accountManagement, UserDTO regUser)
    {
        var userDTO = await accountManagement.RegisterAsync(regUser.Email, regUser.Password, regUser.Username);

        if (userDTO == null)
        {
            return TypedResults.BadRequest("Failed to register user.");
        }
        return TypedResults.Ok(userDTO);
    }

    private static async Task<Results<Ok<string>, NotFound<string>>> LoginAsync(IFirebaseAccountManagement accountManagement, UserDTO regUser)
    {
        var uId = await accountManagement.LoginAsync(regUser.Email, regUser.Password);

        if (uId == null)
        {
            return TypedResults.NotFound("User not found. Wrong username or password");
        }

        return TypedResults.Ok(uId);
    }
    private static async Task<Ok<string>> LogoutAsync(IFirebaseAccountManagement accountManagement)
    {
        try
        {
            await accountManagement.LogoutAsync();
            return TypedResults.Ok("User logged out");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<Ok<bool>> CheckAuthenticatedAsync(IFirebaseAccountManagement accountManagement)
    {
        var isAuthenticated = await accountManagement.CheckAuthenticatedAsync();

        return TypedResults.Ok(isAuthenticated);
    }

    private static async Task<Ok<User>> GetFirebaseUser(IFirebaseAccountManagement accountManagement)
    {
        try
        {
            var user = await accountManagement.GetFirebaseUser();
            return TypedResults.Ok(user);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}