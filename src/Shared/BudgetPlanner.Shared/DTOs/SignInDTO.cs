using System.ComponentModel.DataAnnotations;

namespace BudgetPlanner.Shared.DTOs;

public class SignInDTO
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}