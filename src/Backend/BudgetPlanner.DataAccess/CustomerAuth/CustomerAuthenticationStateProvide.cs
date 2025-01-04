using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using BudgetPlanner.DataAccess.CustomerAuth.Models;
using BudgetPlanner.DataAccess.Models;
using Firebase.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace BudgetPlanner.DataAccess.CustomerAuth;

public class CustomerAuthenticationStateProvide : AuthenticationStateProvider
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

    public CustomerAuthenticationStateProvide(FirebaseAuthClient firebaseAuthClient, ILocalStorageService localStorageService, HttpClient httpClient)
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

                var id = new ClaimsIdentity(claims, nameof(CustomerAuthenticationStateProvide));
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

    public Task<CustomerModel> RegisterAsync(CustomerModel customer)
    {
        throw new NotImplementedException();
    }

    public async Task<CustomerModel> LoginAsync(CustomerModel customer)
    {
        try
        {
            var result = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(customer.Email, customer.Password);
            if (!string.IsNullOrWhiteSpace(result.User.Uid))
            {
                await _localStorageService.SetItemAsync("userAuth", result.User.Credential);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return customer;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }

    public Task LogoutAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckAuthenticatedAsync()
    {
        throw new NotImplementedException();
    }
}