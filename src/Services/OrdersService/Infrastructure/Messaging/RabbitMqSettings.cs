namespace OrdersService.Infrastructure.Messaging
{
    public class RabbitMqSettings
    {
        public const string SectionName = "RabbitMq";

        // Leaving Host unset switches the service to the logging publisher, so the API still
        // runs end to end without a broker on a developer machine.
        public string? Host { get; set; }
        public string Username { get; set; } = "flowerapp";
        public string Password { get; set; } = "C#XDwuCVUTXe!OIc9lj3JnVm";
    }
}
