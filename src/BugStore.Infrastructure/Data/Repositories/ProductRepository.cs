using BugStore.Application.Repositories;
using BugStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Data.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        return await Task.FromResult(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found");
        _context.Products.Remove(entity);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Title)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        return await _context.Products
            .AsNoTracking()
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task UpdateAsync(Product product)
    {
        var tracked = await _context.Products.FindAsync(product.Id)
            ?? throw new KeyNotFoundException("Product not found");

        tracked.Title = product.Title;
        tracked.Description = product.Description;
        tracked.Slug = product.Slug;
        tracked.Price = product.Price;
    }
}
