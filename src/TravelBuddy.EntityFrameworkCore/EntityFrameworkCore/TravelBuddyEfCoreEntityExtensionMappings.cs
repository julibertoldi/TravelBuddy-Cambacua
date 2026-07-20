using Microsoft.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;
using TravelBuddy.Users;

namespace TravelBuddy.EntityFrameworkCore;

public static class TravelBuddyEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        TravelBuddyGlobalFeatureConfigurator.Configure();
        TravelBuddyModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, string>(
                    UserExtensionProperties.ProfilePictureUrl,
                    (entityBuilder, propertyBuilder) =>
                    {
                        propertyBuilder.HasMaxLength(1000);
                    }
                )
                .MapEfCoreProperty<IdentityUser, string>(
                    UserExtensionProperties.Preferences,
                    (entityBuilder, propertyBuilder) =>
                    {
                        propertyBuilder.HasMaxLength(2000);
                    }
                );
        });
    }
}
