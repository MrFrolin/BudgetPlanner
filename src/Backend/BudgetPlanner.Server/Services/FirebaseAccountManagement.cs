using BudgetPlanner.Server.AuthModels;
using BudgetPlanner.Shared.DTOs;
using Firebase.Auth;
using FirebaseAdmin.Auth;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using User = BudgetPlanner.Server.AuthModels.User;

namespace BudgetPlanner.Server.Services;

public interface IFirebaseAccountManagement
{
    Task<UserDTO> RegisterAsync(string email, string password, string username);
    Task<string> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> CheckAuthenticatedAsync();
    Task<User> GetFirebaseUser();
}

public class FirebaseAccountManagement : IFirebaseAccountManagement
{

    private readonly IMemoryCache _cache;
    private readonly FirebaseAuthClient _firebaseAuthClient;
    private ITokenService _tokenService;

    

    public FirebaseAccountManagement(FirebaseAuthClient firebaseAuth, IMemoryCache cache, ITokenService tokenService)
    {
        _firebaseAuthClient = firebaseAuth;
        _cache = cache;
        _tokenService = tokenService;
    }

    public async Task<User> GetFirebaseUser()
    {

        var credential = _cache.Get<Credential>("credential");
        if (string.IsNullOrEmpty(credential.IdToken))
        {
            return null;
        }

        var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(credential.Uid);

            var user = new User
            {
                LocalId = firebaseUser.Uid,
                Email = firebaseUser.Email,
                DisplayName = firebaseUser.DisplayName,
                CustomAttributes = firebaseUser.CustomClaims.Count > 0
                    ? JsonConvert.SerializeObject(firebaseUser.CustomClaims)
                    : "{}"
            };

            return user;
    }

    public async Task<UserDTO> RegisterAsync(string email, string password, string username)
    {
        try
        {
            var result = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password, username);
            return new UserDTO
            {
                Id = result.User.Uid,
                Email = email,
                Password = password,
                Username = username,
                BudgetsId = new List<string>(),
                TransactionIds = new List<string>()
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var result = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, password);
        if (!string.IsNullOrWhiteSpace(result.User.Uid))
        {
            var firebaseCredential = result.User.Credential;

            var credential = new Credential
            {
                Uid = result.User.Uid,
                IdToken = firebaseCredential.IdToken,
                RefereshToken = firebaseCredential.RefreshToken,
                Created = DateTime.Now,
                ExpiresIn = firebaseCredential.ExpiresIn,
                ProviderType = 3600
            };

            _cache.Set("credential", credential, DateTime.Now.AddSeconds(3600));
            

            return result.User.Uid;
        }
        return null;
    }

    public Task LogoutAsync()
    {
        _cache.Remove("credential");
        if (_firebaseAuthClient?.User != null)
        {
            _firebaseAuthClient.SignOut();
        }

        return Task.CompletedTask;
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        var credential = _cache.Get<Credential>("credential");
        if (credential == null)
        {
            return false;
        }

        if (_tokenService.TokenIsExpired(credential.IdToken) && !string.IsNullOrEmpty(credential.RefereshToken))
        {
            credential.IdToken = await _tokenService.RefreshIdTokenAsync(credential.RefereshToken);
            _cache.Set("credential", credential, TimeSpan.FromSeconds(3600));
        }

        FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
            .VerifyIdTokenAsync(credential.IdToken);

        if (decodedToken != null)
        {
            return true;
        }

        return false;
    }
}