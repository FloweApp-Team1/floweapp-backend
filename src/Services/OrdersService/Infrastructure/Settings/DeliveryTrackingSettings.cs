namespace OrdersService.Infrastructure.Settings
{
    // Tunes how driver pings turn into customer-visible movement. Bound from the
    // "DeliveryTracking" configuration section; every value has a working default so the
    // service runs without extra environment variables.
    public class DeliveryTrackingSettings
    {
        public const string SectionName = "DeliveryTracking";

        // A position older than this is reported with IsStale = true so the tracking screen
        // can warn that the marker may be out of date instead of presenting it as current.
        public int StalenessThresholdSeconds { get; set; } = 90;

        // How long the latest position stays readable from Redis. Deliberately far longer
        // than the staleness threshold: an expired key would make a stale-but-known position
        // look like "no location at all", which is a worse answer for the client.
        public int CacheTtlMinutes { get; set; } = 30;

        // A ping that moves the driver less than this and arrives sooner than
        // MinimumBroadcastIntervalSeconds is still persisted, but does not wake the
        // customer's device with a silent push.
        public double MinimumBroadcastDistanceMeters { get; set; } = 25;

        public int MinimumBroadcastIntervalSeconds { get; set; } = 20;

        // Rejects pings whose client-supplied RecordedAt is implausible - too far in the
        // past to be useful, or ahead of server time by more than ordinary clock skew.
        public int MaximumPingAgeMinutes { get; set; } = 15;

        public int MaximumPingClockSkewSeconds { get; set; } = 60;
    }
}
