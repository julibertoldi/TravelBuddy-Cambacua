using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace TravelBuddy.Users;

[Authorize]
public class UserProfileAppService : ApplicationService, IUserProfileAppService
{
    private readonly IdentityUserManager _userManager;

    public UserProfileAppService(IdentityUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task UpdateMyProfileAsync(UpdateUserProfileDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());

        user.Name = input.Nombre;
        user.Surname = input.Apellido;
        user.SetProfilePicture(input.FotoPerfilUrl);
        user.SetPreferences(input.Preferencias);

        (await _userManager.UpdateAsync(user)).CheckErrors();
    }

    public async Task<UserProfileDto> GetMyProfileAsync()
    {
        var currentUserId = CurrentUser.GetId();

        var user = await _userManager.GetByIdAsync(currentUserId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), currentUserId);

        return new UserProfileDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Nombre = user.Name ?? string.Empty,
            Apellido = user.Surname ?? string.Empty,
            FotoPerfilUrl = user.GetProfilePicture(),
            Preferencias = user.GetPreferences()
        };
    }
   public async Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);

        return new PublicUserProfileDto
        {
            UserId = user.Id,
            Nombre = user.Name,
            Apellido = user.Surname,
            FotoPerfilUrl = user.GetProfilePicture()
        };
    }
}