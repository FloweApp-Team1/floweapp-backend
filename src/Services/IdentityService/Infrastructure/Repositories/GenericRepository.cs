using IdentityService.Domain.Entities;
using IdentityService.Domain.Intefaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Repositories
{
    public class GenericRepository<T>(AuthDbContext context) : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly AuthDbContext Context = context;

     
        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct)
            => await Context.Set<T>().FirstOrDefaultAsync(e => e.Id == id , ct);

        public async Task AddAsync(T entity, CancellationToken ct)
            => await Context.Set<T>().AddAsync(entity, ct);
    }
}

