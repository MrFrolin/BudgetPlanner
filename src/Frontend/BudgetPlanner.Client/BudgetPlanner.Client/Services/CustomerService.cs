using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public class CustomerService(IHttpClientFactory factory): IRepository<CustomerDTO>
{

    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public async Task<CustomerDTO> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/customer/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Can find customer with id: {id}");
        }

        return await response.Content.ReadFromJsonAsync<CustomerDTO>();
    }

    public async Task<List<CustomerDTO>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("/budget");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to get items from the database");
        }

        return await response.Content.ReadFromJsonAsync<List<CustomerDTO>>();
    }

    public async Task<string> AddAsync(CustomerDTO item)
    {
        var response = await _httpClient.PostAsJsonAsync($"/customer", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to add customer");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task UpdateAsync(CustomerDTO item, string id)
    {
        var response = await _httpClient.PutAsJsonAsync($"/customer/{id}", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to update customer");
        }
    }

    public async Task RemoveAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/customer/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to delete customer");
        }
    }
}