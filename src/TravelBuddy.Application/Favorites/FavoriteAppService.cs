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
[RemoteService(IsEnabled = false)] // El FavoriteController expone los endpoints HTTP explícitamente
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
        var queryable = await _favoriteRepository.GetQueryableAsync();
        var lista = queryable.Where(x => x.UsuarioId == usuarioId).ToList();

        return lista.Select(x => new FavoriteDto
        {
            UsuarioId = x.UsuarioId,
            DestinoId = x.DestinoId
        }).ToList();
    }
}
