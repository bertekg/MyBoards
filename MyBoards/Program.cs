using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using MyBoards.Entities;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


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

app.MapGet("data", async (MyBoardsContext db) =>
{
    var user = await db.Users
    .Include(u => u.Comments)
    .ThenInclude(c => c.WorkItem)
    .Include(u => u.Address)
    .FirstAsync(u => u.Id == Guid.Parse("68366DBE-0809-490F-CC1D-08DA10AB0E61"));
    
    //var userComments = await db.Comments.Where(c => c.AuthorId == user.Id).ToListAsync();

    return user;
});

app.MapPost("update", async (MyBoardsContext db) =>
{
    var epic = await db.Epics.FirstAsync(epic => epic.Id == 1);

    var doneState = await db.WorkItemStates.FirstAsync(a => a.Value == "Done");

    epic.State = doneState;

    await db.SaveChangesAsync();
    return epic;
});

app.MapPost("create", async (MyBoardsContext db) =>
{
    var adress = new Address
    {
        Id = Guid.Parse("9a8f2278-77b8-48e3-8972-b5f501eeac40"),
        City = "Krak�w",
        Country = "Poland",
        Street = "D�uga"
    };

    var user = new User
    {
        Email = "user@test.com",
        FullName = "Test User",
        Address = adress,
    };

    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();

    return user;
});

app.MapDelete("delete", async (MyBoardsContext db) =>
{
    var user = await db.Users
    .Include(u => u.Comments)
    .FirstAsync(u => u.Id == Guid.Parse("4EBB526D-2196-41E1-CBDA-08DA10AB0E61"));

    db.Users.Remove(user);

    await db.SaveChangesAsync();
});

app.Run();
