using TravelBuddy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace TravelBuddy.Permissions;

public class TravelBuddyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TravelBuddyPermissions.GroupName);

        var admin = myGroup.AddPermission(
            TravelBuddyPermissions.Admin.Default, L("Permission:Admin"));

        admin.AddChild(
            TravelBuddyPermissions.Admin.Metrics, L("Permission:Admin.Metrics"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TravelBuddyResource>(name);
    }
}