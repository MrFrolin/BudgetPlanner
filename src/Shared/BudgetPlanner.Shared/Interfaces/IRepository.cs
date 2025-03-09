namespace BudgetPlanner.Shared.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string docId, string uId);
    Task<List<T>> GetAllAsync(string uId);
    Task<string> AddAsync(T item, string uId);
    Task UpdateAsync(T item, string docId, string uId);
    Task RemoveAsync(string docId, string uId);
}