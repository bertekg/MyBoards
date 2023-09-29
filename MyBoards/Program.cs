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

app.MapGet("data", async (MyBoardsContext db) =>
{
    var authrsCommentCountsQuery = db.Comments.GroupBy (c => c.AuthorId).Select(g => new {g.Key, Count = g.Count()});

    var authrsCommentCounts = await authrsCommentCountsQuery.ToListAsync();

    var topAuthor = authrsCommentCounts.First(a => a.Count == authrsCommentCounts.Max(acc => acc.Count));

    var userDetails = db.Users.First(u => u.Id == topAuthor.Key);

    return new { userDetails, commentCount = topAuthor.Count };
});

app.MapPost("update", async (MyBoardsContext db) =>
{
    var epic = await db.Epics.FirstAsync(epic => epic.Id == 1);

    //var onHoldState = await db.WorkItemStates.FirstAsync(a => a.Value == "On Hold");
    var doneState = await db.WorkItemStates.FirstAsync(a => a.Value == "Done");

    epic.Area = "Updated area";
    epic.Priority = 1;
    epic.StartDate = DateTime.UtcNow;
    //epic.StateId = onHoldState.Id;
    epic.State = doneState;

    await db.SaveChangesAsync();
    return epic;
});

app.Run();
