using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.SettingManagement;

namespace TravelBuddy.Data;

public class TravelBuddyUserDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string UserRoleName = "Usuario";

    private readonly IdentityRoleManager _roleManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ISettingManager _settingManager;

    public TravelBuddyUserDataSeedContributor(
        IdentityRoleManager roleManager,
        IGuidGenerator guidGenerator,
        ISettingManager settingManager)
    {
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
        _settingManager = settingManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var userRole = await _roleManager.FindByNameAsync(UserRoleName);
        if (userRole == null)
        {
            userRole = new IdentityRole(_guidGenerator.Create(), UserRoleName)
            {
                IsDefault = true,
                IsPublic = true
            };

            var result = await _roleManager.CreateAsync(userRole);
            if (!result.Succeeded)
            {
                throw new AbpException(
                    $"Could not create the '{UserRoleName}' role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        await _settingManager.SetGlobalAsync("Abp.Account.IsSelfRegistrationEnabled", false.ToString());
        await _settingManager.SetGlobalAsync("Abp.Identity.User.IsUserNameUpdateEnabled", false.ToString());
        await _settingManager.SetGlobalAsync("Abp.Identity.User.IsEmailUpdateEnabled", false.ToString());
    }
}
