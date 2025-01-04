using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Models;

[FirestoreData]
public class BudgetModel
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty] 
    public string CustomerId { get; set; }

    [FirestoreProperty]
    public int Year { get; set; }

    [FirestoreProperty]
    public int Month { get; set; }

    [FirestoreProperty]
    public List<string> TransactionIds { get; set; } = new();
}