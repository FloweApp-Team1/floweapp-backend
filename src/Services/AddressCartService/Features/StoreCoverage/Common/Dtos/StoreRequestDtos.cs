namespace AddressCartService.Features.StoreCoverage.Common.Dtos
{
    public record PointRequest(double Lat, double Lng);

    public record RadiusRequest(double CenterLat, double CenterLng, double RadiusKm);

    public record CityAreaRequest(string City, string Area);

    public record LocationRequest(string AddressLine, double Lat, double Lng);
    public record CoverageAreaRequest(
        string Type,
        List<PointRequest>? Polygon,
        RadiusRequest? Radius,
        List<CityAreaRequest>? CityAreas);

    public record CreateStoreRequest(
        string Name,
        LocationRequest Location,
        CoverageAreaRequest CoverageArea);

    public record UpdateStoreRequest(
        string Name,
        LocationRequest Location,
        CoverageAreaRequest CoverageArea);
}
