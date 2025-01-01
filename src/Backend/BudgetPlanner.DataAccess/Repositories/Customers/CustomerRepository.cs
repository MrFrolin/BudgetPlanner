using BudgetPlanner.DataAccess.Models;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Repositories.Customers;

public class CustomerRepository : Repository<CustomerModel>, ICustomerRepository
{
    public CustomerRepository(FirestoreDb context, string collection) : base(context, collection)
    {
    }
}