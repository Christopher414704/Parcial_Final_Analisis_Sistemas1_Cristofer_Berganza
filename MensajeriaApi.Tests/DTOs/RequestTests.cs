using MensajeriaApi.DTOs;
using Xunit;

namespace MensajeriaApi.Tests.DTOs;

public class RequestTests
{
    [Fact]
    public void CrearEnvioRequest_DebeGuardarDatosCorrectamente()
    {
        // Arrange
        var request = new CrearEnvioRequest
        {
            Remitente = new ClienteRequest
            {
                Nombre = "Juan Perez",
                Telefono = "5555-1111",
                Direccion = "Zona 1",
                Departamento = "Guatemala",
                NIT = "CF"
            },
            Destinatario = new ClienteRequest
            {
                Nombre = "Maria Lopez",
                Telefono = "5555-2222",
                Direccion = "Centro",
                Departamento = "Jalapa",
                NIT = "CF"
            },
            IdOficinaOrigen = 1,
            IdOficinaDestino = 2,
            PesoKg = 4.5m,
            Notas = "Prueba de DTO"
        };

        // Assert
        Assert.Equal("Juan Perez", request.Remitente.Nombre);
        Assert.Equal("Maria Lopez", request.Destinatario.Nombre);
        Assert.Equal(1, request.IdOficinaOrigen);
        Assert.Equal(2, request.IdOficinaDestino);
        Assert.Equal(4.5m, request.PesoKg);
    }

    [Fact]
    public void ActualizarEstadoRequest_DebeGuardarIdEstadoNuevo()
    {
        // Arrange
        var request = new ActualizarEstadoRequest
        {
            IdEstadoNuevo = 2,
            IdOficina = 1,
            Notas = "Cambio a EnTransito"
        };

        // Assert
        Assert.Equal(2, request.IdEstadoNuevo);
        Assert.Equal(1, request.IdOficina);
        Assert.Equal("Cambio a EnTransito", request.Notas);
    }

    [Fact]
    public void RegistrarIntentoRequest_DebeGuardarDatosCorrectamente()
    {
        // Arrange
        var request = new RegistrarIntentoRequest
        {
            IdOficina = 2,
            Notas = "Cliente no se encontraba"
        };

        // Assert
        Assert.Equal(2, request.IdOficina);
        Assert.Equal("Cliente no se encontraba", request.Notas);
    }
}