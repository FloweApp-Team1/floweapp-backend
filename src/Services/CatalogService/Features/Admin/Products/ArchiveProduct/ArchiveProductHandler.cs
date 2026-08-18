using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Products.ArchiveProduct
{
    public class ArchiveProductHandler : IRequestHandler<ArchiveProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public ArchiveProductHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IIntegrationEventPublisher eventPublisher,
            IContentChangeLogger changeLogger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _eventPublisher = eventPublisher;
            _changeLogger = changeLogger;
        }

        public async Task<Result<ProductDto>> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.Repository<Product>();

            var product = await productRepo.Query()
                .Where(p => p.Id == request.ProductId)
                .Include(p => p.ProductImages)
                .Include(p => p.Occasions)
                .Include(p => p.StoreStocks)
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                return Result.Failure<ProductDto>(Error.New("Product.NotFound", "Product was not found."));

            var currentUserId = _currentUser.UserId ?? Guid.Empty;
            product.LastChangedBy = currentUserId;
            product.UpdatedAt = DateTime.UtcNow;

            await _changeLogger.LogAsync("Product", product.Id, "Archived", currentUserId, $"Archived product '{product.Name}'.", cancellationToken);
            productRepo.Remove(product); // sets IsDeleted = true

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new ProductArchivedEvent(product.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(ProductMapper.ToDto(product));
        }
    }
}
