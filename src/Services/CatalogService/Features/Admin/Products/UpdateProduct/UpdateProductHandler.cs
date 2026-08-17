using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Products.UpdateProduct
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public UpdateProductHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IFileStorageService fileStorage,
            IIntegrationEventPublisher eventPublisher,
            IContentChangeLogger changeLogger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _fileStorage = fileStorage;
            _eventPublisher = eventPublisher;
            _changeLogger = changeLogger;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var productRepo = _unitOfWork.Repository<Product>();

            var product = await productRepo.Query()
                .Where(p => p.Id == request.ProductId)
                .Include(p => p.ProductImages)
                .Include(p => p.Occasions)
                .Include(p => p.StoreStocks)
                .Include(p => p.Includes)
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                return Result.Failure<ProductDto>(Error.New("Product.NotFound", "Product was not found."));

            if (request.Name is not null) product.Name = request.Name;
            if (request.Description is not null) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.DiscountPercent.HasValue) product.DiscountPercent = request.DiscountPercent.Value;

            if (request.CategoryIds is { Count: > 0 })
            {
                var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.CategoryIds[0], cancellationToken);
                if (category is null)
                    return Result.Failure<ProductDto>(Error.New("Category.NotFound", "Category was not found."));
                product.CategoryId = category.Id;
            }

            if (request.OccasionIds is not null)
            {
                var occasions = new List<Occasion>();
                foreach (var occasionId in request.OccasionIds.Distinct())
                {
                    var occasion = await _unitOfWork.Repository<Occasion>().GetByIdAsync(occasionId, cancellationToken);
                    if (occasion is null)
                        return Result.Failure<ProductDto>(Error.New("Occasion.NotFound", $"Occasion '{occasionId}' was not found."));
                    occasions.Add(occasion);
                }
                product.Occasions = occasions;
            }

            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            if (request.Includes is not null)
            {
                // Full replacement: orphaned rows are removed by the cascade delete
                // configured on ProductInclude.ProductId.
                product.Includes ??= new List<ProductInclude>();
                product.Includes.Clear();
                foreach (var name in request.Includes)
                {
                    product.Includes.Add(new ProductInclude
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastChangedBy = currentUserId
                    });
                }
            }

            if (request.Images is { Count: > 0 })
            {
                product.ProductImages ??= new List<ProductImage>();
                var startOrder = product.ProductImages.Count;
                for (var i = 0; i < request.Images.Count; i++)
                {
                    var url = await _fileStorage.SaveAsync(request.Images[i], "products", cancellationToken);
                    product.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ImageUrl = url,
                        IsPrimary = product.ProductImages.Count == 0,
                        DisplayOrder = startOrder + i,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        LastChangedBy = currentUserId
                    });
                }
            }

            if (request.StoreStock is not null)
            {
                foreach (var stockUpdate in request.StoreStock)
                {
                    var existing = product.StoreStocks?.FirstOrDefault(s => s.StoreId == stockUpdate.StoreId);
                    if (existing is not null)
                    {
                        existing.Quantity = stockUpdate.Quantity;
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.LastChangedBy = currentUserId;
                    }
                    else
                    {
                        product.StoreStocks ??= new List<ProductStoreStock>();
                        product.StoreStocks.Add(new ProductStoreStock
                        {
                            Id = Guid.NewGuid(),
                            StoreId = stockUpdate.StoreId,
                            Quantity = stockUpdate.Quantity,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            LastChangedBy = currentUserId
                        });
                    }
                }
                product.StockQuantity = product.StoreStocks?.Sum(s => s.Quantity) ?? product.StockQuantity;
            }

            product.UpdatedAt = DateTime.UtcNow;
            product.LastChangedBy = currentUserId;

            await _changeLogger.LogAsync("Product", product.Id, "Updated", currentUserId, $"Updated product '{product.Name}'.", cancellationToken);
            productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new ProductUpdatedEvent(product.Id, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(ProductMapper.ToDto(product));
        }
    }
}
