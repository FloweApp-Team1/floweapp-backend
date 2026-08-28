namespace OrdersService.Infrastructure.Services
{
    public interface IPaymentSessionClient
    {
        Task<Shared.Results.Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
            Guid orderId, long amountTotalCents, string currency, CancellationToken cancellationToken);
    }

    public sealed record PaymentCheckoutSessionDto(
        string SessionId,
        string SessionUrl,
        Guid PaymentAttemptId,
        string SuccessUrl,
        string CancelUrl,
        DateTime? ExpiresAt);
}
