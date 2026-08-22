using AddressCartService.Features.Cart.GetCart;
using AddressCartService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace AddressCartService.Features.Cart.RemoveCartItem
{
    public class RemoveCartItemHandler : IRequestHandler<RemoveCartItemCommand, Result<GetCartResponse>>
    {
        private readonly AddressCartDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ISender _sender;
        private readonly ILogger<RemoveCartItemHandler> _logger;

        public RemoveCartItemHandler(
            AddressCartDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ISender sender,
            ILogger<RemoveCartItemHandler> logger)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
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

            try
            {
                // Hard delete directly on DbSet and navigation collection to resolve query filter & unique constraint bug
                cart.Items.Remove(item);
                _dbContext.CartItems.Remove(item);

                cart.UpdatedAt = DateTime.UtcNow;
                cart.LastChangedBy = userId;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return await _sender.Send(new GetCartQuery(), cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error removing cart item {ItemId} for user {UserId}", request.ItemId, userId);
                return Result.Failure<GetCartResponse>(
                    Error.New("Cart.Conflict", "The cart was modified by another request. Please try again."));
            }
        }
    }
}
