using Demo.Architecture.Infrastructure.Data;
using Demo.Architecture.Test.Shared.Fixtures;
using NUnit.Framework;

namespace Demo.Architecture.Test.Shared;

public abstract class TestBase
{
    protected AppDbContext Context = default!;
    private DatabaseFixture _fixture = default!;

    [SetUp]
    public void BaseSetup()
    {
        _fixture = new DatabaseFixture();
        Context = _fixture.Context;
    }

    [TearDown]
    public void BaseTearDown()
    {
        _fixture.Dispose();
    }
}
