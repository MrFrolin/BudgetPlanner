namespace BudgetPlanner.Shared.DTOs;

public class UserInfoDTO
{
    public User[] Users { get; set; }
}

public class User
{
    public string LocalId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string CustomAttributes { get; set; }
}