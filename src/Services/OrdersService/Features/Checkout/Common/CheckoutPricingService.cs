using OrdersService.Infrastructure.Services;
using Shared.Results;

namespace OrdersService.Features.Checkout.Common
{
    public class CheckoutPricingService : ICheckoutPricingService
    {
        private readonly ICartServiceClient _cartServiceClient;
        private readonly ICatalogServiceClient _catalogServiceClient;
        private readonly IDeliveryFeeCalculator _deliveryFeeCalculator;
        private readonly IDeliveryEstimateCalculator _deliveryEstimateCalculator;

        public CheckoutPricingService(
            ICartServiceClient cartServiceClient,
            ICatalogServiceClient catalogServiceClient,
            IDeliveryFeeCalculator deliveryFeeCalculator,
            IDeliveryEstimateCalculator deliveryEstimateCalculator)
        {
            _cartServiceClient = cartServiceClient;
            _catalogServiceClient = catalogServiceClient;
            _deliveryFeeCalculator = deliveryFeeCalculator;
            _deliveryEstimateCalculator = deliveryEstimateCalculator;
        }

        public async Task<Result<CheckoutPricingResult>> CalculateAsync(
            Guid cartId, Guid userId, OrderAddressDetails? address, CancellationToken cancellationToken)
        {
           
            var cart = await _cartServiceClient.GetCartAsync(cartId, userId, cancellationToken);
            if (cart is null || cart.Items.Count == 0)
                return Result.Failure<CheckoutPricingResult>(
                    Error.New("Cart.Empty", "Your cart is empty or could not be found."));

            var itemsResult = await BuildPricedItemsAsync(cart, cancellationToken);
            if (itemsResult.IsFailure)
                return Result.Failure<CheckoutPricingResult>(itemsResult.Error);

            var items = itemsResult.Value;
            var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);

            decimal deliveryFee = 0;
            DateTime? estimatedDeliveryAt = null;

        
            if (address is not null && address.IsServiceable && address.StoreId is not null)
            {
                deliveryFee = await _deliveryFeeCalculator.CalculateAsync(
                    address.StoreId.Value, address, cancellationToken);

                estimatedDeliveryAt = await _deliveryEstimateCalculator.EstimateDeliveryAtAsync(
                    address.StoreId.Value, address, cancellationToken);
            }

            var total = subtotal + deliveryFee;

            return Result.Success(new CheckoutPricingResult(items, subtotal, deliveryFee, total, estimatedDeliveryAt));
        }

        private async Task<Result<List<CartItemPricedDto>>> BuildPricedItemsAsync(
            CartDetailsDto cart, CancellationToken cancellationToken)
        {
            var lookups = await Task.WhenAll(cart.Items.Select(async cartItem =>
            {
                var product = await _catalogServiceClient.GetProductDetailsAsync(cartItem.ProductId, cancellationToken);
                return (CartItem: cartItem, Product: product);
            }));

            var missing = lookups.FirstOrDefault(l => l.Product is null);
            if (missing.CartItem is not null)
                return Result.Failure<List<CartItemPricedDto>>(Error.New(
                    "Product.NotFound", $"Product {missing.CartItem.ProductId} is no longer available."));

            var items = lookups.Select(l => new CartItemPricedDto(
                l.CartItem.ProductId, l.Product!.Name, l.Product.ImageUrl, l.Product.Price, l.CartItem.Quantity)).ToList();

            return Result.Success(items);
        }
    }
}