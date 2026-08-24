using AddressCartService.Features.Cart.GetCart;
using MediatR;
using Shared.Contracts;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.RemoveCartItem
{
    public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand, Result<GetCartResponse>>
    {
        private readonly IRedisCacheService _redisCache;
        private readonly ICurrentUserService _currentUser;
        private readonly ISender _sender;
        private readonly ILogger<RemoveCartItemHandler> _logger;

        public RemoveCartItemHandler(
            IRedisCacheService redisCache,
            ICurrentUserService currentUser,
            ISender sender,
            ILogger<RemoveCartItemHandler> logger)
        {
            _redisCache = redisCache;
            _currentUser = currentUser;
            _sender = sender;
            _logger = logger;
        }

        public async Task<Result<GetCartResponse>> Handle(
            RemoveCartItemCommand request, CancellationToken cancellationToken)
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

            cart.Items.Remove(item);

            // Save to cache with 30 days sliding expiration
            await _redisCache.SetAsync(cacheKey, cart, TimeSpan.FromDays(30));

            return await _sender.Send(new GetCartQuery(), cancellationToken);
        }
    }
}
