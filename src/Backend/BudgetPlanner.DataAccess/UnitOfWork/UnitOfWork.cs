using BudgetPlanner.DataAccess;
using BudgetPlanner.DataAccess.Repositories.Budgets;
using BudgetPlanner.DataAccess.Repositories.Customers;
using BudgetPlanner.DataAccess.Repositories.Transactions;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;

namespace BudgetPlanner.DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly FirestoreDb _firebaseDb;

    public IBudgetRepository Budgets { get; set; }
    public ICustomerRepository Customers { get; set; }
    public ITransactionRepository Transactions { get; set; }

    public UnitOfWork(FirestoreDbContext firestoreDbContext)
    {
        _firebaseDb = firestoreDbContext.GetFirestoreDb();

        Budgets = new BudgetRepository(_firebaseDb, "Budgets");
        Customers = new CustomerRepository(_firebaseDb, "Customers");
        Transactions = new TransactionRepository(_firebaseDb, "Transactions");
    }
}