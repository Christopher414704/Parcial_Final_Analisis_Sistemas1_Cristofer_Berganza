namespace MensajeriaApi.Models;

public class HistorialEnvio
{
    public int IdHistorial { get; set; }
    public int IdEnvio { get; set; }

    public int IdEstadoNuevo { get; set; }
    public string EstadoNuevo { get; set; } = "";

    public int IdOficina { get; set; }
    public string Oficina { get; set; } = "";
    public DateTime FechaCambio { get; set; }
    public string? Notas { get; set; }
}