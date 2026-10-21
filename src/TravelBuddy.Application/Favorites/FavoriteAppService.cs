using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using TravelBuddy.Cities;
using TravelBuddy.Destinations;

namespace TravelBuddy.Favorites;

[Authorize]
public class FavoriteAppService : ApplicationService, IFavoriteAppService
{
    private readonly IRepository<Favorite> _favoriteRepository;
    private readonly IRepository<Destination, Guid> _destinationRepository;
    private readonly ICitySearchService _citySearchService;

    public FavoriteAppService(
        IRepository<Favorite> favoriteRepository,
        IRepository<Destination, Guid> destinationRepository,
        ICitySearchService citySearchService)
    {
        _favoriteRepository = favoriteRepository;
        _destinationRepository = destinationRepository;
        _citySearchService = citySearchService;
    }

    public async Task AgregarFavoritoAsync(Guid destinoId)
    {
        var usuarioId = CurrentUser.GetId();
        var existe = await _favoriteRepository.AnyAsync(
            x => x.UsuarioId == usuarioId && x.DestinoId == destinoId);

        if (!existe)
        {
            await _favoriteRepository.InsertAsync(
                new Favorite(usuarioId, destinoId), autoSave: true);
        }
    }

    public async Task AgregarDesdeGeoDbAsync(int geoDbCityId)
    {
        var usuarioId = CurrentUser.GetId();

        // 1. Buscar si el destino ya fue importado previamente
        var destination = await _destinationRepository.FirstOrDefaultAsync(d => d.GeoDbCityId == geoDbCityId);

        if (destination == null)
        {
            var city = await _citySearchService.GetCityDetailsAsync(geoDbCityId);

            destination = new Destination(
                GuidGenerator.Create(),
                city.Name,
                $"Ciudad importada desde GeoDB ({city.Country})",
                city.Region,
                city.Country)
            {
                GeoDbCityId = geoDbCityId,
                Population = city.Population,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                LastUpdated = DateTime.Now,
                ImageUrl = string.IsNullOrWhiteSpace(city.ImageUrl)
                    ? "https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=400"
                    : city.ImageUrl
            };

            await _destinationRepository.InsertAsync(destination, autoSave: true);
        }

        // 2. Asociar a favoritos de forma segura e idempotente
        var existeFavorito = await _favoriteRepository.AnyAsync(
            x => x.UsuarioId == usuarioId && x.DestinoId == destination.Id);

        if (!existeFavorito)
        {
            await _favoriteRepository.InsertAsync(
                new Favorite(usuarioId, destination.Id), autoSave: true);
        }
    }

    public async Task QuitarFavoritoAsync(Guid destinoId)
    {
        var usuarioId = CurrentUser.GetId();
        var entidad = await _favoriteRepository.FirstOrDefaultAsync(
            x => x.UsuarioId == usuarioId && x.DestinoId == destinoId);

        if (entidad != null)
        {
            await _favoriteRepository.DeleteAsync(entidad, autoSave: true);
        }
    }

    public async Task<List<FavoriteDto>> ObtenerMisFavoritosAsync()
    {
        var usuarioId = CurrentUser.GetId();

        var queryable = await _favoriteRepository.WithDetailsAsync(x => x.Destination);
        var favoritos = queryable.Where(x => x.UsuarioId == usuarioId).ToList();

        var favoritosDto = favoritos.Select(f => new FavoriteDto
        {
            DestinoId = f.DestinoId,
            Name = f.Destination.Name,
            Description = f.Destination.Description,
            Region = f.Destination.Region,
            Country = f.Destination.Country,
            ImageUrl = f.Destination.ImageUrl,
            Price = f.Destination.Price,
            IsAvailable = f.Destination.IsAvailable,
            GeoDbCityId = f.Destination.GeoDbCityId,
            Population = f.Destination.Population,
            Latitude = f.Destination.Latitude,
            Longitude = f.Destination.Longitude,
            CreationTime = f.Destination.CreationTime
        }).ToList();

        return favoritosDto;
    }
}