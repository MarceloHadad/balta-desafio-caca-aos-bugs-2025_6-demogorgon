using BugStore.Application.Repositories;
using BugStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Customer> AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        return Task.FromResult(customer);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Customers.FindAsync(id);
        if (entity is null)
            throw new KeyNotFoundException("Customer not found");

        _context.Customers.Remove(entity);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task UpdateAsync(Customer customer)
    {
        var tracked = await _context.Customers.FindAsync(customer.Id)
            ?? throw new KeyNotFoundException("Customer not found");

        tracked.Name = customer.Name;
        tracked.Email = customer.Email;
        tracked.Phone = customer.Phone;
        tracked.BirthDate = customer.BirthDate;
    }
}