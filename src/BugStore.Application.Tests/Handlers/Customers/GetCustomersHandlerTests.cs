using BugStore.Application.Handlers.Customers;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Customers;

public class GetCustomersHandlerTests
{
    private readonly GetCustomersHandler _handler;
    private readonly Mock<ICustomerRepository> _repo;

    public GetCustomersHandlerTests()
    {
        _repo = new Mock<ICustomerRepository>();
        _handler = new GetCustomersHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomersExist_ReturnsAllCustomers()
    {
        // Arrange
        var request = new GetCustomersRequest();
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Jane Doe",
                Email = "jane.doe@example.com",
                Phone = "+55 11 99999-0000",
                BirthDate = new DateTime(1990, 1, 1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "+55 11 88888-0000",
                BirthDate = new DateTime(1985, 5, 5)
            }
        };

        _repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(customers);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().HaveCount(2);
        response.Customers[0].Id.Should().Be(customers[0].Id);
        response.Customers[0].Name.Should().Be(customers[0].Name);
        response.Customers[1].Id.Should().Be(customers[1].Id);
        response.Customers[1].Name.Should().Be(customers[1].Name);

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        var request = new GetCustomersRequest();

        _repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Customer>());

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Customers.Should().BeEmpty();

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
