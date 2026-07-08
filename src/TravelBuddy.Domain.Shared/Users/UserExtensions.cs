using System.Collections.Generic;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;

namespace TravelBuddy.Users;

public static class UserExtensions
{
    public static string? GetProfilePicture(this IdentityUser user)
    {
        return user.ExtraProperties.GetOrDefault("ProfilePictureUrl") as string;
    }

    public static void SetProfilePicture(this IdentityUser user, string? value)
    {
        user.ExtraProperties["ProfilePictureUrl"] = value;
    }

    public static string? GetPreferences(this IdentityUser user)
    {
        return user.ExtraProperties.GetOrDefault("Preferences") as string;
    }

    public static void SetPreferences(this IdentityUser user, string? value)
    {
        user.ExtraProperties["Preferences"] = value;
    }
}
