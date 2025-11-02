using BugStore.Application.Repositories;
using BugStore.Application.UseCases.Orders.Search;
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

    public async Task<IReadOnlyList<Order>> SearchAsync(SearchOrdersRequest request)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(o => o.Id == request.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            var value = request.CustomerName.Trim().ToLower();
            query = query.Where(o => EF.Functions.Like(o.Customer.Name.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            var value = request.CustomerEmail.Trim().ToLower();
            query = query.Where(o => EF.Functions.Like(o.Customer.Email.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            var value = request.CustomerPhone.Trim().ToLower();
            query = query.Where(o => EF.Functions.Like(o.Customer.Phone.ToLower(), $"%{value}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.ProductTitle))
        {
            var value = request.ProductTitle.Trim().ToLower();
            query = query.Where(o => o.Lines.Any(l => EF.Functions.Like(l.Product.Title.ToLower(), $"%{value}%")));
        }

        if (!string.IsNullOrWhiteSpace(request.ProductDescription))
        {
            var value = request.ProductDescription.Trim().ToLower();
            query = query.Where(o => o.Lines.Any(l => EF.Functions.Like(l.Product.Description.ToLower(), $"%{value}%")));
        }

        if (!string.IsNullOrWhiteSpace(request.ProductSlug))
        {
            var value = request.ProductSlug.Trim().ToLower();
            query = query.Where(o => o.Lines.Any(l => EF.Functions.Like(l.Product.Slug.ToLower(), $"%{value}%")));
        }

        if (request.ProductPriceStart.HasValue)
        {
            query = query.Where(o => o.Lines.Any(l => l.Product.Price >= request.ProductPriceStart.Value));
        }

        if (request.ProductPriceEnd.HasValue)
        {
            query = query.Where(o => o.Lines.Any(l => l.Product.Price <= request.ProductPriceEnd.Value));
        }

        if (request.CreatedAtStart.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.CreatedAtStart.Value);
        }

        if (request.CreatedAtEnd.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.CreatedAtEnd.Value);
        }

        if (request.UpdatedAtStart.HasValue)
        {
            query = query.Where(o => o.UpdatedAt >= request.UpdatedAtStart.Value);
        }

        if (request.UpdatedAtEnd.HasValue)
        {
            query = query.Where(o => o.UpdatedAt <= request.UpdatedAtEnd.Value);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        return await query.ToListAsync();
    }
}
