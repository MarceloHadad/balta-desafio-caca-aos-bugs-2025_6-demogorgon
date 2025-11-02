namespace BugStore.Application.Responses.Products;

public class GetProductsResponse
{
    public List<GetByIdProductResponse> Products { get; set; } = [];
}