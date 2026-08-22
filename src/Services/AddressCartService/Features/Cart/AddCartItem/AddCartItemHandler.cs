using AddressCartService.Domain.Entities;
using AddressCartService.Infrastructure.Persistence;
using AddressCartService.Infrastructure.Services.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.AddCartItem
{
    public class AddCartItemHandler : IRequestHandler<AddCartItemCommand, Result<AddCartItemResponse>>
    {
        private readonly AddressCartDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ICatalogClient _catalogClient;
        private readonly ILogger<AddCartItemHandler> _logger;

        public AddCartItemHandler(
            AddressCartDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ICatalogClient catalogClient,
            ILogger<AddCartItemHandler> logger)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
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

            try
            {
                var cart = await _dbContext.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

                if (cart is null)
                {
                    cart = new Domain.Entities.Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastChangedBy = userId
                    };
                    await _dbContext.Carts.AddAsync(cart, cancellationToken);
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
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.LastChangedBy = userId;
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
                        CartId = cart.Id,
                        Cart = cart,
                        ProductId = request.ProductId,
                        Quantity = finalQuantity,
                        PriceAtAdd = priceAtAdd,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastChangedBy = userId
                    };
                    cart.Items.Add(newItem);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                cart.LastChangedBy = userId;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(new AddCartItemResponse(
                    cart.Id,
                    itemId,
                    request.ProductId,
                    finalQuantity,
                    priceAtAdd));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error adding item to cart for user {UserId}", userId);
                return Result.Failure<AddCartItemResponse>(
                    Error.New("Cart.Conflict", "The cart was modified by another request. Please try again."));
            }
        }
    }
}
