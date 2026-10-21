using Volo.Abp.Settings;

namespace TravelBuddy.Settings;

public class TravelBuddySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        Disable(context, "Abp.Identity.User.IsUserNameUpdateEnabled");
        Disable(context, "Abp.Identity.User.IsEmailUpdateEnabled");
    }

    private static void Disable(ISettingDefinitionContext context, string name)
    {
        var setting = context.GetOrNull(name);
        if (setting != null)
        {
            setting.DefaultValue = false.ToString();
        }
    }
}