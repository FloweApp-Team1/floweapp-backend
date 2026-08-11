using MediatR;
using Shared.Responses;

namespace CatalogService.Features.Occasions.ListOccasions
{
    public record ListOccasionsResponse(
        Guid Id,
        string Name,
        string? ImageUrl,
        int DisplayOrder,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        Guid LastChangedBy

    );


}
