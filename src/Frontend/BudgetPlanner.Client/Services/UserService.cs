using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

namespace BudgetPlanner.Client.Services;

public interface IUserService
{
    Task<UserDTO> GetByIdAsync(string uId);
    Task<List<UserDTO>> GetAllAsync(string uId);
    Task<string> AddAsync(UserDTO item);
    Task UpdateAsync(UserDTO item, string uId);
    Task DeleteAsync(string uId);
}

public class UserService(IHttpClientFactory factory) : IUserService
{

    private readonly HttpClient _httpClient = factory.CreateClient("BudgetPlannerAPI");

    public async Task<UserDTO> GetByIdAsync(string docId)
    {
        var response = await _httpClient.GetAsync($"/user/{docId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Can find user with id: {docId}");
        }

        return await response.Content.ReadFromJsonAsync<UserDTO>();
    }

    public async Task<List<UserDTO>> GetAllAsync(string uId)
    {
        var response = await _httpClient.GetAsync($"/budget/{uId}");

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

    public async Task UpdateAsync(UserDTO item, string docId)
    {
        var response = await _httpClient.PutAsJsonAsync($"/user/{docId}", item);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to update user");
        }
    }

    public async Task DeleteAsync(string uId)
    {
        var response = await _httpClient.DeleteAsync($"/user/{uId}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Unable to delete user");
        }
    }
}