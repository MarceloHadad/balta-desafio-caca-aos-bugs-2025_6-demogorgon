using BugStore.Application.Interfaces;
using BugStore.Application.Requests.Customers;
using BugStore.Application.Responses.Customers;
using Microsoft.AspNetCore.Mvc;

namespace BugStore.Api.Endpoints;

public static class CustomersEndpoints
{
    public static void MapCustomersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/customers")
            .WithTags("Customers");

        group.MapGet("/", async ([FromServices] IHandler<GetCustomersRequest, GetCustomersResponse> handler) =>
        {
            var request = new GetCustomersRequest();
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (Guid id, [FromServices] IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse> handler) =>
        {
            var request = new GetByIdCustomerRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async ([FromServices] IHandler<CreateCustomerRequest, CreateCustomerResponse> handler, [FromBody] CreateCustomerRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/customers/{response.Id}", response);
        });

        group.MapPut("/{id}", async (Guid id, [FromBody] UpdateCustomerRequest request, [FromServices] IHandler<UpdateCustomerRequest, UpdateCustomerResponse> handler) =>
        {
            request.Id = id;
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapDelete("/{id}", async (Guid id, [FromServices] IHandler<DeleteCustomerRequest, DeleteCustomerResponse> handler) =>
        {
            var request = new DeleteCustomerRequest { Id = id };
            var response = await handler.HandleAsync(request);
            return Results.NoContent();
        });
    }
}