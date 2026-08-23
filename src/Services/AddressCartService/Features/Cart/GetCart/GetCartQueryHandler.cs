using AddressCartService.Infrastructure.Persistence;
using AddressCartService.Infrastructure.Services.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.GetCart
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, Result<GetCartResponse>>
    {
        private readonly AddressCartDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly ICatalogClient _catalogClient;

        public GetCartQueryHandler(
            AddressCartDbContext dbContext,
            ICurrentUserService currentUser,
            ICatalogClient catalogClient)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _catalogClient = catalogClient;
        }

        public async Task<Result<GetCartResponse>> Handle(
            GetCartQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
                return Result.Failure<GetCartResponse>(
                    Error.New("Cart.Unauthorized", "User is not authenticated."));

            var cartQuery = _dbContext.Carts
                .AsNoTracking()
                .Include(c => c.Items);

            Domain.Entities.Cart? cart;
            if (request.CartId.HasValue && request.CartId.Value != Guid.Empty)
            {
                cart = await cartQuery.FirstOrDefaultAsync(c => c.Id == request.CartId.Value && c.UserId == userId, cancellationToken);
            }
            else
            {
                cart = await cartQuery.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
            }

            if (cart is null || cart.Items.Count == 0)
            {
                var emptyResponse = new GetCartResponse(
                    CartId: cart?.Id ?? Guid.Empty,
                    Items: Array.Empty<CartItemResponse>(),
                    TotalQuantity: 0,
                    LineCount: 0,
                    Subtotal: 0,
                    DeliveryFee: null,
                    Total: 0,
                    HasChanges: false);

                return Result.Success(emptyResponse);
            }

            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var catalogProducts = await _catalogClient.GetProductsBatchAsync(productIds, storeId: null, cancellationToken);

            var itemResponses = new List<CartItemResponse>();
            var hasChanges = false;

            foreach (var item in cart.Items)
            {
                catalogProducts.TryGetValue(item.ProductId, out var product);

                var productName = product?.Name ?? "Product Unavailable";
                var productImage = product?.PrimaryImageUrl;
                var currentPrice = product?.EffectivePrice ?? item.PriceAtAdd;
                var availableStock = product?.AvailableStock ?? 0;
                var isAvailable = product is not null && product.IsAvailable && !product.IsArchived;

                var lineSubtotal = currentPrice * item.Quantity;
                var priceChanged = item.PriceAtAdd != currentPrice;
                var stockChanged = (item.Quantity > availableStock) || !isAvailable;

                if (priceChanged || stockChanged || !isAvailable)
                {
                    hasChanges = true;
                }

                itemResponses.Add(new CartItemResponse(
                    ItemId: item.Id,
                    ProductId: item.ProductId,
                    ProductName: productName,
                    ProductImage: productImage,
                    UnitPrice: currentPrice,
                    PriceAtAdd: item.PriceAtAdd,
                    Quantity: item.Quantity,
                    LineSubtotal: lineSubtotal,
                    AvailableStock: availableStock,
                    IsAvailable: isAvailable,
                    PriceChanged: priceChanged,
                    StockChanged: stockChanged));
            }

            var lineCount = itemResponses.Count;
            var totalQuantity = itemResponses.Sum(i => i.Quantity);
            var subtotal = itemResponses.Sum(i => i.LineSubtotal);
            decimal? deliveryFee = null;
            var total = subtotal + (deliveryFee ?? 0);

            var response = new GetCartResponse(
                CartId: cart.Id,
                Items: itemResponses,
                TotalQuantity: totalQuantity,
                LineCount: lineCount,
                Subtotal: subtotal,
                DeliveryFee: deliveryFee,
                Total: total,
                HasChanges: hasChanges);

            return Result.Success(response);
        }
    }
}
