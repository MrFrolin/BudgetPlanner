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

    private static async Task<IResult> GetTransactionById(IUnitOfWork unitOfWork, string docId)
    {
        var transaction = await unitOfWork.Transactions.GetByIdAsync(docId);

        if (transaction == null)
        {
            return Results.NotFound($"Transaction with Id {docId} not found.");
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

    private static async Task<IResult> UpdateTransaction(IUnitOfWork unitOfWork, TransactionModel updTransaction, string docId)
    {
        var existingTransaction = await unitOfWork.Transactions.GetByIdAsync(docId);
        if (existingTransaction == null)
        {
            return Results.NotFound($"Transaction with Id {docId} not found.");
        }

        await unitOfWork.Transactions.UpdateAsync(updTransaction, docId);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteTransaction(IUnitOfWork unitOfWork, string docId)
    {
        var transactions = await unitOfWork.Transactions.GetByIdAsync(docId);

        if (transactions == null)
        {
            return Results.NotFound("No transactions found.");
        }

        await unitOfWork.Transactions.RemoveAsync(docId);
        return Results.Ok();
    }
}