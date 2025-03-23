using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Newtonsoft.Json;
using Firebase.Auth;

namespace BudgetPlanner.Client.Services.Auth;

public interface IFirebaseAuthStateProvider
{
    Task<UserDTO?> RegisterAsync(string email, string password, string username);
    Task<string?> LoginAsync(string email, string password);
    Task<string> LogoutAsync();
    Task<bool> CheckAuthenticatedAsync();
}

public class FirebaseAuthStateProvider : AuthenticationStateProvider, IFirebaseAuthStateProvider
{

    private readonly IHttpClientFactory _httpClientFactory;

    private bool _authenticated = false;
    private readonly ClaimsPrincipal _unAuthenticated = new(new ClaimsIdentity());

    private readonly Dictionary<string, string> keyMap = new()
    {
        {"IsAdmin", "Admin"},
        {"IsUser", "User"}
    };

    public FirebaseAuthStateProvider(IHttpClientFactory factory)
    {
        _httpClientFactory = factory;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = _unAuthenticated;

        if (!await CheckAuthenticatedAsync())
        {
            return new AuthenticationState(_unAuthenticated);
        }

        var client = _httpClientFactory.CreateClient("BudgetPlannerAPI");
        var response = await client.GetAsync($"account/userInfo");

        if (!response.IsSuccessStatusCode)
        {
            return new AuthenticationState(_unAuthenticated);
        }

        var firebaseUser = await response.Content.ReadFromJsonAsync<FirebaseUserDto>();

        var userInfo = new UserInfoDto
        {
            Users =
            [
                new FirebaseUserDto() {LocalId = firebaseUser.LocalId ,Email = firebaseUser.Email, DisplayName = firebaseUser.DisplayName, CustomAttributes = firebaseUser.CustomAttributes }
            ]
        };

        if (userInfo != null)
        {
            var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userInfo.Users.FirstOrDefault().DisplayName),
                        new(ClaimTypes.Email, userInfo.Users.FirstOrDefault().Email),
                    };

            Dictionary<string, bool> settings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(userInfo.Users.FirstOrDefault().CustomAttributes);

            var trueSettings = settings.Where(x => x.Value).Select(x => keyMap[x.Key]);

            foreach (var role in trueSettings)
            {
                claims.Add(new(ClaimTypes.Role, role));
            }

            var id = new ClaimsIdentity(claims, nameof(FirebaseAuthStateProvider));
            user = new ClaimsPrincipal(id);
            _authenticated = true;
        }



        return new AuthenticationState(user);
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

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

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

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

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

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

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


}