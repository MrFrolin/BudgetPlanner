using Blazored.LocalStorage;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using BudgetPlanner.Shared.Interfaces;
using Newtonsoft.Json;

namespace BudgetPlanner.Client.Services.Auth;

public interface IAccountManagement
{
    public Task<UserDTO> RegisterAsync(string email, string password, string username);
    public Task<UserDTO> LoginAsync(UserDTO user);
    public Task LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}

public class CustomAuthenticationStateProvide : AuthenticationStateProvider, IAccountManagement
{

    private bool _authenticated = false;
    private readonly ClaimsPrincipal UnAuthenticated = new(new ClaimsIdentity());

    private readonly HttpClient _httpClient;
    private readonly CookieStorageAccessor _cookieService;

    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly Dictionary<string, string> keyMap = new()
    {
        { "IsAdmin", "Admin" },
        { "IsUser", "User" }
    };

    public CustomAuthenticationStateProvide(CookieStorageAccessor cookieService, HttpClient httpClient)
    {
        _cookieService = cookieService;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = UnAuthenticated;

        try
        {
            var credential = await _cookieService.GetValueAsync("userAuth");

            var body = new StringContent($"{{\"idtoken\":\"{credential.IdToken}\"}}",
                Encoding.UTF8, "application/json");

            //Skicka till backend och google provider för att kontrollera token
            var userResponse = await _httpClient.PostAsync("GETGOOGLETOKENPROVIDER", body);

            userResponse.EnsureSuccessStatusCode();
            var userJson = await userResponse.Content.ReadAsStringAsync();

            var userInfo =
                System.Text.Json.JsonSerializer.Deserialize<Models.UserInfo>(userJson, jsonSerializerOptions);

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
            //SKICKA TILL BACKEND
            //var result = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password,
            //    username);
            return new UserDTO
            {
                //Email = email,
                //Password = password,
                //Username = username,
                //Id = result.User.Uid
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<UserDTO> LoginAsync(UserDTO user)
    {
        //try
        //{
        //SKICKA TILL BACKEND
        //var result = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(user.Email, user.Password);
        //if (!string.IsNullOrWhiteSpace(result.User.Uid))
        //{
        //await _cookieService.SetValueAsync("userAuth", result.User.Credential);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return user;
        //}
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine(e);
        //    throw;
        //}

        return null;
    }

    public async Task LogoutAsync()
    {
        await _cookieService.DeleteValueAsync("userAuth");

        //if (_firebaseAuthClient?.User != null)
        //{
        //    _firebaseAuthClient.SignOut();
        //}
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _authenticated;
    }
}