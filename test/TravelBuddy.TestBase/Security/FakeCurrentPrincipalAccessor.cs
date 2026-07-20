using System.Collections.Generic;
using System.Security.Claims;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace TravelBuddy.Security;

[Dependency(ReplaceServices = true)]
public class FakeCurrentPrincipalAccessor : ThreadCurrentPrincipalAccessor
{
    protected override ClaimsPrincipal GetClaimsPrincipal()
    {
        return GetPrincipal();
    }

    private ClaimsPrincipal GetPrincipal()
    {
        // Crea los datos del usuario simulado que se utilizará en los tests.
        var claims = new List<Claim>
        {
            new Claim(
                AbpClaimTypes.UserId,
                "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"
            ),
            new Claim(AbpClaimTypes.UserName, "admin"),
            new Claim(AbpClaimTypes.Email, "admin@abp.io")
        };

        // "TestAuth" hace que la identidad sea considerada autenticada.
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "TestAuth"
        );

        return new ClaimsPrincipal(identity);
    }
}