using System.ComponentModel.DataAnnotations;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;
using TravelBuddy.Users;

namespace TravelBuddy;

public static class TravelBuddyModuleExtensionConfigurator
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ConfigureExistingProperties();
            ConfigureExtraProperties();
        });
    }

    private static void ConfigureExistingProperties() { }

    private static void ConfigureExtraProperties()
    {
        ObjectExtensionManager.Instance.Modules().ConfigureIdentity(identity =>
        {
            identity.ConfigureUser(user =>
            {
                user.AddOrUpdateProperty<string>(UserExtensionProperties.ProfilePictureUrl,
                p => p.Attributes.Add(new StringLengthAttribute(1000)));
                user.AddOrUpdateProperty<string>(UserExtensionProperties.Preferences,
                p => p.Attributes.Add(new StringLengthAttribute(2000)));
            });
        });
    }
}