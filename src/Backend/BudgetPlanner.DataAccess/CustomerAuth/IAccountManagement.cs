using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.Shared.DTOs;

namespace BudgetPlanner.DataAccess.CustomerAuth;

public interface IAccountManagement
{
    public Task<UserDTO> RegisterAsync(string email, string password, string username);
    public Task<UserModel> LoginAsync(UserModel user);
    public Task LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}