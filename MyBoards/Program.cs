using Microsoft.EntityFrameworkCore;
using MyBoards.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyBoardsContext>(
    option => option.UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString"))
    );

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<MyBoardsContext>();

var pendingMigration = dbContext.Database.GetPendingMigrations();
if (pendingMigration.Any())
{
    dbContext.Database.Migrate();
}

var users = dbContext.Users.ToList();
if (users.Any() == false)
{
    var user1 = new User
    {
        Email = "user1@test.com",
        FullName = "User One",
        Address = new Address
        {
            City = "Warszawa",
            Street = "Szeroka"
        }
    };
    var user2 = new User
    {
        Email = "user2@test.com",
        FullName = "User Two",
        Address = new Address
        {
            City = "Krak�w",
            Street = "D�uga"
        }
    };
    dbContext.Users.AddRange(user1, user2);
    dbContext.SaveChanges();
}

app.Run();
