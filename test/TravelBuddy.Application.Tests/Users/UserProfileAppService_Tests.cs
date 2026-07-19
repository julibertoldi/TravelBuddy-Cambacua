using Microsoft.AspNetCore.Identity;
using Shouldly;
using System;
using System.Threading.Tasks;
using TravelBuddy.Users;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Xunit;

namespace TravelBuddy.Users;

public class UserProfileAppService_Tests
    : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IUserProfileAppService _service;
    private readonly IdentityUserManager _userManager;

    public UserProfileAppService_Tests()
    {
        _service = GetRequiredService<IUserProfileAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
    }

    [Fact]
    public async Task Actualizar_El_Perfil_Correctamente()
    {
        // Prepara
        var user = new IdentityUser(Guid.NewGuid(), "juliana", "juliana@test.com");
        (await _userManager.CreateAsync(user)).CheckErrors();

        // ejecuta
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


        var updatedUser = await _userManager.GetByIdAsync(user.Id);

        updatedUser.Name.ShouldBe("Juliana");
        updatedUser.Surname.ShouldBe("Bertoldi");
        updatedUser.Email.ShouldBe("juliana.nuevo@test.com");

        updatedUser.GetProfilePicture().ShouldBe("http://foto.test/juli.png");
        updatedUser.GetPreferences().ShouldBe("playa");
    }

    [Fact]
    public async Task Obtener_El_Perfil_Publico()
    {

        var user = new IdentityUser(Guid.NewGuid(), "ana", "ana@test.com")
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
}
   
