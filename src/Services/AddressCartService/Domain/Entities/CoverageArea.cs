using AddressCartService.Domain.Enums;

namespace AddressCartService.Domain.Entities
{
    // Owned by Store (one-to-one). Exactly one of the type-specific groups below is
    // populated, matching Type - mirrors the discriminated-union shape of the
    // CoverageArea schema in the API contract.
    public class CoverageArea
    {
        public CoverageAreaTypeEnum Type { get; set; }

        // Populated when Type == Polygon: JSON array of { lat, lng } points. Kept as raw
        // JSON text rather than a normalized table until the polygon-editing flow is built.
        public string? PolygonJson { get; set; }

        // Populated when Type == Radius.
        public double? RadiusCenterLat { get; set; }
        public double? RadiusCenterLng { get; set; }
        public double? RadiusKm { get; set; }

        // Populated when Type == CityAreaList: JSON array of { city, area }.
        public string? CityAreasJson { get; set; }
    }
}
