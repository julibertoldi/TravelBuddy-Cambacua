using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Data;

namespace TravelBuddy.Users;

public static class UserExtensions
{
    public static string? GetProfilePicture(this IdentityUser user) => 
        user.GetProperty<string?>(UserExtensionProperties.ProfilePictureUrl);

    public static void SetProfilePicture(this IdentityUser user, string? value) =>
        user.SetProperty(UserExtensionProperties.ProfilePictureUrl, value);

    public static string? GetProfilePictureContentType(this IdentityUser user) =>
        user.GetProperty<string?>(UserExtensionProperties.ProfilePictureContentType);

    public static void SetProfilePictureContentType(this IdentityUser user, string? value) =>
        user.SetProperty(UserExtensionProperties.ProfilePictureContentType, value);

    public static string? GetPreferences(this IdentityUser user) => 
        user.GetProperty<string?>(UserExtensionProperties.Preferences);

    public static void SetPreferences(this IdentityUser user, string? value) =>
        user.SetProperty(UserExtensionProperties.Preferences, value);

    public static string? GetNotificationSettings(this IdentityUser user) =>
        user.GetProperty<string?>(UserExtensionProperties.NotificationSettings);

    public static void SetNotificationSettings(this IdentityUser user, string? value) =>
        user.SetProperty(UserExtensionProperties.NotificationSettings, value);
}