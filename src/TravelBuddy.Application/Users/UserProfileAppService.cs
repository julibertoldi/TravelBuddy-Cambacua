using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;

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

        user.SetProperty("FotoPerfilUrl", input.FotoPerfilUrl);
        user.SetProperty("Preferencias", input.Preferencias);

        var result = await _userManager.UpdateAsync(user);
        result.CheckErrors();
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
            FotoPerfilUrl = user.GetProperty<string>("FotoPerfilUrl")
        };
    }

    [Authorize]
    public async Task DeleteMyAccountAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);

        var result = await _userManager.DeleteAsync(user);
        result.CheckErrors();
    }
}
