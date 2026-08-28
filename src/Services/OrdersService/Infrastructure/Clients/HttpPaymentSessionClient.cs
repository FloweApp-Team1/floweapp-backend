using OrdersService.Infrastructure.Services;
using Shared.Results;
using System.Text.Json;

namespace OrdersService.Infrastructure.Clients
{
    
    public class HttpPaymentSessionClient : IPaymentSessionClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpPaymentSessionClient> _logger;

        public HttpPaymentSessionClient(IHttpClientFactory httpClientFactory, ILogger<HttpPaymentSessionClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("PaymentServiceClient");
            _logger = logger;
        }

        public async Task<Result<PaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
            Guid orderId, long amountTotalCents, string currency, CancellationToken cancellationToken)
        {
            try
            {
                var checkoutRequest = new { OrderId = orderId, AmountTotal = amountTotalCents, Currency = currency };

                var response = await _httpClient.PostAsJsonAsync("/checkout", checkoutRequest, cancellationToken);
                response.EnsureSuccessStatusCode();

                var envelope = await response.Content.ReadFromJsonAsync<PaymentResultDto<CreateCheckoutSessionPayload>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

                if (envelope is not { IsSuccess: true, Value: not null })
                {
                    _logger.LogWarning("PaymentService returned an unsuccessful result for order {OrderId}", orderId);
                    return Result.Failure<PaymentCheckoutSessionDto>(
                        Error.New("Payment.ProviderError", "Payment provider could not create a checkout session."));
                }

                var value = envelope.Value;

                return Result.Success(new PaymentCheckoutSessionDto(
                    value.StripeSessionId,
                    value.CheckoutUrl,
                    value.PaymentAttemptId,
                    value.SuccessUrl??string.Empty ,
                    value.CancelUrl?? string.Empty,
                    value.ExpiresAt?.UtcDateTime));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "Failed to create checkout session with PaymentService for order {OrderId}", orderId);
                return Result.Failure<PaymentCheckoutSessionDto>(
                    Error.New("Payment.ProviderError", "Payment service is temporarily unavailable."));
            }
        }

        private sealed class PaymentResultDto<T>
        {
            public bool IsSuccess { get; set; }
            public T? Value { get; set; }
        }

       
        private sealed class CreateCheckoutSessionPayload
        {
            public string CheckoutUrl { get; set; } = string.Empty;
            public string StripeSessionId { get; set; } = string.Empty;
            public Guid PaymentAttemptId { get; set; }
            public string? SuccessUrl { get; set; }
            public string? CancelUrl { get; set; }
            public DateTimeOffset? ExpiresAt { get; set; }
        }
    }
}
