using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.UnitOfWork;

namespace BudgetPlanner.Server.Endpoints;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("user").RequireAuthorization();

        group.MapGet("/", GetAllUsers);
        group.MapGet("/{id}", GetUserById);
        group.MapPost("/", AddUser);
        group.MapPut("/{id}", UpdateUser);
        group.MapDelete("/{id}", DeleteUser);


        return app;
    }

    private static async Task<IResult> GetAllUsers(IUnitOfWork unitOfWork)
    {
        var customers = await unitOfWork.Users.GetAllAsync();

        return Results.Ok(customers);
    }

    private static async Task<IResult> GetUserById(IUnitOfWork unitOfWork, string id)
    {

        var customer = await unitOfWork.Users.GetByIdAsync(id);

        if (customer == null)
        {
            return Results.NotFound($"Customer with Id {id} not found.");
        }

        return Results.Ok(customer);
    }

    private static async Task<IResult> AddUser(IUnitOfWork unitOfWork, UserModel newUser)
    {
       var customerId = await unitOfWork.Users.AddAsync(newUser);

        if (customerId == null)
        {
            return Results.BadRequest("Failed to add user.");
        }

        newUser.Id = customerId;

        return Results.Created($"/user/{customerId}", newUser);
    }

    private static async Task<IResult> UpdateUser(IUnitOfWork unitOfWork, UserModel updUser, string id)
    {
        var existingCustomer = await unitOfWork.Users.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return Results.NotFound($"Customer with Id {id} not found.");
        }

        await unitOfWork.Users.UpdateAsync(updUser, id);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteUser(IUnitOfWork unitOfWork, string id)
    {
        var existingCustomer = await unitOfWork.Users.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return Results.NotFound($"Customer with Id {id} not found.");
        }

        await unitOfWork.Users.RemoveAsync(id);
        return Results.Ok();
    }
}