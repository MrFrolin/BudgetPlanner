using Blazored.LocalStorage;
using BudgetPlanner.DataAccess;
using BudgetPlanner.DataAccess.CustomerAuth;
using BudgetPlanner.DataAccess.Repositories.Budgets;
using BudgetPlanner.DataAccess.Repositories.Customers;
using BudgetPlanner.DataAccess.Repositories.Transactions;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.UnitOfWork;

public interface IUnitOfWork
{
    IBudgetRepository Budgets { get; }
    IUserRepository Users { get; }
    ITransactionRepository Transactions { get; }
}


public class UnitOfWork : IUnitOfWork
{
    private readonly FirestoreDb _firebaseDb;
    private readonly ILocalStorageService _localStorage;

    public IBudgetRepository Budgets { get; set; }
    public IUserRepository Users { get; set; }
    public ITransactionRepository Transactions { get; set; }


    public UnitOfWork(FirestoreDbContext firestoreDbContext)
    {
        _firebaseDb = firestoreDbContext.GetFirestoreDb();

        Budgets = new BudgetRepository(_firebaseDb, "Budgets");
        Users = new UserRepository(_firebaseDb, "Users");
        Transactions = new TransactionRepository(_firebaseDb, "Transactions");
    }
}