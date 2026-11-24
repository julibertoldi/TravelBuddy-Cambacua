using System;
﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.InMemory;
using TravelBuddy.EntityFrameworkCore;
using TravelBuddy.Users;
using Volo.Abp.Autofac;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BlobStoring;
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
        context.Services.Replace(
            ServiceDescriptor.Singleton<IBlobContainer<ProfilePictureContainer>, InMemoryProfilePictureBlobContainer>());

        Configure<AbpEfCoreGlobalFilterOptions>(options =>
        {
            options.UseDbFunction = false;
        });

        var databaseName = $"TestDb-{Guid.NewGuid():N}";
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<TravelBuddyDbContext>(dbContextOptions =>
            {
                dbContextOptions.DbContextOptions.UseInMemoryDatabase(databaseName);
            });
            options.Configure<IdentityDbContext>(dbContextOptions =>
            {
                dbContextOptions.DbContextOptions.UseInMemoryDatabase($"{databaseName}-identity");
            });
            options.Configure<AbpAuditLoggingDbContext>(dbContextOptions =>
            {
                dbContextOptions.DbContextOptions.UseInMemoryDatabase($"{databaseName}-audit");
            });
        });
    }
}
