using CatalogService.Features.Admin.Occasions.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Results;

namespace CatalogService.Features.Admin.Occasions.CreateOccasion
{
    public record CreateOccasionCommand(string Name, int Order, IFormFile? Image) : IRequest<Result<OccasionDto>>;
}
