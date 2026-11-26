using System;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TravelBuddy.Cities;

public sealed class WikipediaCityImageProvider : ICityImageProvider
{
    private const string SummaryEndpoint = "https://en.wikipedia.org/api/rest_v1/page/summary/";
    private readonly HttpClient _httpClient;
    private readonly ILogger<WikipediaCityImageProvider> _logger;

    public WikipediaCityImageProvider(
        HttpClient httpClient,
        ILogger<WikipediaCityImageProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetImageUrlAsync(
        string cityName,
        string? wikiDataId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cityName) || string.IsNullOrWhiteSpace(wikiDataId))
            return null;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        return await TryGetImageUrlAsync(cityName.Trim(), wikiDataId.Trim(), timeout.Token);
    }

    private async Task<string?> TryGetImageUrlAsync(
        string title,
        string wikiDataId,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                SummaryEndpoint + Uri.EscapeDataString(title));
            request.Headers.UserAgent.ParseAdd("TravelBuddy/1.0");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = document.RootElement;
            if (!root.TryGetProperty("wikibase_item", out var wikibaseItem) ||
                !string.Equals(wikibaseItem.GetString(), wikiDataId, StringComparison.OrdinalIgnoreCase))
                return null;

            if (root.TryGetProperty("type", out var type) &&
                string.Equals(type.GetString(), "disambiguation", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!root.TryGetProperty("thumbnail", out var thumbnail) ||
                !thumbnail.TryGetProperty("source", out var source))
                return null;

            var imageUrl = source.GetString();
            if (CityImageUrlValidator.IsTrusted(imageUrl) && IsCityPhoto(imageUrl))
                return imageUrl;

            _logger.LogWarning("Wikipedia returned a thumbnail outside the trusted Wikimedia upload domain.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not retrieve a Wikipedia thumbnail for {Title}.", title);
        }

        return null;
    }

    private static bool IsCityPhoto(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return false;

        var fileName = Uri.UnescapeDataString(Path.GetFileName(uri.AbsolutePath));
        var disallowedTerms = new[]
        {
            "map", "locator", "location", "coat_of_arms", "flag",
            "escudo", "bandera", "blason", "seal", "logo"
        };
        return !fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) &&
               !fileName.EndsWith(".svg.png", StringComparison.OrdinalIgnoreCase) &&
               !disallowedTerms.Any(term => fileName.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
