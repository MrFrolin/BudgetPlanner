using BudgetPlanner.DataAccess;
using BudgetPlanner.DataAccess.Repositories;
using BudgetPlanner.DataAccess.UnitOfWork;
using BudgetPlanner.Server.Endpoints;
using BudgetPlanner.Server.Services;
using BudgetPlanner.Server.Services.Middleware;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

string? filepath = builder.Configuration["GoogleCloud:KeyFilePath"];
string projectId = builder.Configuration["GoogleCloud:ProjectId"];
builder.Services.AddSingleton(provider => new FirestoreDbContext(filepath, projectId));


builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(filepath)
}));


// Add authentication services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:ValidAudience"],
            ValidateLifetime = true
        };
    });

var firebaseAuthAPIKey = builder.Configuration["Firebase:APIKey"];
var authDomain = builder.Configuration["Firebase:AuthDomain"];

builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
{
    ApiKey = firebaseAuthAPIKey,
    AuthDomain = authDomain,
    Providers = [
        new EmailProvider()
    ]
}));

builder.Services.AddHttpClient("RefreshTokenAPI", client =>
{
    string providerURL = builder.Configuration["Provider:Google"];
    client.BaseAddress = new Uri(providerURL + firebaseAuthAPIKey);
});

builder.Services.AddMemoryCache();
builder.Services.AddAuthorization();

// Register AuthenticationCore and UnitOfWork
builder.Services.AddAuthenticationCore();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAccountManagement, FirebaseAuthenticationStateProvide>();
builder.Services.AddScoped<AuthenticationStateProvider, FirebaseAuthenticationStateProvide>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("budgetPlanner",
        builder => builder
            .WithOrigins("https://localhost:7236")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("budgetPlanner");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}


app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenValidationMiddleware>();


app.MapBudgetEndpoints();
app.MapUserEndpoints();
app.MapTransactionEndpoints();
app.MapAccountManagementEndpoints();

app.Run();




