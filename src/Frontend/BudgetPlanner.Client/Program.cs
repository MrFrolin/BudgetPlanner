using BudgetPlanner.Client.Components;
using BudgetPlanner.Client.Services;
using BudgetPlanner.Shared.DTOs;
using BudgetPlanner.Shared.Interfaces;

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

            builder.Services.AddScoped<IRepository<BudgetDTO>, BudgetService>();

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
