using System;
using Shouldly;
using TravelBuddy.Destinations;
using Xunit;

namespace TravelBuddy.Destinations.Tests;

public class DestinationTests
{
    [Fact]
    public void Destination_ConstructorCompleto_AsignaDatosYValoresPorDefecto()
    {
        var id = Guid.NewGuid();
        var antes = DateTime.Now;

        var destination = new Destination(id, "Nombre", "Descripción", "Región", "País");

        var despues = DateTime.Now;
        destination.Id.ShouldBe(id);
        destination.Name.ShouldBe("Nombre");
        destination.Description.ShouldBe("Descripción");
        destination.Region.ShouldBe("Región");
        destination.Country.ShouldBe("País");
        destination.IsAvailable.ShouldBeTrue();
        destination.ViewCount.ShouldBe(0);
        destination.Price.ShouldBe(0m);
        destination.ImageUrl.ShouldBe(string.Empty);
        destination.GeoDbCityId.ShouldBeNull();
        destination.Population.ShouldBe(0);
        destination.Latitude.ShouldBe(0d);
        destination.Longitude.ShouldBe(0d);
        (destination.LastUpdated >= antes).ShouldBeTrue();
        (destination.LastUpdated <= despues).ShouldBeTrue();
    }

    [Fact]
    public void Destination_ConstructorConDatosGeograficos_AsignaDatosYValoresPorDefecto()
    {
        var id = Guid.NewGuid();
        var antes = DateTime.Now;

        var destination = new Destination(id, "Nombre", "País", 1200, 40.4, -3.7);

        var despues = DateTime.Now;
        destination.Id.ShouldBe(id);
        destination.Name.ShouldBe("Nombre");
        destination.Country.ShouldBe("País");
        destination.Population.ShouldBe(1200);
        destination.Latitude.ShouldBe(40.4);
        destination.Longitude.ShouldBe(-3.7);
        destination.Description.ShouldBe(string.Empty);
        destination.Region.ShouldBe(string.Empty);
        destination.IsAvailable.ShouldBeTrue();
        destination.ViewCount.ShouldBe(0);
        (destination.LastUpdated >= antes).ShouldBeTrue();
        (destination.LastUpdated <= despues).ShouldBeTrue();
    }

    [Fact]
    public void Destination_ViewCount_ConservaElValorAsignado()
    {
        var destination = new Destination(Guid.NewGuid(), "Nombre", "Descripción", "Región", "País");

        destination.ViewCount = 12;

        destination.ViewCount.ShouldBe(12);
    }
}
