using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using TravelBuddy.Permissions; 
using TravelBuddy.Statistics;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TravelBuddy.Destinations
{
    public class DestinationAppService :
        CrudAppService<
            Destination,
            DestinationDto,
            Guid,
            Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto,
            CreateUpdateDestinationDto>, IDestinationAppService 
    {
        private readonly ICitySearchService _citySearchService;
        private readonly ICityImageProvider _cityImageProvider;
        private readonly IMemoryCache _imageCache;
        private readonly ICurrentUser _currentUser;
        private readonly MetricsLogWriter _metricsLogWriter;
        private readonly ILogger<DestinationAppService> _logger;
  

        public DestinationAppService(
            IRepository<Destination, Guid> repository,
            ICitySearchService citySearchService,
            ICityImageProvider cityImageProvider,
            IMemoryCache imageCache,
            ICurrentUser currentUser,
            MetricsLogWriter metricsLogWriter,
            ILogger<DestinationAppService> logger)
            : base(repository)
        {
            _citySearchService = citySearchService;
            _cityImageProvider = cityImageProvider;
            _imageCache = imageCache;
            _currentUser = currentUser;
            _metricsLogWriter = metricsLogWriter;
            _logger = logger;
        }

        public async Task<CitySearchResultDto> SearchCitiesAsync(CitySearchRequestDto request)
        {
            var searchTerm = request.PartialName?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Pais))
            {
                searchTerm = string.IsNullOrWhiteSpace(searchTerm)
                    ? $"País: {request.Pais.Trim()}"
                    : $"{searchTerm} | País: {request.Pais.Trim()}";
            }

            try
            {
                var result = await _citySearchService.SearchCitiesAsync(request);
                await PopulateCityImagesAsync(result);
                return result;
            }
            finally
            {
                await _metricsLogWriter.RecordSearchAsync(searchTerm, _currentUser.Id);
            }
        }

        public async Task<CityDetailDto> GetCityDetailsAsync(int cityId)
        {
            var city = await _citySearchService.GetCityDetailsAsync(cityId);
            await _metricsLogWriter.RecordDestinationViewAsync(cityId);
            return city;
        }

        public async Task<CitySearchResultDto> GetPopularCitiesAsync()
        {
            return await _citySearchService.GetPopularCitiesAsync();
        }
        // --- MÉTODOS ADMINISTRATIVOS PROTEGIDOS ---s        
        [Authorize]
        public override async Task<DestinationDto> CreateAsync(CreateUpdateDestinationDto input)
        {
            var destination = ObjectMapper.Map<CreateUpdateDestinationDto, Destination>(input);

            await Repository.InsertAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task<DestinationDto> UpdateAsync(Guid id, CreateUpdateDestinationDto input)
        {
            var destination = await Repository.GetAsync(id);

            ObjectMapper.Map(input, destination);

            await Repository.UpdateAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);          
        }

        [Authorize(TravelBuddyPermissions.Admin.Default)]
        public override async Task DeleteAsync(Guid id)
        {
            await base.DeleteAsync(id);
        }

        [Authorize]
        public async Task<DestinationDto> ImportFromGeoDbAsync(int geoDbCityId)
        {
            var existing = await Repository.FirstOrDefaultAsync(d => d.GeoDbCityId == geoDbCityId);
            if (existing != null)
                return ObjectMapper.Map<Destination, DestinationDto>(existing);

            var city = await _citySearchService.GetCityDetailsAsync(geoDbCityId);
            string? imageUrl = null;
            if (!string.IsNullOrWhiteSpace(city.WikiDataId))
            {
                try
                {
                    imageUrl = await _cityImageProvider.GetImageUrlAsync(city.Name, city.WikiDataId);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Could not retrieve an image for city {CityName}.", city.Name);
                }
            }

            var destination = new Destination(GuidGenerator.Create(), city.Name,
                $"Ciudad importada desde GeoDB ({city.Country})", city.Region ?? string.Empty, city.Country)
            {
                GeoDbCityId = geoDbCityId,      
                Population = city.Population,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                LastUpdated = DateTime.Now,
                ImageUrl = CityImageUrlValidator.IsTrusted(imageUrl)
                    ? imageUrl!
                    : CityImageFallback.GetUrl(geoDbCityId)
            };

            await Repository.InsertAsync(destination, autoSave: true);
            return ObjectMapper.Map<Destination, DestinationDto>(destination);
        }

        private async Task PopulateCityImagesAsync(CitySearchResultDto result)
        {
            var cityIds = result.Cities.Select(city => city.Id).Distinct().ToList();
            if (cityIds.Count == 0)
                return;

            var savedDestinations = await Repository.GetListAsync(destination =>
                destination.GeoDbCityId.HasValue &&
                cityIds.Contains(destination.GeoDbCityId.Value));
            var savedImages = savedDestinations
                .Where(destination => !string.IsNullOrWhiteSpace(destination.ImageUrl))
                .GroupBy(destination => destination.GeoDbCityId!.Value)
                .ToDictionary(group => group.Key, group => group.First().ImageUrl);

            var imageUrls = new Dictionary<int, string?>();
            var citiesToFetch = new Dictionary<int, CityDto>();
            foreach (var city in result.Cities)
            {
                if (savedImages.TryGetValue(city.Id, out var savedImageUrl))
                {
                    imageUrls[city.Id] = savedImageUrl;
                    _imageCache.Set(
                        GetImageCacheKey(city.Id),
                        new CityImageCacheEntry(savedImageUrl),
                        TimeSpan.FromDays(7));
                }
                else if (_imageCache.TryGetValue<CityImageCacheEntry>(GetImageCacheKey(city.Id), out var cachedImage))
                {
                    imageUrls[city.Id] = cachedImage!.ImageUrl;
                }
                else if (!string.IsNullOrWhiteSpace(city.WikiDataId))
                {
                    citiesToFetch.TryAdd(city.Id, city);
                }
                else
                {
                    imageUrls[city.Id] = null;
                    _imageCache.Set(
                        GetImageCacheKey(city.Id),
                        new CityImageCacheEntry(null),
                        TimeSpan.FromDays(1));
                }
            }

            if (citiesToFetch.Count > 0)
            {
                using var timeout = new CancellationTokenSource();
                var imageTasks = citiesToFetch.ToDictionary(
                    entry => entry.Key,
                    entry => FetchCityImageAsync(entry.Value, timeout.Token));
                try
                {
                    await Task.WhenAll(imageTasks.Values).WaitAsync(TimeSpan.FromSeconds(3));
                }
                catch (TimeoutException)
                {
                    timeout.Cancel();
                    _logger.LogWarning("City image lookup batch exceeded its 3 second timeout.");
                }

                foreach (var (cityId, imageTask) in imageTasks)
                {
                    var imageUrl = imageTask.IsCompletedSuccessfully ? imageTask.Result : null;
                    var hasImage = CityImageUrlValidator.IsTrusted(imageUrl);
                    imageUrl = hasImage ? imageUrl : null;
                    _imageCache.Set(
                        GetImageCacheKey(cityId),
                        new CityImageCacheEntry(imageUrl),
                        TimeSpan.FromDays(hasImage ? 7 : 1));
                    imageUrls[cityId] = imageUrl;
                }
            }

            foreach (var city in result.Cities)
                city.ImageUrl = imageUrls.GetValueOrDefault(city.Id);
        }

        private async Task<string?> FetchCityImageAsync(CityDto city, CancellationToken cancellationToken)
        {
            try
            {
                return await _cityImageProvider.GetImageUrlAsync(
                    city.Name,
                    city.WikiDataId,
                    cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not retrieve an image for city {CityName}.", city.Name);
                return null;
            }
        }

        private static string GetImageCacheKey(int geoDbCityId) => $"CityImage:v2:{geoDbCityId}";

        private sealed record CityImageCacheEntry(string? ImageUrl);

    }
}