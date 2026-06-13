using MensajeriaApi.Models;

namespace MensajeriaApi.DTOs;

public class RastreoResponse
{
    public int IdEnvio { get; set; }
    public string CodigoRastreo { get; set; } = "";

    public int IdEstado { get; set; }
    public string Estado { get; set; } = "";

    public decimal PesoKg { get; set; }
    public decimal TarifaBase { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalPagar { get; set; }
    public int IntentosEntrega { get; set; }

    public string Remitente { get; set; } = "";
    public string Destinatario { get; set; } = "";
    public string OficinaOrigen { get; set; } = "";
    public string OficinaDestino { get; set; } = "";

    public DateTime FechaRegistro { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public List<HistorialEnvio> Historial { get; set; } = new();
}