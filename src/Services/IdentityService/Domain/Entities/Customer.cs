namespace IdentityService.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Guid UserId { get; set; } 

        public string Address { get; set; } = null!;

        public string? City { get; set; }

    }
}
