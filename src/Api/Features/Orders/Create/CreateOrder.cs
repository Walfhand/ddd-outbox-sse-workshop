using Api.Domain.UberEatsOrders.AggregateRoots;
using Api.Domain.UberEatsOrders.AggregateRoots.ValueObjects;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Orders.Create;

public sealed class CreateOrder
{
    public sealed record Request
    {
        [FromBody] public RequestBody Body { get; set; } = null!;

        public sealed record RequestBody
        {
            public Guid CustomerId { get; set; }
            public Guid RestaurantId { get; set; }
            public List<OrderItemDto> Items { get; set; } = [];
        }
    }

    public sealed record OrderItemDto(string Name, int Quantity);
    public sealed record Response(Guid OrderId, string Status, Guid CustomerId, Guid RestaurantId);

    public sealed class Endpoint() : PostMinimalEndpoint<Request, Response>("workshop/ubereats/orders");

    public sealed class Handler(
        IAppDbContextFactory dbContextFactory,
        IHttpContextAccessor contextAccessor)
        : Engine.Wolverine.Handler(dbContextFactory, contextAccessor)
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var items = request.Body.Items
                .Select(x => UberEatsOrderItem.Create(x.Name, x.Quantity))
                .ToArray();

            var order = UberEatsOrder.Place(
                request.Body.CustomerId,
                request.Body.RestaurantId,
                items);

            await DbContext.Set<UberEatsOrder>().AddAsync(order, cancellationToken);

            return new Response(
                order.Id.Value,
                order.Status.ToString(),
                order.CustomerId,
                order.RestaurantId);
        }
    }
}
