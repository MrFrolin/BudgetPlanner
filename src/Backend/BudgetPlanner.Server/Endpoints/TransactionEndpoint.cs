using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.UnitOfWork;

namespace BudgetPlanner.Server.Endpoints;

public static class TransactionEndpoint
{

    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("transaction").RequireAuthorization();

        group.MapGet("/", GetAllTransactions);
        group.MapGet("/{id}", GetTransactionById);
        group.MapPost("/", AddTransaction);
        group.MapPut("/{id}", UpdateTransaction);
        group.MapDelete("/{id}", DeleteTransaction);

        return group;
    }

    private static async Task<IResult> GetAllTransactions(IUnitOfWork unitOfWork)
    {
        var transactions = await unitOfWork.Transactions.GetAllAsync();

        if(transactions == null)
        {
            return Results.NotFound("No transactions found.");
        }
        return Results.Ok(transactions);
    }

    private static async Task<IResult> GetTransactionById(IUnitOfWork unitOfWork, string id)
    {
        var transaction = await unitOfWork.Transactions.GetByIdAsync(id);

        if (transaction == null)
        {
            return Results.NotFound($"Transaction with Id {id} not found.");
        }
        return Results.Ok(transaction);
    }

    private static async Task<IResult> AddTransaction(IUnitOfWork unitOfWork, TransactionModel transaction)
    {
        var transactionId = await unitOfWork.Transactions.AddAsync(transaction);

        if (transactionId == null)
        {
            return Results.BadRequest("Failed to add transaction.");
        }

        transaction.Id = transactionId;

        return Results.Created($"/transaction/{transaction.Id}", transaction);
    }

    private static async Task<IResult> UpdateTransaction(IUnitOfWork unitOfWork, TransactionModel updTransaction, string id)
    {
        var existingTransaction = await unitOfWork.Transactions.GetByIdAsync(id);
        if (existingTransaction == null)
        {
            return Results.NotFound($"Transaction with Id {id} not found.");
        }

        await unitOfWork.Transactions.UpdateAsync(updTransaction, id);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteTransaction(IUnitOfWork unitOfWork, string id)
    {
        var transactions = await unitOfWork.Transactions.GetByIdAsync(id);

        if (transactions == null)
        {
            return Results.NotFound("No transactions found.");
        }

        await unitOfWork.Transactions.RemoveAsync(id);
        return Results.Ok();
    }
}