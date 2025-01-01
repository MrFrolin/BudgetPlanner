using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public class CustomerService(IHttpClientFactory factory): IRepository<CustomerDTO>
{

    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public Task<CustomerDTO> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<List<CustomerDTO>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> AddAsync(CustomerDTO item)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(CustomerDTO item, string id)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string id)
    {
        throw new NotImplementedException();
    }
}