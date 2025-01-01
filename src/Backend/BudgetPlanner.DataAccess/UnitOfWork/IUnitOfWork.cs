using BudgetPlanner.DataAccess.Repositories.Budgets;
using BudgetPlanner.DataAccess.Repositories.Customers;
using BudgetPlanner.DataAccess.Repositories.Transactions;

namespace BudgetPlanner.DataAccess.UnitOfWork;

public interface IUnitOfWork
{
    IBudgetRepository Budgets { get; }
    ICustomerRepository Customers { get; }
    ITransactionRepository Transactions { get; }
}