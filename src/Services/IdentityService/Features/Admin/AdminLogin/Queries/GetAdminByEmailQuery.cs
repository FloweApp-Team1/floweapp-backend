using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Admin.AdminLogin.Queries
{
    public sealed record GetAdminByEmailQuery(string Email) : IRequest<Result<User?>>;

    public sealed class GetAdminByEmailHandler(IUnitOfWork unitOfWork)
       : IRequestHandler<GetAdminByEmailQuery, Result<User?>>
    {
        public async Task<Result<User?>> Handle(GetAdminByEmailQuery request, CancellationToken ct)
        {
            var user = await unitOfWork.Repository<User>().Query()
                .AsNoTracking()
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

            return Result.Success(user);
        }
    }
}
