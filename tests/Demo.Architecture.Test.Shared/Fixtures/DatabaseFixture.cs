using Demo.Architecture.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Demo.Architecture.Test.Shared.Fixtures;

internal class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }
    public Mock<IMediator> MediatorMock { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        MediatorMock = new Mock<IMediator>();

        Context = new AppDbContext(options, MediatorMock.Object);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.CloseConnection();
    }
}
