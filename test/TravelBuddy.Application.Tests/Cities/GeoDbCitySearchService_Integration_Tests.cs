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

namespace TravelBuddy.Application.Tests.Integration;

public class GeoDbCitySearchService_Integration_Tests
{
    [Fact]
    public async Task ShouldReturnCityDetailsFromResponse()
    {
        var service = CreateService(
            """{"data":{"id":123,"city":"Buenos Aires","country":"Argentina","region":"Buenos Aires","latitude":-34.6,"longitude":-58.4,"population":3000000}}""");

        var result = await service.GetCityDetailsAsync(123);

        result.Name.ShouldBe("Buenos Aires");
        result.Country.ShouldBe("Argentina");
        result.Population.ShouldBe(3000000);
    }

    [Fact]
    public async Task ShouldReturnPopularCitiesFromResponse()
    {
        var service = CreateService(
            """{"data":[{"id":1,"city":"Buenos Aires","country":"Argentina"}]}""");

        var result = await service.GetPopularCitiesAsync();

        result.Cities.Count.ShouldBe(1);
        result.Cities[0].Name.ShouldBe("Buenos Aires");
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
