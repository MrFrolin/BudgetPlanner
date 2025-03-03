using Blazored.LocalStorage;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using BudgetPlanner.Shared.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Firebase.Auth;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace BudgetPlanner.Client.Services.Auth;

public interface IAccountManagement
{
    public Task<UserDTO?> RegisterAsync(string email, string password, string username);
    public Task<UserDTO?> LoginAsync(UserDTO user);
    public Task<string> LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}

public class AccountManagement : IAccountManagement
{

    private readonly IHttpClientFactory _httpClient;

    public AccountManagement(IHttpClientFactory factory)
    {
        _httpClient = factory;
    }

    public async Task<UserDTO?> RegisterAsync(string email, string password, string username)
    {
        var client = _httpClient.CreateClient("BudgetPlannerAPI");

        var response = await client.PostAsync("account/register", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }

        return response.Content.ReadFromJsonAsync<UserDTO>().Result;
    }

    public async Task<UserDTO?> LoginAsync(UserDTO user)
    {

        var client = _httpClient.CreateClient("BudgetPlannerAPI");

        var response = await client.PostAsync("account/login", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }

        return response.Content.ReadFromJsonAsync<UserDTO>().Result;
    }

    public async Task<string> LogoutAsync()
    {
        var client = _httpClient.CreateClient("BudgetPlannerAPI");

        var response = await client.PostAsync("account/logout", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to log out.");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        var client = _httpClient.CreateClient("BudgetPlannerAPI");

        var response = await client.PostAsync("account/checkToken", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Cannot retrieve data. Status code: {response.StatusCode}");
        }
        
        return response.Content.ReadFromJsonAsync<bool>().Result;
    }

}