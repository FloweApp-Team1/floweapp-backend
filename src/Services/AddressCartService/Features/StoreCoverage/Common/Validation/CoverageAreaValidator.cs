using AddressCartService.Features.StoreCoverage.Common.Dtos;
using FluentValidation;

namespace AddressCartService.Features.StoreCoverage.Common.Validation
{
    public static class CoverageAreaValidator
    {
        private static readonly string[] AllowedTypes = { "POLYGON", "RADIUS", "CITY_AREA_LIST" };

        public static void Validate<T>(CoverageAreaRequest? coverageArea, ValidationContext<T> context)
        {
            if (coverageArea is null)
                return; 

            var type = coverageArea.Type?.Trim().ToUpperInvariant();
            if (type is null || !AllowedTypes.Contains(type))
            {
                context.AddFailure("coverageArea.type", "coverageArea.type must be one of: POLYGON, RADIUS, CITY_AREA_LIST.");
                return;
            }

            switch (type)
            {
                case "POLYGON":
                    ValidatePolygon(coverageArea.Polygon, context);
                    break;
                case "RADIUS":
                    ValidateRadius(coverageArea.Radius, context);
                    break;
                case "CITY_AREA_LIST":
                    ValidateCityAreas(coverageArea.CityAreas, context);
                    break;
            }
        }

        private static void ValidatePolygon<T>(List<PointRequest>? polygon, ValidationContext<T> context)
        {
            if (polygon is null || polygon.Count == 0)
            {
                context.AddFailure("coverageArea.polygon", "coverageArea.polygon is required when type is POLYGON.");
                return;
            }

            foreach (var (p, i) in polygon.Select((p, i) => (p, i)))
            {
                if (p.Lat is < -90 or > 90)
                    context.AddFailure($"coverageArea.polygon[{i}].lat", "Latitude must be between -90 and 90.");
                if (p.Lng is < -180 or > 180)
                    context.AddFailure($"coverageArea.polygon[{i}].lng", "Longitude must be between -180 and 180.");
            }
            var isExplicitlyClosed = polygon.Count > 3 && PointsEqual(polygon[0], polygon[^1]);
            var vertices = isExplicitlyClosed ? polygon.Take(polygon.Count - 1).ToList() : polygon;

            if (vertices.Count < 3)
            {
                context.AddFailure(
                    "coverageArea.polygon",
                    "coverageArea.polygon must contain at least 3 distinct points to form a closed shape.");
                return;
            }

            for (var i = 0; i < vertices.Count; i++)
            {
                var next = vertices[(i + 1) % vertices.Count];
                if (PointsEqual(vertices[i], next))
                {
                    context.AddFailure(
                        "coverageArea.polygon", "coverageArea.polygon must not contain consecutive duplicate points.");
                    break;
                }
            }
        }

        private static void ValidateRadius<T>(RadiusRequest? radius, ValidationContext<T> context)
        {
            if (radius is null)
            {
                context.AddFailure("coverageArea.radius", "coverageArea.radius is required when type is RADIUS.");
                return;
            }

            if (radius.CenterLat is < -90 or > 90)
                context.AddFailure("coverageArea.radius.centerLat", "centerLat must be between -90 and 90.");
            if (radius.CenterLng is < -180 or > 180)
                context.AddFailure("coverageArea.radius.centerLng", "centerLng must be between -180 and 180.");
            if (radius.RadiusKm <= 0)
                context.AddFailure("coverageArea.radius.radiusKm", "radiusKm must be greater than 0.");
        }

        private static void ValidateCityAreas<T>(List<CityAreaRequest>? cityAreas, ValidationContext<T> context)
        {
            if (cityAreas is null || cityAreas.Count == 0)
            {
                context.AddFailure(
                    "coverageArea.cityAreas",
                    "coverageArea.cityAreas must contain at least one city/area when type is CITY_AREA_LIST.");
                return;
            }

            for (var i = 0; i < cityAreas.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(cityAreas[i].City))
                    context.AddFailure($"coverageArea.cityAreas[{i}].city", "city is required.");
                if (string.IsNullOrWhiteSpace(cityAreas[i].Area))
                    context.AddFailure($"coverageArea.cityAreas[{i}].area", "area is required.");
            }

            var hasDuplicates = cityAreas
                .GroupBy(c => (City: c.City?.Trim().ToLowerInvariant(), Area: c.Area?.Trim().ToLowerInvariant()))
                .Any(g => g.Count() > 1);

            if (hasDuplicates)
                context.AddFailure("coverageArea.cityAreas", "coverageArea.cityAreas must not contain duplicate city/area pairs.");
        }

        private static bool PointsEqual(PointRequest a, PointRequest b) =>
            Math.Abs(a.Lat - b.Lat) < 1e-9 && Math.Abs(a.Lng - b.Lng) < 1e-9;
    }
}
