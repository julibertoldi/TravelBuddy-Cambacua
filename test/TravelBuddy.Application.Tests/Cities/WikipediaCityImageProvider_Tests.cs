using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using TravelBuddy.Cities;
using Xunit;

namespace TravelBuddy.Application.Tests.Cities;

public class WikipediaCityImageProvider_Tests
{
    [Fact]
    public async Task ShouldReturnThumbnailWhenWikiDataIdMatches()
    {
        var requestCount = 0;
        var provider = CreateProvider(new FakeHttpMessageHandler(request =>
        {
            requestCount++;
            request.Headers.UserAgent.ToString().ShouldContain("TravelBuddy/1.0");
            return JsonResponse("""{"type":"standard","wikibase_item":"Q123","thumbnail":{"source":"https://upload.wikimedia.org/wikipedia/commons/c/c1/Example.jpg"}}""");
        }));

        var imageUrl = await provider.GetImageUrlAsync("Test City", "Q123");

        imageUrl.ShouldBe("https://upload.wikimedia.org/wikipedia/commons/c/c1/Example.jpg");
        requestCount.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldRejectThumbnailWhenWikiDataIdDoesNotMatch()
    {
        var provider = CreateProvider(new FakeHttpMessageHandler(_ =>
            JsonResponse("""{"type":"standard","wikibase_item":"Q999","thumbnail":{"source":"https://upload.wikimedia.org/wikipedia/commons/c/c1/Example.jpg"}}""")));

        var imageUrl = await provider.GetImageUrlAsync("Test City", "Q123");

        imageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldRejectMapThumbnail()
    {
        var provider = CreateProvider(new FakeHttpMessageHandler(_ =>
            JsonResponse("""{"type":"standard","wikibase_item":"Q123","thumbnail":{"source":"https://upload.wikimedia.org/wikipedia/commons/c/c1/City_map.jpg"}}""")));

        var imageUrl = await provider.GetImageUrlAsync("Test City", "Q123");

        imageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldRejectDisambiguationThumbnail()
    {
        var provider = CreateProvider(new FakeHttpMessageHandler(_ =>
            JsonResponse("""{"type":"disambiguation","wikibase_item":"Q123","thumbnail":{"source":"https://upload.wikimedia.org/wikipedia/commons/c/c1/Example.jpg"}}""")));

        var imageUrl = await provider.GetImageUrlAsync("Test City", "Q123");

        imageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldRejectThumbnailOutsideHttpsUploadWikimediaDomain()
    {
        var provider = CreateProvider(new FakeHttpMessageHandler(_ =>
            JsonResponse("""{"type":"standard","wikibase_item":"Q123","thumbnail":{"source":"http://upload.wikimedia.org/example.jpg"}}""")));

        var imageUrl = await provider.GetImageUrlAsync("Test City", "Q123");

        imageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldAcceptHttpsThumbnailFromThumbWikimediaDomain()
    {
        var provider = CreateProvider(new FakeHttpMessageHandler(_ =>
            JsonResponse("""{"type":"standard","wikibase_item":"Q123","thumbnail":{"source":"https://thumb.wikimedia.org/example.jpg"}}""")));

        var imageUrl = await provider.GetImageUrlAsync("Test City", "Q123");

        imageUrl.ShouldBe("https://thumb.wikimedia.org/example.jpg");
    }

    private static WikipediaCityImageProvider CreateProvider(HttpMessageHandler handler)
    {
        return new WikipediaCityImageProvider(
            new HttpClient(handler),
            NullLogger<WikipediaCityImageProvider>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FakeHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }
}
