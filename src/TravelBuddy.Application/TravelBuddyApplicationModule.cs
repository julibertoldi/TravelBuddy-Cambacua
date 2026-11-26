using Microsoft.Extensions.DependencyInjection;
using TravelBuddy.Cities;
using TravelBuddy.Calificaciones;
using TravelBuddy.Favorites;
using TravelBuddy.Infraestructure;
using TravelBuddy.Notifications;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Auditing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp;


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
    public class TravelBuddyApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAuditingOptions>(options =>
            {
                options.EntityHistorySelectors.Add(
                    new NamedTypeSelector(
                        "TravelBuddy",
                        entityType => entityType == typeof(Favorite) ||
                                      entityType == typeof(Calificacion)));
                options.EntityHistorySelectors.Add(
                    new NamedTypeSelector(
                        "IdentityUserRole",
                        entityType => entityType == typeof(IdentityUserRole)));
            });

            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<TravelBuddyApplicationModule>();
            });

            // 1. Registramos el handler en el contenedor de dependencias
            context.Services.AddTransient<GeoDbMetricsHandler>();

            // 2. Asociamos el handler al cliente HTTP de GeoDB
            context.Services.AddHttpClient<ICitySearchService, GeoDbCitySearchService>()
                .AddHttpMessageHandler<GeoDbMetricsHandler>();

            context.Services.AddHttpClient();
            context.Services.AddHostedService<NotificationBackgroundWorker>();
        }
    }
}



