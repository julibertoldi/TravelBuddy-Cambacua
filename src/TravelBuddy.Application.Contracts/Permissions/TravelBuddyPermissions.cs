namespace TravelBuddy.Permissions;

public static class TravelBuddyPermissions
{
    public const string GroupName = "TravelBuddy";
    public class Admin
    {
        public const string Default = GroupName + ".Admin";
        public const string Metrics = Default + ".Metrics"; // Para el panel de métricas
    }
}
