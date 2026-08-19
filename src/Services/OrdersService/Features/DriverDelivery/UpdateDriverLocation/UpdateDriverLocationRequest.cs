namespace OrdersService.Features.DriverDelivery.UpdateDriverLocation
{
    public class UpdateDriverLocationRequest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        // The device clock reading for the fix. Optional: pings queued while the driver was
        // offline can report when they were actually taken, and anything else falls back to
        // server time.
        public DateTime? RecordedAt { get; set; }
    }
}
