using OrdersService.Infrastructure.Services;
using Shared.Responses;
using Shared.Results;
using System.Net;
using System.Text.Json;
namespace OrdersService.Infrastructure.Clients
{
    public class HttpAddressServiceClient : IAddressServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpAddressServiceClient> _logger;

        public HttpAddressServiceClient(IHttpClientFactory httpClientFactory, ILogger<HttpAddressServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AddressCartServiceClient");
            _logger = logger;
        }

        public async Task<Result<OrderAddressDetails>> GetAddressForOrderAsync(
            Guid addressId, Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/me/addresses/{addressId}", cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return Result.Failure<OrderAddressDetails>(Error.New("Address.NotFound", "Address not found."));

                response.EnsureSuccessStatusCode();

                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<AddressPayload>>(
                    cancellationToken: cancellationToken);

                if (envelope?.Data is null)
                    return Result.Failure<OrderAddressDetails>(
                        Error.New("Address.ProviderError", "Unexpected response from address service."));

                return Result.Success(Map(envelope.Data));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "Failed to fetch address {AddressId} from AddressCartService", addressId);
                return Result.Failure<OrderAddressDetails>(
                    Error.New("Address.ProviderError", "Address service is temporarily unavailable."));
            }
        }


        public async Task<Result<OrderAddressDetails?>> GetDefaultOrLastUsedAddressAsync(
            Guid userId, CancellationToken cancellationToken)
        {
            try
            {
        
                var response = await _httpClient.GetAsync("/addresses", cancellationToken);
                response.EnsureSuccessStatusCode();

                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<List<AddressPayload>>>(
                    cancellationToken: cancellationToken);

                var first = envelope?.Data?.FirstOrDefault();

                return Result.Success(first is null ? null : Map(first));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "Failed to fetch default address for user {UserId} from AddressCartService", userId);
                return Result.Failure<OrderAddressDetails?>(
                    Error.New("Address.ProviderError", "Address service is temporarily unavailable."));
            }
        }

        private static OrderAddressDetails Map(AddressPayload p) => new(
            p.Id, p.RecipientName, p.RecipientPhone, p.AddressLine,
            p.GovernorateId, p.GovernorateName, p.CityId, p.CityName, p.Area,
            p.Lat, p.Lng, p.StoreId, p.IsServiceable);

      
        private sealed record AddressPayload(
            Guid Id,
            string RecipientName,
            string RecipientPhone,
            string AddressLine,
            int GovernorateId,
            string GovernorateName,
            int CityId,
            string CityName,
            string Area,
            string? Label,
            double? Lat,
            double? Lng,
            bool IsDefault,
            bool IsServiceable,
            Guid? StoreId,
            DateTime CreatedAt,
            DateTime UpdatedAt);
    }
}