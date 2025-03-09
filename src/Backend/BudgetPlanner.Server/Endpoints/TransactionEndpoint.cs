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

    private static async Task<IResult> GetAllTransactions(IUnitOfWork unitOfWork, string uId)
    {
        var transactions = await unitOfWork.Transactions.GetAllAsync(uId);

        if(transactions == null)
        {
            return Results.NotFound("No transactions found.");
        }
        return Results.Ok(transactions);
    }

    private static async Task<IResult> GetTransactionById(IUnitOfWork unitOfWork, string docId, string uId)
    {
        var transaction = await unitOfWork.Transactions.GetByIdAsync(docId, uId);

        if (transaction == null)
        {
            return Results.NotFound($"Transaction with Id {docId} not found.");
        }
        return Results.Ok(transaction);
    }

    private static async Task<IResult> AddTransaction(IUnitOfWork unitOfWork, TransactionModel transaction, string uId)
    {
        var transactionId = await unitOfWork.Transactions.AddAsync(transaction, uId);

        if (transactionId == null)
        {
            return Results.BadRequest("Failed to add transaction.");
        }

        transaction.Id = transactionId;

        return Results.Created($"/transaction/{transaction.Id}", transaction);
    }

    private static async Task<IResult> UpdateTransaction(IUnitOfWork unitOfWork, TransactionModel updTransaction, string docId, string uId)
    {
        var existingTransaction = await unitOfWork.Transactions.GetByIdAsync(docId, uId);
        if (existingTransaction == null)
        {
            return Results.NotFound($"Transaction with Id {docId} not found.");
        }

        await unitOfWork.Transactions.UpdateAsync(updTransaction, docId, uId);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteTransaction(IUnitOfWork unitOfWork, string docId, string uId)
    {
        var transactions = await unitOfWork.Transactions.GetByIdAsync(docId, uId);

        if (transactions == null)
        {
            return Results.NotFound("No transactions found.");
        }

        await unitOfWork.Transactions.RemoveAsync(docId, uId);
        return Results.Ok();
    }
}