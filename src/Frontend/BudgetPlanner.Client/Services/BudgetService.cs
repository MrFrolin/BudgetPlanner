using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public class BudgetService(IHttpClientFactory factory) : IRepository<BudgetDTO>
{
    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public async Task<BudgetDTO> GetByIdAsync(string docId, string uId)
    {

        var response = await _httpClient.GetAsync($"budget/{docId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }
        return await response.Content.ReadFromJsonAsync<BudgetDTO>();
    }

    public async Task<List<BudgetDTO>> GetAllAsync(string uId)
    {
        var response = await _httpClient.GetAsync($"budget/{uId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<List<BudgetDTO>>();
    }

    public async Task<string> AddAsync(BudgetDTO item, string uId)
    {
        var response = await _httpClient.PostAsJsonAsync($"/budget/{uId}", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("failed to post");
        }
        return await response.Content.ReadAsStringAsync();
    }

    public async Task UpdateAsync(BudgetDTO item, string docId, string uId)
    {
        var response = await _httpClient.PutAsJsonAsync($"/budget/{docId}", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("failed to post");
        }
    }

    public async Task RemoveAsync(string docId, string uId)
    {
        var response = await _httpClient.DeleteAsync($"/budget/{docId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("failed to delete");
        }
    }
}