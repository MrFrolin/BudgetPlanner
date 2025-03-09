namespace BudgetPlanner.Server.Services.Middleware;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AllowAnonymousAttribute : Attribute
{
}