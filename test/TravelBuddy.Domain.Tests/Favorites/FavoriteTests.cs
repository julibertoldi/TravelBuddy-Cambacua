using System;
using Shouldly;
using TravelBuddy.Favorites;
using Xunit;

namespace TravelBuddy.Favorites.Tests;

public class FavoriteTests
{
    [Fact]
    public void Favorite_Constructor_AsignaUsuarioYDestino()
    {
        var usuarioId = Guid.NewGuid();
        var destinoId = Guid.NewGuid();

        var favorite = new Favorite(usuarioId, destinoId);

        favorite.UsuarioId.ShouldBe(usuarioId);
        favorite.DestinoId.ShouldBe(destinoId);
    }

    [Fact]
    public void Favorite_GetKeys_DevuelveUsuarioYDestinoEnOrden()
    {
        var usuarioId = Guid.NewGuid();
        var destinoId = Guid.NewGuid();
        var favorite = new Favorite(usuarioId, destinoId);

        favorite.GetKeys().ShouldBe(new object[] { usuarioId, destinoId });
    }
}
