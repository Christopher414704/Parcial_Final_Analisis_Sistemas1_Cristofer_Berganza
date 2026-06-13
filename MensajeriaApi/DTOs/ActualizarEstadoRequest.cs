namespace MensajeriaApi.DTOs;

public class ActualizarEstadoRequest
{
    public int IdEstadoNuevo { get; set; }
    public int IdOficina { get; set; }
    public string? Notas { get; set; }
}