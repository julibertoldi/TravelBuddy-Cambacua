using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TravelBuddy.Users;
using Volo.Abp.Content;
using Volo.Abp.Identity;

namespace TravelBuddy.Controllers;

[Authorize]
[Route("api/admin/profile-pictures")]
public class AdminProfilePictureController : TravelBuddyController
{
    private readonly IUserProfileAppService _userProfileAppService;

    public AdminProfilePictureController(IUserProfileAppService userProfileAppService)
    {
        _userProfileAppService = userProfileAppService;
    }

    [HttpPost("{userId:guid}")]
    [Authorize(IdentityPermissions.Users.Update)]
    public Task<string> UploadAsync(Guid userId, [FromForm] IRemoteStreamContent input) =>
        _userProfileAppService.UploadUserProfilePictureAsync(userId, input);
}
