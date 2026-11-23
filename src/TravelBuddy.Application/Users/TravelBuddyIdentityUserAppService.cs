using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Data;

namespace TravelBuddy.Users;

[Dependency(ReplaceServices = true)]
[ExposeServices(
    typeof(IIdentityUserAppService),
    typeof(IdentityUserAppService),
    typeof(TravelBuddyIdentityUserAppService))]
public class TravelBuddyIdentityUserAppService : IdentityUserAppService
{
    public TravelBuddyIdentityUserAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IOptions<IdentityOptions> identityOptions,
        IPermissionChecker permissionChecker)
        : base(userManager, userRepository, roleRepository, identityOptions, permissionChecker)
    {
    }

    [Authorize(IdentityPermissions.Users.Create)]
    public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        var submittedProfilePicture = input.GetProperty<string?>(UserExtensionProperties.ProfilePictureUrl);
        if (!string.IsNullOrWhiteSpace(submittedProfilePicture))
        {
            throw new UserFriendlyException("La foto de perfil debe cargarse mediante la acción de subida.");
        }

        input.ExtraProperties.Remove(UserExtensionProperties.ProfilePictureUrl);
        return await base.CreateAsync(input);
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public override async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        var existing = await UserManager.GetByIdAsync(id);

        if (!string.Equals(existing.UserName, input.UserName, StringComparison.Ordinal))
        {
            throw new UserFriendlyException("El nombre de usuario no se puede modificar.");
        }

        if (input.ExtraProperties.TryGetValue(UserExtensionProperties.ProfilePictureUrl, out var submittedProfilePicture))
        {
            if (!string.Equals(
                    submittedProfilePicture as string,
                    existing.GetProfilePicture(),
                    StringComparison.Ordinal))
            {
                throw new UserFriendlyException("La foto de perfil debe modificarse mediante la acción de subida.");
            }

            input.ExtraProperties.Remove(UserExtensionProperties.ProfilePictureUrl);
        }

        return await base.UpdateAsync(id, input);
    }
}