using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TravelBuddy.Users;

namespace TravelBuddy.Controllers;

[AllowAnonymous]
[Route("api/profile-pictures")]
public class ProfilePictureController : TravelBuddyController
{
    private readonly IUserProfileAppService _userProfileAppService;

    public ProfilePictureController(IUserProfileAppService userProfileAppService)
    {
        _userProfileAppService = userProfileAppService;
    }

    [HttpGet("{userId:guid}/{blobName}")]
    public async Task<IActionResult> GetAsync(Guid userId, string blobName)
    {
        var image = await _userProfileAppService.GetProfilePictureAsync(userId, blobName);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.GetStream(), image.ContentType ?? "application/octet-stream");
    }
}
