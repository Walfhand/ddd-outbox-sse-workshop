using Api.Domain.Orders.AggregateRoots;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Orders.CreateOrderRepr;

public sealed class CreateOrder
{
    public sealed record Request
    {
        [FromBody] public RequestBody Body { get; set; } = null!;

        public sealed record RequestBody
        {
            public List<OrderLineDto> OrderLines { get; set; } = null!;
        }
    }

    public sealed record OrderLineDto(int ProductId, int Quantity);

    public sealed record Response(Guid OrderId);

    public sealed class CreateOrderEndpoint()
        : PostMinimalEndpoint<Request, Response>("orders");

    public sealed class CreateRequestHandler(
        IAppDbContextFactory dbContextFactory,
        IHttpContextAccessor contextAccessor)
        : Handler(dbContextFactory, contextAccessor)
    {
        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var order = Order.Create();
            foreach (var orderLine in request.Body.OrderLines) order.AddLine(orderLine.ProductId, orderLine.Quantity);

            await DbContext.Set<Order>().AddAsync(order, cancellationToken);
            return new Response(order.Id);
        }
    }
}