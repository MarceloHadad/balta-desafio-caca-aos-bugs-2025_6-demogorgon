using BugStore.Application.Interfaces;
using BugStore.Application.Requests.Orders;
using BugStore.Application.Responses.Orders;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/orders")
            .WithTags("Orders");

        group.MapGet("/{id}", async (Guid id, [FromServices] IHandler<GetByIdOrderRequest, GetByIdOrderResponse> handler) =>
        {
            var request = new GetByIdOrderRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async ([FromServices] IHandler<CreateOrderRequest, CreateOrderResponse> handler, [FromBody] CreateOrderRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/orders/{response.Id}", response);
        });
    }
}