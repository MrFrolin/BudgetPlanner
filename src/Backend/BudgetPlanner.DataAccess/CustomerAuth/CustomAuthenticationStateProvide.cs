using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using BudgetPlanner.DataAccess.CustomerAuth.Models;
using BudgetPlanner.DataAccess.Models;
using BudgetPlanner.DataAccess.Repositories.Customers;
using BudgetPlanner.DataAccess.UnitOfWork;
using BudgetPlanner.Shared.DTOs;
using Firebase.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace BudgetPlanner.DataAccess.CustomerAuth;

public class CustomAuthenticationStateProvide : AuthenticationStateProvider
    , IAccountManagement
{

    private bool _authenticated = false;
    private readonly ClaimsPrincipal UnAuthenticated = new(new ClaimsIdentity());
    private readonly FirebaseAuthClient _firebaseAuthClient;

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorageService;

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly Dictionary<string, string> keyMap = new()
    {
        {"IsAdmin", "Admin"},
        {"IsUser", "User"}
    };

    public CustomAuthenticationStateProvide(FirebaseAuthClient firebaseAuthClient, ILocalStorageService localStorageService, HttpClient httpClient)
    {
        _firebaseAuthClient = firebaseAuthClient;
        _localStorageService = localStorageService;
        _httpClient = httpClient;
    }
    
    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = UnAuthenticated;

        try
        {
            var localUserInfo = await _localStorageService.GetItemAsync<Credential>("userAuth");

            var body = new StringContent($"{{\"idtoken\":\"{localUserInfo.IdToken}\"}}",
                Encoding.UTF8, "application/json");
            
            var userResponse = await _httpClient.PostAsync("GETGOOGLETOKENPROVIDER", body);

            userResponse.EnsureSuccessStatusCode();
            var userJson = await userResponse.Content.ReadAsStringAsync();

            var userInfo = System.Text.Json.JsonSerializer.Deserialize<Models.UserInfo>(userJson, jsonSerializerOptions);

            if (userInfo != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Users.FirstOrDefault().Email),
                    new(ClaimTypes.Email, userInfo.Users.FirstOrDefault().Email),
                };

                Dictionary<string, bool> settings = JsonConvert.DeserializeObject<Dictionary<string, bool>>(userInfo.Users.FirstOrDefault().CustomAttributes);

                var trueSettings = settings.Where(x => x.Value).Select(x => keyMap[x.Key]);

                foreach (var role in trueSettings)
                {
                    claims.Add(new(ClaimTypes.Role, role));
                }

                var id = new ClaimsIdentity(claims, nameof(CustomAuthenticationStateProvide));
                user = new ClaimsPrincipal(id);
                _authenticated = true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return new AuthenticationState(user);
    }

    public async Task<UserDTO> RegisterAsync(string email, string password, string username)
    {
        string[] defaultDetail = ["An unknown error occurred."];

        try
        {
            var result = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password,
                username);
            return new UserDTO
            {
                Email = email,
                Password = password,
                Username = username,
                Id = result.User.Uid
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UserModel> LoginAsync(UserModel user)
    {
        try
        {
            var result = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(user.Email, user.Password);
            if (!string.IsNullOrWhiteSpace(result.User.Uid))
            {
                await _localStorageService.SetItemAsync("userAuth", result.User.Credential);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return user;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }

    public async Task LogoutAsync()
    {
        await _localStorageService.RemoveItemAsync("userAuth");

        if (_firebaseAuthClient?.User != null)
        {
            _firebaseAuthClient.SignOut();
        }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _authenticated;
    }
}