using IdentityService.Common.Security;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace IdentityService.Infrastructure.Persistence.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(AuthDbContext context, IConfiguration configuration, CancellationToken ct = default)
        {
            
            string? adminEmail = configuration["AdminSeed:Email"];
            string? adminPassword = configuration["AdminSeed:Password"];
            string fullName = configuration["AdminSeed:FullName"] ?? "System Admin";
            string phoneNumber = configuration["AdminSeed:PhoneNumber"] ?? "";

           
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

          
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", ct);
            if (adminRole is null)
            {
                adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
                context.Roles.Add(adminRole);
            }

          
            var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail, ct);

            if (!adminExists)
            {
                var hasher = new BCryptPasswordHasher();

                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = fullName,
                    Email = adminEmail,
                    PhoneNumber = phoneNumber,
                    PasswordHash = hasher.Hash(adminPassword),
                    BirthDate = new DateOnly(1990, 1, 1),
                    IsEmailConfirmed = true,
                    Gender = GenderEnum.Male,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    UserRoles = new List<UserRole>()
                };

                context.Users.Add(adminUser);
                context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });

                await context.SaveChangesAsync(ct);
            }
        }
    }
}
