using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace BudgetPlanner.DataAccess;

public class FirestoreDbContext
{
    private FirestoreDb _firestoreDb;

    public FirestoreDbContext(string filepath, string projectId)
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filepath);
        _firestoreDb = FirestoreDb.Create(projectId);

    }

    public FirestoreDb GetFirestoreDb()
    {
        return _firestoreDb;
    }
   
}