using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Customers.Search;
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

    public async Task<IReadOnlyList<Customer>> SearchAsync(SearchCustomersRequest request)
    {
        var query = _context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var value = request.Name.Trim().ToLower();
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var value = request.Email.Trim().ToLower();
            query = query.Where(c => EF.Functions.Like(c.Email.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var value = request.Phone.Trim().ToLower();
            query = query.Where(c => EF.Functions.Like(c.Phone.ToLower(), $"%{value}%"));
        }

        query = query.OrderBy(c => c.Name);

        return await query.ToListAsync();
    }
}