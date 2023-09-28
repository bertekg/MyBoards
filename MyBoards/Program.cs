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
            City = "Kraków",
            Street = "D³uga"
        }
    };
    dbContext.Users.AddRange(user1, user2);
    dbContext.SaveChanges();
}

app.MapGet("tags", (MyBoardsContext db) =>
{
    var tags = db.Tags.ToList();
    return tags;
});
app.MapGet("first-epic-and-user-one", (MyBoardsContext db) =>
{
    var epic = db.Epics.First();
    var user = db.Users.First(u => u.FullName == "User One");
    return new { epic, user };
});
app.MapGet("work-items-to-do", (MyBoardsContext db) =>
{
    var toDoWorkItems = db.WorkItems.Where(w => w.StateId == 1).ToList();
    return new { toDoWorkItems.Count, toDoWorkItems };
});

app.MapGet("comments-after-2022-7-23", async (MyBoardsContext db) =>
{
    var newComments = await db.Comments.Where(c => c.CreatedDate > new DateTime(2022, 7, 23)).ToListAsync();
    return new { newComments.Count, newComments };
});

app.MapGet("top-5-newst-comments", async (MyBoardsContext db) =>
{
    var top5NewestComments = await db.Comments.OrderByDescending(c => c.CreatedDate).Take(5).ToListAsync();
    return top5NewestComments;
});

app.MapGet("states-count", async (MyBoardsContext db) =>
{
    var statesCount = await db.WorkItems.GroupBy(wi => wi.StateId).Select(g => new { stateId = g.Key, count = g.Count() }).ToListAsync();
    return statesCount;
});

app.Run();
