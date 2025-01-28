namespace BudgetPlanner.Shared.DTOs;

public class CredentialDTO
{
    public string IdToken { get; set; }
    public string RefereshToken { get; set; }
    public DateTime Created { get; set; }
    public int ExpiresIn { get; set; }
    public int ProviderType { get; set; }
}