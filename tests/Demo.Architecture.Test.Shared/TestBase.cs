using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Fixtures;
using Demo.Architecture.UseCases.Common.Interfaces;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.Shared;

public abstract class TestBase
{
    protected AppDbContext Context = default!;
    protected Mock<IMediator> MediatorMock = default!;
    protected IApplicationDbContext WriteContext => Context;
    protected IReadOnlyApplicationDbContext ReadContext => Context;

    private DatabaseFixture _fixture = default!;

    [SetUp]
    public void BaseSetup()
    {
        _fixture = new DatabaseFixture();
        Context = _fixture.Context;
        MediatorMock = _fixture.MediatorMock;
    }

    [TearDown]
    public void BaseTearDown()
    {
        _fixture.Dispose();
    }
}
