using BugStore.Application.Interfaces;
using BugStore.Application.Repositories;
using BugStore.Application.Responses.Products;
using BugStore.Application.UseCases.Products.Search;

namespace BugStore.Application.Handlers.Products;

public class GetProductsHandler : IHandler<SearchProductsRequest, GetProductsResponse>
{
    private readonly IProductRepository _products;

    public GetProductsHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<GetProductsResponse> HandleAsync(SearchProductsRequest request)
    {
        var items = await _products.SearchAsync(request);

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
