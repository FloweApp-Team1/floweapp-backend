namespace OrdersService.Infrastructure.Services
{
    public interface IPaymentMethodProvider
    {
        Task<IReadOnlyList<PaymentMethodOption>> GetAvailableMethodsAsync(CancellationToken cancellationToken);
    }

    public sealed record PaymentMethodOption(string Method, IReadOnlyList<string>? Gateways = null);

    public class StaticPaymentMethodProvider : IPaymentMethodProvider
    {
        public Task<IReadOnlyList<PaymentMethodOption>> GetAvailableMethodsAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<PaymentMethodOption> methods =
            [
                new PaymentMethodOption("COD"),
                new PaymentMethodOption("Card", ["Stripe"])
            ];

            return Task.FromResult(methods);
        }
    }
}
