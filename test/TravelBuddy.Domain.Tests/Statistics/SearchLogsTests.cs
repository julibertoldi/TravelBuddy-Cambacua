using System;
using Shouldly;
using TravelBuddy.Statistics;
using Xunit;

namespace TravelBuddy.Statistics.Tests;

public class SearchLogsTests
{
    [Fact]
    public void SearchLogs_Constructor_AsignaDatosIncluyendoUsuarioOpcional()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var searchTime = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc);

        var searchLog = new SearchLogs(id, "montaña", searchTime, userId);

        searchLog.Id.ShouldBe(id);
        searchLog.SearchTerm.ShouldBe("montaña");
        searchLog.SearchTime.ShouldBe(searchTime);
        searchLog.UserId.ShouldBe(userId);
    }

    [Fact]
    public void SearchLogs_ConstructorSinUsuario_DejaUsuarioNulo()
    {
        var searchLog = new SearchLogs(Guid.NewGuid(), "playa", DateTime.UtcNow);

        searchLog.UserId.ShouldBeNull();
    }
}
