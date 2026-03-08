using Api.Domain.UberEatsOrders.AggregateRoots;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Orders.Accept;

public sealed class AcceptOrder
{
    public sealed record Request
    {
        [FromRoute] public Guid OrderId { get; set; }
        [FromBody] public RequestBody Body { get; set; } = null!;

        public sealed record RequestBody
        {
            public Guid RestaurantId { get; set; }
        }
    }

    public sealed record Response(bool Success, Guid OrderId, string Status, string? Error);

    public sealed class Endpoint() : PostMinimalEndpoint<Request, Response>("workshop/ubereats/orders/{orderId:guid}/accept");

    public sealed class Handler(
        IAppDbContextFactory dbContextFactory,
        IHttpContextAccessor contextAccessor)
        : Engine.Wolverine.Handler(dbContextFactory, contextAccessor)
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var orderId = new UberEatsOrderId(request.OrderId);
            var order = await DbContext.Set<UberEatsOrder>()
                .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null)
                return new Response(false, request.OrderId, "Unknown", "Order not found.");

            order.AcceptByRestaurant(request.Body.RestaurantId);
            return new Response(true, order.Id.Value, order.Status.ToString(), null);
        }
    }
}
