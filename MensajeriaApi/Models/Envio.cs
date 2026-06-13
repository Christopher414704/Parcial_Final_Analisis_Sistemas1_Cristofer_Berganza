namespace MensajeriaApi.Models;

public class Envio
{
    public int IdEnvio { get; set; }
    public string CodigoRastreo { get; set; } = "";
    public int IdRemitente { get; set; }
    public int IdDestinatario { get; set; }
    public int IdOficinaOrigen { get; set; }
    public int IdOficinaDestino { get; set; }

    public int IdEstado { get; set; }
    public string Estado { get; set; } = "";

    public decimal PesoKg { get; set; }
    public decimal TarifaBase { get; set; }
    public decimal Descuento { get; set; }
    public decimal TotalPagar { get; set; }
    public int IntentosEntrega { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime FechaActualizacion { get; set; }
}