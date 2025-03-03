using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetPlanner.Server.Endpoints;

public static class BudgetEndpoint
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("budget").RequireAuthorization();

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

    private static async Task<IResult> GetBudgetById(IUnitOfWork unitOfWork, string docId)
    {
        var budget = await unitOfWork.Budgets.GetByIdAsync(docId);

        if (budget == null)
        {
            return Results.NotFound($"Budget with Id {docId} not found.");
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

    private static async Task<IResult> UpdateBudget(IUnitOfWork unitOfWork, BudgetModel budget, string docId)
    {
        var existingBudget = await unitOfWork.Budgets.GetByIdAsync(docId);
        if (existingBudget == null)
        {
            return Results.NotFound($"Budget with Id {docId} not found.");
        }

        budget.Id = docId;
        await unitOfWork.Budgets.UpdateAsync(budget, docId);

        return Results.Ok(budget);
    }

    private static async Task<IResult> DeleteBudget(IUnitOfWork unitOfWork, string docId)
    {
        var budget = await unitOfWork.Budgets.GetByIdAsync(docId);
        if (budget == null)
        {
            return Results.NotFound($"Budget with Id {docId} not found.");
        }

        await unitOfWork.Budgets.RemoveAsync(docId);

        return Results.Ok();
    }
}