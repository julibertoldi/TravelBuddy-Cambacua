using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Shouldly;
using TravelBuddy.Cities;
using Xunit;

namespace TravelBuddy.Application.Tests.Cities;

public class GeoDbCitySearchService_Tests
{
    [Fact]
    public async Task ShouldReturnCitiesWhenSearchingByName()
    {
        var service = CreateService(
            """{"data":[{"id":1,"city":"Rio de Janeiro","country":"Brazil","population":6000000,"latitude":-22.9,"longitude":-43.2}]}""");

        var result = await service.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "Rio" });

        result.Cities.Count.ShouldBe(1);
        result.Cities[0].Name.ShouldBe("Rio de Janeiro");
    }

    [Fact]
    public async Task ShouldReturnEmptyWhenNoCitiesMatch()
    {
        var service = CreateService("""{"data":[]}""");

        var result = await service.SearchCitiesAsync(new CitySearchRequestDto { PartialName = "Unknown" });

        result.Cities.ShouldBeEmpty();
    }

    private static ICitySearchService CreateService(string json)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GeoDb:BaseUrl"] = "https://example.test/cities",
                ["GeoDb:ApiKey"] = "test-key",
                ["GeoDb:ApiHost"] = "example.test"
            })
            .Build();
        var client = new HttpClient(new FakeGeoDbHttpMessageHandler(
            _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));

        return new GeoDbCitySearchService(client, configuration);
    }

    private sealed class FakeGeoDbHttpMessageHandler(
        System.Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }
}
