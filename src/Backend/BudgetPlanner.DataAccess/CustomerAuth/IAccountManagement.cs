using BudgetPlanner.DataAccess.Models;

namespace BudgetPlanner.DataAccess.CustomerAuth;

public interface IAccountManagement
{
    public Task<CustomerModel> RegisterAsync(string email, string password, string username);
    public Task<CustomerModel> LoginAsync(CustomerModel customer);
    public Task LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}