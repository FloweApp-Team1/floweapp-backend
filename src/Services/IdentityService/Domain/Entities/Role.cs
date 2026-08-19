using Shared.Domain;
namespace IdentityService.Domain.Entities
{
    public class Role:BaseEntity
    {
        public string Name { get; set; } = null!;
        public ICollection<UserRole> UserRoles { get; set; }

    }
}
