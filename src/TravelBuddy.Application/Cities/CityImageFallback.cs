namespace TravelBuddy.Cities;

internal static class CityImageFallback
{
    public static string GetUrl(int? geoDbCityId)
    {
        var imageNumber = geoDbCityId.HasValue
            ? System.Math.Abs(geoDbCityId.Value % 6) + 1
            : 1;

        return $"/assets/images/generic-{imageNumber}.jpg";
    }
}
