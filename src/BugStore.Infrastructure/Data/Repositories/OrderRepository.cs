using BugStore.Application.Repositories;
using BugStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Data.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    private readonly AppDbContext _context = context;

    public async Task<List<Order>> GetAllWithDetailsAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        return Task.CompletedTask;
    }

    public async Task Update(Order order)
    {
        var tracked = await _context.Orders.FindAsync(order.Id)
            ?? throw new KeyNotFoundException("Order not found");

        tracked.CustomerId = order.CustomerId;
        tracked.UpdatedAt = order.UpdatedAt;

        _context.Orders.Update(tracked);
    }

    public async Task Delete(Order order)
    {
        var entity = await _context.Orders.FindAsync(order.Id)
            ?? throw new KeyNotFoundException("Order not found");

        _context.Orders.Remove(entity);
    }
}
