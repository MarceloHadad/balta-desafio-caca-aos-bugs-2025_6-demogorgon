using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;

namespace BugStore.Application.Handlers.Products;

public class GetProductsHandler : IHandler<GetProductsRequest, GetProductsResponse>
{
    private readonly IProductRepository _products;

    public GetProductsHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<GetProductsResponse> HandleAsync(GetProductsRequest request)
    {
        var items = await _products.GetAllAsync();
        return new GetProductsResponse
        {
            Products = items.Select(p => new GetByIdProductResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Slug = p.Slug,
                Price = p.Price
            }).ToList()
        };
    }
}
