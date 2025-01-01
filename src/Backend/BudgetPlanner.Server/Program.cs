using BudgetPlanner.DataAccess;
using BudgetPlanner.DataAccess.UnitOfWork;
using BudgetPlanner.Server.Endpoints;

var builder = WebApplication.CreateBuilder(args);

string? filepath = builder.Configuration["GoogleCloud:KeyFilePath"];
string projectId = builder.Configuration["GoogleCloud:ProjectId"];
builder.Services.AddSingleton(provider => new FirestoreDbContext(filepath, projectId));

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
app.MapCustomerEndpoints();
app.MapTransactionEndpoints();


app.Run();



