using MediatR;
using OrdersService.Domain.Enums;
using Shared.Results;
namespace OrdersService.Features.Checkout.PlaceOrder
{
    public sealed record PlaceOrderCommand(
        Guid CartId,
        Guid AddressId,
        bool IsGift,
        GiftRecipientRequest? GiftRecipient,
        PaymentMethodEnum PaymentMethod,
        string? PaymentGateway,
        string IdempotencyKey) : IRequest<Result<PlaceOrderResponse?>>;
    public sealed record GiftRecipientRequest(string RecipientName, string RecipientPhone);
}

