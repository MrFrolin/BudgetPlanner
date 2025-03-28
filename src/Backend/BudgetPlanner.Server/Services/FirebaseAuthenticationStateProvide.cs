﻿using BudgetPlanner.Shared.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Firebase.Auth;
using FirebaseAdmin.Auth;
using Newtonsoft.Json;
using UserInfo = BudgetPlanner.DataAccess.CustomerAuth.Models.UserInfo;
using Microsoft.Extensions.Caching.Memory;
using User = BudgetPlanner.DataAccess.CustomerAuth.Models.User;

namespace BudgetPlanner.Server.Services;

public interface IAccountManagement
{
    Task<UserDTO> RegisterAsync(string email, string password, string username);
    Task<string> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> CheckAuthenticatedAsync();
    Task<AuthenticationState> GetAuthenticationStateAsync();
}

public class FirebaseAuthenticationStateProvide : AuthenticationStateProvider
    , IAccountManagement
{

    private bool _authenticated = false;
    private readonly ClaimsPrincipal _unAuthenticated = new(new ClaimsIdentity());

    private readonly IMemoryCache _cache;

    private readonly FirebaseAuthClient _firebaseAuthClient;
    private ITokenService _tokenService;

    private readonly Dictionary<string, string> keyMap = new()
    {
        {"IsAdmin", "Admin"},
        {"IsUser", "User"}
    };

    public FirebaseAuthenticationStateProvide(FirebaseAuthClient firebaseAuth, IMemoryCache cache, ITokenService tokenService)
    {
        _firebaseAuthClient = firebaseAuth;
        _cache = cache;
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;
        var user = _unAuthenticated;

        try
        {
            var idToken = _cache.Get("userAuth")?.ToString();
            if (string.IsNullOrEmpty(idToken))
            {
                _authenticated = false;
                return new AuthenticationState(_unAuthenticated);
            }

            var refreshToken = _cache.Get("refresh_token")?.ToString();
            if (_tokenService.TokenIsExpired(idToken) && !string.IsNullOrEmpty(refreshToken))
            {
                idToken = await _tokenService.RefreshIdTokenAsync(refreshToken);
                _cache.Set("userAuth", idToken, TimeSpan.FromSeconds(3600));
            }

            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(idToken);

            string uid = decodedToken.Uid;

            var firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

            var userInfo = new UserInfo
            {
                Users = new[]
                {
                    new User
                    {
                        LocalId = firebaseUser.Uid,
                        Email = firebaseUser.Email,
                        DisplayName = firebaseUser.DisplayName,
                        CustomAttributes = firebaseUser.CustomClaims.Count > 0
                            ? JsonConvert.SerializeObject(firebaseUser.CustomClaims)
                            : "{}"
                    }
                }
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

            var credential = new CredentialDTO
            {
                IdToken = firebaseCredential.IdToken,
                RefereshToken = firebaseCredential.RefreshToken,
                Created = DateTime.Now,
                ExpiresIn = firebaseCredential.ExpiresIn,
                ProviderType = 3600
            };

            _cache.Set("userAuth", credential.IdToken, DateTime.Now.AddSeconds(3600));
            _cache.Set("refresh_token", firebaseCredential.RefreshToken, DateTime.Now.AddDays(30));

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            return result.User.Uid;
        }
        return null;
    }

    public Task LogoutAsync()
    {
        _cache.Remove("userAuth");
        _cache.Remove("refresh_token");

        if (_firebaseAuthClient?.User != null)
        {
            _firebaseAuthClient.SignOut();
        }
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return Task.CompletedTask;
    }

    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _authenticated;
    }
}