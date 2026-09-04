namespace OrdersService.Features.DriverDelivery.GetAvailableOrders
{
        public sealed record AvailableOrderDto(
            Guid OrderId,
            string CustomerName,
            string CustomerAddress,
            string StoreName,
            string StoreAddress,
            decimal TotalAmount,
            string Status,
            DateTime CreatedAt);

        public sealed record GetAvailableOrdersResponse(
            IReadOnlyList<AvailableOrderDto> Orders,
            int TotalCount);
}
