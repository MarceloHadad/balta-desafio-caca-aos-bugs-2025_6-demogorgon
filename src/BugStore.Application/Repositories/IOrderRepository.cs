using BugStore.Domain.Entities;

namespace BugStore.Application.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllWithDetailsAsync();
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task AddAsync(Order order);
    Task Update(Order order);
    Task Delete(Order order);
}
