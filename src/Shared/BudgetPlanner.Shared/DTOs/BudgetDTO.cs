namespace BudgetPlanner.Shared.DTOs;

public class BudgetDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<string> TransactionIds { get; set; } = new();
}