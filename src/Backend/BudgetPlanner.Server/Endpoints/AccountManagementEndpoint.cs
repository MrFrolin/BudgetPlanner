﻿using BudgetPlanner.Server.Services;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

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

    private static async Task<Results<Ok<UserDTO>, BadRequest<string>>> RegisterUser(IAccountManagement accountManagement, string email, string password, string username)
    {
        var userDTO = await accountManagement.RegisterAsync(email, password, username);

        if (userDTO == null)
        {
            return TypedResults.BadRequest("Failed to register user.");
        }
        return TypedResults.Ok(userDTO);
    }

    private static async Task<Results<Ok<CredentialDTO>, NotFound<string>>> LoginAsync(IAccountManagement accountManagement, string email, string password)
    {
        var credentialDTO = await accountManagement.LoginAsync(email, password);

        if (credentialDTO == null)
        {
            return TypedResults.NotFound("Wrong username or password");
        }

        return TypedResults.Ok(credentialDTO);
    }
    private static async Task<Ok<string>> LogoutAsync(IAccountManagement accountManagement)
    {
        try
        {
            accountManagement.LogoutAsync();
            return TypedResults.Ok("User logged out");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task<Ok<bool>> CheckAuthenticatedAsync(IAccountManagement accountManagement)
    {
        var isAuthenticated = await accountManagement.CheckAuthenticatedAsync();

        return TypedResults.Ok(isAuthenticated);
    }
}