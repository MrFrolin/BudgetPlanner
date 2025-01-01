namespace BudgetPlanner.Shared.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string id);
    Task<List<T>> GetAllAsync();
    Task<string> AddAsync(T item);
    Task UpdateAsync(T item, string id);
    Task RemoveAsync(string id);
}