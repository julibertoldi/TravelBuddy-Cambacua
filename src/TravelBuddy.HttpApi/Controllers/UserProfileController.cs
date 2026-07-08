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

    [HttpPut("me")]
    public async Task UpdateMyProfileAsync(UpdateUserProfileDto input)
    {
        var userId = CurrentUser.Id;

        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException();
        }

        await _userProfileAppService.UpdateMyProfileAsync(userId.Value, input);
    }

    [HttpGet("{userId}")]
    public async Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId)
    {
        return await _userProfileAppService.GetPublicProfileAsync(userId);
    }
}
