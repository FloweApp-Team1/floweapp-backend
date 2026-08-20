namespace AddressCartService.Features.StoreCoverage.Common.Dtos
{
    public record PointDto(double Lat, double Lng);

    public record RadiusDto(double CenterLat, double CenterLng, double RadiusKm);

    public record CityAreaDto(string City, string Area);

    public record LocationDto(string AddressLine, double Lat, double Lng);
    public record CoverageAreaDto(
        string Type,
        List<PointDto>? Polygon,
        RadiusDto? Radius,
        List<CityAreaDto>? CityAreas);

    public record StoreResponse(
        Guid Id,
        string Name,
        LocationDto Location,
        CoverageAreaDto CoverageArea,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
