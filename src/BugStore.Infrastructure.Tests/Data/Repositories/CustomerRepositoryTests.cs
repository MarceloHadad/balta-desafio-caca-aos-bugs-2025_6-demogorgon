using BugStore.Application.Repositories;
using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Data.Repositories;
using BugStore.Infrastructure.Tests.Data.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Tests.Data.Repositories;

public class CustomerRepositoryTests
{
    private static AppDbContext CreateInMemoryContext()
        => TestDbContextFactory.CreateInMemoryContext();

    [Fact]
    public async Task AddAsync_ShouldAddCustomerToContext()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "+1 555-1234",
            BirthDate = new DateTime(1985, 5, 15)
        };

        // Act
        await repository.AddAsync(customer);

        // Assert
        var entry = context.Entry(customer);
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Should().BeEquivalentTo(customer);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            Phone = "+1 555-5678",
            BirthDate = new DateTime(1990, 8, 20)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(customer.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(customer);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WhenCustomerExists_ReturnsCustomer()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Bob Johnson",
            Email = "bob.johnson@example.com",
            Phone = "+1 555-9012",
            BirthDate = new DateTime(1988, 3, 10)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByEmailAsync(customer.Email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(customer);
    }

    [Fact]
    public async Task GetByEmailAsync_WhenCustomerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WhenCustomersExist_ReturnsOrderedList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var customers = new[]
        {
            new Customer { Id = Guid.NewGuid(), Name = "Zoe Adams", Email = "zoe@example.com", Phone = "+1 555-0001", BirthDate = new DateTime(1995, 1, 1) },
            new Customer { Id = Guid.NewGuid(), Name = "Alice Brown", Email = "alice@example.com", Phone = "+1 555-0002", BirthDate = new DateTime(1992, 2, 2) },
            new Customer { Id = Guid.NewGuid(), Name = "Mike Carter", Email = "mike@example.com", Phone = "+1 555-0003", BirthDate = new DateTime(1987, 3, 3) }
        };
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alice Brown");
        result[1].Name.Should().Be("Mike Carter");
        result[2].Name.Should().Be("Zoe Adams");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerExists_UpdatesTrackedEntity()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Email = "original@example.com",
            Phone = "+1 555-0000",
            BirthDate = new DateTime(1980, 1, 1)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var updatedCustomer = new Customer
        {
            Id = customer.Id,
            Name = "Updated Name",
            Email = "updated@example.com",
            Phone = "+1 555-9999",
            BirthDate = new DateTime(1985, 12, 31)
        };

        // Act
        await repository.UpdateAsync(updatedCustomer);

        // Assert
        var entry = context.Entry(context.Customers.Local.First(c => c.Id == customer.Id));
        entry.State.Should().Be(EntityState.Modified);
        entry.Entity.Name.Should().Be("Updated Name");
        entry.Entity.Email.Should().Be("updated@example.com");
        entry.Entity.Phone.Should().Be("+1 555-9999");
        entry.Entity.BirthDate.Should().Be(new DateTime(1985, 12, 31));
    }

    [Fact]
    public async Task UpdateAsync_WhenCustomerDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var nonExistentCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "Ghost",
            Email = "ghost@example.com",
            Phone = "+1 555-0000",
            BirthDate = new DateTime(1980, 1, 1)
        };

        // Act
        var act = async () => await repository.UpdateAsync(nonExistentCustomer);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer not found");
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerExists_MarksAsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Email = "delete@example.com",
            Phone = "+1 555-0000",
            BirthDate = new DateTime(1980, 1, 1)
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(customer.Id);

        // Assert
        var entry = context.Entry(customer);
        entry.State.Should().Be(EntityState.Deleted);
    }

    [Fact]
    public async Task DeleteAsync_WhenCustomerDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new CustomerRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = async () => await repository.DeleteAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Customer not found");
    }
}