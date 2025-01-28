using Blazored.LocalStorage;
using BudgetPlanner.DataAccess;
using BudgetPlanner.DataAccess.CustomerAuth;
using BudgetPlanner.DataAccess.UnitOfWork;
using BudgetPlanner.Server.Endpoints;
using Firebase.Auth;
using Firebase.Auth.Providers;


var builder = WebApplication.CreateBuilder(args);

string? filepath = builder.Configuration["GoogleCloud:KeyFilePath"];
string projectId = builder.Configuration["GoogleCloud:ProjectId"];


builder.Services.AddSingleton(provider => new FirestoreDbContext(filepath, projectId));


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


// Register AuthenticationCore and UnitOfWork
builder.Services.AddAuthenticationCore();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapBudgetEndpoints();
app.MapUserEndpoints();
app.MapTransactionEndpoints();

app.Run();




