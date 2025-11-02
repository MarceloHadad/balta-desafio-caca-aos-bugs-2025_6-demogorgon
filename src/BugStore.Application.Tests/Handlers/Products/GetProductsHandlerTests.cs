using BugStore.Application.Handlers.Products;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugStore.Application.Tests.Products;

public class GetProductsHandlerTests
{
    private readonly GetProductsHandler _handler;
    private readonly Mock<IProductRepository> _repo;

    public GetProductsHandlerTests()
    {
        _repo = new Mock<IProductRepository>();
        _handler = new GetProductsHandler(_repo.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProductsExist_ReturnsAllProducts()
    {
        // Arrange
        var request = new GetProductsRequest();
        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product 1",
                Description = "Description 1",
                Slug = "product-1",
                Price = 100.00m
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Product 2",
                Description = "Description 2",
                Slug = "product-2",
                Price = 200.00m
            }
        };

        _repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(2);
        response.Products[0].Id.Should().Be(products[0].Id);
        response.Products[0].Title.Should().Be(products[0].Title);
        response.Products[1].Id.Should().Be(products[1].Id);
        response.Products[1].Title.Should().Be(products[1].Title);

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        var request = new GetProductsRequest();

        _repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().BeEmpty();

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }
}
