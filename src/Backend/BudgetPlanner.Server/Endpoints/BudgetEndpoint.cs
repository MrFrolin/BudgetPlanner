using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.Repositories;
using BudgetPlanner.DataAccess.Repositories.Budgets;
using BudgetPlanner.DataAccess.UnitOfWork;
using Google.Type;
using DateTime = System.DateTime;

namespace BudgetPlanner.Server.Endpoints;

public static class BudgetEndpoint
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("budget");

        group.MapGet("/", GetAllBudgets);
        group.MapGet("/{id}", GetBudgetById);
        group.MapPost("/", AddBudget);
        group.MapPut("/{id}", UpdateBudget);
        group.MapDelete("/{id}", DeleteBudget);

        return app;
    }

    private static async Task<IResult> GetAllBudgets(IUnitOfWork unitOfWork)
    {
        var budgets = await unitOfWork.Budgets.GetAllAsync();

        return Results.Ok(budgets);
    }

    private static async Task<IResult> GetBudgetById(IUnitOfWork unitOfWork, string id)
    {
        var budget = await unitOfWork.Budgets.GetByIdAsync(id);

        if (budget == null)
        {
            return Results.NotFound($"Budget with Id {id} not found.");
        }

        return Results.Ok(budget);
    }

    private static async Task<IResult> AddBudget(IUnitOfWork unitOfWork, BudgetModel budget)
    {
        var budgetId = await unitOfWork.Budgets.AddAsync(budget);
        budget.Id = budgetId;

        if(budgetId == null)
        {
            return Results.BadRequest("Failed to add budget.");
        }

        return Results.Ok(budget);
    }

    private static async Task<IResult> UpdateBudget(IUnitOfWork unitOfWork, BudgetModel budget, string id)
    {
        var existingBudget = await unitOfWork.Budgets.GetByIdAsync(id);
        if (existingBudget == null)
        {
            return Results.NotFound($"Budget with Id {id} not found.");
        }

        budget.Id = id;
        await unitOfWork.Budgets.UpdateAsync(budget, id);

        return Results.Ok(budget);
    }

    private static async Task<IResult> DeleteBudget(IUnitOfWork unitOfWork, string id)
    {
        var budget = await unitOfWork.Budgets.GetByIdAsync(id);
        if (budget == null)
        {
            return Results.NotFound($"Budget with Id {id} not found.");
        }

        await unitOfWork.Budgets.RemoveAsync(id);

        return Results.Ok();
    }
}