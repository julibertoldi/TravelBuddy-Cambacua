using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shouldly;
using TravelBuddy.Infraestructure;
using TravelBuddy.Statistics;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace TravelBuddy.Application.Tests.Cities;

public class GeoDbMetricsHandler_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly MetricsLogWriter _metricsLogWriter;
    private readonly ILogger<GeoDbMetricsHandler> _logger;
    private readonly IRepository<ApiCallLog, Guid> _apiCallLogRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public GeoDbMetricsHandler_Tests()
    {
        _metricsLogWriter = GetRequiredService<MetricsLogWriter>();
        _logger = GetRequiredService<ILogger<GeoDbMetricsHandler>>();
        _apiCallLogRepository = GetRequiredService<IRepository<ApiCallLog, Guid>>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, true)]
    [InlineData(HttpStatusCode.ServiceUnavailable, false)]
    public async Task Should_Log_Response_Status_Without_Query_String(
        HttpStatusCode statusCode,
        bool expectedSuccess)
    {
        var handler = CreateHandler(_ => Task.FromResult(new HttpResponseMessage(statusCode)));
        using var invoker = new HttpMessageInvoker(handler);

        using var response = await invoker.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "https://geo.example.test/cities?apikey=secret"),
            CancellationToken.None);

        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        var log = await _apiCallLogRepository.FirstOrDefaultAsync(
            item => item.Endpoint == "GET /cities" && item.StatusCode == (int)statusCode);

        log.ShouldNotBeNull();
        log.IsSuccess.ShouldBe(expectedSuccess);
        log.ResponseTimeMs.ShouldBeGreaterThanOrEqualTo(0);
        if (!expectedSuccess)
        {
            log.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        }

        await uow.CompleteAsync();
    }

    [Fact]
    public async Task Should_Log_Transport_Exception_And_Rethrow_It()
    {
        var handler = CreateHandler(_ => throw new HttpRequestException("network unavailable"));
        using var invoker = new HttpMessageInvoker(handler);

        await Should.ThrowAsync<HttpRequestException>(() =>
            invoker.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, "https://geo.example.test/cities"),
                CancellationToken.None));

        using var uow = _unitOfWorkManager.Begin(requiresNew: true);
        var log = await _apiCallLogRepository.FirstOrDefaultAsync(
            item => item.Endpoint == "GET /cities" && item.StatusCode == 0);

        log.ShouldNotBeNull();
        log.IsSuccess.ShouldBeFalse();
        log.ErrorMessage.ShouldBe("network unavailable");

        await uow.CompleteAsync();
    }

    private GeoDbMetricsHandler CreateHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        var handler = new TestableGeoDbMetricsHandler(_logger, _metricsLogWriter);
        handler.SetInnerHandler(new FakeHttpMessageHandler(sendAsync));
        return handler;
    }

    private sealed class TestableGeoDbMetricsHandler(
        ILogger<GeoDbMetricsHandler> logger,
        MetricsLogWriter metricsLogWriter)
        : GeoDbMetricsHandler(logger, metricsLogWriter)
    {
        public void SetInnerHandler(HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
        }
    }

    private sealed class FakeHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return sendAsync(request);
        }
    }
}
