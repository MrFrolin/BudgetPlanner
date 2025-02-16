using BudgetPlanner.DataAccess.CustomerAuth.Models;
using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Firebase.Auth;
using FirebaseAdmin.Auth;
using Newtonsoft.Json;
using UserInfo = BudgetPlanner.DataAccess.CustomerAuth.Models.UserInfo;
using Google.Apis.Auth;
using User = BudgetPlanner.DataAccess.CustomerAuth.Models.User;

namespace BudgetPlanner.Server.Services;

public interface IAccountManagement
{
    public Task<UserDTO> RegisterAsync(string email, string password, string username);
    public Task<CredentialDTO> LoginAsync(UserDTO user);
    public Task LogoutAsync();
    public Task<bool> CheckAuthenticatedAsync();
}

public class FirebaseAuthenticationStateProvide : AuthenticationStateProvider
    , IAccountManagement
{

    private bool _authenticated = false;
    private readonly ClaimsPrincipal UnAuthenticated = new(new ClaimsIdentity());

    private readonly IHttpClientFactory _httpClient;
    private readonly FirebaseAuthClient _firebaseAuthClient;
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly Dictionary<string, string> keyMap = new()
    {
        {"IsAdmin", "Admin"},
        {"IsUser", "User"}
    };

    public FirebaseAuthenticationStateProvide(IHttpClientFactory httpClient, FirebaseAuthClient firebaseAuth)
    {
        _httpClient = httpClient;
        _firebaseAuthClient = firebaseAuth;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = UnAuthenticated;

        //var idToken = _cookieService.GetCookie("userAuth");

        var idToken = "";


        try
        {
           

            //var body = new StringContent($"{{\"idtoken\":\"{credential.IdToken}\"}}",
            //    Encoding.UTF8, "application/json");

            //Check the token with provider (google)
            var client = _httpClient.CreateClient("ProviderAPI");
            var userResponse = await client.GetAsync($"");

            userResponse.EnsureSuccessStatusCode();
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userInfo = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(userJson, jsonSerializerOptions);



            //ny implementation
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(idToken);
            string uid = decodedToken.Uid;

            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

            var tet = new User
            {
                LocalId = new Guid().ToString(),
                Email = firebaseUser.Email,
                DisplayName = firebaseUser.DisplayName,
                CustomAttributes = JsonConvert.SerializeObject(firebaseUser.CustomClaims)
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

                var id = new ClaimsIdentity(claims, nameof(FirebaseAuthenticationStateProvide));
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
        //string[] defaultDetail = ["An unknown error occurred."];

        //try
        //{
        //    var result = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);
        //    return new UserDTO
        //    {
        //        Email = email,
        //        Password = password,
        //        Username = username,
        //        Id = result.User.Uid
        //    };
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine(e);
        //    throw;
        //}

        


        return null;
    }

    public async Task<CredentialDTO> LoginAsync(UserDTO user)
    {
        //var result = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(user.Email, user.Password);
        //if (!string.IsNullOrWhiteSpace(result.User.Uid))
        //{
        //    var firebaseCredential = result.User.Credential;

        //    var credential = new Credential
        //    {
        //        IdToken = firebaseCredential.IdToken,
        //        RefereshToken = firebaseCredential.RefreshToken,
        //        Created = DateTime.Now,
        //        ExpiresIn = firebaseCredential.ExpiresIn,
        //        ProviderType = 3600
        //    };

        //    _cookieService.SetCookie("userAuth", credential.IdToken, DateTime.Now.AddSeconds(360000));
        //    _cookieService.SetCookie("refresh_token", firebaseCredential.RefreshToken, DateTime.Now.AddDays(30));

        //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        //    return credential;
        //}

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

                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return credential;
            
        }

        throw new Exception("Unable to login");

        return null;
    }

    public Task LogoutAsync()
    {
        //_cookieService.DeleteCookie("id_token");
        //_cookieService.DeleteCookie("refresh_token");

        //if (_firebaseAuthClient?.User != null)
        //{
        //    _firebaseAuthClient.SignOut();
        //}

        //NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _authenticated;
    }
}