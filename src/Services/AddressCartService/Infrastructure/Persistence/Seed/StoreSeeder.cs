using global::AddressCartService.Domain.Entities;
using global::AddressCartService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

    namespace AddressCartService.Infrastructure.Persistence.Seed
    {
        public static class StoreSeeder
        {
            public static async Task SeedAsync(AddressCartDbContext context, CancellationToken ct = default)
            {
                await SeedStoresAsync(context, ct);
            }

            private static async Task SeedStoresAsync(AddressCartDbContext context, CancellationToken ct)
            {
                if (await context.Stores.AnyAsync(ct))
                    return;

                var now = DateTime.UtcNow;

                var stores = new List<Store>
            {
                // 1. RADIUS - 50 km 
                new()
                {
                    Id = SeedIds.Stores.CairoMainRadius,
                    Name = "Cairo Main Flower Hub (Radius)",
                    Status = StoreStatusEnum.Active,
                    Location = new StoreLocation
                    {
                        AddressLine = "10 Abbas El Akkad St, Nasr City, Cairo",
                        Lat = 30.0561,
                        Lng = 31.3452
                    },
                    CoverageArea = new CoverageArea
                    {
                        Type = CoverageAreaTypeEnum.Radius,
                        RadiusCenterLat = 30.0561,
                        RadiusCenterLng = 31.3452,
                        RadiusKm = 50.0
                    },
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Admin.Default
                },

                // 2. CITY_AREA_LIST
                new()
                {
                    Id = SeedIds.Stores.NewCairoAreaList,
                    Name = "East Cairo & Tagamoa Hub (AreaList)",
                    Status = StoreStatusEnum.Active,
                    Location = new StoreLocation
                    {
                        AddressLine = "Road 90 North, 5th Settlement",
                        Lat = 30.0254,
                        Lng = 31.4789
                    },
                    CoverageArea = new CoverageArea
                    {
                        Type = CoverageAreaTypeEnum.CityAreaList,
                        CityAreasJson = JsonSerializer.Serialize(new[]
                        {
                            new { city = "Cairo", area = "New Cairo" },
                            new { city = "Al Basatin", area = "Nasr City" },
                            new { city = "Cairo", area = "Maadi" },
                            new { city = "Cairo", area = "Al Rehab" }
                        })
                    },
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Admin.Default
                },

                // 3. POLYGON
                new()
                {
                    Id = SeedIds.Stores.AlexandriaPolygon,
                    Name = "Alexandria Coastal Hub (Polygon)",
                    Status = StoreStatusEnum.Active,
                    Location = new StoreLocation
                    {
                        AddressLine = "Stanley Bridge, Corniche, Alexandria",
                        Lat = 31.2333,
                        Lng = 29.9500
                    },
                    CoverageArea = new CoverageArea
                    {
                        Type = CoverageAreaTypeEnum.Polygon,
                        PolygonJson = JsonSerializer.Serialize(new[]
                        {
                            new { lat = 31.2000, lng = 29.8800 },
                            new { lat = 31.2600, lng = 29.9700 },
                            new { lat = 31.2300, lng = 30.0200 },
                            new { lat = 31.1800, lng = 29.9100 }
                        })
                    },
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Admin.Default
                },

                // 4. RADIUS - 25 km
                new()
                {
                    Id = SeedIds.Stores.GizaZayedRadius,
                    Name = "Sheikh Zayed & October Hub (Radius)",
                    Status = StoreStatusEnum.Active,
                    Location = new StoreLocation
                    {
                        AddressLine = "Hyper One Area, Sheikh Zayed, Giza",
                        Lat = 30.0131,
                        Lng = 30.9804
                    },
                    CoverageArea = new CoverageArea
                    {
                        Type = CoverageAreaTypeEnum.Radius,
                        RadiusCenterLat = 30.0131,
                        RadiusCenterLng = 30.9804,
                        RadiusKm = 25.0
                    },
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Admin.Default
                }
            };

                await context.Stores.AddRangeAsync(stores, ct);
                await context.SaveChangesAsync(ct);
            }

          
              
        }
    }