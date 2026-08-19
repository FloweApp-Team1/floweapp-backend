namespace OrdersService.Features.DriverDelivery.Common
{
    public static class GeoCalculator
    {
        private const double EarthRadiusMeters = 6_371_000d;

        // Haversine great-circle distance. Only used to decide whether a ping moved far
        // enough to be worth a silent push, so metre-level accuracy is more than enough.
        public static double DistanceInMeters(double fromLat, double fromLng, double toLat, double toLng)
        {
            var dLat = ToRadians(toLat - fromLat);
            var dLng = ToRadians(toLng - fromLng);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(ToRadians(fromLat)) * Math.Cos(ToRadians(toLat))
                    * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            return EarthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180d;
    }
}
