using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Services.Catalog;
using MediatR;
using Shared.Contracts;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.AddCartItem
{
    public class AddCartItemHandler : IRequestHandler<AddCartItemCommand, Result<AddCartItemResponse>>
    {
        private readonly IRedisCacheService _redisCache;
        private readonly ICurrentUserService _currentUser;
        private readonly ICatalogClient _catalogClient;
        private readonly ILogger<AddCartItemHandler> _logger;

        public AddCartItemHandler(
            IRedisCacheService redisCache,
            ICurrentUserService currentUser,
            ICatalogClient catalogClient,
            ILogger<AddCartItemHandler> logger)
        {
            _redisCache = redisCache;
            _currentUser = currentUser;
            _catalogClient = catalogClient;
            _logger = logger;
        }

        public async Task<Result<AddCartItemResponse>> Handle(
            AddCartItemCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
                return Result.Failure<AddCartItemResponse>(
                    Error.New("Cart.Unauthorized", "User is not authenticated."));

            var product = await _catalogClient.GetProductByIdAsync(request.ProductId, storeId: null, cancellationToken);
            if (product is null)
                return Result.Failure<AddCartItemResponse>(
                    Error.New("Product.NotFound", "Product was not found."));

            if (!product.IsAvailable || product.IsArchived)
                return Result.Failure<AddCartItemResponse>(
                    Error.New("Product.Unavailable", "Product is not available for purchase."));

            if (product.AvailableStock <= 0)
                return Result.Failure<AddCartItemResponse>(
                    Error.New("Cart.Conflict.Stock", "Product is currently out of stock."));

            var cacheKey = CartCacheKeys.Cart(userId);
            var cart = await _redisCache.GetAsync<Domain.Entities.Cart>(cacheKey);

            if (cart is null)
            {
                cart = new Domain.Entities.Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            Guid itemId;
            int finalQuantity;
            decimal priceAtAdd;

            if (existingItem is not null)
            {
                finalQuantity = existingItem.Quantity + request.Quantity;
                if (finalQuantity > product.AvailableStock)
                {
                    return Result.Failure<AddCartItemResponse>(
                        Error.New("Cart.Conflict.Stock", $"Requested total quantity ({finalQuantity}) exceeds available stock ({product.AvailableStock})."));
                }

                existingItem.Quantity = finalQuantity;
                itemId = existingItem.Id;
                priceAtAdd = existingItem.PriceAtAdd;
            }
            else
            {
                finalQuantity = request.Quantity;
                if (finalQuantity > product.AvailableStock)
                {
                    return Result.Failure<AddCartItemResponse>(
                        Error.New("Cart.Conflict.Stock", $"Requested quantity ({finalQuantity}) exceeds available stock ({product.AvailableStock})."));
                }

                itemId = Guid.NewGuid();
                priceAtAdd = product.EffectivePrice;

                var newItem = new CartItem
                {
                    Id = itemId,
                    ProductId = request.ProductId,
                    Quantity = finalQuantity,
                    PriceAtAdd = priceAtAdd
                };
                cart.Items.Add(newItem);
            }

            // Save to cache with 30 days sliding expiration
            await _redisCache.SetAsync(cacheKey, cart, TimeSpan.FromDays(30));

            return Result.Success(new AddCartItemResponse(
                cart.Id,
                itemId,
                request.ProductId,
                finalQuantity,
                priceAtAdd));
        }
    }
}
