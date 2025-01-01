using BudgetPlanner.DataAccess.Models.Enums;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace BudgetPlanner.DataAccess.Models;

[FirestoreData]
public class TransactionModel
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public TransactionCategory Category { get; set; }

    [FirestoreProperty]
    public string Name { get; set; }
    
    [FirestoreProperty]
    public double Amount { get; set; }
}