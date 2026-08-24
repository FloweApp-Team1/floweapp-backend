using AddressCartService.Features.Cart.GetCart;
using AddressCartService.Infrastructure.Services.Catalog;
using MediatR;
using Shared.Contracts;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.UpdateCartItem
{
    public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, Result<GetCartResponse>>
    {
        private readonly IRedisCacheService _redisCache;
        private readonly ICurrentUserService _currentUser;
        private readonly ICatalogClient _catalogClient;
        private readonly ISender _sender;
        private readonly ILogger<UpdateCartItemHandler> _logger;

        public UpdateCartItemHandler(
            IRedisCacheService redisCache,
            ICurrentUserService currentUser,
            ICatalogClient catalogClient,
            ISender sender,
            ILogger<UpdateCartItemHandler> logger)
        {
            _redisCache = redisCache;
            _currentUser = currentUser;
            _catalogClient = catalogClient;
            _sender = sender;
            _logger = logger;
        }

        public async Task<Result<GetCartResponse>> Handle(
            UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
                return Result.Failure<GetCartResponse>(
                    Error.New("Cart.Unauthorized", "User is not authenticated."));

            var cacheKey = CartCacheKeys.Cart(userId);
            var cart = await _redisCache.GetAsync<Domain.Entities.Cart>(cacheKey);

            if (cart is null)
                return Result.Failure<GetCartResponse>(
                    Error.New("CartItem.NotFound", "The cart item was not found."));

            var item = cart.Items.FirstOrDefault(i => i.Id == request.ItemId);
            if (item is null)
                return Result.Failure<GetCartResponse>(
                    Error.New("CartItem.NotFound", "The cart item was not found."));

            var product = await _catalogClient.GetProductByIdAsync(item.ProductId, storeId: null, cancellationToken);
            if (product is null || !product.IsAvailable || product.IsArchived)
                return Result.Failure<GetCartResponse>(
                    Error.New("Product.Unavailable", "Product is not available for purchase."));

            if (request.Quantity > product.AvailableStock)
            {
                return Result.Failure<GetCartResponse>(
                    Error.New("Cart.Conflict.Stock", $"Requested quantity ({request.Quantity}) exceeds available stock ({product.AvailableStock})."));
            }

            item.Quantity = request.Quantity;

            // Save to cache with 30 days sliding expiration
            await _redisCache.SetAsync(cacheKey, cart, TimeSpan.FromDays(30));

            return await _sender.Send(new GetCartQuery(), cancellationToken);
        }
    }
}
