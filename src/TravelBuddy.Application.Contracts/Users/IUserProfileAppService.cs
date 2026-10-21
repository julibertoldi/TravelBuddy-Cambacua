using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Users;

public interface IUserProfileAppService : IApplicationService
{
    Task UpdateMyProfileAsync(UpdateUserProfileDto input);

    Task<UserProfileDto> GetMyProfileAsync();

    Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId);

}
