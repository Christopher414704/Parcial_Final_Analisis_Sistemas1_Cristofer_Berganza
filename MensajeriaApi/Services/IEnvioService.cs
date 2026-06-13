using MensajeriaApi.DTOs;
using MensajeriaApi.Models;

namespace MensajeriaApi.Services;

public interface IEnvioService
{
    Task<IEnumerable<Oficina>> ListarOficinasAsync();

    Task<IEnumerable<EstadoEnvio>> ListarEstadosAsync();
    Task<IEnumerable<EstadoEnvio>> ListarEstadosSiguientesAsync(int idEstadoActual);

    Task<IEnumerable<RastreoResponse>> ListarEnviosAsync();
    Task<RastreoResponse?> ObtenerPorCodigoAsync(string codigo);
    Task<RastreoResponse> CrearEnvioAsync(CrearEnvioRequest request);
    Task ActualizarEstadoAsync(int idEnvio, ActualizarEstadoRequest request);
    Task RegistrarIntentoEntregaAsync(int idEnvio, RegistrarIntentoRequest request);
}