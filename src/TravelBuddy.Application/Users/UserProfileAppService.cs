using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace TravelBuddy.Users;

[Authorize]
public class UserProfileAppService : ApplicationService, IUserProfileAppService
{
    private const int MaxProfilePictureSize = 2 * 1024 * 1024;
    private const string ProfilePictureRoute = "/api/profile-pictures";

    private readonly IdentityUserManager _userManager;
    private readonly IBlobContainer<ProfilePictureContainer> _blobContainer;

    public UserProfileAppService(
        IdentityUserManager userManager,
        IBlobContainer<ProfilePictureContainer> blobContainer)
    {
        _userManager = userManager;
        _blobContainer = blobContainer;
    }

    public async Task UpdateMyProfileAsync(UpdateUserProfileDto input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());

        user.Name = input.Nombre;
        user.Surname = input.Apellido;
        user.SetPreferences(input.Preferencias);

        (await _userManager.UpdateAsync(user)).CheckErrors();
    }

    public async Task<UserProfileDto> GetMyProfileAsync()
    {
        var currentUserId = CurrentUser.GetId();

        var user = await _userManager.GetByIdAsync(currentUserId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), currentUserId);

        return new UserProfileDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Nombre = user.Name ?? string.Empty,
            Apellido = user.Surname ?? string.Empty,
            FotoPerfilUrl = GetOwnedProfilePictureUrl(user),
            Preferencias = user.GetPreferences()
        };
    }

    public async Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId)
    {
        var user = await _userManager.GetByIdAsync(userId)
                   ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);

        return new PublicUserProfileDto
        {
            UserId = user.Id,
            Nombre = user.Name,
            Apellido = user.Surname,
            FotoPerfilUrl = GetOwnedProfilePictureUrl(user)
        };
    }

    public async Task<string> UploadMyProfilePictureAsync(IRemoteStreamContent input)
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        return await ReplaceProfilePictureAsync(user, input);
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public async Task<string> UploadUserProfilePictureAsync(Guid userId, IRemoteStreamContent input)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), userId);
        }

        return await ReplaceProfilePictureAsync(user, input);
    }

    public async Task DeleteMyProfilePictureAsync()
    {
        var user = await _userManager.GetByIdAsync(CurrentUser.GetId());
        var oldBlobName = GetOwnedBlobName(user);

        user.SetProfilePicture(null);
        user.SetProfilePictureContentType(null);
        (await _userManager.UpdateAsync(user)).CheckErrors();

        if (oldBlobName != null)
        {
            await _blobContainer.DeleteAsync(oldBlobName);
        }
    }

    [AllowAnonymous]
    public async Task<IRemoteStreamContent> GetProfilePictureAsync(Guid userId, string blobName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var contentType = user?.GetProfilePictureContentType();
        if (user == null ||
            !string.Equals(GetOwnedBlobName(user), blobName, StringComparison.Ordinal) ||
            !IsAllowedContentType(contentType))
        {
            throw ProfilePictureNotFound();
        }

        var stream = await _blobContainer.GetOrNullAsync(blobName);
        if (stream == null)
        {
            throw ProfilePictureNotFound();
        }

        return new RemoteStreamContent(
            stream,
            $"{blobName}{GetFileExtension(contentType!)}",
            contentType,
            stream.CanSeek ? stream.Length : null);
    }

    private async Task<string> ReplaceProfilePictureAsync(IdentityUser user, IRemoteStreamContent input)
    {
        var validatedPicture = await ValidateAndReadPictureAsync(input);
        var oldBlobName = GetOwnedBlobName(user);
        var newBlobName = Guid.NewGuid().ToString("N");

        await using var pictureStream = new MemoryStream(validatedPicture.Content);
        await _blobContainer.SaveAsync(newBlobName, pictureStream, overrideExisting: false);

        user.SetProfilePicture(BuildProfilePictureUrl(user.Id, newBlobName));
        user.SetProfilePictureContentType(validatedPicture.ContentType);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            await _blobContainer.DeleteAsync(newBlobName);
            updateResult.CheckErrors();
        }

        if (oldBlobName != null && !string.Equals(oldBlobName, newBlobName, StringComparison.Ordinal))
        {
            await _blobContainer.DeleteAsync(oldBlobName);
        }

        return BuildProfilePictureUrl(user.Id, newBlobName);
    }

    private static async Task<(byte[] Content, string ContentType)> ValidateAndReadPictureAsync(
        IRemoteStreamContent input)
    {
        var extension = Path.GetExtension(input.FileName ?? string.Empty).ToLowerInvariant();
        var expectedContentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new UserFriendlyException("Solo se permiten imágenes JPEG, PNG o WebP.")
        };

        if (!string.Equals(input.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("El tipo de contenido no coincide con la extensión del archivo.");
        }

        if (input.ContentLength > MaxProfilePictureSize)
        {
            throw new UserFriendlyException("La imagen no puede superar los 2 MiB.");
        }

        var content = await ReadLimitedAsync(input.GetStream());
        if (!HasValidSignature(content, extension))
        {
            throw new UserFriendlyException("La firma del archivo no corresponde a una imagen permitida.");
        }

        return (content, expectedContentType);
    }

    private static async Task<byte[]> ReadLimitedAsync(Stream source)
    {
        await using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(chunk.AsMemory(0, chunk.Length))) > 0)
        {
            if (buffer.Length + bytesRead > MaxProfilePictureSize)
            {
                throw new UserFriendlyException("La imagen no puede superar los 2 MiB.");
            }

            await buffer.WriteAsync(chunk.AsMemory(0, bytesRead));
        }

        return buffer.ToArray();
    }

    private static bool HasValidSignature(byte[] content, string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" =>
                content.Length >= 3 &&
                content[0] == 0xFF &&
                content[1] == 0xD8 &&
                content[2] == 0xFF,
            ".png" =>
                content.Length >= 8 &&
                content.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" =>
                content.Length >= 12 &&
                content.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                content.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private static string? GetOwnedProfilePictureUrl(IdentityUser user)
    {
        var blobName = GetOwnedBlobName(user);
        return blobName == null ? null : BuildProfilePictureUrl(user.Id, blobName);
    }

    private static string? GetOwnedBlobName(IdentityUser user)
    {
        var prefix = $"{ProfilePictureRoute}/{user.Id:D}/";
        var url = user.GetProfilePicture();
        if (url == null || !url.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var blobName = url[prefix.Length..];
        return Guid.TryParseExact(blobName, "N", out _) ? blobName : null;
    }

    private static string BuildProfilePictureUrl(Guid userId, string blobName) =>
        $"{ProfilePictureRoute}/{userId:D}/{blobName}";

    private static string GetFileExtension(string contentType) =>
        contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw ProfilePictureNotFound()
        };

    private static bool IsAllowedContentType(string? contentType) =>
        contentType is "image/jpeg" or "image/png" or "image/webp";

    private static EntityNotFoundException ProfilePictureNotFound() =>
        new(typeof(IdentityUser), Guid.Empty);
}