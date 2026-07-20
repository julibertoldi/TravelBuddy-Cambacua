using TravelBuddy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace TravelBuddy.Permissions;

public class TravelBuddyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TravelBuddyPermissions.GroupName);
        myGroup.AddPermission(TravelBuddyPermissions.Admin.Default, L("Permission:Admin"));
        myGroup.AddPermission(TravelBuddyPermissions.Admin.Metrics, L("Permission:Admin.Metrics"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TravelBuddyResource>(name);
    }
}
