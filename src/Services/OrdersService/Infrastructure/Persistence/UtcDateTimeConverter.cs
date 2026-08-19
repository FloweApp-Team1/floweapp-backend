using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OrdersService.Infrastructure.Persistence
{
    // SQL Server's datetime2 carries no offset, so EF hands back DateTimeKind.Unspecified and
    // System.Text.Json then serialises timestamps without a trailing "Z". A tracking client
    // reading "recordedAt" would parse that as local time and mis-judge how old the driver's
    // position is, so every DateTime this context reads is re-stamped as UTC on the way out.
    public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(
                write => write.Kind == DateTimeKind.Local ? write.ToUniversalTime() : write,
                read => DateTime.SpecifyKind(read, DateTimeKind.Utc))
        {
        }
    }

    public class NullableUtcDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public NullableUtcDateTimeConverter()
            : base(
                write => write.HasValue && write.Value.Kind == DateTimeKind.Local
                    ? write.Value.ToUniversalTime()
                    : write,
                read => read.HasValue
                    ? DateTime.SpecifyKind(read.Value, DateTimeKind.Utc)
                    : read)
        {
        }
    }
}
