using global::AddressCartService.Domain.Entities;
using global::AddressCartService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

    namespace AddressCartService.Infrastructure.Persistence.Seed
    {
        public static class StoreAndAddressSeeder
        {
            public static async Task SeedAsync(AddressCartDbContext context, CancellationToken ct = default)
            {
                await SeedStoresAsync(context, ct);
                await SeedAddressesAsync(context, ct);
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

            private static async Task SeedAddressesAsync(AddressCartDbContext context, CancellationToken ct)
            {
                if (await context.Addresses.AnyAsync(ct))
                    return;

                var now = DateTime.UtcNow;

                var addresses = new List<Address>
            {
                // 1. Default -  1
                new()
                {
                    Id = SeedIds.Addresses.Maadi,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Nour Ibrahim",
                    RecipientPhone = "+201234567890",
                    AddressLine = "27 Road 9, Maadi",
                    GovernorateId = 1,
                    CityId = 3,
                    Area = "Maadi",
                    Label = "Home",
                    Lat = 29.9602,
                    Lng = 31.2569,
                    IsDefault = true,
                    StoreId = SeedIds.Stores.CairoMainRadius,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                },

                // 2. 1
                new()
                {
                    Id = SeedIds.Addresses.NasrCity,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Salma Farouk",
                    RecipientPhone = "+201098765432",
                    AddressLine = "8 Abbas El Akkad St, Nasr City",
                    GovernorateId = 1,
                    CityId = 3,
                    Area = "Nasr City",
                    Label = "Work",
                    Lat = 30.0561,
                    Lng = 31.3389,
                    IsDefault = false,
                    StoreId = SeedIds.Stores.CairoMainRadius,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                },

                // 3. 1
                new()
                {
                    Id = SeedIds.Addresses.Zamalek,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Karim Nabil",
                    RecipientPhone = "+201155667788",
                    AddressLine = "14 Brazil St, Zamalek",
                    GovernorateId = 1,
                    CityId = 3,
                    Area = "Zamalek",
                    Label = "Family",
                    Lat = 30.0626,
                    Lng = 31.2197,
                    IsDefault = false,
                    StoreId = SeedIds.Stores.CairoMainRadius,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                },

                // 4. 1
                new()
                {
                    Id = SeedIds.Addresses.Heliopolis,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Hana Mostafa",
                    RecipientPhone = "+201033445566",
                    AddressLine = "5 El Higaz St, Heliopolis",
                    GovernorateId = 1,
                    CityId = 3,
                    Area = "Heliopolis",
                    Label = "Other",
                    Lat = 30.0875,
                    Lng = 31.3260,
                    IsDefault = false,
                    StoreId = SeedIds.Stores.CairoMainRadius,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                },

                // 5. 2
                new()
                {
                    Id = SeedIds.Addresses.Tagamoa,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Sara Maged (Tagamoa)",
                    RecipientPhone = "+201041349296",
                    AddressLine = "Building 12, South 90th St",
                    GovernorateId = 1,
                    CityId = 3,
                    Area = "New Cairo",
                    Label = "Office",
                    Lat = 30.0254,
                    Lng = 31.4789,
                    IsDefault = false,
                    StoreId = SeedIds.Stores.NewCairoAreaList,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                },

                // 6. 3
                new()
                {
                    Id = SeedIds.Addresses.Alexandria,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Ahmed Alex",
                    RecipientPhone = "+201122334455",
                    AddressLine = "Stanley Bridge, Corniche",
                    GovernorateId = 2,
                    CityId = 10,
                    Area = "Stanley",
                    Label = "Beach House",
                    Lat = 31.2333,
                    Lng = 29.9500,
                    IsDefault = false,
                    StoreId = SeedIds.Stores.AlexandriaPolygon,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                },

                // 7. 4
                new()
                {
                    Id = SeedIds.Addresses.Zayed,
                    UserId = SeedIds.Customers.Default,
                    RecipientName = "Hesham Zayed",
                    RecipientPhone = "+201099887766",
                    AddressLine = "Beverly Hills, Sheikh Zayed",
                    GovernorateId = 1,
                    CityId = 3,
                    Area = "Sheikh Zayed",
                    Label = "Villa",
                    Lat = 30.0131,
                    Lng = 30.9804,
                    IsDefault = false,
                    StoreId = SeedIds.Stores.GizaZayedRadius,
                    IsServiceable = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    LastChangedBy = SeedIds.Customers.Default
                }
            };

                await context.Addresses.AddRangeAsync(addresses, ct);
                await context.SaveChangesAsync(ct);
            }
        }
    }