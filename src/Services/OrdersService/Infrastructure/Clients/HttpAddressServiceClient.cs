 using global::OrdersService.Infrastructure.Services;
 using Shared.Responses;
 using Shared.Results;
 using System.Net;
 using System.Net.Http.Json;
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

            public async Task<Result<OrderAddressDetails>> CreateAddressForOrderAsync(
                Guid userId, NewAddressRequestPayload newAddress, CancellationToken cancellationToken)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("/users/me/addresses", newAddress, cancellationToken);
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
                    _logger.LogError(ex, "Failed to create address for user {UserId} via AddressCartService", userId);
                    return Result.Failure<OrderAddressDetails>(
                        Error.New("Address.ProviderError", "Address service is temporarily unavailable."));
                }
            }

            public async Task<Result<OrderAddressDetails?>> GetDefaultOrLastUsedAddressAsync(
             Guid userId, CancellationToken cancellationToken)
            {
                try
            {
                     var response = await _httpClient.GetAsync("/users/me/addresses", cancellationToken);
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
                p.Id, p.RecipientName, p.RecipientPhone, p.AddressLine, p.City, p.Area, p.Lat, p.Lng, p.StoreId, p.IsServiceable);

          
            private sealed record AddressPayload(
                Guid Id, string RecipientName, string RecipientPhone, string AddressLine, string City, string Area,
                string? Label, double? Lat, double? Lng, bool IsDefault, bool IsServiceable, Guid? StoreId,
                DateTime CreatedAt, DateTime UpdatedAt);
        }

        
        public sealed record NewAddressRequestPayload(
            string RecipientName, string RecipientPhone, string AddressLine, string City, string Area,
            double? Lat, double? Lng);
    }
