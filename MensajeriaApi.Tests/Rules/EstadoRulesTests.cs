using MensajeriaApi.Models;
using Xunit;

namespace MensajeriaApi.Tests.Rules;

public class EstadoRulesTests
{
    [Fact]
    public void Estados_DebenTenerIdsCorrectos()
    {
        // Arrange
        var estados = new List<EstadoEnvio>
        {
            new() { IdEstado = 1, NombreEstado = "Registrado", EsFinal = false },
            new() { IdEstado = 2, NombreEstado = "EnTransito", EsFinal = false },
            new() { IdEstado = 3, NombreEstado = "EnReparto", EsFinal = false },
            new() { IdEstado = 4, NombreEstado = "Entregado", EsFinal = true },
            new() { IdEstado = 5, NombreEstado = "EnDevolucion", EsFinal = false },
            new() { IdEstado = 6, NombreEstado = "Devuelto", EsFinal = true }
        };

        // Assert
        Assert.Contains(estados, e => e.IdEstado == 1 && e.NombreEstado == "Registrado");
        Assert.Contains(estados, e => e.IdEstado == 2 && e.NombreEstado == "EnTransito");
        Assert.Contains(estados, e => e.IdEstado == 3 && e.NombreEstado == "EnReparto");
        Assert.Contains(estados, e => e.IdEstado == 4 && e.NombreEstado == "Entregado");
        Assert.Contains(estados, e => e.IdEstado == 5 && e.NombreEstado == "EnDevolucion");
        Assert.Contains(estados, e => e.IdEstado == 6 && e.NombreEstado == "Devuelto");
    }

    [Fact]
    public void EstadosFinales_DebenSerEntregadoYDevuelto()
    {
        // Arrange
        var estados = new List<EstadoEnvio>
        {
            new() { IdEstado = 1, NombreEstado = "Registrado", EsFinal = false },
            new() { IdEstado = 2, NombreEstado = "EnTransito", EsFinal = false },
            new() { IdEstado = 3, NombreEstado = "EnReparto", EsFinal = false },
            new() { IdEstado = 4, NombreEstado = "Entregado", EsFinal = true },
            new() { IdEstado = 5, NombreEstado = "EnDevolucion", EsFinal = false },
            new() { IdEstado = 6, NombreEstado = "Devuelto", EsFinal = true }
        };

        // Act
        var finales = estados.Where(e => e.EsFinal).ToList();

        // Assert
        Assert.Equal(2, finales.Count);
        Assert.Contains(finales, e => e.NombreEstado == "Entregado");
        Assert.Contains(finales, e => e.NombreEstado == "Devuelto");
    }
}