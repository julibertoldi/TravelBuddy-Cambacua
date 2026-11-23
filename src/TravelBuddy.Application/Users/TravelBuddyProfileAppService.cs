using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TravelBuddy.Permissions;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Settings;
using Volo.Abp.Users;
using Volo.Abp.Data;

namespace TravelBuddy.Users;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IProfileAppService), typeof(ProfileAppService), typeof(TravelBuddyProfileAppService))]
public class TravelBuddyProfileAppService : ProfileAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IPermissionChecker _permissionChecker;

    public TravelBuddyProfileAppService(
        IdentityUserManager userManager,
        IOptions<IdentityOptions> identityOptions,
        IPermissionChecker permissionChecker)
        : base(userManager, identityOptions)
    {
        _userManager = userManager;
        _permissionChecker = permissionChecker;
    }

    [Authorize]
    public override async Task<ProfileDto> UpdateAsync(UpdateProfileDto input)
    {
        var currentUser = await _userManager.GetByIdAsync(CurrentUser.GetId());
        input.SetProperty(UserExtensionProperties.ProfilePictureUrl, currentUser.GetProfilePicture());

        if (!await _permissionChecker.IsGrantedAsync(TravelBuddyPermissions.Admin.Default))
        {
            if (!string.Equals(currentUser.UserName, input.UserName, StringComparison.InvariantCultureIgnoreCase) ||
                !string.Equals(currentUser.Email, input.Email, StringComparison.InvariantCultureIgnoreCase) ||
                !string.Equals(currentUser.PhoneNumber, input.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UserFriendlyException("Solo se permite modificar nombre, foto y preferencias de notificación.");
            }
        }

        return await base.UpdateAsync(input);
    }
}
