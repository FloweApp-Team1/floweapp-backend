namespace OrdersService.Features.Checkout.GetEstimateDelivery
{
    public sealed record EstimateDeliveryResponse(
        Guid AddressId,
        bool IsServiceable,
        decimal DeliveryFee,
        DateTime EstimatedDeliveryAt);
}
