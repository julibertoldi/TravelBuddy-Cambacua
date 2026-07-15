using Shouldly;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelBuddy.Experiencias;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;

namespace TravelBuddy.Application.Tests.Experiencias
{
    // Pruebas integradas del servicio de experiencias.
    // Se utiliza la infraestructura de ABP y la base InMemory del proyecto de tests.
    public class ExperienciaAppService_Tests
        : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
    {
        // ID utilizado para simular al usuario autenticado durante las pruebas.
        private static readonly Guid CurrentUserId =
            Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

        private readonly IExperienciaAppService _service;
        private readonly IRepository<Experiencia, Guid> _repository;
        private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

        public ExperienciaAppService_Tests()
        {
            // Obtiene el servicio real, el repositorio y el acceso
            // al usuario actual mediante la inyección de dependencias de ABP.
            _service = GetRequiredService<IExperienciaAppService>();
            _repository = GetRequiredService<IRepository<Experiencia, Guid>>();
            _currentPrincipalAccessor =
                GetRequiredService<ICurrentPrincipalAccessor>();
        }

        // Simula un usuario autenticado durante la ejecución de una prueba.
        private IDisposable LoginAs(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(AbpClaimTypes.UserId, userId.ToString()),
                new Claim(AbpClaimTypes.UserName, "usuario-test"),
                new Claim(AbpClaimTypes.Email, "usuario@test.com")
            };

            // El tipo de autenticación hace que la identidad
            // sea reconocida como autenticada por ABP.
            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "TestAuth"
            );

            var principal = new ClaimsPrincipal(identity);

            return _currentPrincipalAccessor.Change(principal);
        }

        [Fact]
        public async Task Should_Create_Experience_With_Current_User()
        {
            // Arrange:
            // Se preparan los datos necesarios para crear una experiencia.
            var input = new CreateUpdateExperienciaDto
            {
                DestinoId = Guid.NewGuid(),
                Titulo = "Viaje a Córdoba",
                Descripcion = "Una experiencia excelente.",
                Valoracion = ExperienciaValoracion.Excelente,
                PalabrasClave = "sierras, turismo"
            };

            // Act:
            // Se ejecuta CreateAsync simulando al usuario autenticado.
            ExperienciaDto result;

            using (LoginAs(CurrentUserId))
            {
                result = await _service.CreateAsync(input);
            }

            // Assert:
            // Se verifica que la experiencia se haya creado
            // y que el usuario asignado sea el usuario autenticado.
            result.ShouldNotBeNull();
            result.UsuarioId.ShouldBe(CurrentUserId);
            result.Titulo.ShouldBe("Viaje a Córdoba");
        }

        [Fact]
        public async Task Should_Update_Own_Experience()
        {
            // Arrange:
            // Se crea una experiencia perteneciente al usuario autenticado.
            var entity = new Experiencia(
                Guid.NewGuid(),
                Guid.NewGuid(),
                CurrentUserId,
                "Título original",
                "Descripción original",
                ExperienciaValoracion.Buena,
                "original"
            );

            // Guarda la experiencia en la base de datos InMemory.
            await WithUnitOfWorkAsync(async () =>
            {
                await _repository.InsertAsync(entity, autoSave: true);
            });

            var input = new CreateUpdateExperienciaDto
            {
                DestinoId = entity.DestinoId,
                Titulo = "Título actualizado",
                Descripcion = "Descripción actualizada",
                Valoracion = ExperienciaValoracion.Excelente,
                PalabrasClave = "actualizado"
            };

            // Act:
            // Se actualiza la experiencia simulando al propietario.
            ExperienciaDto result;

            using (LoginAs(CurrentUserId))
            {
                result = await _service.UpdateAsync(entity.Id, input);
            }

            // Assert:
            // Se comprueba que los datos hayan sido modificados correctamente.
            result.Titulo.ShouldBe("Título actualizado");
            result.Descripcion.ShouldBe("Descripción actualizada");
            result.Valoracion.ShouldBe(ExperienciaValoracion.Excelente);
        }

        [Fact]
        public async Task Should_Throw_When_Updating_Another_Users_Experience()
        {
            // Arrange:
            // Se crea una experiencia perteneciente a otro usuario.
            var anotherUserId = Guid.NewGuid();

            var entity = new Experiencia(
                Guid.NewGuid(),
                Guid.NewGuid(),
                anotherUserId,
                "Experiencia ajena",
                "No pertenece al usuario actual.",
                ExperienciaValoracion.Regular,
                "ajena"
            );

            await WithUnitOfWorkAsync(async () =>
            {
                await _repository.InsertAsync(entity, autoSave: true);
            });

            var input = new CreateUpdateExperienciaDto
            {
                DestinoId = entity.DestinoId,
                Titulo = "Intento de modificación",
                Descripcion = "Este cambio no debería permitirse.",
                Valoracion = ExperienciaValoracion.Buena,
                PalabrasClave = "prohibido"
            };

            // Act + Assert:
            // Se verifica que el servicio lance una excepción de autorización
            // porque la experiencia no pertenece al usuario autenticado.
            using (LoginAs(CurrentUserId))
            {
                await Assert.ThrowsAsync<AbpAuthorizationException>(
                    () => _service.UpdateAsync(entity.Id, input)
                );
            }
        }
    }
}