namespace BudgetPlanner.Shared.DTOs;

public class UserDTO
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<string> BudgetsId { get; set; } = new();
    public List<string> TransactionIds { get; set; } = new();
}