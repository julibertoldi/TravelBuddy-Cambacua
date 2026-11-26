using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelBuddy.Calificaciones;
using TravelBuddy.Favorites;
using TravelBuddy.Statistics;
using Volo.Abp.AuditLogging;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.SecurityLog;

namespace TravelBuddy.EntityFrameworkCore.Statistics;

public class EfCoreAuditEventReader : IAuditEventReader, ITransientDependency
{
    private const int AuditEventsLimit = 100;

    private static readonly string[] FailedLoginActions =
    {
        IdentitySecurityLogActionConsts.LoginFailed,
        IdentitySecurityLogActionConsts.LoginLockedout,
        IdentitySecurityLogActionConsts.LoginNotAllowed,
        IdentitySecurityLogActionConsts.LoginInvalidUserName,
        IdentitySecurityLogActionConsts.LoginInvalidUserNameOrPassword
    };

    private readonly IDbContextProvider<AbpAuditLoggingDbContext> _dbContextProvider;
    private readonly IRepository<IdentitySecurityLog, Guid> _securityLogRepository;

    public EfCoreAuditEventReader(
        IDbContextProvider<AbpAuditLoggingDbContext> dbContextProvider,
        IRepository<IdentitySecurityLog, Guid> securityLogRepository)
    {
        _dbContextProvider = dbContextProvider;
        _securityLogRepository = securityLogRepository;
    }

    public async Task<List<AuditEventDto>> GetCriticalAuditEventsAsync()
    {
        var dbContext = await _dbContextProvider.GetDbContextAsync();
        var auditLogs = dbContext.Set<AuditLog>();
        var favoriteType = typeof(Favorite).FullName!;
        var ratingType = typeof(Calificacion).FullName!;

        var entityChangeEvents = await (
            from change in dbContext.Set<EntityChange>()
            join auditLog in auditLogs on change.AuditLogId equals auditLog.Id
            where change.EntityTypeFullName == favoriteType ||
                  change.EntityTypeFullName == ratingType
            orderby change.ChangeTime descending
            select new
            {
                change.EntityTypeFullName,
                change.ChangeType,
                change.ChangeTime,
                auditLog.UserName,
                auditLog.ClientIpAddress,
                auditLog.Exceptions,
                auditLog.HttpStatusCode
            })
            .Take(AuditEventsLimit)
            .ToListAsync();

        var entityChangeEventsMapped = entityChangeEvents.Select(change => new AuditEventDto
        {
            UserName = NormalizeUserName(change.UserName),
            Action = DescribeEntityChange(change.EntityTypeFullName, change.ChangeType),
            ExecutionTime = change.ChangeTime,
            ClientIpAddress = change.ClientIpAddress,
            Result = IsFailed(change.Exceptions, change.HttpStatusCode) ? "Fallido" : "Correcto"
        });

        var userActionEvents = await (
            from action in dbContext.Set<AuditLogAction>()
            join auditLog in auditLogs on action.AuditLogId equals auditLog.Id
            where action.ServiceName != null &&
                  action.ServiceName.Contains("IdentityUserAppService") &&
                  (action.MethodName == "CreateAsync" ||
                   action.MethodName == "UpdateAsync" ||
                   action.MethodName == "DeleteAsync" ||
                   action.MethodName == "UpdateRolesAsync")
            orderby action.ExecutionTime descending
            select new
            {
                action.MethodName,
                action.ExecutionTime,
                auditLog.UserName,
                auditLog.ClientIpAddress,
                auditLog.Exceptions,
                auditLog.HttpStatusCode,
                HasRoleChanges = action.MethodName == "UpdateAsync" &&
                                 dbContext.Set<EntityChange>().Any(change =>
                                     change.AuditLogId == auditLog.Id &&
                                     change.EntityTypeFullName == typeof(IdentityUserRole).FullName)
            })
            .Take(AuditEventsLimit)
            .ToListAsync();

        var userActionEventsMapped = userActionEvents.Select(action => new AuditEventDto
        {
            UserName = NormalizeUserName(action.UserName),
            Action = DescribeIdentityAction(action.MethodName, action.HasRoleChanges),
            ExecutionTime = action.ExecutionTime,
            ClientIpAddress = action.ClientIpAddress,
            Result = IsFailed(action.Exceptions, action.HttpStatusCode) ? "Fallido" : "Correcto"
        });

        var securityLogs = await _securityLogRepository.GetQueryableAsync();
        var failedLoginRows = await securityLogs
            .Where(log => FailedLoginActions.Contains(log.Action))
            .OrderByDescending(log => log.CreationTime)
            .Take(AuditEventsLimit)
            .Select(log => new
            {
                log.UserName,
                Action = log.Action,
                ExecutionTime = log.CreationTime,
                log.ClientIpAddress
            })
            .ToListAsync();
        var failedLoginEvents = failedLoginRows.Select(log => new AuditEventDto
        {
            UserName = NormalizeUserName(log.UserName),
            Action = DescribeFailedLoginAction(log.Action),
            ExecutionTime = log.ExecutionTime,
            ClientIpAddress = log.ClientIpAddress,
            Result = "Fallido"
        });

        return entityChangeEventsMapped
            .Concat(userActionEventsMapped)
            .Concat(failedLoginEvents)
            .OrderByDescending(auditEvent => auditEvent.ExecutionTime)
            .Take(AuditEventsLimit)
            .ToList();
    }

    private static string NormalizeUserName(string? userName)
    {
        return string.IsNullOrWhiteSpace(userName) ? "Sistema/anónimo" : userName;
    }

    private static string DescribeEntityChange(string entityTypeFullName, EntityChangeType changeType)
    {
        if (entityTypeFullName == typeof(Favorite).FullName)
        {
            return (int)changeType switch
            {
                0 => "Favorito agregado",
                1 => "Favorito modificado",
                2 => "Favorito quitado",
                _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "Tipo de cambio de entidad desconocido.")
            };
        }

        return (int)changeType switch
        {
            0 => "Calificación creada",
            1 => "Calificación modificada",
            2 => "Calificación eliminada",
            _ => throw new ArgumentOutOfRangeException(nameof(changeType), changeType, "Tipo de cambio de entidad desconocido.")
        };
    }

    private static string DescribeIdentityAction(string methodName, bool hasRoleChanges)
    {
        return methodName switch
        {
            "CreateAsync" => "Usuario creado",
            "UpdateAsync" => hasRoleChanges
                ? "Roles de usuario modificados"
                : "Usuario modificado (datos o roles)",
            "DeleteAsync" => "Usuario eliminado",
            "UpdateRolesAsync" => "Roles de usuario actualizados",
            _ => throw new ArgumentOutOfRangeException(nameof(methodName), methodName, "Acción de Identity desconocida.")
        };
    }

    private static string DescribeFailedLoginAction(string action)
    {
        if (action == IdentitySecurityLogActionConsts.LoginFailed)
        {
            return "Inicio de sesión fallido";
        }

        if (action == IdentitySecurityLogActionConsts.LoginInvalidUserNameOrPassword)
        {
            return "Usuario o contraseña incorrectos";
        }

        if (action == IdentitySecurityLogActionConsts.LoginInvalidUserName)
        {
            return "Usuario inexistente";
        }

        if (action == IdentitySecurityLogActionConsts.LoginLockedout)
        {
            return "Cuenta bloqueada por intentos fallidos";
        }

        if (action == IdentitySecurityLogActionConsts.LoginNotAllowed)
        {
            return "Inicio de sesión no permitido";
        }

        return action;
    }

    private static bool IsFailed(string? exceptions, int? httpStatusCode)
    {
        return !string.IsNullOrWhiteSpace(exceptions) ||
               (httpStatusCode.HasValue && httpStatusCode.Value >= 400);
    }
}
