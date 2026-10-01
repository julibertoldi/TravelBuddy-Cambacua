using Microsoft.AspNetCore.Identity;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelBuddy.Users;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Xunit;

namespace TravelBuddy.Users;

public class UserProfileAppService_Tests
    : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IUserProfileAppService _service;
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public UserProfileAppService_Tests()
    {
        _service = GetRequiredService<IUserProfileAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable LoginAs(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "testuser"),
            new Claim(AbpClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return _currentPrincipalAccessor.Change(new ClaimsPrincipal(identity));
    }

    [Fact]
    public async Task Actualizar_El_Perfil_Correctamente()
    {
        // Prepara
        var user = new IdentityUser(Guid.NewGuid(), "juliana", "juliana@test.com");
        (await _userManager.CreateAsync(user)).CheckErrors();

        // ejecuta
        using (LoginAs(user.Id))
        {
            await _service.UpdateMyProfileAsync(
                user.Id,
                new UpdateUserProfileDto
                {
                    Nombre = "Juliana",
                    Apellido = "Bertoldi",
                    Email = "juliana.nuevo@test.com",
                    FotoPerfilUrl = "http://foto.test/juli.png",
                    Preferencias = "playa"
                });
        }

        var updatedUser = await _userManager.GetByIdAsync(user.Id);

        updatedUser.UserName.ShouldBe("juliana");
        updatedUser.Name.ShouldBe("Juliana");
        updatedUser.Surname.ShouldBe("Bertoldi");
        updatedUser.Email.ShouldBe("juliana.nuevo@test.com");

        updatedUser.GetProfilePicture().ShouldBe("http://foto.test/juli.png");
        updatedUser.GetPreferences().ShouldBe("playa");
    }

    [Fact]
    public async Task Obtener_El_Perfil_Completo()
    {
        var id = Guid.NewGuid();
        var user = new IdentityUser(id, "user_" + id.ToString("N")[..8], "user_" + id.ToString("N")[..8] + "@test.com")
        {
            Name = "Ana",
            Surname = "Gomez"
        };
        user.SetProfilePicture("https://example.com/foto.jpg");
        user.SetPreferences("montaña y playa");

        (await _userManager.CreateAsync(user)).CheckErrors();

        using (LoginAs(user.Id))
        {
            var profile = await _service.GetMyProfileAsync(user.Id);

            profile.UserId.ShouldBe(user.Id);
            profile.UserName.ShouldBe(user.UserName);
            profile.Nombre.ShouldBe("Ana");
            profile.Apellido.ShouldBe("Gomez");
            profile.Email.ShouldBe(user.Email);
            profile.FotoPerfilUrl.ShouldBe("https://example.com/foto.jpg");
            profile.Preferencias.ShouldBe("montaña y playa");
        }
    }

    [Fact]
    public async Task Obtener_El_Perfil_Publico()
    {
        var id = Guid.NewGuid();
        var user = new IdentityUser(id, "user_" + id.ToString("N")[..8], "user_" + id.ToString("N")[..8] + "@test.com")
        {
            Name = "Ana",
            Surname = "Gomez"
        };

        (await _userManager.CreateAsync(user)).CheckErrors();

        var profile = await _service.GetPublicProfileAsync(user.Id);

        profile.UserId.ShouldBe(user.Id);
        profile.Nombre.ShouldBe("Ana");
        profile.Apellido.ShouldBe("Gomez");
    }

    [Fact]
    public async Task NombreDeUsuario_DebePermanecerInmutable_AlActualizarPerfil()
    {
        var initialUsername = "inmutableUser";
        var user = new IdentityUser(Guid.NewGuid(), initialUsername, "inmutable@test.com")
        {
            Name = "NombreOriginal",
            Surname = "ApellidoOriginal"
        };
        (await _userManager.CreateAsync(user)).CheckErrors();

        using (LoginAs(user.Id))
        {
            await _service.UpdateMyProfileAsync(
                user.Id,
                new UpdateUserProfileDto
                {
                    Nombre = "NuevoNombre",
                    Apellido = "NuevoApellido",
                    Email = "nuevo.email@test.com"
                });

            var profile = await _service.GetMyProfileAsync(user.Id);

            // El username se mantiene inalterado y de solo lectura
            profile.UserName.ShouldBe(initialUsername);
            profile.Nombre.ShouldBe("NuevoNombre");
            profile.Apellido.ShouldBe("NuevoApellido");
        }

        var persistedUser = await _userManager.GetByIdAsync(user.Id);
        persistedUser.UserName.ShouldBe(initialUsername);
    }
}
   
