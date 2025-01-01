using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess.Models;

[FirestoreData]
public class CustomerModel
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Username { get; set; }

    [FirestoreProperty]
    public string Email { get; set; }

    [FirestoreProperty]
    public string Password { get; set; }

    [FirestoreProperty]
    public List<string> BudgetsId { get; set; } = new();

    [FirestoreProperty]
    public List<string> TransactionIds { get; set; } = new();
}