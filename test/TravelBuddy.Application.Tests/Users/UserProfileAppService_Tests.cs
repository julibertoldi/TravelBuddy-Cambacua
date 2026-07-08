using Microsoft.AspNetCore.Identity;
using Shouldly;
using System;
using System.Threading.Tasks;
using TravelBuddy.Users;
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
    public async Task Should_Update_User_Profile()
    {
        var user = new IdentityUser(Guid.NewGuid(), "juliana", "juliana@test.com");
        (await _userManager.CreateAsync(user)).CheckErrors();

        await _service.UpdateMyProfileAsync(
            user.Id,
            new UpdateUserProfileDto
            {
                Nombre = "Juliana",
                Apellido = "Bertoldi",
                FotoPerfilUrl = "http://foto.test/juli.png",
                Preferencias = "playa"
            }
        );

        var updatedUser = await _userManager.GetByIdAsync(user.Id);

        updatedUser.Name.ShouldBe("Juliana");
        updatedUser.Surname.ShouldBe("Bertoldi");
    }

    [Fact]
    public async Task Should_Get_Public_Profile()
    {
        var user = new IdentityUser(Guid.NewGuid(), "ana", "ana@test.com")
        {
            Name = "Ana",
            Surname = "Gomez"
        };
        (await _userManager.CreateAsync(user)).CheckErrors();

        var profile = await _service.GetPublicProfileAsync(user.Id);

        profile.Nombre.ShouldBe("Ana");
        profile.Apellido.ShouldBe("Gomez");
    }
}
