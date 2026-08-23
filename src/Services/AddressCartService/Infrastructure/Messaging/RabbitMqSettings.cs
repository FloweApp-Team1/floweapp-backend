namespace AddressCartService.Infrastructure.Messaging
{
    public class RabbitMqSettings
    {
        public const string SectionName = "RabbitMq";

        public string? Host { get; set; }
        public string Username { get; set; } = "floweapp";
        public string Password { get; set; } = "C#XDwuCVUTXe!OIc9lj3JnVm";
    }
}
