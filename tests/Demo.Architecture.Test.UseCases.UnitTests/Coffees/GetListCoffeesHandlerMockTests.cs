using Demo.Architecture.Shared.Models;
using Demo.Architecture.UseCases.ExternalServices;
using Demo.Architecture.UseCases.Features.Coffees.Queries.GetList;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Demo.Architecture.Test.UseCases.UnitTests.Coffees;

public class GetListCoffeesHandlerMockTests
{
    private Mock<ICoffeeApiClient> _clientMock = default!;
    private GetListCoffeesHandler _handler = default!;
    private Mock<ILogger<GetListCoffeesHandler>> _loggerMock = default!;
    private List<CoffeeResponse> _coffees = default!;

    [SetUp]
    public void Setup()
    {
        _coffees = new List<CoffeeResponse>
        {
            new CoffeeResponse
            {
                Id = 1,
                Title = "Black Coffee",
                Description = "A hot beverage made from roasted coffee beans.",
                Image = "https://example.com/images/black-coffee.jpg"
            },
            new CoffeeResponse
            {
                Id = 2,
                Title = "Espresso",
                Description = "A concentrated form of coffee served in small, strong shots.",
                Image = "https://example.com/images/espresso.jpg"
            },
            new CoffeeResponse
            {
                Id = 3,
                Title = "Latte",
                Description = "A coffee drink made with espresso and steamed milk.",
                Image = "https://example.com/images/latte.jpg"
            }
        };

        _clientMock = new Mock<ICoffeeApiClient>();
        _clientMock
            .Setup(x => x.GetHotCoffeeAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_coffees);
        _loggerMock = new Mock<ILogger<GetListCoffeesHandler>>();
        _handler = new GetListCoffeesHandler(_loggerMock.Object, _clientMock.Object);
    }

    [Test]
    public async Task Should_Return_Paged_Coffee_List()
    {
        // Arrange
        var query = new GetListCoffeesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TotalCount.Should().Be(3);
        result.Value.Items.Should().HaveCount(3);
        result.Value.Items[0].Title.Should().Be("Black Coffee");

        _clientMock.Verify(
            x => x.GetHotCoffeeAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
