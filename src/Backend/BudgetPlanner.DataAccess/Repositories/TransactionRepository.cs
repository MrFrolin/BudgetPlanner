using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.Shared.Interfaces;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Repositories;

public interface ITransactionRepository : IRepository<TransactionModel>
{
}
public class TransactionRepository : Repository<TransactionModel>, ITransactionRepository
{
    public TransactionRepository(FirestoreDb context, string collection) : base(context, collection)
    {
    }
}