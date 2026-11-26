using System;

namespace TravelBuddy.Cities;

internal static class CityImageUrlValidator
{
    public static bool IsTrusted(string? imageUrl)
    {
        return Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) &&
               uri.Scheme == Uri.UriSchemeHttps &&
               uri.IsDefaultPort &&
               (string.Equals(uri.Host, "upload.wikimedia.org", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Host, "thumb.wikimedia.org", StringComparison.OrdinalIgnoreCase));
    }
}
