using BudgetPlanner.DataAccess.Models;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace BudgetPlanner.DataAccess.Repositories.Budgets;

public class BudgetRepository : Repository<BudgetModel>, IBudgetRepository
{
    private readonly FirestoreDb _firebaseDb;
    private readonly string _collection;

    public BudgetRepository(FirestoreDb context, string collection) : base(context, collection)
    {
        _firebaseDb = context;
        _collection = collection;
    }
}