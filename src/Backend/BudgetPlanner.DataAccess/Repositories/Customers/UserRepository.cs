using BudgetPlanner.DataAccess.CustomerAuth.Models;
using BudgetPlanner.DataAccess.Models;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Repositories.Customers;

public class UserRepository : Repository<UserModel>, IUserRepository
{
    public UserRepository(FirestoreDb context, string collection) : base(context, collection)
    {

    }
}