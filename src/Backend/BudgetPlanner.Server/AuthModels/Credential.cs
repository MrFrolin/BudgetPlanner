namespace BudgetPlanner.Server.AuthModels;

public class Credential
{
    public string Uid { get; set; }
    public string IdToken { get; set; }
    public string RefereshToken { get; set; }
    public DateTime Created { get; set; }
    public int ExpiresIn { get; set; }
    public int ProviderType { get; set; }
}