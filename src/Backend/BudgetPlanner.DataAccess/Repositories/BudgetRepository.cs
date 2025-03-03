using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.Shared.Interfaces;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Repositories;

public interface IBudgetRepository : IRepository<BudgetModel>
{
}
public class BudgetRepository : Repository<BudgetModel>, IBudgetRepository
{
    public BudgetRepository(FirestoreDb context, string collection) : base(context, collection)
    {
    }
}