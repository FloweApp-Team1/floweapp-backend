using System.Text.Json;
using AddressCartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AddressCartService.Infrastructure.Persistence.Seed
{
    public static class LocationSeeder
    {
        public static async Task SeedAsync(AddressCartDbContext context, CancellationToken ct = default)
        {
            await SeedGovernoratesAsync(context, ct);
            await SeedCitiesAsync(context, ct);
        }

        private static async Task SeedGovernoratesAsync(AddressCartDbContext context, CancellationToken ct)
        {
            if (await context.Governorates.AnyAsync(ct))
                return;

            var path = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Persistence", "Seed", "Data", "governorates.json");
            if (!File.Exists(path))
            {
                path = Path.Combine("Infrastructure", "Persistence", "Seed", "Data", "governorates.json");
            }
            if (!File.Exists(path)) return;

            var json = await File.ReadAllTextAsync(path, ct);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<List<GovernorateDto>>(json, options);

            if (data == null) return;

            var governorates = data.Select(d => new Governorate
            {
                Id = int.Parse(d.Id.ToString()),
                NameAr = d.Governorate_Name_Ar,
                NameEn = d.Governorate_Name_En
            });

            // Need to allow Identity Insert for EF Core since Id is an int and we are supplying it.
            // Note: EF Core generates values by default for int keys.
            // Using SET IDENTITY_INSERT ON is required for SQL Server, but since we are seeding data we might need it.
            // Or we can just use `Database.ExecuteSqlRaw` or disable identity on the model.
            // We will let EF handle it by disabling ValueGeneratedOnAdd on the model configuration.
            context.Governorates.AddRange(governorates);
            await context.SaveChangesAsync(ct);
        }

        private static async Task SeedCitiesAsync(AddressCartDbContext context, CancellationToken ct)
        {
            if (await context.Cities.AnyAsync(ct))
                return;

            var path = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Persistence", "Seed", "Data", "cities.json");
            if (!File.Exists(path))
            {
                path = Path.Combine("Infrastructure", "Persistence", "Seed", "Data", "cities.json");
            }
            if (!File.Exists(path)) return;

            var json = await File.ReadAllTextAsync(path, ct);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            var data = JsonSerializer.Deserialize<List<CityDto>>(json, options);

            if (data == null) return;

            var cities = data.Select(d => new City
            {
                Id = int.Parse(d.Id),
                GovernorateId = int.Parse(d.Governorate_Id),
                NameAr = d.City_Name_Ar,
                NameEn = d.City_Name_En
            });

            context.Cities.AddRange(cities);
            await context.SaveChangesAsync(ct);
        }

        private class GovernorateDto
        {
            public object Id { get; set; } = null!;
            public string Governorate_Name_Ar { get; set; } = string.Empty;
            public string Governorate_Name_En { get; set; } = string.Empty;
        }

        private class CityDto
        {
            public string Id { get; set; } = string.Empty;
            public string Governorate_Id { get; set; } = string.Empty;
            public string City_Name_Ar { get; set; } = string.Empty;
            public string City_Name_En { get; set; } = string.Empty;
        }
    }
}
