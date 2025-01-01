using Google.Api;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using System.Collections.Generic;
using BudgetPlanner.DataAccess.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Specialized;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly FirestoreDb _firebaseDb;
    private readonly string _collection;
    public Repository(FirestoreDb context, string collection)
    {
        _firebaseDb = context;
        _collection = collection;
    }

    public async Task<T> GetByIdAsync(string id)
    {
        var docRef = await _firebaseDb.Collection(_collection).Document(id).GetSnapshotAsync();
        if (!docRef.Exists)
        {
            return null;
        }

        Dictionary<string, object> emp = docRef.ToDictionary();
        string json = JsonConvert.SerializeObject(emp);
        T objectModel = JsonConvert.DeserializeObject<T>(json);
        return objectModel;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var querySnapshot = await _firebaseDb.Collection(_collection).GetSnapshotAsync();

        List<T> newObjectsLst = new List<T>();

        foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
        {
            if (documentSnapshot.Exists)
            {
                Dictionary<string, object> emp = documentSnapshot.ToDictionary();
                string json = JsonConvert.SerializeObject(emp);
                T objectModel = JsonConvert.DeserializeObject<T>(json);
                objectModel.GetType().GetProperty("Id").SetValue(objectModel, documentSnapshot.Id);
                newObjectsLst.Add(objectModel);
            }
        }
        return newObjectsLst;
    }

    public async Task<string> AddAsync(T item)
    {
        WriteBatch batch = _firebaseDb.StartBatch();

        var docRef = _firebaseDb.Collection(_collection).Document();
        batch.Create(docRef, item);
        await batch.CommitAsync();

        return docRef.Id;
    }

    public async Task UpdateAsync(T updItem, string id)
    {
        WriteBatch batch = _firebaseDb.StartBatch();

        var docRef = _firebaseDb.Collection(_collection).Document(id);
        batch.Set(docRef, updItem, SetOptions.MergeAll);
        await batch.CommitAsync();
    }

    public async Task RemoveAsync(string id)
    {
        WriteBatch batch = _firebaseDb.StartBatch();

        var docRef = _firebaseDb.Collection(_collection).Document(id);
        batch.Delete(docRef);
        await batch.CommitAsync();
    }
}