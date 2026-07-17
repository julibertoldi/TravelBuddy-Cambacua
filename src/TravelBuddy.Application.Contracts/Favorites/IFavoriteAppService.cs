using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Favorites;

public interface IFavoriteAppService : IApplicationService
{
    Task AgregarFavoritoAsync(Guid destinoId);
    Task QuitarFavoritoAsync(Guid destinoId);
    Task<List<FavoriteDto>> ObtenerMisFavoritosAsync();
}