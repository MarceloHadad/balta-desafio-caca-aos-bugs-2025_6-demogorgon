namespace BugStore.Application.Responses.Customers;

public class GetCustomersResponse
{
    public List<GetByIdCustomerResponse> Customers { get; set; } = [];
}