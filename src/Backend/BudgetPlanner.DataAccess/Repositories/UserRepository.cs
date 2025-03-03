using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.Shared.Interfaces;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Repositories;

public interface IUserRepository : IRepository<UserModel>
{
}
public class UserRepository : Repository<UserModel>, IUserRepository
{
    public UserRepository(FirestoreDb context, string collection) : base(context, collection)
    {

    }
}