using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;

namespace BugStore.Application.Handlers.Customers;

public class GetCustomersHandler : IHandler<GetCustomersRequest, GetCustomersResponse>
{
    private readonly ICustomerRepository _repository;

    public GetCustomersHandler(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetCustomersResponse> HandleAsync(GetCustomersRequest request)
    {
        var customers = await _repository.GetAllAsync();

        var items = customers.Select(c => new GetByIdCustomerResponse
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            BirthDate = c.BirthDate
        }).ToList();

        return new GetCustomersResponse
        {
            Customers = items
        };
    }
}