using BudgetPlanner.DataAccess.Models;

namespace BudgetPlanner.DataAccess.CustomerAuth;

public interface IAccountManagement
{
    public Task<CustomerModel> RegisterAsync(CustomerModel customer);
    public Task<CustomerModel> LoginAsync(CustomerModel customer);
    public Task LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}