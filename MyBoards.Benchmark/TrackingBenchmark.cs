using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using MyBoards.Entities;

namespace MyBoards.Benchmark;

[MemoryDiagnoser]
public class TrackingBenchmark
{
    [Benchmark]
    public int WithTracking()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyBoardsContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyBoardsDb;Trusted_Connection=True;");
        var _dbContext = new MyBoardsContext(optionsBuilder.Options);

        var comments = _dbContext.Comments.ToList();

        return comments.Count;
    }

    [Benchmark]
    public int WithNoTracking()
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyBoardsContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyBoardsDb;Trusted_Connection=True;");
        var _dbContext = new MyBoardsContext(optionsBuilder.Options);

        var comments = _dbContext.Comments.AsNoTracking().ToList();

        return comments.Count;
    }
}
