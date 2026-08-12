using CatalogService.Domain.Entities;
using CatalogService.Features.Home.Dtos;
using CatalogService.Infrastructure.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;
using Shared.Results;

namespace CatalogService.Features.Home.Queries
{
    public class GetHomeLayoutQuery : IRequest<Result<List<HomeLayoutSectionDto>>>
    {
    }
}
