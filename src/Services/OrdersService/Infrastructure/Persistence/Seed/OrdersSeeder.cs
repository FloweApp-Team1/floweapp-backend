using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;

namespace OrdersService.Infrastructure.Persistence.Seed
{
    public static class OrdersSeeder
    {
        public const string SectionName = "OrdersSeed";

        private static readonly Guid DefaultCustomerId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
        private static readonly Guid DefaultOtherCustomerId = new("cccccccc-cccc-cccc-cccc-ffffffffffff");
        private static readonly Guid DefaultDriverId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
        private static readonly Guid DefaultSecondDriverId = new("dddddddd-dddd-dddd-dddd-eeeeeeeeeeee");

        private static readonly Guid StoreId = new("55555555-5555-5555-5555-555555555555");

        private const decimal DeliveryFee = 25.00m;

        public static async Task SeedAsync(
            OrdersDbContext context,
            IConfiguration configuration,
            CancellationToken ct = default)
        {
            var customerId = ConfiguredId(configuration, "CustomerId", DefaultCustomerId);
            var otherCustomerId = ConfiguredId(configuration, "OtherCustomerId", DefaultOtherCustomerId);

            var driver = new DriverSeed(
                ConfiguredId(configuration, "DriverId", DefaultDriverId),
                "Omar Hassan",
                "+201001234567",
                Placeholder("driver-omar-hassan"));

            var secondDriver = new DriverSeed(
                ConfiguredId(configuration, "SecondDriverId", DefaultSecondDriverId),
                "Youssef Adel",
                "+201007654321",
                Placeholder("driver-youssef-adel"));

            // Existing rows win: a seeded order that has since been claimed or advanced by
            // hand must not be reset on the next start.
            var existingNumbers = await context.Orders
                .IgnoreQueryFilters()
                .Select(o => o.OrderNumber)
                .ToListAsync(ct);

            var now = DateTime.UtcNow;

            foreach (var (order, location) in BuildOrders(now, customerId, otherCustomerId, driver, secondDriver))
            {
                if (existingNumbers.Contains(order.OrderNumber))
                    continue;

                context.Orders.Add(order);

                if (location is not null)
                    context.DriverLocations.Add(location);
            }

            await context.SaveChangesAsync(ct);
        }

        private static IEnumerable<(Order Order, DriverLocation? Location)> BuildOrders(
            DateTime now,
            Guid customerId,
            Guid otherCustomerId,
            DriverSeed driver,
            DriverSeed secondDriver)
        {
            // 1 - Freshly placed and unclaimed: the happy path for the claim endpoint, and
            // the "waiting for a driver" state on the tracking screen.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000001",
                number: "FLW-SEED-0001",
                userId: customerId,
                status: OrderStatusEnum.Placed,
                paymentMethod: PaymentMethodEnum.Cod,
                paymentStatus: PaymentStatusEnum.Pending,
                placedAt: now.AddMinutes(-5),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000001", "Classic Red Roses", 45.00m, 1)
                ],
                address: MaadiAddress,
                history: [(OrderStatusEnum.Placed, now.AddMinutes(-5), null)]);

            // 2 - Paid by card and already accepted by the store, still unclaimed. The other
            // claimable status, and the only seeded order that is Paid before delivery.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000002",
                number: "FLW-SEED-0002",
                userId: customerId,
                status: OrderStatusEnum.Preparing,
                paymentMethod: PaymentMethodEnum.Card,
                paymentStatus: PaymentStatusEnum.Paid,
                placedAt: now.AddMinutes(-25),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000003", "Spring Tulip Mix", 32.50m, 2),
                    new ItemSeed("77777777-0000-0000-0000-000000000010", "Petite Posy", 24.00m, 1)
                ],
                address: NasrCityAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddMinutes(-25), null),
                    (OrderStatusEnum.Preparing, now.AddMinutes(-18), null)
                ]);

            // 3 - Live delivery with a position reported seconds ago: the tracking screen
            // gets a marker and IsStale is false.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000003",
                number: "FLW-SEED-0003",
                userId: customerId,
                status: OrderStatusEnum.PickedUp,
                paymentMethod: PaymentMethodEnum.Cod,
                paymentStatus: PaymentStatusEnum.Pending,
                placedAt: now.AddMinutes(-55),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000005", "Pure White Lilies", 48.00m, 1)
                ],
                address: MaadiAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddMinutes(-55), null),
                    (OrderStatusEnum.Preparing, now.AddMinutes(-45), null),
                    (OrderStatusEnum.PickedUp, now.AddMinutes(-8), null)
                ],
                driver: driver,
                driverAssignedAt: now.AddMinutes(-12),
                location: new LocationSeed(30.0100, 31.2380, now.AddSeconds(-20), now.AddSeconds(-20)));

            // 4 - Same flow, but the last ping is well past DeliveryTracking's 90s staleness
            // threshold: the response still carries a position, flagged IsStale.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000004",
                number: "FLW-SEED-0004",
                userId: customerId,
                status: OrderStatusEnum.OutForDelivery,
                paymentMethod: PaymentMethodEnum.Card,
                paymentStatus: PaymentStatusEnum.Paid,
                placedAt: now.AddMinutes(-70),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000009", "Deluxe Mixed Bouquet", 75.00m, 1)
                ],
                address: ZamalekAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddMinutes(-70), null),
                    (OrderStatusEnum.Preparing, now.AddMinutes(-62), null),
                    (OrderStatusEnum.PickedUp, now.AddMinutes(-30), null),
                    (OrderStatusEnum.OutForDelivery, now.AddMinutes(-28), null)
                ],
                driver: driver,
                driverAssignedAt: now.AddMinutes(-34),
                location: new LocationSeed(29.9950, 31.2450, now.AddMinutes(-11), now.AddMinutes(-11)));

            // 5 - Claimed and out for delivery, but the driver has not pinged once: driver
            // card present, location null. Easy to break by assuming the two arrive together.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000005",
                number: "FLW-SEED-0005",
                userId: customerId,
                status: OrderStatusEnum.OutForDelivery,
                paymentMethod: PaymentMethodEnum.Cod,
                paymentStatus: PaymentStatusEnum.Pending,
                placedAt: now.AddMinutes(-40),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000008", "Sunshine Bouquet", 38.00m, 2)
                ],
                address: HeliopolisAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddMinutes(-40), null),
                    (OrderStatusEnum.Preparing, now.AddMinutes(-33), null),
                    (OrderStatusEnum.PickedUp, now.AddMinutes(-10), null),
                    (OrderStatusEnum.OutForDelivery, now.AddMinutes(-9), null)
                ],
                driver: secondDriver,
                driverAssignedAt: now.AddMinutes(-15));

            // 6 - Terminal success: a complete timeline, and a driver card that must keep
            // rendering after delivery even though live tracking is over.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000006",
                number: "FLW-SEED-0006",
                userId: customerId,
                status: OrderStatusEnum.Delivered,
                paymentMethod: PaymentMethodEnum.Card,
                paymentStatus: PaymentStatusEnum.Paid,
                placedAt: now.AddDays(-2),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000007", "Purple Orchid Grace", 65.00m, 1),
                    new ItemSeed("77777777-0000-0000-0000-000000000010", "Petite Posy", 24.00m, 2)
                ],
                address: MaadiAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddDays(-2), null),
                    (OrderStatusEnum.Preparing, now.AddDays(-2).AddMinutes(9), null),
                    (OrderStatusEnum.PickedUp, now.AddDays(-2).AddMinutes(41), null),
                    (OrderStatusEnum.OutForDelivery, now.AddDays(-2).AddMinutes(43), null),
                    (OrderStatusEnum.Delivered, now.AddDays(-2).AddMinutes(72), null)
                ],
                driver: driver,
                driverAssignedAt: now.AddDays(-2).AddMinutes(15));

            // 7 - Cancelled midway: the timeline has to show how far it actually got and
            // append Cancelled as terminal, rather than blanking the earlier stages.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000007",
                number: "FLW-SEED-0007",
                userId: customerId,
                status: OrderStatusEnum.Cancelled,
                paymentMethod: PaymentMethodEnum.Cod,
                paymentStatus: PaymentStatusEnum.Failed,
                placedAt: now.AddDays(-1),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000006", "Stargazer Lily Bunch", 44.00m, 1)
                ],
                address: NasrCityAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddDays(-1), null),
                    (OrderStatusEnum.Preparing, now.AddDays(-1).AddMinutes(12), null),
                    (OrderStatusEnum.Cancelled, now.AddDays(-1).AddMinutes(35), "Customer cancelled before pickup.")
                ]);

            // 8 - Gift order already claimed by the second driver: claiming it as the primary
            // seeded driver is the "another driver has already claimed this" conflict.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000008",
                number: "FLW-SEED-0008",
                userId: customerId,
                status: OrderStatusEnum.Preparing,
                paymentMethod: PaymentMethodEnum.Card,
                paymentStatus: PaymentStatusEnum.Paid,
                placedAt: now.AddMinutes(-20),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000002", "White Rose Elegance", 52.00m, 1)
                ],
                address: ZamalekAddress,
                history:
                [
                    (OrderStatusEnum.Placed, now.AddMinutes(-20), null),
                    (OrderStatusEnum.Preparing, now.AddMinutes(-14), null)
                ],
                driver: secondDriver,
                driverAssignedAt: now.AddMinutes(-11),
                gift: new GiftSeed("Mona Saeed", "+201112223334", "14 Brazil St, Zamalek, Cairo"));

            // 9 - Belongs to a different customer: unclaimed so any driver can take it, but
            // tracking it with the primary customer's token must come back Forbidden.
            yield return Build(
                id: "a0000000-0000-0000-0000-000000000009",
                number: "FLW-SEED-0009",
                userId: otherCustomerId,
                status: OrderStatusEnum.Placed,
                paymentMethod: PaymentMethodEnum.Cod,
                paymentStatus: PaymentStatusEnum.Pending,
                placedAt: now.AddMinutes(-3),
                items:
                [
                    new ItemSeed("77777777-0000-0000-0000-000000000004", "Yellow Tulip Cheer", 28.00m, 3)
                ],
                address: HeliopolisAddress,
                history: [(OrderStatusEnum.Placed, now.AddMinutes(-3), null)]);
        }

        private static (Order Order, DriverLocation? Location) Build(
            string id,
            string number,
            Guid userId,
            OrderStatusEnum status,
            PaymentMethodEnum paymentMethod,
            PaymentStatusEnum paymentStatus,
            DateTime placedAt,
            ItemSeed[] items,
            AddressSeed address,
            (OrderStatusEnum Status, DateTime At, string? Note)[] history,
            DriverSeed? driver = null,
            DateTime? driverAssignedAt = null,
            LocationSeed? location = null,
            GiftSeed? gift = null)
        {
            var orderId = new Guid(id);
            var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
            var lastChangeAt = history.Max(h => h.At);

            var order = new Order
            {
                Id = orderId,
                OrderNumber = number,
                UserId = userId,
                StoreId = StoreId,
                AddressId = address.AddressId,
                Status = status,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                Subtotal = subtotal,
                DeliveryFee = DeliveryFee,
                Total = subtotal + DeliveryFee,
                IsGift = gift is not null,
                GiftRecipientName = gift?.RecipientName,
                GiftRecipientPhone = gift?.RecipientPhone,
                GiftRecipientAddress = gift?.Address,
                DriverId = driver?.Id,
                DriverName = driver?.Name,
                DriverPhone = driver?.Phone,
                DriverImageUrl = driver?.ImageUrl,
                DriverAssignedAt = driverAssignedAt,
                CreatedAt = placedAt,
                UpdatedAt = lastChangeAt,
                LastChangedBy = driver?.Id ?? userId,
                Items = items
                    .Select((item, index) => new OrderItem
                    {
                        Id = ChildId(orderId, 0x10 + index),
                        OrderId = orderId,
                        ProductId = new Guid(item.ProductId),
                        ProductName = item.Name,
                        ProductImageUrl = Placeholder(item.Name),
                        UnitPrice = item.UnitPrice,
                        Quantity = item.Quantity,
                        CreatedAt = placedAt,
                        UpdatedAt = placedAt,
                        LastChangedBy = userId
                    })
                    .ToList(),
                AddressSnapshot = new OrderAddressSnapshot
                {
                    Id = ChildId(orderId, 0x20),
                    OrderId = orderId,
                    RecipientName = address.RecipientName,
                    RecipientPhone = address.RecipientPhone,
                    AddressLine = address.AddressLine,
                    City = address.City,
                    Area = address.Area,
                    Lat = address.Lat,
                    Lng = address.Lng,
                    CreatedAt = placedAt,
                    UpdatedAt = placedAt,
                    LastChangedBy = userId
                },
                StatusHistory = history
                    .Select((entry, index) => new OrderStatusHistory
                    {
                        Id = ChildId(orderId, 0x30 + index),
                        OrderId = orderId,
                        Status = entry.Status,
                        OccurredAt = entry.At,
                        // Everything from pickup onwards is the driver's doing; the earlier
                        // stages belong to the customer and the store.
                        ChangedBy = entry.Status is OrderStatusEnum.PickedUp
                                                  or OrderStatusEnum.OutForDelivery
                                                  or OrderStatusEnum.Delivered
                            ? driver?.Id
                            : userId,
                        Note = entry.Note,
                        CreatedAt = entry.At,
                        UpdatedAt = entry.At,
                        LastChangedBy = driver?.Id ?? userId
                    })
                    .ToList()
            };

            DriverLocation? driverLocation = location is null || driver is null
                ? null
                : new DriverLocation
                {
                    Id = ChildId(orderId, 0x40),
                    OrderId = orderId,
                    DriverId = driver.Id,
                    Lat = location.Lat,
                    Lng = location.Lng,
                    RecordedAt = location.RecordedAt,
                    LastBroadcastAt = location.LastBroadcastAt,
                    CreatedAt = location.RecordedAt,
                    UpdatedAt = location.RecordedAt,
                    LastChangedBy = driver.Id
                };

            return (order, driverLocation);
        }

        private static Guid ChildId(Guid orderId, int discriminator)
        {
            var bytes = orderId.ToByteArray();
            bytes[^2] = (byte)discriminator;
            return new Guid(bytes);
        }

        private static Guid ConfiguredId(IConfiguration configuration, string key, Guid fallback) =>
            Guid.TryParse(configuration[$"{SectionName}:{key}"], out var parsed) ? parsed : fallback;

        private static string Placeholder(string name) =>
            $"https://picsum.photos/seed/{Uri.EscapeDataString(name.ToLowerInvariant().Replace(' ', '-'))}/600/600";

        // Destinations sit around Cairo so the seeded driver positions fall between the
        // store and the address rather than on opposite sides of the map.
        private static readonly AddressSeed MaadiAddress = new(
            new Guid("bbbbbbbb-0000-0000-0000-000000000001"),
            "Nour Ibrahim", "+201234567890",
            "27 Road 9, Maadi", "Cairo", "Maadi", 29.9602, 31.2569);

        private static readonly AddressSeed NasrCityAddress = new(
            new Guid("bbbbbbbb-0000-0000-0000-000000000002"),
            "Salma Farouk", "+201098765432",
            "8 Abbas El Akkad St, Nasr City", "Cairo", "Nasr City", 30.0561, 31.3389);

        private static readonly AddressSeed ZamalekAddress = new(
            new Guid("bbbbbbbb-0000-0000-0000-000000000003"),
            "Karim Nabil", "+201155667788",
            "14 Brazil St, Zamalek", "Cairo", "Zamalek", 30.0626, 31.2197);

        private static readonly AddressSeed HeliopolisAddress = new(
            new Guid("bbbbbbbb-0000-0000-0000-000000000004"),
            "Hana Mostafa", "+201033445566",
            "5 El Higaz St, Heliopolis", "Cairo", "Heliopolis", 30.0875, 31.3260);

        private sealed record ItemSeed(string ProductId, string Name, decimal UnitPrice, int Quantity);

        private sealed record DriverSeed(Guid Id, string Name, string Phone, string ImageUrl);

        private sealed record LocationSeed(double Lat, double Lng, DateTime RecordedAt, DateTime? LastBroadcastAt);

        private sealed record GiftSeed(string RecipientName, string RecipientPhone, string Address);

        private sealed record AddressSeed(
            Guid AddressId,
            string RecipientName,
            string RecipientPhone,
            string AddressLine,
            string City,
            string Area,
            double Lat,
            double Lng);
    }
}
