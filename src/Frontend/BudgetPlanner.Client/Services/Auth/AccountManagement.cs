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
    Task<UserDTO?> RegisterAsync(string email, string password, string username);
    Task<string?> LoginAsync(string email, string password);
    Task<string> LogoutAsync();
    Task<bool> CheckAuthenticatedAsync();
    Task<AuthenticationState> GetAuthenticationStateAsync();
}

public class AccountManagement : IAccountManagement
{

    private readonly IHttpClientFactory _httpClientFactory;

    public AccountManagement(IHttpClientFactory factory)
    {
        _httpClientFactory = factory;
    }

    public async Task<UserDTO?> RegisterAsync(string email, string password, string username)
    {
        var client = _httpClientFactory.CreateClient("BudgetPlannerAPI");

        var regUser = new UserDTO
        {
            Email = email,
            Password = password,
            Username = username
        };

        var response = await client.PostAsJsonAsync("account/register", regUser);

        Console.WriteLine("RESPONSE:" + response);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Cannot retrieve data. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<UserDTO>();
    }

    public async Task<string?> LoginAsync(string email, string password)
    {

        var client = _httpClientFactory.CreateClient("BudgetPlannerAPI");

        var regUser = new UserDTO
        {
            Email = email,
            Password = password
        };

        var response = await client.PostAsJsonAsync("account/login", regUser);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<string>();
    }

    public async Task<string> LogoutAsync()
    {
        var client = _httpClientFactory.CreateClient("BudgetPlannerAPI");

        var response = await client.PostAsync("account/logout", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Failed to log out.");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        var client = _httpClientFactory.CreateClient("BudgetPlannerAPI");

        var response = await client.GetAsync("account/checkToken");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Cannot retrieve data. Status code: {response.StatusCode}");
        }

        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var client = _httpClientFactory.CreateClient("BudgetPlannerAPI");
        var response = await client.GetAsync("account/getState");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthenticationState>();
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

}