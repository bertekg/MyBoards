using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Northwind.Entities;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddDbContext<NorthwindContext>(
    option => option
    .UseSqlServer(builder.Configuration.GetConnectionString("NorthwindConnectionString"))
    );

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPut("updateLinq2Db", async (NorthwindContext db) =>
{
    var employees = db.Employees
        .Where(e => e.HireDate > new DateTime(1992, 6, 1));

    await LinqToDB.LinqExtensions.UpdateAsync(employees.ToLinqToDB(), x => new Employee
    {
        Notes = "New employee"
    });
});

app.MapPut("update", async (NorthwindContext db) =>
{
    var users = await db.Employees
        .Where(e => e.HireDate > new DateTime(1992, 6, 1))
        .ToListAsync();

    foreach (var user in users)
    {
        user.Notes = "New employee";
    }

    db.SaveChanges();
});

app.MapGet("getOrderDetails", async (NorthwindContext db) =>
{
    //Order order = await GetOrder(10248, db);

    Order order = await GetOrder(10248, db, o => o.OrderDetails);

    return new { order.OrderId, Details = order.OrderDetails };
});

app.MapGet("getOrderWithShipper", async (NorthwindContext db) =>
{
    //Order order = await GetOrder(10248, db);

    Order order = await GetOrder(10248, db, o => o.ShipViaNavigation);

    return new { order.OrderId, order.ShipVia, Shipper = order.ShipViaNavigation };
});

app.MapGet("getOrderWithCustomer", async (NorthwindContext db) =>
{
    //Order order = await GetOrder(10248, db);

    Order order = await GetOrder(10248, db, o => o.Customer, o => o.Employee);

    return new { order.OrderId, order.Customer };
});

app.MapGet("data", async (NorthwindContext db) =>
{
    var sampleData = await db.Products.Take(100).ToListAsync();

    return new { sampleData.Count, sampleData };
});

app.Run();

async Task<Order> GetOrder(int orderId, NorthwindContext db, params Expression<Func<Order, object>>[] includes)
{
    //Order order = await db.Orders
    //    .Include(o => o.OrderDetails)
    //    .Include(o => o.ShipViaNavigation)
    //    .Include(o => o.Customer)
    //    .FirstAsync(o => o.OrderId == orderId);

    //return order;


    var baseQuery = db.Orders
        .AsQueryable();

    if (includes.Any())
    {
        foreach (var include in includes)
        {
            baseQuery = baseQuery.Include(include);
        }
    }

    var order = await baseQuery.FirstAsync(o => o.OrderId == orderId);

    return order;
}