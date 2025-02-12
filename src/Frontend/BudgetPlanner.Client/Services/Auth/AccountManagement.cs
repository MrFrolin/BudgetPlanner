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
    public Task<UserDTO> RegisterAsync(string email, string password, string username);
    public Task<UserDTO> LoginAsync(UserDTO user);
    public Task LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}

public class AccountManagement : AuthenticationStateProvider, IAccountManagement
{

    private bool _authenticated = false;
    private readonly ClaimsPrincipal UnAuthenticated = new(new ClaimsIdentity());

    private readonly IHttpClientFactory _httpClient;
    private readonly FirebaseAuthClient _firebaseAuthClient;
    private readonly CookieStorageAccessor _cookieService;

    private readonly IRepository<UserDTO> _userService;

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly Dictionary<string, string> keyMap = new()
    {
        { "IsAdmin", "Admin" },
        { "IsUser", "User" }
    };

    public AccountManagement(IHttpClientFactory factory, FirebaseAuthClient firebaseAuthClient, CookieStorageAccessor cookieService, IRepository<UserDTO> userService)
    {
        _httpClient = factory;
        _firebaseAuthClient = firebaseAuthClient;
        _cookieService = cookieService;
        _userService = userService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = UnAuthenticated;

        try
        {
            var credential = await _cookieService.GetValueAsync("userAuth");

            //Skicka till backend och google provider för att kontrollera token
            var client = _httpClient.CreateClient("ProviderAPI");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credential.IdToken);
            var userResponse = await client.PostAsync("checkToken", null);

            userResponse.EnsureSuccessStatusCode();
            var userJson = await userResponse.Content.ReadAsStringAsync();

            var userInfo =
                System.Text.Json.JsonSerializer.Deserialize<UserInfoDTO>(userJson, jsonSerializerOptions);

            if (userInfo != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.Users.FirstOrDefault().Email),
                    new(ClaimTypes.Email, userInfo.Users.FirstOrDefault().Email),
                };

                Dictionary<string, bool> settings =
                    JsonConvert.DeserializeObject<Dictionary<string, bool>>(userInfo.Users.FirstOrDefault()
                        .CustomAttributes);

                var trueSettings = settings.Where(x => x.Value).Select(x => keyMap[x.Key]);

                foreach (var role in trueSettings)
                {
                    claims.Add(new(ClaimTypes.Role, role));
                }

                var id = new ClaimsIdentity(claims, nameof(AccountManagement));
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

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username))
        {
            throw new Exception("Unable to register user");
        }

        var result = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);
        if (!string.IsNullOrWhiteSpace(result.User.Uid))
        {
            var firebaseCredential = result.User.Credential;

            var credential = new CredentialDTO
            {
                IdToken = firebaseCredential.IdToken,
                RefereshToken = firebaseCredential.RefreshToken,
                Created = DateTime.Now,
                ExpiresIn = firebaseCredential.ExpiresIn,
                ProviderType = (int)firebaseCredential.ProviderType
            };

            var user = new UserDTO
            {
                Email = email,
                Password = password,
                Username = username,
                Id = result.User.Uid
            };

            //Eventuellt bytas mot att skicka till backend för att spara user
            var client = _httpClient.CreateClient("BudgetPlannerAPI");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credential.IdToken);

            var response = await client.PostAsJsonAsync($"user", user);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDTO>();
            }
        }
        throw new Exception("Unable to register user");
    }

    public async Task<UserDTO> LoginAsync(UserDTO user)
    {

        var result = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(user.Email, user.Password);
        if (!string.IsNullOrWhiteSpace(result.User.Uid))
        {
            var firebaseCredential = result.User.Credential;

            var credential = new CredentialDTO
            {
                IdToken = firebaseCredential.IdToken,
                RefereshToken = firebaseCredential.RefreshToken,
                Created = DateTime.Now,
                ExpiresIn = firebaseCredential.ExpiresIn,
                ProviderType = 3600
            };

            var client = _httpClient.CreateClient("BudgetPlannerAPI");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credential.IdToken);

            var userResponse = await client.GetAsync($"user/{result.User.Uid}");

            if (userResponse.IsSuccessStatusCode)
            {
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userDTO = System.Text.Json.JsonSerializer.Deserialize<UserDTO>(userJson, jsonSerializerOptions);

                await _cookieService.SetValueAsync<CredentialDTO>("userAuth", credential);

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                return userDTO;
            }
        }

        throw new Exception("Unable to login");
    }

    public async Task LogoutAsync()
    {
        var client = _httpClient.CreateClient("BudgetPlannerAPI");

        await client.PostAsync("account/logout", null);

        if (_firebaseAuthClient?.User != null)
        {
            _firebaseAuthClient.SignOut();
        }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        var isAuth = await GetAuthenticationStateAsync();
        if (isAuth != null)
        {
            return false;
        }
        return true;
    }

}