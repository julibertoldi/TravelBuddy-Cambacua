using System;
using Shouldly;
using TravelBuddy.Calificaciones;
using Volo.Abp;
using Xunit;

namespace TravelBuddy.Calificaciones.Tests;

public class CalificacionTests
{
    [Fact]
    public void Calificacion_ConstructorConDatosValidos_AsignaPropiedades()
    {
        var id = Guid.NewGuid();
        var destinoId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var calificacion = new Calificacion(id, destinoId, usuarioId, 3, "Título", "Comentario", "playa");

        calificacion.Id.ShouldBe(id);
        calificacion.DestinoId.ShouldBe(destinoId);
        calificacion.UsuarioId.ShouldBe(usuarioId);
        calificacion.Estrellas.ShouldBe(3);
        calificacion.Titulo.ShouldBe("Título");
        calificacion.Comentario.ShouldBe("Comentario");
        calificacion.PalabrasClave.ShouldBe("playa");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Calificacion_ConstructorConEstrellasEnLosBordes_EsValida(int estrellas)
    {
        var calificacion = CrearCalificacion(estrellas);

        calificacion.Estrellas.ShouldBe(estrellas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Calificacion_ConstructorConEstrellasFueraDeRango_LanzaBusinessExceptionConCodigo(int estrellas)
    {
        var exception = Should.Throw<BusinessException>(() => CrearCalificacion(estrellas));

        exception.Code.ShouldBe("TravelBuddy:Calificacion:EstrellasInvalidas");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Calificacion_SetEstrellasEnLosBordes_ActualizaElValor(int estrellas)
    {
        var calificacion = CrearCalificacion();

        calificacion.SetEstrellas(estrellas);

        calificacion.Estrellas.ShouldBe(estrellas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Calificacion_SetEstrellasFueraDeRango_LanzaBusinessExceptionConCodigo(int estrellas)
    {
        var calificacion = CrearCalificacion();

        var exception = Should.Throw<BusinessException>(() => calificacion.SetEstrellas(estrellas));

        exception.Code.ShouldBe("TravelBuddy:Calificacion:EstrellasInvalidas");
    }

    [Fact]
    public void Calificacion_SetTituloCon200Caracteres_EsValido()
    {
        var calificacion = CrearCalificacion();
        var titulo = new string('t', 200);

        calificacion.SetTitulo(titulo);

        calificacion.Titulo.ShouldBe(titulo);
    }

    [Fact]
    public void Calificacion_SetTituloCon201Caracteres_LanzaArgumentException()
    {
        var calificacion = CrearCalificacion();

        Should.Throw<ArgumentException>(() => calificacion.SetTitulo(new string('t', 201)));
    }

    [Fact]
    public void Calificacion_SetComentarioCon2000Caracteres_EsValido()
    {
        var calificacion = CrearCalificacion();
        var comentario = new string('c', 2000);

        calificacion.SetComentario(comentario);

        calificacion.Comentario.ShouldBe(comentario);
    }

    [Fact]
    public void Calificacion_SetComentarioCon2001Caracteres_LanzaArgumentException()
    {
        var calificacion = CrearCalificacion();

        Should.Throw<ArgumentException>(() => calificacion.SetComentario(new string('c', 2001)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Calificacion_SetTituloVacioOEnBlanco_LanzaArgumentException(string titulo)
    {
        var calificacion = CrearCalificacion();

        Should.Throw<ArgumentException>(() => calificacion.SetTitulo(titulo));
    }

    [Fact]
    public void Calificacion_SetTituloNulo_LanzaArgumentException()
    {
        var calificacion = CrearCalificacion();

        Should.Throw<ArgumentException>(() => calificacion.SetTitulo(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Calificacion_SetComentarioVacioOEnBlanco_LanzaArgumentException(string comentario)
    {
        var calificacion = CrearCalificacion();

        Should.Throw<ArgumentException>(() => calificacion.SetComentario(comentario));
    }

    [Fact]
    public void Calificacion_SetComentarioNulo_LanzaArgumentException()
    {
        var calificacion = CrearCalificacion();

        Should.Throw<ArgumentException>(() => calificacion.SetComentario(null!));
    }

    [Fact]
    public void Calificacion_SetPalabrasClaveNulas_LasConvierteEnVacio()
    {
        var calificacion = CrearCalificacion();

        calificacion.SetPalabrasClave(null!);

        calificacion.PalabrasClave.ShouldBe(string.Empty);
    }

    [Fact]
    public void Calificacion_ConstructorConPalabrasClaveNulas_LasConvierteEnVacio()
    {
        var calificacion = new Calificacion(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, "Título", "Comentario", null!);

        calificacion.PalabrasClave.ShouldBe(string.Empty);
    }

    private static Calificacion CrearCalificacion(int estrellas = 3)
    {
        return new Calificacion(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), estrellas, "Título", "Comentario", "playa");
    }
}
