using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using BugStore.Infrastructure.Tests.Data.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Tests.Data.Repositories;

public class OrderRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task AddAsync_ShouldAddOrderToContext()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+1 555-0000",
            BirthDate = new DateTime(1990, 1, 1)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(order);

        // Assert
        var entry = context.Entry(order);
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenOrderExists_ReturnsOrderWithDetails()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1 555-1234",
            BirthDate = new DateTime(1985, 5, 15)
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = "Test Product",
            Description = "Description",
            Slug = "test-product",
            Price = 50m
        };
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        var orderLine = new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = 2,
            Total = 100m
        };

        context.Customers.Add(customer);
        context.Products.Add(product);
        context.Orders.Add(order);
        context.OrderLines.Add(orderLine);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdWithDetailsAsync(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Customer.Should().NotBeNull();
        result.Customer.Id.Should().Be(customer.Id);
        result.Lines.Should().HaveCount(1);
        result.Lines.First().Product.Should().NotBeNull();
        result.Lines.First().Product.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdWithDetailsAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_WhenOrdersExist_ReturnsAllOrdersWithDetails()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        var customer1 = new Customer { Id = Guid.NewGuid(), Name = "Customer 1", Email = "c1@example.com", Phone = "+1 555-0001", BirthDate = DateTime.UtcNow };
        var customer2 = new Customer { Id = Guid.NewGuid(), Name = "Customer 2", Email = "c2@example.com", Phone = "+1 555-0002", BirthDate = DateTime.UtcNow };
        var product = new Product { Id = Guid.NewGuid(), Title = "Product", Description = "Desc", Slug = "product", Price = 10m };

        var order1 = new Order { Id = Guid.NewGuid(), CustomerId = customer1.Id, CreatedAt = DateTime.UtcNow };
        var order2 = new Order { Id = Guid.NewGuid(), CustomerId = customer2.Id, CreatedAt = DateTime.UtcNow };

        context.Customers.AddRange(customer1, customer2);
        context.Products.Add(product);
        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllWithDetailsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == order1.Id);
        result.Should().Contain(o => o.Id == order2.Id);
        result.All(o => o.Customer != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_WhenNoOrders_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetAllWithDetailsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_ShouldMarkOrderAsModified()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@test.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var trackedOrder = context.Orders.Find(order.Id);

        // Act
        await repository.Update(trackedOrder!);

        // Assert
        var entry = context.Entry(trackedOrder!);
        entry.State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task Update_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var nonExistentOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var act = async () => await repository.Update(nonExistentOrder);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Order not found");
    }

    [Fact]
    public async Task Delete_ShouldMarkOrderAsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@test.com",
            Phone = "123456789",
            BirthDate = new DateTime(1990, 1, 1)
        };
        context.Customers.Add(customer);
        context.SaveChanges();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Act
        await repository.Delete(order);

        // Assert
        var entry = context.Entry(order);
        entry.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task Delete_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new OrderRepository(context);
        var nonExistentOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var act = async () => await repository.Delete(nonExistentOrder);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Order not found");
    }
}
