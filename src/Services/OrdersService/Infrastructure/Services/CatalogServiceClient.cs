using Shared.Results;
using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace OrdersService.Infrastructure.Services
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogServiceClient> _logger;

        public CatalogServiceClient(IHttpClientFactory httpClientFactory, ILogger<CatalogServiceClient> logger)
        {
            _httpClient = httpClientFactory.CreateClient("CatalogServiceClient");
            _logger = logger;
        }

        public async Task<CatalogProductDto?> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/products/{productId}", cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<Shared.Responses.ApiResponse<CatalogProductDto>>(responseString, options);

                if (result != null && result.Status)
                {
                    return result.Data;
                }
                
                _logger.LogWarning("CatalogService returned unsuccessful result for product {ProductId}: {Error}", productId, result?.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch product details for {ProductId} from CatalogService", productId);
                return null;
            }
        }
    }
}
