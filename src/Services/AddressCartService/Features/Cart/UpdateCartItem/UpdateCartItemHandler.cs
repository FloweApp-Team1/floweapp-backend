using AddressCartService.Features.Cart.GetCart;
using AddressCartService.Infrastructure.Persistence;
using AddressCartService.Infrastructure.Services.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.UpdateCartItem
{
    public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, Result<GetCartResponse>>
    {
        private readonly AddressCartDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ICatalogClient _catalogClient;
        private readonly ISender _sender;
        private readonly ILogger<UpdateCartItemHandler> _logger;

        public UpdateCartItemHandler(
            AddressCartDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ICatalogClient catalogClient,
            ISender sender,
            ILogger<UpdateCartItemHandler> logger)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
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

            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

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

            try
            {
                item.Quantity = request.Quantity;
                item.UpdatedAt = DateTime.UtcNow;
                item.LastChangedBy = userId;

                cart.UpdatedAt = DateTime.UtcNow;
                cart.LastChangedBy = userId;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return await _sender.Send(new GetCartQuery(), cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error updating cart item {ItemId} for user {UserId}", request.ItemId, userId);
                return Result.Failure<GetCartResponse>(
                    Error.New("Cart.Conflict", "The cart was modified by another request. Please try again."));
            }
        }
    }
}
