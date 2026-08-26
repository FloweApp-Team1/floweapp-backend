namespace OrdersService.Infrastructure.Services
{
    public interface IDeliveryEstimateCalculator
    {
        Task<DateTime> EstimateDeliveryAtAsync(
            Guid storeId, OrderAddressDetails address, CancellationToken cancellationToken);
    }

    public class FlatDeliveryEstimateCalculator : IDeliveryEstimateCalculator
    {
        private static readonly TimeSpan FlatEstimate = TimeSpan.FromMinutes(45);

        public Task<DateTime> EstimateDeliveryAtAsync(
            Guid storeId, OrderAddressDetails address, CancellationToken cancellationToken)
            => Task.FromResult(DateTime.UtcNow.Add(FlatEstimate));
    }
}
