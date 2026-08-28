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

        
    }
}
