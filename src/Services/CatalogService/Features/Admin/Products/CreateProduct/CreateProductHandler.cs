using CatalogService.Domain.Entities;
using CatalogService.Features.Admin.Products.Common;
using MediatR;
using Shared.Events.IntegrationEvents;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Admin.Products.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileStorageService _fileStorage;
        private readonly IIntegrationEventPublisher _eventPublisher;
        private readonly IContentChangeLogger _changeLogger;

        public CreateProductHandler(
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

        public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var categoryRepo = _unitOfWork.Repository<Category>();
            var occasionRepo = _unitOfWork.Repository<Occasion>();
            var primaryCategoryId = request.CategoryIds.First();
            var category = await categoryRepo.GetByIdAsync(primaryCategoryId, cancellationToken);
            if (category is null)
                return Result.Failure<ProductDto>(Error.New("Category.NotFound", "One or more categories were not found."));

            var occasions = new List<Occasion>();
            if (request.OccasionIds is { Count: > 0 })
            {
                foreach (var occasionId in request.OccasionIds.Distinct())
                {
                    var occasion = await occasionRepo.GetByIdAsync(occasionId, cancellationToken);
                    if (occasion is null)
                        return Result.Failure<ProductDto>(Error.New("Occasion.NotFound", $"Occasion '{occasionId}' was not found."));
                    occasions.Add(occasion);
                }
            }

            var currentUserId = _currentUser.UserId ?? Guid.Empty;

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Includes = request.Includes ?? new List<string>(),
                Price = request.Price,
                DiscountPercent = request.DiscountPercent,
                CategoryId = category.Id,
                Occasions = occasions,
                StockQuantity = request.StoreStock.Sum(s => s.Quantity),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedBy = currentUserId
            };

            product.ProductImages = new List<ProductImage>();
            for (var i = 0; i < request.Images.Count; i++)
            {
                var url = await _fileStorage.SaveAsync(request.Images[i], "products", cancellationToken);
                product.ProductImages.Add(new ProductImage
                {
                    Id = Guid.NewGuid(),
                    ImageUrl = url,
                    IsPrimary = i == 0,
                    DisplayOrder = i,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    LastChangedBy = currentUserId
                });
            }

            product.StoreStocks = request.StoreStock.Select(s => new ProductStoreStock
            {
                Id = Guid.NewGuid(),
                StoreId = s.StoreId,
                Quantity = s.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedBy = currentUserId
            }).ToList();

            await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
            await _changeLogger.LogAsync("Product", product.Id, "Created", currentUserId, $"Created product '{product.Name}'.", cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _eventPublisher.PublishAsync(
                new ProductCreatedEvent(product.Id, product.CategoryId, currentUserId, DateTime.UtcNow),
                cancellationToken);

            return Result.Success(ProductMapper.ToDto(product));
        }
    }
    }
