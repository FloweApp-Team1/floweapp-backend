using Shared.Contracts;
using Shared.Security;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence.Seed
{
    public static class AdminSeeder
    {
        // Uses the application's single IPasswordHasher, so the seeded admin can log
        // in through both /auth/login and /auth/admin-login.
        public static async Task SeedAsync(
            AuthDbContext context,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            CancellationToken ct = default)
        {
            await SeedRolesAsync(context, ct);

            await SeedAdminUserAsync(context, configuration, passwordHasher, ct);

            await context.SaveChangesAsync(ct);
        }

        private static async Task SeedRolesAsync(AuthDbContext context, CancellationToken ct)
        {
            string[] requiredRoles = [AppRoles.Admin, AppRoles.Customer, AppRoles.Driver];

            var existingRoles = await context.Roles
                .Select(r => r.Name)
                .ToListAsync(ct);

            foreach (var roleName in requiredRoles)
            {
                if (existingRoles.Contains(roleName))
                    continue;

                context.Roles.Add(new Role { Id = Guid.NewGuid(), Name = roleName });
            }
        }

        private static async Task SeedAdminUserAsync(
            AuthDbContext context,
            IConfiguration configuration,
            IPasswordHasher passwordHasher,
            CancellationToken ct)
        {
            string? adminEmail = configuration["AdminSeed:Email"];
            string? adminPassword = configuration["AdminSeed:Password"];
            string firstName = configuration["AdminSeed:FirstName"] ?? "System";
            string lastName = configuration["AdminSeed:LastName"] ?? "Admin";
            string phoneNumber = configuration["AdminSeed:PhoneNumber"] ?? "";

            // Leaving either value unset is the supported way to skip admin seeding.
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                return;

            if (await context.Users.AnyAsync(u => u.Email == adminEmail, ct))
                return;

            // The ADMIN role may still be pending in the change tracker from
            // SeedRolesAsync on a brand-new database, so check both.
            var adminRole =
                await context.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.Admin, ct)
                ?? context.ChangeTracker.Entries<Role>()
                    .Select(e => e.Entity)
                    .First(r => r.Name == AppRoles.Admin);

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = adminEmail,
                PhoneNumber = phoneNumber,
                PasswordHash = passwordHasher.Hash(adminPassword),
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
        }
    }
}
