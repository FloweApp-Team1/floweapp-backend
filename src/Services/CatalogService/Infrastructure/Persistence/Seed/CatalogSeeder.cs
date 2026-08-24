using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;

namespace CatalogService.Infrastructure.Persistence.Seed
{

    public static class CatalogSeeder
    {
        public static async Task SeedAsync(CatalogDbContext context, CancellationToken ct = default)
        {
            var categories = await SeedCategoriesAsync(context, ct);
            var occasions = await SeedOccasionsAsync(context, ct);

            await SeedProductsAsync(context, categories, occasions, ct);

            await SeedHomeLayoutSectionsAsync(context, ct);

            await context.SaveChangesAsync(ct);
        }

        // ---------- Categories ----------

        private static readonly CategorySeed[] CategorySeeds =
        [
            new("11111111-1111-1111-1111-000000000001", "Roses",         1, false),
            new("11111111-1111-1111-1111-000000000002", "Tulips",        2, false),
            new("11111111-1111-1111-1111-000000000003", "Lilies",        3, false),
            new("11111111-1111-1111-1111-000000000004", "Orchids",       4, false),
            new("11111111-1111-1111-1111-000000000005", "Sunflowers",    5, false),
            new("11111111-1111-1111-1111-000000000006", "Bouquets",      6, false),
            // Intentionally left without products: exercises the "200 + empty array"
            // case for GET /products?categoryId=.
            new("11111111-1111-1111-1111-000000000007", "Plants",        7, false),
            // Archived: must not appear in GET /categories, but a deep link to this
            // id has to render "no longer available" rather than crash.
            new("11111111-1111-1111-1111-000000000008", "Dried Flowers", 8, true),
        ];

        private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(
            CatalogDbContext context, CancellationToken ct)
        {

            var existing = await context.Categories
                .IgnoreQueryFilters()
                .ToDictionaryAsync(c => c.Name, ct);

            foreach (var seed in CategorySeeds)
            {
                if (existing.ContainsKey(seed.Name))
                    continue;

                var category = new Category
                {
                    Id = new Guid(seed.Id),
                    Name = seed.Name,
                    IconUrl = Placeholder(seed.Name, 200),
                    DisplayOrder = seed.DisplayOrder,
                    IsDeleted = seed.IsDeleted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Categories.Add(category);
                existing[seed.Name] = category;
            }

            return existing;
        }

        // ---------- Occasions ----------

        private static readonly OccasionSeed[] OccasionSeeds =
        [
            new("22222222-2222-2222-2222-000000000001", "Birthday",      1),
            new("22222222-2222-2222-2222-000000000002", "Anniversary",   2),
            new("22222222-2222-2222-2222-000000000003", "Wedding",       3),
            new("22222222-2222-2222-2222-000000000004", "Graduation",    4),
            new("22222222-2222-2222-2222-000000000005", "Sympathy",      5),
            new("22222222-2222-2222-2222-000000000006", "Get Well Soon", 6),
            new("22222222-2222-2222-2222-000000000007", "Thank You",     7),
        ];

        private static async Task<Dictionary<string, Occasion>> SeedOccasionsAsync(
            CatalogDbContext context, CancellationToken ct)
        {
            var existing = await context.Occasions
                .IgnoreQueryFilters()
                .ToDictionaryAsync(o => o.Name, ct);

            foreach (var seed in OccasionSeeds)
            {
                if (existing.ContainsKey(seed.Name))
                    continue;

                var occasion = new Occasion
                {
                    Id = new Guid(seed.Id),
                    Name = seed.Name,
                    ImageUrl = Placeholder(seed.Name, 400),
                    DisplayOrder = seed.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Occasions.Add(occasion);
                existing[seed.Name] = occasion;
            }

            return existing;
        }

        // ---------- Products ----------

        private static readonly ProductSeed[] ProductSeeds =
        [
            new("Classic Red Roses",
                "A dozen long-stemmed red roses hand-tied with eucalyptus and finished in kraft wrap.",
                45.00m, 25, "Roses", ["Anniversary", "Birthday"]),

            new("White Rose Elegance",
                "Eighteen ivory roses arranged with baby's breath - a quiet, formal statement.",
                52.00m, 15, "Roses", ["Wedding", "Sympathy"]),

            new("Spring Tulip Mix",
                "Twenty seasonal tulips in mixed pastels, cut fresh the morning they ship.",
                32.50m, 40, "Tulips", ["Birthday", "Thank You"]),

            new("Yellow Tulip Cheer",
                "Fifteen bright yellow tulips in a clear glass vase.",
                28.00m, 30, "Tulips", ["Get Well Soon", "Graduation"]),

            new("Pure White Lilies",
                "Six stems of white oriental lilies, delivered in bud so they open over several days.",
                48.00m, 18, "Lilies", ["Sympathy", "Wedding"]),

            new("Stargazer Lily Bunch",
                "Pink stargazer lilies with a deep fragrance, wrapped in soft tissue.",
                44.00m, 12, "Lilies", ["Anniversary"]),

            new("Purple Orchid Grace",
                "A twin-stem phalaenopsis orchid in a ceramic pot - lasts weeks with weekly watering.",
                65.00m, 12, "Orchids", ["Anniversary", "Thank You"]),

            new("Sunshine Bouquet",
                "Five large sunflowers with seasonal greenery in a rustic tie.",
                38.00m, 22, "Sunflowers", ["Birthday", "Get Well Soon"]),

            new("Deluxe Mixed Bouquet",
                "Our florist's choice of the day's best stems, built to a generous size.",
                75.00m, 10, "Bouquets", ["Wedding", "Anniversary", "Graduation"]),

            new("Petite Posy",
                "A small hand-tied posy of seasonal blooms - the everyday thank-you.",
                24.00m, 35, "Bouquets", ["Thank You", "Birthday"]),

            // Sits under the archived category on purpose, so the archived deep-link
            // path has real data behind it.
            new("Everlasting Lavender",
                "Naturally dried lavender bundled with twine. Keeps its scent for months.",
                19.50m, 50, "Dried Flowers", ["Thank You"]),
        ];

        private static async Task SeedProductsAsync(
            CatalogDbContext context,
            Dictionary<string, Category> categories,
            Dictionary<string, Occasion> occasions,
            CancellationToken ct)
        {
            var existingNames = await context.Products
                .IgnoreQueryFilters()
                .Select(p => p.Name)
                .ToListAsync(ct);

            foreach (var seed in ProductSeeds)
            {
                if (existingNames.Contains(seed.Name))
                    continue;

                if (!categories.TryGetValue(seed.CategoryName, out var category))
                    continue;

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = seed.Name,
                    Description = seed.Description,
                    Price = seed.Price,
                    StockQuantity = seed.StockQuantity,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Occasions = seed.OccasionNames
                        .Where(occasions.ContainsKey)
                        .Select(name => occasions[name])
                        .ToList(),
                    ProductImages =
                    [
                        NewImage(seed.Name, variant: 1, isPrimary: true),
                        NewImage(seed.Name, variant: 2, isPrimary: false)
                    ]
                };

                context.Products.Add(product);
            }

            var random = new Random(42);
            var categoryList = categories.Values.ToList();
            var occasionList = occasions.Values.ToList();

            for (int i = 1; i <= 100; i++)
            {
                var name = $"Test Pagination Product {i}";
                if (existingNames.Contains(name))
                    continue;

                var category = categoryList[random.Next(categoryList.Count)];
                var productOccasions = new List<Occasion> { occasionList[random.Next(occasionList.Count)] };
                if (random.Next(2) == 0)
                {
                    var secondOccasion = occasionList[random.Next(occasionList.Count)];
                    if (secondOccasion.Id != productOccasions[0].Id)
                        productOccasions.Add(secondOccasion);
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = $"This is an auto-generated product #{i} designed specifically to test pagination.",
                    Price = random.Next(10, 100) + 0.99m,
                    StockQuantity = random.Next(10, 500),
                    CategoryId = category.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Occasions = productOccasions,
                    ProductImages =
                    [
                        NewImage(name, variant: 1, isPrimary: true),
                        NewImage(name, variant: 2, isPrimary: false)
                    ]
                };

                context.Products.Add(product);
            }
        }

        private static ProductImage NewImage(string productName, int variant, bool isPrimary) => new()
        {
            Id = Guid.NewGuid(),
            ImageUrl = Placeholder($"{productName}-{variant}", 600),
            IsPrimary = isPrimary,
            DisplayOrder = variant,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private static string Placeholder(string name, int size) =>
            $"https://picsum.photos/seed/{Uri.EscapeDataString(name.ToLowerInvariant().Replace(' ', '-'))}/{size}/{size}";

        private sealed record CategorySeed(string Id, string Name, int DisplayOrder, bool IsDeleted);

        private sealed record OccasionSeed(string Id, string Name, int DisplayOrder);

        private sealed record ProductSeed(
            string Name,
            string Description,
            decimal Price,
            int StockQuantity,
            string CategoryName,
            string[] OccasionNames);

        // ---------- Home Layout Sections ----------

        private static async Task SeedHomeLayoutSectionsAsync(CatalogDbContext context, CancellationToken ct)
        {
            var anyLayouts = await context.HomeLayoutSections.AnyAsync(ct);
            if (anyLayouts)
                return;

            var bannerSection = new HomeLayoutSection
            {
                Id = Guid.NewGuid(),
                Title = "Spring Sale",
                Type = Domain.Enums.HomeSectionType.Banner,
                Order = 1,
                IsEnabled = true,
                Payload = new BannerPayload
                {
                    ImageUrl = Placeholder("Spring Sale Banner", 1200),
                    ClickAction = "flowerapp://promotions?id=spring"
                }
            };

            var categorySection = new HomeLayoutSection
            {
                Id = Guid.NewGuid(),
                Title = "Categories",
                Type = Domain.Enums.HomeSectionType.CategoryRail,
                Order = 2,
                IsEnabled = true,
                Payload = new CategoryRailPayload
                {
                    Count = 5,
                    ViewAllAction = AppDeepLinks.Categories
                }
            };

            var bestSellerSection = new HomeLayoutSection
            {
                Id = Guid.NewGuid(),
                Title = "Best Sellers",
                Type = Domain.Enums.HomeSectionType.ProductRail,
                Order = 3,
                IsEnabled = true,
                Payload = new ProductRailPayload
                {
                    Count = 5,
                    ViewAllAction = AppDeepLinks.BestSellers
                }
            };

            var occasionSection = new HomeLayoutSection
            {
                Id = Guid.NewGuid(),
                Title = "Shop By Occasion",
                Type = Domain.Enums.HomeSectionType.OccasionRail,
                Order = 4,
                IsEnabled = true,
                Payload = new OccasionRailPayload
                {
                    Count = 5,
                    ViewAllAction = AppDeepLinks.Occasions
                }
            };

            context.HomeLayoutSections.AddRange(bannerSection, categorySection, bestSellerSection, occasionSection);
        }
    }
}
