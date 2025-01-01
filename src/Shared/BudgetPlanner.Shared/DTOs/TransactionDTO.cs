namespace BudgetPlanner.Shared.DTOs;

public class TransactionDTO
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public double Amount { get; set; }
}