using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using TravelBuddy.Destinations;
using TravelBuddy.Favorites;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;
using Xunit;

namespace TravelBuddy.Favorites;

public class FavoriteAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly IFavoriteAppService _favoriteAppService;
    private readonly IRepository<Destination, Guid> _destinationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    // IDs de destinos
    private readonly Guid _destinoCanabacuaId = Guid.Parse("053ad7de-baf1-55de-dec4-3a1d69591b3e");
    private readonly Guid _destinoMarDelPlataId = Guid.Parse("eebc0c82-3b40-ecb8-5751-3a1d6be140fa");

    // ID falso para simular el usuario logueado
    private readonly Guid _usuarioSimuladoId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    public FavoriteAppService_Tests()
    {
        _favoriteAppService = GetRequiredService<IFavoriteAppService>();
        _destinationRepository = GetRequiredService<IRepository<Destination, Guid>>();
        _currentUser = GetRequiredService<ICurrentUser>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable LoginUsuarioSimulado()
    {
        var claims = new[]
        {
            new Claim(AbpClaimTypes.UserId, _usuarioSimuladoId.ToString()),
            new Claim(AbpClaimTypes.UserName, "juliana")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return _currentPrincipalAccessor.Change(claimsPrincipal);
    }

    private Task SeedDestinationAsync(Guid id, string name, DateTime? lastUpdated = null)
    {
        return _destinationRepository.InsertAsync(
            new Destination(id, name, "Description", "Region", "Country")
            {
                LastUpdated = lastUpdated ?? DateTime.UtcNow
            },
            autoSave: true);
    }

    [Fact]
    public async Task Agregar_Un_Destino_A_Favoritos()
    {
        await SeedDestinationAsync(_destinoCanabacuaId, "Canabacua");

        using (LoginUsuarioSimulado())
        {
            _currentUser.IsAuthenticated.ShouldBeTrue();
            _currentUser.Id.ShouldBe(_usuarioSimuladoId);

            // Agregamos Canabacua
            await _favoriteAppService.AgregarFavoritoAsync(_destinoCanabacuaId);

            // Ver que exista en la lista
            var favoritos = await _favoriteAppService.ObtenerMisFavoritosAsync();
            favoritos.ShouldNotBeEmpty();
            favoritos.Count.ShouldBe(1);
            favoritos[0].DestinoId.ShouldBe(_destinoCanabacuaId);
        }
    }

    [Fact]
    public async Task No_Debe_Duplicar_Favorito_Si_Ya_Existe()
    {
        await SeedDestinationAsync(_destinoMarDelPlataId, "Mar del Plata");

        using (LoginUsuarioSimulado())
        {
            // Act: Se agrega el mismo destino dos veces
            await _favoriteAppService.AgregarFavoritoAsync(_destinoMarDelPlataId);
            await _favoriteAppService.AgregarFavoritoAsync(_destinoMarDelPlataId);

            // Assert: Se guardan una sola vez
            var favoritos = await _favoriteAppService.ObtenerMisFavoritosAsync();
            favoritos.Count.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Quitar_Un_Destino_De_Favoritos()
    {
        await SeedDestinationAsync(_destinoCanabacuaId, "Canabacua");
        await SeedDestinationAsync(_destinoMarDelPlataId, "Mar del Plata");

        using (LoginUsuarioSimulado())
        {
            // Arrange: Agregamos ambos destinos previamente
            await _favoriteAppService.AgregarFavoritoAsync(_destinoCanabacuaId);
            await _favoriteAppService.AgregarFavoritoAsync(_destinoMarDelPlataId);

            // Act: Eliminamos únicamente Canabacua
            await _favoriteAppService.QuitarFavoritoAsync(_destinoCanabacuaId);

            // Assert: Solo debe quedar Mar del Plata en la lista
            var favoritos = await _favoriteAppService.ObtenerMisFavoritosAsync();
            favoritos.Count.ShouldBe(1);
            favoritos[0].DestinoId.ShouldBe(_destinoMarDelPlataId);
        }
    }

    [Fact]
    public async Task Favorito_Expone_LastUpdated_Del_Destino()
    {
        var lastUpdated = new DateTime(2026, 11, 23, 12, 0, 0, DateTimeKind.Utc);
        await SeedDestinationAsync(_destinoCanabacuaId, "Canabacua", lastUpdated);

        using (LoginUsuarioSimulado())
        {
            await _favoriteAppService.AgregarFavoritoAsync(_destinoCanabacuaId);

            var favorite = await _favoriteAppService.ObtenerMisFavoritosAsync();

            favorite.ShouldHaveSingleItem().LastUpdated.ShouldBe(lastUpdated);
        }
    }
}