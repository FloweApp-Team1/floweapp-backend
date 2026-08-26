using OrdersService.Infrastructure.Services;
using Shared.Results;

namespace OrdersService.Features.Checkout.Common
{
    public interface ICheckoutPricingService
    {
        Task<Result<CheckoutPricingResult>> CalculateAsync(
            Guid cartId,
            Guid userId,
            OrderAddressDetails? address,
            CancellationToken cancellationToken);
    }

    public sealed record CheckoutPricingResult(
        IReadOnlyList<CartItemPricedDto> Items,
        decimal Subtotal,
        decimal DeliveryFee,
        decimal Total,
        DateTime? EstimatedDeliveryAt);

    public sealed record CartItemPricedDto(
        Guid ProductId, string ProductName, string? ProductImageUrl, decimal UnitPrice, int Quantity);
}
