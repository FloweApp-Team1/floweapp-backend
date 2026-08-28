namespace AddressCartService.Infrastructure.Persistence.Seed
{
    public static class SeedIds
    {
        public static class Customers
        {
          
            public static readonly Guid Default = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        }

        public static class Admin
        {
            public static readonly Guid Default = Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        public static class Stores
        {
            public static readonly Guid CairoMainRadius = Guid.Parse("55555555-5555-5555-5555-555555555555");
            public static readonly Guid NewCairoAreaList = Guid.Parse("66666666-6666-6666-6666-666666666666");
            public static readonly Guid AlexandriaPolygon = Guid.Parse("77777777-7777-7777-7777-777777777777");
            public static readonly Guid GizaZayedRadius = Guid.Parse("88888888-8888-8888-8888-888888888888");
        }

        public static class Addresses
        {
            public static readonly Guid Maadi = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
            public static readonly Guid NasrCity = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");
            public static readonly Guid Zamalek = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003");
            public static readonly Guid Heliopolis = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004");
            public static readonly Guid Tagamoa = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005");
            public static readonly Guid Alexandria = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000006");
            public static readonly Guid Zayed = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000007");
        }
    }
}
