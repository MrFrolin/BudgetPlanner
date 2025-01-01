using BudgetPlanner.DataAccess.Models;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Repositories.Transactions;

public class TransactionRepository : Repository<TransactionModel>, ITransactionRepository
{
    public TransactionRepository(FirestoreDb context, string collection) : base(context, collection)
    {
    }
}