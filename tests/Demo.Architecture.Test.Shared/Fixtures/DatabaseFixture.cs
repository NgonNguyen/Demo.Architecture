using Demo.Architecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Demo.Architecture.Test.Shared.Fixtures;

internal class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        Context = new AppDbContext(options);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.CloseConnection();
    }
}
