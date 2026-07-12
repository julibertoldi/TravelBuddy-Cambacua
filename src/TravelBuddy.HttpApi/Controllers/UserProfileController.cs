using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBuddy.Users;
using Volo.Abp.AspNetCore.Mvc;

namespace TravelBuddy.Controllers;

[Authorize]
[Route("api/user-profile")]
public class UserProfileController : TravelBuddyController
{
    private readonly IUserProfileAppService _userProfileAppService;

    public UserProfileController(IUserProfileAppService userProfileAppService)
    {
        _userProfileAppService = userProfileAppService;
    }

    [HttpDelete("me")]
    public async Task DeleteMyAccountAsync()
    {
        if (!CurrentUser.Id.HasValue)
            throw new UnauthorizedAccessException();

        await _userProfileAppService.DeleteMyAccountAsync(CurrentUser.Id.Value);
    }

    [HttpGet("{userId}")]
    public async Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId)
    {
        return await _userProfileAppService.GetPublicProfileAsync(userId);
    }
}
