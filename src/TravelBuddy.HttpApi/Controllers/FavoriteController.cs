using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelBuddy.Favorites;
using Volo.Abp.AspNetCore.Mvc;

namespace TravelBuddy.Controllers;

[Authorize]
[Route("api/app/favorites")]
public class FavoriteController : TravelBuddyController
{
    private readonly IFavoriteAppService _favoriteAppService;

    public FavoriteController(IFavoriteAppService favoriteAppService)
    {
        _favoriteAppService = favoriteAppService;
    }

    /// <summary>
    /// Agrega un destino a los favoritos del usuario actual.
    /// POST /api/app/favorites/agregar/{destinoId}
    /// </summary>
    [HttpPost("agregar/{destinoId}")]
    [ProducesResponseType(204)]
    public async Task AgregarFavoritoAsync(Guid destinoId)
        => await _favoriteAppService.AgregarFavoritoAsync(destinoId);

    /// <summary>
    /// Quita un destino de los favoritos del usuario actual.
    /// DELETE /api/app/favorites/quitar/{destinoId}
    /// </summary>
    [HttpDelete("quitar/{destinoId}")]
    [ProducesResponseType(204)]
    public async Task QuitarFavoritoAsync(Guid destinoId)
        => await _favoriteAppService.QuitarFavoritoAsync(destinoId);

    /// <summary>
    /// Lista todos los favoritos del usuario autenticado.
    /// GET /api/app/favorites
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FavoriteDto>), 200)]
    public async Task<List<FavoriteDto>> ObtenerMisFavoritosAsync()
        => await _favoriteAppService.ObtenerMisFavoritosAsync();
}

