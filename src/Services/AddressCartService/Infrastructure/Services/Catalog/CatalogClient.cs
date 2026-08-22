using System.Net.Http.Json;
using System.Text.Json;
using AddressCartService.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Shared.Responses;

namespace AddressCartService.Infrastructure.Services.Catalog
{
    public class CatalogClient : ICatalogClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogClient> _logger;

        public CatalogClient(
            HttpClient httpClient,
            IOptions<CatalogSettings> settings,
            ILogger<CatalogClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            if (!string.IsNullOrWhiteSpace(settings.Value.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(settings.Value.BaseUrl.TrimEnd('/') + "/");
            }
            _httpClient.Timeout = TimeSpan.FromSeconds(settings.Value.TimeoutSeconds > 0 ? settings.Value.TimeoutSeconds : 10);
        }

        public async Task<CatalogProductDto?> GetProductByIdAsync(
            Guid productId, Guid? storeId = null, CancellationToken cancellationToken = default)
        {
            var dict = await GetProductsBatchAsync([productId], storeId, cancellationToken);
            return dict.TryGetValue(productId, out var product) ? product : null;
        }

        public async Task<IReadOnlyDictionary<Guid, CatalogProductDto>> GetProductsBatchAsync(
            IEnumerable<Guid> productIds, Guid? storeId = null, CancellationToken cancellationToken = default)
        {
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, CatalogProductDto>();

            try
            {
                var payload = new { ProductIds = ids, StoreId = storeId };
                var response = await _httpClient.PostAsJsonAsync("products/batch", payload, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CatalogService batch request returned HTTP status {StatusCode}", response.StatusCode);
                    return new Dictionary<Guid, CatalogProductDto>();
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BatchGetProductsPayload>>(options, cancellationToken);

                if (envelope?.Data?.Products is null)
                    return new Dictionary<Guid, CatalogProductDto>();

                return envelope.Data.Products.ToDictionary(
                    p => p.Id,
                    p => new CatalogProductDto(
                        p.Id,
                        p.Name,
                        p.Price,
                        p.EffectivePrice,
                        p.DiscountedPrice,
                        p.DiscountPercent,
                        p.IsAvailable,
                        p.AvailableStock,
                        p.PrimaryImageUrl,
                        p.IsArchived));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "Failed to fetch product batch from CatalogService");
                return new Dictionary<Guid, CatalogProductDto>();
            }
        }

        private sealed record BatchGetProductsPayload(List<BatchProductPayload> Products);

        private sealed record BatchProductPayload(
            Guid Id,
            string Name,
            decimal Price,
            decimal EffectivePrice,
            decimal? DiscountedPrice,
            decimal? DiscountPercent,
            bool IsAvailable,
            int AvailableStock,
            string? PrimaryImageUrl,
            bool IsArchived);
    }
}
