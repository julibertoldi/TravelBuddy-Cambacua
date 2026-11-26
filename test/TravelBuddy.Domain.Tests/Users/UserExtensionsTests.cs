using System;
using Shouldly;
using TravelBuddy.Users;
using Volo.Abp.Identity;
using Xunit;

namespace TravelBuddy.Users.Tests;

public class UserExtensionsTests
{
    [Fact]
    public void UserExtensions_SetGetFoto_RoundTrip()
    {
        var user = CrearUsuario();
        const string foto = "https://example.test/foto.jpg";

        user.SetProfilePicture(foto);

        user.GetProfilePicture().ShouldBe(foto);
    }

    [Fact]
    public void UserExtensions_SetGetTipoDeContenido_RoundTrip()
    {
        var user = CrearUsuario();
        const string contentType = "image/png";

        user.SetProfilePictureContentType(contentType);

        user.GetProfilePictureContentType().ShouldBe(contentType);
    }

    [Fact]
    public void UserExtensions_SetGetPreferencias_RoundTrip()
    {
        var user = CrearUsuario();
        const string preferencias = "playa y montaña";

        user.SetPreferences(preferencias);

        user.GetPreferences().ShouldBe(preferencias);
    }

    [Fact]
    public void UserExtensions_SetFotoNula_DevuelveNulo()
    {
        var user = CrearUsuario();

        user.SetProfilePicture(null);

        user.GetProfilePicture().ShouldBeNull();
    }

    [Fact]
    public void UserExtensions_SetTipoDeContenidoNulo_DevuelveNulo()
    {
        var user = CrearUsuario();

        user.SetProfilePictureContentType(null);

        user.GetProfilePictureContentType().ShouldBeNull();
    }

    [Fact]
    public void UserExtensions_SetPreferenciasNulas_DevuelveNulo()
    {
        var user = CrearUsuario();

        user.SetPreferences(null);

        user.GetPreferences().ShouldBeNull();
    }

    [Fact]
    public void UserExtensions_SetValoresVacios_ConservaCadenasVacias()
    {
        var user = CrearUsuario();

        user.SetProfilePicture(string.Empty);
        user.SetProfilePictureContentType(string.Empty);
        user.SetPreferences(string.Empty);

        user.GetProfilePicture().ShouldBe(string.Empty);
        user.GetProfilePictureContentType().ShouldBe(string.Empty);
        user.GetPreferences().ShouldBe(string.Empty);
    }

    private static IdentityUser CrearUsuario()
    {
        return new IdentityUser(Guid.NewGuid(), "usuario-pruebas", "usuario-pruebas@example.test");
    }
}
