using System.Text.Json;
using System.Text.Json.Serialization;
using AddressCartService.Domain.Entities;
using AddressCartService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;

namespace AddressCartService.Infrastructure.Services.StoreCoverage
{
    // Matches an address against every active store's CoverageArea. All three coverage
    // types live in the same service/database, so this is a plain DB query + in-memory
    // geometry check - no external geo service involved.
    public class StoreResolutionService : IStoreResolutionService
    {
        private readonly IUnitOfWork _unitOfWork;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public StoreResolutionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid?> ResolveServingStoreAsync(
            double? lat, double? lng, string city, string area, CancellationToken cancellationToken)
        {
            var activeStores = await _unitOfWork.Repository<Store>()
                .Query()
                .Where(s => s.Status == StoreStatusEnum.Active)
                .ToListAsync(cancellationToken);

            return activeStores
                .FirstOrDefault(store => Covers(store.CoverageArea, lat, lng, city, area))
                ?.Id;
        }

        private static bool Covers(
            CoverageArea coverage, double? lat, double? lng, string city, string area) => coverage.Type switch
        {
            CoverageAreaTypeEnum.Polygon => lat.HasValue && lng.HasValue &&
                IsPointInPolygon(lat.Value, lng.Value, ParsePolygon(coverage.PolygonJson)),

            CoverageAreaTypeEnum.Radius => lat.HasValue && lng.HasValue &&
                coverage.RadiusCenterLat.HasValue && coverage.RadiusCenterLng.HasValue && coverage.RadiusKm.HasValue &&
                HaversineDistanceKm(lat.Value, lng.Value, coverage.RadiusCenterLat.Value, coverage.RadiusCenterLng.Value)
                    <= coverage.RadiusKm.Value,

            CoverageAreaTypeEnum.CityAreaList =>
                MatchesCityArea(ParseCityAreas(coverage.CityAreasJson), city, area),

            _ => false
        };

        private static List<PolygonPoint> ParsePolygon(string? polygonJson)
        {
            if (string.IsNullOrWhiteSpace(polygonJson))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<PolygonPoint>>(polygonJson, JsonOptions) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static List<CityAreaEntry> ParseCityAreas(string? cityAreasJson)
        {
            if (string.IsNullOrWhiteSpace(cityAreasJson))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<CityAreaEntry>>(cityAreasJson, JsonOptions) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private static bool MatchesCityArea(List<CityAreaEntry> entries, string city, string area) =>
            entries.Any(e =>
                string.Equals(e.City, city, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Area, area, StringComparison.OrdinalIgnoreCase));

        // Standard ray-casting point-in-polygon test.
        private static bool IsPointInPolygon(double lat, double lng, List<PolygonPoint> polygon)
        {
            if (polygon.Count < 3)
                return false;

            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];

                var intersects = pi.Lng > lng != pj.Lng > lng &&
                    lat < (pj.Lat - pi.Lat) * (lng - pi.Lng) / (pj.Lng - pi.Lng) + pi.Lat;

                if (intersects)
                    inside = !inside;
            }

            return inside;
        }

        private static double HaversineDistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLng = DegreesToRadians(lng2 - lng1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

        private sealed record PolygonPoint(
            [property: JsonPropertyName("lat")] double Lat,
            [property: JsonPropertyName("lng")] double Lng);

        private sealed record CityAreaEntry(
            [property: JsonPropertyName("city")] string City,
            [property: JsonPropertyName("area")] string Area);
    }
}
