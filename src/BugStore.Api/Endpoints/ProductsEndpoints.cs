using BugStore.Application.Interfaces;
using BugStore.Application.Requests.Products;
using BugStore.Application.Responses.Products;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/products")
            .WithTags("Products");

        group.MapGet("/", async ([FromServices] IHandler<GetProductsRequest, GetProductsResponse> handler) =>
        {
            var request = new GetProductsRequest();
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (Guid id, [FromServices] IHandler<GetByIdProductRequest, GetByIdProductResponse> handler) =>
        {
            var request = new GetByIdProductRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async ([FromServices] IHandler<CreateProductRequest, CreateProductResponse> handler, [FromBody] CreateProductRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/products/{response.Id}", response);
        });

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateProductRequest request, [FromServices] IHandler<UpdateProductRequest, UpdateProductResponse> handler) =>
        {
            request.Id = id;
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapDelete("/{id}", async (Guid id, [FromServices] IHandler<DeleteProductRequest, DeleteProductResponse> handler) =>
        {
            var request = new DeleteProductRequest { Id = id };
            var response = await handler.HandleAsync(request);
            return Results.NoContent();
        });
    }
}