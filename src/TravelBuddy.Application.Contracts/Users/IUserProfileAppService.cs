using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TravelBuddy.Users;

public interface IUserProfileAppService : IApplicationService
{
    Task UpdateMyProfileAsync(Guid userId, UpdateUserProfileDto input);

    Task<UserProfileDto> GetMyProfileAsync(Guid userId);

    Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId);

    Task DeleteMyAccountAsync(Guid userId);
}
