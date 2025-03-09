using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.Repositories;
using BudgetPlanner.DataAccess.UnitOfWork;
using BudgetPlanner.Server.Services;
using BudgetPlanner.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace BudgetPlanner.Server.Endpoints;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("user");

        group.MapGet("/{uId}", GetUserById);
        group.MapPost("/", AddUser);
        group.MapPut("/{uId}", UpdateUser);
        group.MapDelete("/{uId}", DeleteUser);

        return app;

    }

    private static async Task<IResult> GetUserById(IUserRepository userRepo, string uId)
    {
        var customer = await userRepo.GetUserByIdAsync(uId);

        if (customer == null)
        {
            return Results.NotFound($"Customer with Id {uId} not found.");
        }

        return Results.Ok(customer);
    }

    private static async Task<IResult> AddUser(IUserRepository userRepo, IUnitOfWork unitOfWork, UserDTO newUser)
    {
        var userModel = new UserModel
        {
            Id = newUser.Id,
            Email = newUser.Email,
            Username = newUser.Username,
            Password = newUser.Password,
            BudgetsId = newUser.BudgetsId,
            TransactionIds = newUser.TransactionIds
        };

        var customerId = await userRepo.AddUserAsync(userModel);

        if (customerId.IsNullOrEmpty())
        {
            return Results.BadRequest("Failed to add user.");
        }

        newUser.Id = customerId;

        return Results.Created($"/user/{customerId}", userModel);
    }

    private static async Task<IResult> UpdateUser(IUserRepository userRepo, UserDTO updUser, string uId)
    {
        var userModel = new UserModel
        {
            Id = uId,
            Email = updUser.Email,
            Username = updUser.Username,
            Password = updUser.Password,
            BudgetsId = updUser.BudgetsId,
            TransactionIds = updUser.TransactionIds
        };

        var existingCustomer = await userRepo.GetUserByIdAsync(uId);
        if (existingCustomer == null)
        {
            return Results.NotFound($"Customer with Id {uId} not found.");
        }

        await userRepo.UpdateUserAsync(uId, userModel);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteUser(IUserRepository userRepo, string uId)
    {
        var existingCustomer = await userRepo.GetUserByIdAsync(uId);
        if (existingCustomer == null)
        {
            return Results.NotFound($"Customer with Id {uId} not found.");
        }

        await userRepo.DeleteUserAsync(uId);
        return Results.Ok();
    }
}