using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelBuddy.Users;

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
        if (!CurrentUser.Id.HasValue)
            throw new UnauthorizedAccessException();

        await _userProfileAppService.UpdateMyProfileAsync(CurrentUser.Id.Value, input);
    }

    [HttpGet("me")]
    public async Task<PublicUserProfileDto> GetMyProfileAsync()
    {
        if (!CurrentUser.Id.HasValue)
            throw new UnauthorizedAccessException();

        return await _userProfileAppService.GetPublicProfileAsync(CurrentUser.Id.Value);
    }

    [HttpDelete("me")]
    public async Task DeleteMyAccountAsync()
    {
        if (!CurrentUser.Id.HasValue)
            throw new UnauthorizedAccessException();

        await _userProfileAppService.DeleteMyAccountAsync(CurrentUser.Id.Value);
    }
}