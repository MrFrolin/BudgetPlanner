using Blazored.LocalStorage;
using BudgetPlanner.Client.Components;
using BudgetPlanner.Client.Services;
using BudgetPlanner.Client.Services.Auth;
using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Microsoft.AspNetCore.Components.Authorization;

namespace BudgetPlanner.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpClient("BudgetPlannerAPI", client =>
            {
                string localhostUrl = builder.Configuration["BaseAddress:localhost"];
                client.BaseAddress = new Uri(localhostUrl);
            });

            var firebaseAuthAPIKey = builder.Configuration["FirebaseAuthConfig:APIKey"];
            var authDomain = builder.Configuration["FirebaseAuthConfig:AuthDomain"];

            builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
            {
                ApiKey = firebaseAuthAPIKey,
                AuthDomain = authDomain,
                Providers = [
                    new EmailProvider()
                ]
            }));

            builder.Services.AddScoped<CookieStorageAccessor>();

            builder.Services.AddScoped<IAccountManagement, AccountManagement>();
            builder.Services.AddScoped<AuthenticationStateProvider, AccountManagement>();

            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddSingleton<IRepository<BudgetDTO>, BudgetService>();
            builder.Services.AddSingleton<IRepository<UserDTO>, UserService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
