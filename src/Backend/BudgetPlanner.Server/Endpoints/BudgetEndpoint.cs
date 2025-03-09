using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.UnitOfWork;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using BudgetPlanner.Server.Services;

namespace BudgetPlanner.Server.Endpoints;

public static class BudgetEndpoint
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("budget").RequireAuthorization();

        group.MapGet("/", GetAllBudgets);
        group.MapGet("/{docId}", GetBudgetById);
        group.MapPost("/", AddBudget);
        group.MapPut("/{docId}", UpdateBudget);
        group.MapDelete("/{docId}", DeleteBudget);

        return app;
    }

    private static async Task<IResult> GetAllBudgets(IUnitOfWork unitOfWork, string uId)
    {
        var budgets = await unitOfWork.Budgets.GetAllAsync(uId);

        return Results.Ok(budgets);
    }

    private static async Task<IResult> GetBudgetById(IUnitOfWork unitOfWork, string docId, string uId)
    {
        var budget = await unitOfWork.Budgets.GetByIdAsync(docId, uId);

        if (budget == null)
        {
            return Results.NotFound($"Budget with Id {docId} not found.");
        }

        return Results.Ok(budget);
    }

    private static async Task<IResult> AddBudget(IUnitOfWork unitOfWork, BudgetModel budget, string uId)
    {
        var budgetId = await unitOfWork.Budgets.AddAsync(budget, uId);
        budget.Id = budgetId;

        if(budgetId == null)
        {
            return Results.BadRequest("Failed to add budget.");
        }

        return Results.Ok(budget);
    }

    private static async Task<IResult> UpdateBudget(IUnitOfWork unitOfWork, BudgetModel budget, string docId, string uId)
    {
        var existingBudget = await unitOfWork.Budgets.GetByIdAsync(docId, uId);
        if (existingBudget == null)
        {
            return Results.NotFound($"Budget with Id {docId} not found.");
        }

        budget.Id = docId;
        await unitOfWork.Budgets.UpdateAsync(budget, docId, uId);

        return Results.Ok(budget);
    }

    private static async Task<IResult> DeleteBudget(IUnitOfWork unitOfWork, string docId, string uId)
    {
        var budget = await unitOfWork.Budgets.GetByIdAsync(docId, uId);
        if (budget == null)
        {
            return Results.NotFound($"Budget with Id {docId} not found.");
        }

        await unitOfWork.Budgets.RemoveAsync(docId, uId);

        return Results.Ok();
    }
}