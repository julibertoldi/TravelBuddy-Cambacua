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

public class UserProfileAppService : ApplicationService, IUserProfileAppService
{
    private readonly IdentityUserManager _userManager;

    public UserProfileAppService(IdentityUserManager userManager)
    {
        _userManager = userManager;
    }
   
    [Authorize]
    public async Task UpdateMyProfileAsync(Guid userId, UpdateUserProfileDto input)
    {
        var user = await _userManager.GetByIdAsync(userId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);

        user.Name = input.Nombre;
        user.Surname = input.Apellido;
        user.SetProfilePicture(input.FotoPerfilUrl);
        user.SetPreferences(input.Preferencias);

        if (!string.IsNullOrWhiteSpace(input.Email) &&
            !string.Equals(user.Email, input.Email, StringComparison.OrdinalIgnoreCase))
        {
            (await _userManager.SetEmailAsync(user, input.Email)).CheckErrors();
        }

        var result = await _userManager.UpdateAsync(user);
        result.CheckErrors();
    }

    [Authorize]
    public async Task<UserProfileDto> GetMyProfileAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);

        return new UserProfileDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Nombre = user.Name ?? string.Empty,
            Apellido = user.Surname ?? string.Empty,
            Email = user.Email ?? string.Empty,
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

    [Authorize]
    public async Task DeleteMyAccountAsync(Guid userId)
    {
        var currentUserId = CurrentUser.GetId();
        if (userId != currentUserId)
        {
            throw new AbpAuthorizationException("No estás autorizado para eliminar esta cuenta.");
        }

        var user = await _userManager.GetByIdAsync(currentUserId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), currentUserId);

        var result = await _userManager.DeleteAsync(user);
        result.CheckErrors();
    }
}
