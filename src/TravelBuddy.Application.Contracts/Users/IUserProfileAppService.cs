using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace TravelBuddy.Users;

public interface IUserProfileAppService : IApplicationService
{
    Task UpdateMyProfileAsync(UpdateUserProfileDto input);

    Task<UserProfileDto> GetMyProfileAsync();

    Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId);

    Task<ProfilePictureUploadResultDto> UploadMyProfilePictureAsync(IRemoteStreamContent input);

    [RemoteService(IsEnabled = false)]
    Task<ProfilePictureUploadResultDto> UploadUserProfilePictureAsync(Guid userId, IRemoteStreamContent input);

    Task DeleteMyProfilePictureAsync();

    [RemoteService(IsEnabled = false)]
    Task<IRemoteStreamContent> GetProfilePictureAsync(Guid userId, string blobName);
}
