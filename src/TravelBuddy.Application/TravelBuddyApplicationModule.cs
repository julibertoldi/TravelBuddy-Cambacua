using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using TravelBuddy.Cities;
using Microsoft.Extensions.DependencyInjection;


namespace TravelBuddy
{
    // La clase debe HEREDAR de AbpModule y tener sus llaves de inicio/fin
    [DependsOn(
        typeof(TravelBuddyDomainModule),
        typeof(TravelBuddyApplicationContractsModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpFeatureManagementApplicationModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpAccountApplicationModule),
        typeof(AbpSettingManagementApplicationModule)
    )]
    public class TravelBuddyApplicationModule : AbpModule // <-- Clase de Módulo
    {
        public override void ConfigureServices(ServiceConfigurationContext context) // <-- Método dentro de la clase
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<TravelBuddyApplicationModule>();
            });

            // Esto configura la inyección de dependencia para HttpClient
            context.Services.AddHttpClient<ICitySearchService, GeoDbCitySearchService>();
        }
    }
}



