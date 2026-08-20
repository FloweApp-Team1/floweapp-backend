using AddressCartService.Domain.Entities;
using AddressCartService.Domain.Enums;
using AddressCartService.Features.StoreCoverage.Common.Dtos;
using System.Text.Json;

namespace AddressCartService.Features.StoreCoverage.Common.Mapping
{
    public static class CoverageAreaMapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public static CoverageAreaTypeEnum ParseType(string? type) => type?.Trim().ToUpperInvariant() switch
        {
            "POLYGON" => CoverageAreaTypeEnum.Polygon,
            "RADIUS" => CoverageAreaTypeEnum.Radius,
            "CITY_AREA_LIST" => CoverageAreaTypeEnum.CityAreaList,
            _ => throw new ArgumentOutOfRangeException(
                nameof(type), type, "coverageArea.type must be one of: POLYGON, RADIUS, CITY_AREA_LIST.")
        };

        public static string ToContractType(CoverageAreaTypeEnum type) => type switch
        {
            CoverageAreaTypeEnum.Polygon => "POLYGON",
            CoverageAreaTypeEnum.Radius => "RADIUS",
            CoverageAreaTypeEnum.CityAreaList => "CITY_AREA_LIST",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        public static CoverageArea ToDomain(CoverageAreaRequest request)
        {
            var type = ParseType(request.Type);

            return new CoverageArea
            {
                Type = type,
                PolygonJson = type == CoverageAreaTypeEnum.Polygon && request.Polygon is not null
                    ? JsonSerializer.Serialize(
                        request.Polygon.Select(p => new PointDto(p.Lat, p.Lng)), JsonOptions)
                    : null,
                RadiusCenterLat = type == CoverageAreaTypeEnum.Radius ? request.Radius?.CenterLat : null,
                RadiusCenterLng = type == CoverageAreaTypeEnum.Radius ? request.Radius?.CenterLng : null,
                RadiusKm = type == CoverageAreaTypeEnum.Radius ? request.Radius?.RadiusKm : null,
                CityAreasJson = type == CoverageAreaTypeEnum.CityAreaList && request.CityAreas is not null
                    ? JsonSerializer.Serialize(
                        request.CityAreas.Select(c => new CityAreaDto(c.City, c.Area)), JsonOptions)
                    : null
            };
        }
        public static void ApplyTo(CoverageArea target, CoverageAreaRequest request)
        {
            var updated = ToDomain(request);
            target.Type = updated.Type;
            target.PolygonJson = updated.PolygonJson;
            target.RadiusCenterLat = updated.RadiusCenterLat;
            target.RadiusCenterLng = updated.RadiusCenterLng;
            target.RadiusKm = updated.RadiusKm;
            target.CityAreasJson = updated.CityAreasJson;
        }

        public static CoverageAreaDto ToDto(CoverageArea coverageArea)
        {
            List<PointDto>? polygon = null;
            RadiusDto? radius = null;
            List<CityAreaDto>? cityAreas = null;

            switch (coverageArea.Type)
            {
                case CoverageAreaTypeEnum.Polygon when !string.IsNullOrWhiteSpace(coverageArea.PolygonJson):
                    polygon = JsonSerializer.Deserialize<List<PointDto>>(coverageArea.PolygonJson!, JsonOptions);
                    break;

                case CoverageAreaTypeEnum.Radius:
                    radius = coverageArea is
                    { RadiusCenterLat: not null, RadiusCenterLng: not null, RadiusKm: not null }
                        ? new RadiusDto(
                            coverageArea.RadiusCenterLat.Value,
                            coverageArea.RadiusCenterLng.Value,
                            coverageArea.RadiusKm.Value)
                        : null;
                    break;

                case CoverageAreaTypeEnum.CityAreaList when !string.IsNullOrWhiteSpace(coverageArea.CityAreasJson):
                    cityAreas = JsonSerializer.Deserialize<List<CityAreaDto>>(coverageArea.CityAreasJson!, JsonOptions);
                    break;
            }

            return new CoverageAreaDto(ToContractType(coverageArea.Type), polygon, radius, cityAreas);
        }

        public static StoreResponse ToStoreResponse(Store store) => new(
            store.Id,
            store.Name,
            new LocationDto(store.Location.AddressLine, store.Location.Lat, store.Location.Lng),
            ToDto(store.CoverageArea),
            store.Status.ToString().ToUpperInvariant(),
            store.CreatedAt,
            store.UpdatedAt);
    }
}
