using TravelBuddy.Localization;
using Volo.Abp.Application.Services;

namespace TravelBuddy;


public abstract class TravelBuddyAppService : ApplicationService
{
    protected TravelBuddyAppService()
    {
        LocalizationResource = typeof(TravelBuddyResource);
    }
}
