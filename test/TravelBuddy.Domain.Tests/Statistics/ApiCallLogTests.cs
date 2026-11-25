using System;
using Shouldly;
using TravelBuddy.Statistics;
using Xunit;

namespace TravelBuddy.Statistics.Tests;

public class ApiCallLogTests
{
    [Fact]
    public void ApiCallLog_ConstructorConDatos_AsignaValoresYTimestampEnIntervalo()
    {
        var id = Guid.NewGuid();
        var antes = DateTime.Now;

        var apiCallLog = new ApiCallLog(id, "/search", 200, 125, true, "sin errores");

        var despues = DateTime.Now;
        apiCallLog.Id.ShouldBe(id);
        apiCallLog.Endpoint.ShouldBe("/search");
        apiCallLog.StatusCode.ShouldBe(200);
        apiCallLog.ResponseTimeMs.ShouldBe(125);
        apiCallLog.IsSuccess.ShouldBeTrue();
        apiCallLog.ErrorMessage.ShouldBe("sin errores");
        (apiCallLog.Timestamp >= antes).ShouldBeTrue();
        (apiCallLog.Timestamp <= despues).ShouldBeTrue();
    }

    [Fact]
    public void ApiCallLog_ConstructorConErrorNulo_LoConvierteEnVacio()
    {
        var apiCallLog = new ApiCallLog(Guid.NewGuid(), "/search", 200, 125, true, null);

        apiCallLog.ErrorMessage.ShouldBe(string.Empty);
    }
}
