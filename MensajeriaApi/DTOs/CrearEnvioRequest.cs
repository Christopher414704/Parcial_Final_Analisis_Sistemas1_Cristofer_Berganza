namespace MensajeriaApi.DTOs;

public class CrearEnvioRequest
{
    public ClienteRequest Remitente { get; set; } = new();
    public ClienteRequest Destinatario { get; set; } = new();

    public int IdOficinaOrigen { get; set; }
    public int IdOficinaDestino { get; set; }

    public decimal PesoKg { get; set; }

    public string? Notas { get; set; }
}