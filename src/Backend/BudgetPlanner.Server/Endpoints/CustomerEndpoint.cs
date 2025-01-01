using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.UnitOfWork;

namespace BudgetPlanner.Server.Endpoints;

public static class CustomerEndpoint
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("customer");

        group.MapGet("/", GetAllCustomers);
        group.MapGet("/{id}", GetCustomerById);
        group.MapPost("/", AddCustomer);
        group.MapPut("/{id}", UpdateCustomer);
        group.MapDelete("/{id}", DeleteCustomer);


        return app;
    }

    private static async Task<IResult> GetAllCustomers(IUnitOfWork unitOfWork)
    {
        var customers = await unitOfWork.Customers.GetAllAsync();

        return Results.Ok(customers);
    }

    private static async Task<IResult> GetCustomerById(IUnitOfWork unitOfWork, string id)
    {
        var customer = await unitOfWork.Customers.GetByIdAsync(id);

        if (customer == null)
        {
            return Results.NotFound($"Customer with Id {id} not found.");
        }

        return Results.Ok(customer);
    }

    private static async Task<IResult> AddCustomer(IUnitOfWork unitOfWork, CustomerModel newCustomer)
    {
       var customerId = await unitOfWork.Customers.AddAsync(newCustomer);

        if (customerId == null)
        {
            return Results.BadRequest("Failed to add customer.");
        }

        newCustomer.Id = customerId;

        return Results.Created($"/customer/{customerId}", newCustomer);
    }

    private static async Task<IResult> UpdateCustomer(IUnitOfWork unitOfWork, CustomerModel updCustomer, string id)
    {
        var existingCustomer = await unitOfWork.Customers.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return Results.NotFound($"Customer with Id {id} not found.");
        }

        await unitOfWork.Customers.UpdateAsync(updCustomer, id);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteCustomer(IUnitOfWork unitOfWork, string id)
    {
        var existingCustomer = await unitOfWork.Customers.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            return Results.NotFound($"Customer with Id {id} not found.");
        }

        await unitOfWork.Customers.RemoveAsync(id);
        return Results.Ok();
    }
}