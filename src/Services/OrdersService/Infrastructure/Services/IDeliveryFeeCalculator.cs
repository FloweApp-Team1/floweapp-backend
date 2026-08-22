namespace OrdersService.Infrastructure.Services
{
  
    namespace OrdersService.Infrastructure.Services
    {
     
        public interface IDeliveryFeeCalculator
        {
            Task<decimal> CalculateAsync(
                Guid storeId, OrderAddressDetails address, CancellationToken cancellationToken);
        }

        public class FlatRateDeliveryFeeCalculator : IDeliveryFeeCalculator
        {
            private const decimal FlatFee = 25.0m;

            public Task<decimal> CalculateAsync(
                Guid storeId, OrderAddressDetails address, CancellationToken cancellationToken)
                => Task.FromResult(FlatFee);
        }
    }
}
