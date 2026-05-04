using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.UseCases.Common.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Demo.Architecture.Test.Shared.Fixtures;

internal class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }
    public Mock<IMediator> MediatorMock { get; }
    public Mock<ICurrentUser> CurrentUserMock { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Filename=:memory:")
            .Options;

        MediatorMock = new Mock<IMediator>();

        CurrentUserMock = new Mock<ICurrentUser>();
        CurrentUserMock
            .Setup(x => x.GetUser())
            .Returns(new UserInfo(
                "test-user-id",
                "test@email.com",
                new[] { "Admin" }
            ));

        Context = new AppDbContext(options, MediatorMock.Object, CurrentUserMock.Object);
        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.CloseConnection();
    }
}
