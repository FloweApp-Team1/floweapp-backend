using IdentityService.Common.Result;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Intefaces;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin.Queries
{
    public sealed record GetAdminByEmailQuery(string Email) : IRequest<Result<User?>>;
    public sealed class GetAdminByEmailHandler(IUserRepository userRepository)
       : IRequestHandler<GetAdminByEmailQuery, Result<User?>>
    {
        public async Task<Result<User?>> Handle(GetAdminByEmailQuery request, CancellationToken ct)
        {
            var user = await userRepository.GetByEmailWithRolesAsync(request.Email, ct);
            return Result.Success(user);
        }
    }
}
