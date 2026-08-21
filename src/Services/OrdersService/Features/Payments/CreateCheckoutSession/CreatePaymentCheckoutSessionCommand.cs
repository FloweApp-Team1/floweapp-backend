using MediatR;
using Shared.Results;

namespace OrdersService.Features.Payments.CreateCheckoutSession
{
    public record CreatePaymentCheckoutSessionCommand(
        Guid StoreId,
        Guid? AddressId,
        bool IsGift,
        string? GiftRecipientName,
        string? GiftRecipientPhone,
        string? GiftRecipientAddress,
        List<CreatePaymentCheckoutSessionItem> Items) : IRequest<Result<CreatePaymentCheckoutSessionResponse>>;

    public record CreatePaymentCheckoutSessionItem(Guid ProductId, int Quantity);

    public record CreatePaymentCheckoutSessionResponse(string CheckoutUrl, string StripeSessionId, Guid PaymentAttemptId, Guid OrderId);
}
