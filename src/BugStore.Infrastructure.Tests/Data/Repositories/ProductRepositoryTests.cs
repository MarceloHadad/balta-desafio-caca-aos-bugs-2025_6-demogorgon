using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using BugStore.Infrastructure.Tests.Data.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Tests.Data.Repositories;

public class ProductRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task AddAsync_ShouldAddProductToContext()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Test Product",
            Description = "Test Description",
            Slug = "test-product",
            Price = 99.99m
        };

        // Act
        await repository.AddAsync(product);

        // Assert
        var entry = context.Entry(product);
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Existing Product",
            Description = "Some description",
            Slug = "existing-product",
            Price = 49.99m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Slugged Product",
            Description = "Product with slug",
            Slug = "unique-slug",
            Price = 29.99m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetBySlugAsync(product.Slug);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetBySlugAsync("nonexistent-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_WhenProductsExist_ReturnsMatchingProducts()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product1 = new Product { Id = Guid.NewGuid(), Title = "Product 1", Description = "Desc 1", Slug = "product-1", Price = 10m };
        var product2 = new Product { Id = Guid.NewGuid(), Title = "Product 2", Description = "Desc 2", Slug = "product-2", Price = 20m };
        var product3 = new Product { Id = Guid.NewGuid(), Title = "Product 3", Description = "Desc 3", Slug = "product-3", Price = 30m };
        context.Products.AddRange(product1, product2, product3);
        await context.SaveChangesAsync();

        var idsToRetrieve = new[] { product1.Id, product3.Id };

        // Act
        var result = await repository.GetByIdsAsync(idsToRetrieve);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == product1.Id);
        result.Should().Contain(p => p.Id == product3.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WhenNoProductsMatch_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var result = await repository.GetByIdsAsync(nonExistentIds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsOrderedByTitle()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var products = new[]
        {
            new Product { Id = Guid.NewGuid(), Title = "Zebra Product", Description = "Desc", Slug = "zebra", Price = 10m },
            new Product { Id = Guid.NewGuid(), Title = "Alpha Product", Description = "Desc", Slug = "alpha", Price = 20m },
            new Product { Id = Guid.NewGuid(), Title = "Beta Product", Description = "Desc", Slug = "beta", Price = 30m }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Alpha Product");
        result[1].Title.Should().Be("Beta Product");
        result[2].Title.Should().Be("Zebra Product");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesTrackedEntity()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Description = "Original Description",
            Slug = "original-slug",
            Price = 100m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var updatedProduct = new Product
        {
            Id = product.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            Slug = "updated-slug",
            Price = 200m
        };

        // Act
        await repository.UpdateAsync(updatedProduct);

        // Assert
        var entry = context.Entry(context.Products.Local.First(p => p.Id == product.Id));
        entry.State.Should().Be(EntityState.Modified);
        entry.Entity.Title.Should().Be("Updated Title");
        entry.Entity.Description.Should().Be("Updated Description");
        entry.Entity.Slug.Should().Be("updated-slug");
        entry.Entity.Price.Should().Be(200m);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentProduct = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Ghost Product",
            Description = "Does not exist",
            Slug = "ghost",
            Price = 0m
        };

        // Act
        var act = async () => await repository.UpdateAsync(nonExistentProduct);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_MarksAsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            Description = "Will be deleted",
            Slug = "to-delete",
            Price = 50m
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(product.Id);

        // Assert
        var entry = context.Entry(product);
        entry.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await repository.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product not found");
    }
}
