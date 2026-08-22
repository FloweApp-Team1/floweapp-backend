
    using global::OrdersService.Infrastructure.Services;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;

    namespace OrdersService.Infrastructure.Clients
    {
       
        public class HttpCartServiceClient : ICartServiceClient
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<HttpCartServiceClient> _logger;

            public HttpCartServiceClient(IHttpClientFactory httpClientFactory, ILogger<HttpCartServiceClient> logger)
            {
                _httpClient = httpClientFactory.CreateClient("AddressCartServiceClient");
                _logger = logger;
            }

            public async Task<CartDetailsDto?> GetCartAsync(Guid cartId, Guid userId, CancellationToken cancellationToken)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"/users/me/carts/{cartId}", cancellationToken);

                    if (response.StatusCode == HttpStatusCode.NotFound)
                        return null;

                    response.EnsureSuccessStatusCode();

                    var envelope = await response.Content.ReadFromJsonAsync<Shared.Responses.ApiResponse<CartPayload>>(
                        cancellationToken: cancellationToken);

                    if (envelope?.Data is null)
                        return null;

                    return new CartDetailsDto(
                        envelope.Data.Id,
                        envelope.Data.Items.Select(i => new CartItemDto(i.ProductId, i.Quantity)).ToList());
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    _logger.LogError(ex, "Failed to fetch cart {CartId} from AddressCartService", cartId);
                    return null;
                }
            }

            private sealed record CartPayload(Guid Id, List<CartItemPayload> Items);
            private sealed record CartItemPayload(Guid ProductId, int Quantity);
        }
    }

