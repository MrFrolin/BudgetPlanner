namespace BudgetPlanner.DataAccess.CustomerAuth.Models;

public class Credential
{
    public string IdToken { get; set; }
    public string RefereshToken { get; set; }
    public DateTime Created { get; set; }
    public int ExpiresIn { get; set; }
    public int ProviderType { get; set; }
}