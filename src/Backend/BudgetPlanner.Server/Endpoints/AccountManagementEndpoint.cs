using BudgetPlanner.DataAccess.CustomerAuth;
using BudgetPlanner.Server.Services;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BudgetPlanner.Server.Endpoints;

public static class AccountManagementEndpoint
{
    public static IEndpointRouteBuilder MapAccountManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("account");

        group.MapPost("/login", LoginAsync);
        group.MapPost("/register", RegisterUser);
        group.MapPost("/logout", LogoutAsync);
        group.MapGet("/checkToken", CheckAuthenticatedAsync);
        
        return app;
    }

    

    private static async Task<Results<Ok<UserDTO>, BadRequest<string>>> RegisterUser(IAccountManagement accountManagement, [FromBody] UserDTO registerUser)
    {
        var result = await accountManagement.RegisterAsync(registerUser.Email, registerUser.Password, registerUser.Password);

        if (result == null)
        {
            return TypedResults.BadRequest("Failed to register user.");
        }
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<CredentialDTO>, NotFound<string>>> LoginAsync(IAccountManagement accountManagement, [FromBody] UserDTO user)
    {
        var result = await accountManagement.LoginAsync(user);
        if (result == null)
        {
            return TypedResults.NotFound("Wrong username or password");
        }

        var credential = new CredentialDTO
        {
            IdToken = result.IdToken,
            RefereshToken = result.RefereshToken,
            Created = result.Created,
            ExpiresIn = result.ExpiresIn,
            ProviderType = result.ProviderType
        };

        return TypedResults.Ok(credential);
    }
    private static async Task<Ok> LogoutAsync(IAccountManagement accountManagement)
    {
        try
        {
            await accountManagement.LogoutAsync();
            return TypedResults.Ok();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<Ok<bool>> CheckAuthenticatedAsync(IAccountManagement accountManagement)
    {
        var result = await accountManagement.CheckAuthenticatedAsync();

        return TypedResults.Ok(result);
    }
}