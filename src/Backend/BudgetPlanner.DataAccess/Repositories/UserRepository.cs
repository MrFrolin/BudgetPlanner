using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;
using Google.Cloud.Firestore;
using Newtonsoft.Json;

namespace BudgetPlanner.DataAccess.Repositories;

public interface IUserRepository
{
    Task <UserModel> GetUserByIdAsync(string uId);
    Task <string> AddUserAsync(UserModel user);
    Task <UserModel> UpdateUserAsync(string uId, UserModel updUser);
    Task DeleteUserAsync(string uId);
}
public class UserRepository(FirestoreDbContext firestoreDbContext) : IUserRepository
{

    private readonly FirestoreDb _firebaseDb = firestoreDbContext.GetFirestoreDb();

    public async Task<UserModel> GetUserByIdAsync(string uId)
    {
        var docRef = await _firebaseDb.Collection("Users").Document(uId).GetSnapshotAsync();
        if (!docRef.Exists)
        {
            return null;
        }

        Dictionary<string, object> emp = docRef.ToDictionary();
        string json = JsonConvert.SerializeObject(emp);
        var userModel = JsonConvert.DeserializeObject<UserModel>(json);
        return userModel;
    }

    public async Task<string> AddUserAsync(UserModel item)
    {
        WriteBatch batch = _firebaseDb.StartBatch();

        var docRef = _firebaseDb.Collection("Users").Document(item.Id);

        batch.Create(docRef, item);
        await batch.CommitAsync();

        return docRef.Id;
    }

    public async Task<UserModel> UpdateUserAsync(string uId, UserModel updUser)
    {
        WriteBatch batch = _firebaseDb.StartBatch();

        var docRef = _firebaseDb.Collection("Users").Document(uId);
        batch.Set(docRef, updUser, SetOptions.MergeAll);
        await batch.CommitAsync();

        return await GetUserByIdAsync(uId);
    }

    public async Task DeleteUserAsync(string uId)
    {
        WriteBatch batch = _firebaseDb.StartBatch();

        var docRef = _firebaseDb.Collection("Users").Document(uId);
        batch.Delete(docRef);
        await batch.CommitAsync();
    }
}