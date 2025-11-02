namespace BugStore.Application.Responses.Orders;

public class GetOrdersResponse
{
    public List<GetByIdOrderResponse> Orders { get; set; } = [];
}
