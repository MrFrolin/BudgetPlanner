using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public class UserService(IHttpClientFactory factory) : IRepository<UserDTO>
{

    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public async Task<UserDTO> GetByIdAsync(string id)
    {
        var response = await _httpClient.GetAsync($"/user/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Can find user with id: {id}");
        }

        return await response.Content.ReadFromJsonAsync<UserDTO>();
    }

    public async Task<List<UserDTO>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("/budget");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to get items from the database");
        }

        return await response.Content.ReadFromJsonAsync<List<UserDTO>>();
    }

    public async Task<string> AddAsync(UserDTO item)
    {
        var response = await _httpClient.PostAsJsonAsync($"/user", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to add user");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task UpdateAsync(UserDTO item, string id)
    {
        var response = await _httpClient.PutAsJsonAsync($"/user/{id}", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to update user");
        }
    }

    public async Task RemoveAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"/user/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to delete user");
        }
    }
}