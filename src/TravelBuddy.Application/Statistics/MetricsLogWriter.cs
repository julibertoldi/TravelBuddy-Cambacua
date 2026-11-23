using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TravelBuddy.Destinations;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace TravelBuddy.Statistics;

public class MetricsLogWriter : ITransientDependency
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MetricsLogWriter> _logger;

    public MetricsLogWriter(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MetricsLogWriter> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public Task RecordSearchAsync(string searchTerm, Guid? userId)
    {
        return RecordAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IRepository<SearchLogs, Guid>>();
            await repository.InsertAsync(
                new SearchLogs(Guid.NewGuid(), searchTerm, DateTime.UtcNow, userId),
                autoSave: true);
        }, "SearchLogs");
    }

    public Task RecordApiCallAsync(
        string endpoint,
        int statusCode,
        long responseTimeMs,
        bool isSuccess,
        string? errorMessage)
    {
        return RecordAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IRepository<ApiCallLog, Guid>>();
            await repository.InsertAsync(
                new ApiCallLog(
                    Guid.NewGuid(),
                    endpoint,
                    statusCode,
                    responseTimeMs,
                    isSuccess,
                    errorMessage ?? string.Empty),
                autoSave: true);
        }, "ApiCallLog");
    }

    public Task RecordDestinationViewAsync(int geoDbCityId)
    {
        return RecordAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IRepository<Destination, Guid>>();
            var destination = await repository.FirstOrDefaultAsync(
                item => item.GeoDbCityId == geoDbCityId);

            if (destination == null)
            {
                return;
            }

            destination.ViewCount++;
            await repository.UpdateAsync(destination, autoSave: true);
        }, "Destination.ViewCount");
    }

    private async Task RecordAsync(
        Func<IServiceProvider, Task> record,
        string metricName)
    {
        try
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var serviceProvider = scope.ServiceProvider;
            var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();

            using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true);
            await record(serviceProvider);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not persist {MetricName}; the user operation will continue.", metricName);
        }
    }
}
