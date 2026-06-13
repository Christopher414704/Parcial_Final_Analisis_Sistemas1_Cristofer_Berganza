using MensajeriaApi.Rules;
using Xunit;

namespace MensajeriaApi.Tests.Rules;

public class EnvioRulesTests
{
    [Theory]
    [InlineData(0.01, 25)]
    [InlineData(0.5, 25)]
    [InlineData(1.0, 25)]
    [InlineData(1.01, 45)]
    [InlineData(3.5, 45)]
    [InlineData(5.0, 45)]
    [InlineData(5.01, 75)]
    [InlineData(8.2, 75)]
    [InlineData(10.0, 75)]
    [InlineData(10.01, 100)]
    [InlineData(15.0, 100)]
    public void CalcularTarifa_DebeRetornarTarifaCorrecta(decimal pesoKg, decimal tarifaEsperada)
    {
        // Act
        decimal resultado = EnvioRules.CalcularTarifa(pesoKg);

        // Assert
        Assert.Equal(tarifaEsperada, resultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5.5)]
    public void CalcularTarifa_ConPesoInvalido_DebeLanzarExcepcion(decimal pesoKg)
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            EnvioRules.CalcularTarifa(pesoKg));
    }

    [Theory]
    [InlineData("12345679", true)]
    [InlineData("1234560", true)]
    [InlineData("87654326", true)]
    [InlineData("CF", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("12345678", false)]
    [InlineData("ABC123", false)]
    public void NitEsValido_DebeValidarCorrectamente(string? nit, bool esperado)
    {
        // Act
        bool resultado = EnvioRules.NitEsValido(nit);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void CalcularDescuento_ConNitValido_DebeAplicarCincoPorCiento()
    {
        // Arrange
        decimal tarifaBase = 45m;

        // Act
        decimal descuento = EnvioRules.CalcularDescuento(
            tarifaBase,
            nitRemitenteValido: true,
            nitDestinatarioValido: false);

        // Assert
        Assert.Equal(2.25m, descuento);
    }

    [Fact]
    public void CalcularDescuento_SinNitValido_DebeRetornarCero()
    {
        // Arrange
        decimal tarifaBase = 45m;

        // Act
        decimal descuento = EnvioRules.CalcularDescuento(
            tarifaBase,
            nitRemitenteValido: false,
            nitDestinatarioValido: false);

        // Assert
        Assert.Equal(0m, descuento);
    }

    [Fact]
    public void CalcularTotal_DebeRestarDescuento()
    {
        // Arrange
        decimal tarifaBase = 45m;
        decimal descuento = 2.25m;

        // Act
        decimal total = EnvioRules.CalcularTotal(tarifaBase, descuento);

        // Assert
        Assert.Equal(42.75m, total);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    public void CalcularNuevoIntento_DebeIncrementarIntentos(int intentoActual, int esperado)
    {
        // Act
        int resultado = EnvioRules.CalcularNuevoIntento(intentoActual);

        // Assert
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public void CalcularNuevoIntento_CuandoYaTieneTresIntentos_DebeLanzarExcepcion()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            EnvioRules.CalcularNuevoIntento(3));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public void DebePasarADevolucion_DebeRetornarCorrectamente(int nuevoIntento, bool esperado)
    {
        // Act
        bool resultado = EnvioRules.DebePasarADevolucion(nuevoIntento);

        // Assert
        Assert.Equal(esperado, resultado);
    }
}