using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace TravelBuddy.Favorites;

[Authorize]
[RemoteService(IsEnabled = false)]
public class FavoriteAppService : ApplicationService, IFavoriteAppService
{
    private readonly IRepository<Favorite> _favoriteRepository;

    public FavoriteAppService(IRepository<Favorite> favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
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

        // Carga los favoritos junto a su relación 'Destino' usando el repositorio de ABP
        var queryable = await _favoriteRepository.WithDetailsAsync(x => x.Destination);
        var lista = queryable.Where(x => x.UsuarioId == usuarioId).ToList();

        return lista.Select(x => new FavoriteDto
        {
            DestinoId = x.DestinoId,
            UsuarioId = x.UsuarioId,
            Nombre = x.Destination?.Name,         
            Ubicacion = x.Destination?.Country,   
            ImagenUrl = x.Destination?.ImageUrl, 
            Precio = x.Destination?.Price ?? 0
        }).ToList();
    }
}
