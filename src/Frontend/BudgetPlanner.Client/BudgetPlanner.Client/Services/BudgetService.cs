using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public class BudgetService(IHttpClientFactory factory) : IRepository<BudgetDTO>
{
    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public async Task<BudgetDTO> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"budget/{id}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }
        return await response.Content.ReadFromJsonAsync<BudgetDTO>();
    }

    public async Task<List<BudgetDTO>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("budget");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<List<BudgetDTO>>();
    }

    public async Task<string> AddAsync(BudgetDTO item)
    {
        var response = await _httpClient.PostAsJsonAsync($"/budget", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("failed to post");
        }
        return await response.Content.ReadAsStringAsync();
    }

    public async Task UpdateAsync(BudgetDTO item, string id)
    {
        var response = await _httpClient.PutAsJsonAsync($"/budget/{id}", item );

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("failed to post");
        }
    }

    public async Task RemoveAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/budget/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("failed to delete");
        }
    }
}