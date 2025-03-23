namespace BudgetPlanner.Shared.DTOs;

public class UserInfoDto
{
    public FirebaseUserDto[] Users { get; set; }
}

public class FirebaseUserDto
{
    public string LocalId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string CustomAttributes { get; set; }
}