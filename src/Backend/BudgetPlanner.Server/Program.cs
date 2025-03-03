using BudgetPlanner.DataAccess;
using BudgetPlanner.DataAccess.UnitOfWork;
using BudgetPlanner.Server.Endpoints;
using BudgetPlanner.Server.Services;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
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
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = $"{projectId}",
            ValidateLifetime = true
        };
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
            .AllowCredentials());  // This is essential for sending cookies.
});



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


//// Middleware to validate the token gets called before every endpoint call
//app.Use(async (context, next) =>
//{
//    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

//    if (string.IsNullOrEmpty(token))
//    {
//        context.Response.StatusCode = 401;
//        await context.Response.WriteAsync("Missing or invalid token");
//        return;
//    }

//    FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
//        .VerifyIdTokenAsync(token);

//    if (decodedToken.Audience != projectId)
//    {
//        context.Response.StatusCode = 401;
//        await context.Response.WriteAsync("Invalid or expired token");
//        return;
//    }

//    await next.Invoke();
//});


app.UseCors("budgetPlanner");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();


app.MapBudgetEndpoints();
app.MapUserEndpoints();
app.MapTransactionEndpoints();
app.MapAccountManagementEndpoints();

app.Run();




