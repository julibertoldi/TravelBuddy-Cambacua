using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
using TravelBuddy.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.GlobalFilters;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Security;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.Uow;

namespace TravelBuddy;

[DependsOn(
    typeof(TravelBuddyApplicationModule),
    typeof(TravelBuddyEntityFrameworkCoreModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpSecurityModule),
    typeof(AbpUnitOfWorkModule)
)]
public class TravelBuddyApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddEntityFrameworkInMemoryDatabase();

        Configure<AbpEfCoreGlobalFilterOptions>(options =>
        {
            options.UseDbFunction = false;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<TravelBuddyDbContext>(dbContextOptions =>
            {
                dbContextOptions.DbContextOptions.UseInMemoryDatabase("TestDb");
            });
        });
    }
}
