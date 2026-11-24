using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.AuditLogging;
using Volo.Abp.Auditing;
using TravelBuddy.Calificaciones;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Xunit;

namespace TravelBuddy.Application.Tests.Calificaciones;

public class CalificacionAppService_Tests : TravelBuddyApplicationTestBase<TravelBuddyApplicationTestModule>
{
    private readonly ICalificacionAppService _service;
    private readonly IRepository<Calificacion, Guid> _repository;
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public CalificacionAppService_Tests()
    {
        _service = GetRequiredService<ICalificacionAppService>();
        _repository = GetRequiredService<IRepository<Calificacion, Guid>>();
        _auditLogRepository = GetRequiredService<IRepository<AuditLog, Guid>>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable LoginAs(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(AbpClaimTypes.UserId, userId.ToString()),
            new(AbpClaimTypes.UserName, "reviewer")
        };

        return _currentPrincipalAccessor.Change(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));
    }

    [Fact]
    public async Task Promedio_y_total_solo_incluyen_calificaciones_del_usuario_actual()
    {
        var destinoId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await _repository.InsertAsync(new Calificacion(
            Guid.NewGuid(), destinoId, currentUserId, 4, "Buena", "Comentario propio", string.Empty));
        await _repository.InsertAsync(new Calificacion(
            Guid.NewGuid(), destinoId, currentUserId, 2, "Regular", "Otro comentario propio", string.Empty));
        await _repository.InsertAsync(new Calificacion(
            Guid.NewGuid(), destinoId, otherUserId, 5, "Excelente", "Comentario privado ajeno", string.Empty));

        using (LoginAs(currentUserId))
        {
            var result = await _service.GetPromedioByDestinoAsync(destinoId);

            result.TotalCalificaciones.ShouldBe(2);
            result.Promedio.ShouldBe(3);
        }
    }

    [Fact]
    public async Task Promedio_y_total_son_cero_si_el_usuario_no_tiene_calificaciones()
    {
        var destinoId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        await _repository.InsertAsync(new Calificacion(
            Guid.NewGuid(), destinoId, Guid.NewGuid(), 5, "Ajena", "Comentario privado ajeno", string.Empty));

        using (LoginAs(currentUserId))
        {
            var result = await _service.GetPromedioByDestinoAsync(destinoId);

            result.TotalCalificaciones.ShouldBe(0);
            result.Promedio.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Promedio_requiere_usuario_autenticado()
    {
        await Should.ThrowAsync<AbpAuthorizationException>(
            () => _service.GetPromedioByDestinoAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Alta_cambio_y_baja_de_calificacion_quedan_en_entity_history()
    {
        using (LoginAs(Guid.NewGuid()))
        {
            var input = new CreateUpdateCalificacionDto
            {
                DestinoId = Guid.NewGuid(),
                Estrellas = 4,
                Titulo = "Primera opinión",
                Comentario = "Comentario inicial",
                PalabrasClave = "viaje"
            };

            var created = await _service.CreateAsync(input);
            input.Estrellas = 5;
            input.Comentario = "Comentario actualizado";
            await _service.UpdateAsync(created.Id, input);
            await _service.DeleteAsync(created.Id);
        }

        using (var uow = _unitOfWorkManager.Begin())
        {
            var auditLogsQuery = await _auditLogRepository.WithDetailsAsync(x => x.EntityChanges);
            var changes = auditLogsQuery
                .ToList()
                .SelectMany(auditLog => auditLog.EntityChanges)
                .Where(change => change.EntityTypeFullName == typeof(Calificacion).FullName)
                .Select(change => change.ChangeType)
                .ToList();

            changes.ShouldContain(EntityChangeType.Created);
            changes.ShouldContain(EntityChangeType.Updated);
            changes.ShouldContain(EntityChangeType.Deleted);

            await uow.CompleteAsync();
        }
    }
}
