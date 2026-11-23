using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shouldly;
using TravelBuddy.Users;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Xunit;

namespace TravelBuddy.Users;

public class UserProfilePictureAppService_Tests
    : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IUserProfileAppService _service;
    private readonly IdentityUserManager _userManager;
    private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public UserProfilePictureAppService_Tests()
    {
        _service = GetRequiredService<IUserProfileAppService>();
        _userManager = GetRequiredService<IdentityUserManager>();
        _blobContainer = GetRequiredService<IBlobContainer<ProfilePictureContainer>>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    [Fact]
    public async Task Rechaza_un_tipo_no_permitido()
    {
        var user = await CreateUserAsync();
        using (LoginAs(user.Id))
        {
            await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() =>
                _service.UploadMyProfilePictureAsync(CreateFile("image.svg", "image/svg+xml", [0x3C, 0x73, 0x76])));
        }
    }

    [Fact]
    public async Task Rechaza_un_archivo_mayor_a_2_MiB()
    {
        var user = await CreateUserAsync();
        var content = new byte[2 * 1024 * 1024 + 1];
        content[0] = 0xFF;
        content[1] = 0xD8;
        content[2] = 0xFF;

        using (LoginAs(user.Id))
        {
            await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() =>
                _service.UploadMyProfilePictureAsync(CreateFile(
                    "large.jpg",
                    "image/jpeg",
                    content,
                    declaredContentLength: 0)));
        }
    }

    [Fact]
    public async Task Rechaza_un_Content_Type_que_no_coincide_con_la_extension()
    {
        var user = await CreateUserAsync();
        using (LoginAs(user.Id))
        {
            await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() =>
                _service.UploadMyProfilePictureAsync(
                    CreateFile("image.png", "image/jpeg", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])));
        }
    }

    [Fact]
    public async Task Rechaza_firma_falsa_aunque_la_extension_sea_jpg()
    {
        var user = await CreateUserAsync();
        using (LoginAs(user.Id))
        {
            await Should.ThrowAsync<Volo.Abp.UserFriendlyException>(() =>
                _service.UploadMyProfilePictureAsync(CreateFile("fake.jpg", "image/jpeg", [0x00, 0x01, 0x02])));
        }
    }

    [Fact]
    public async Task Acepta_firmas_JPEG_y_PNG_y_borra_el_blob_anterior_al_reemplazar()
    {
        var user = await CreateUserAsync();
        using (LoginAs(user.Id))
        {
            await _service.UploadMyProfilePictureAsync(
                CreateFile("first.jpg", "image/jpeg", [0xFF, 0xD8, 0xFF, 0xD9]));
            var userAfterJpeg = await _userManager.GetByIdAsync(user.Id);
            var oldBlobName = GetBlobName(userAfterJpeg.GetProfilePicture());
            (await _blobContainer.ExistsAsync(oldBlobName)).ShouldBeTrue();

            await _service.UploadMyProfilePictureAsync(
                CreateFile("second.png", "image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]));

            var replacedUser = await _userManager.GetByIdAsync(user.Id);
            var newBlobName = GetBlobName(replacedUser.GetProfilePicture());
            newBlobName.ShouldNotBe(oldBlobName);
            replacedUser.GetProfilePictureContentType().ShouldBe("image/png");
            (await _blobContainer.ExistsAsync(oldBlobName)).ShouldBeFalse();
            (await _blobContainer.ExistsAsync(newBlobName)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task Acepta_un_archivo_WebP_con_firma_RIFF_WEBP()
    {
        var user = await CreateUserAsync();
        using (LoginAs(user.Id))
        {
            await _service.UploadMyProfilePictureAsync(
                CreateFile(
                    "photo.webp",
                    "image/webp",
                    [0x52, 0x49, 0x46, 0x46, 0x04, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50]));
        }

        var updatedUser = await _userManager.GetByIdAsync(user.Id);
        updatedUser.GetProfilePictureContentType().ShouldBe("image/webp");
    }

    [Fact]
    public async Task Quitar_foto_externa_no_intenta_eliminar_un_blob()
    {
        var user = await CreateUserAsync();
        user.SetProfilePicture("https://example.test/avatar.jpg");
        (await _userManager.UpdateAsync(user)).CheckErrors();

        using (LoginAs(user.Id))
        {
            await _service.DeleteMyProfilePictureAsync();
        }

        var updatedUser = await _userManager.GetByIdAsync(user.Id);
        updatedUser.GetProfilePicture().ShouldBeNull();
        updatedUser.GetProfilePictureContentType().ShouldBeNull();
    }

    private async Task<IdentityUser> CreateUserAsync()
    {
        var id = Guid.NewGuid();
        var user = new IdentityUser(id, $"picture_{id:N}"[..16], $"{id:N}@test.com");
        (await _userManager.CreateAsync(user)).CheckErrors();
        return user;
    }

    private IDisposable LoginAs(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(AbpClaimTypes.UserId, userId.ToString()),
            new(AbpClaimTypes.UserName, "pictureuser"),
            new(AbpClaimTypes.Email, "picture@test.com")
        };
        return _currentPrincipalAccessor.Change(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
    }

    private static IRemoteStreamContent CreateFile(
        string fileName,
        string contentType,
        byte[] content,
        long? declaredContentLength = null) =>
        new RemoteStreamContent(
            new MemoryStream(content),
            fileName,
            contentType,
            declaredContentLength ?? content.Length);

    private static string GetBlobName(string? profilePictureUrl) =>
        profilePictureUrl!.Split('/')[^1];
}
