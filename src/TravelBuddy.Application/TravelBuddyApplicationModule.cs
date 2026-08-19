using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TravelBuddy.Cities;
using TravelBuddy.Infraestructure;
using TravelBuddy.Workers;
using Volo.Abp; //  para usar ApplicationInitializationContext
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;

namespace TravelBuddy
{
    [DependsOn(
        typeof(TravelBuddyDomainModule),
        typeof(TravelBuddyApplicationContractsModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpFeatureManagementApplicationModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpAccountApplicationModule),
        typeof(AbpSettingManagementApplicationModule),
        typeof(AbpBackgroundWorkersModule)
    )]
    public class TravelBuddyApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
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
        }

        // Para registrar y asrrancar EL WORKER
        public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            // Registramos y ponemos en marcha el Worker en segundo plano
            await context.AddBackgroundWorkerAsync<FavoriteCheckWorker>();
        }
    }
}


