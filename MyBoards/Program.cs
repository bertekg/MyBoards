using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using MyBoards.Dto;
using MyBoards.Entities;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


builder.Services.AddDbContext<MyBoardsContext>(
    option => option
    //.UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString"))
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
            City = "Kraków",
            Street = "D³uga"
        }
    };
    dbContext.Users.AddRange(user1, user2);
    dbContext.SaveChanges();
}

app.MapGet("pagination", (MyBoardsContext db) =>
{
    // user input
    var filter = "a";
    string sortBy = nameof(User.FullName); // "FullName" "Email" null
    bool sortByDescending = false;
    int pageNumber = 2;
    int pageSize = 10;
    //

    var query = db.Users
        .Where(u => filter == null ||
            (u.Email.ToLower().Contains(filter.ToLower()) ||
            u.FullName.ToLower().Contains(filter.ToLower())));

    var totalItems = query.Count();

    if (sortBy != null)
    {
        var columsSelector = new Dictionary<string, Expression<Func<User, object>>>
        {
            { nameof(User.Email), user => user.Email },
            { nameof(User.FullName), user => user.FullName },
        };

        var sortByExpression = columsSelector[sortBy];

        query = sortByDescending
            ? query.OrderByDescending(sortByExpression)
            : query.OrderBy(sortByExpression);
    }

    var result = query.Skip((pageNumber - 1) * pageSize)
        .Take(pageSize).ToList();

    var pageResult = new PageResult<User>(result, totalItems, pageSize, pageNumber);

    return pageResult;
});

app.MapGet("data", (MyBoardsContext db) =>
{
    var user = db.Users.Include(u => u.Address).First(u => u.Id == Guid.Parse("EBFBD70D-AC83-4D08-CBC6-08DA10AB0E61"));

    return new { user.FullName, Address = $"{user.Address.Street} {user.Address.City}" };
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
        City = "Kraków",
        Country = "Poland",
        Street = "D³uga"
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
