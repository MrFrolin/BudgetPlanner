using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public class BudgetService(IHttpClientFactory factory) : IRepository<BudgetDTO>
{
    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public Task<BudgetDTO> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<List<BudgetDTO>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> AddAsync(BudgetDTO item)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(BudgetDTO item, string id)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string id)
    {
        throw new NotImplementedException();
    }
}