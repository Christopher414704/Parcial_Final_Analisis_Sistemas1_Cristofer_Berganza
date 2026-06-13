namespace MensajeriaApi.Models;

public class EstadoEnvio
{
    public int IdEstado { get; set; }
    public string NombreEstado { get; set; } = "";
    public bool EsFinal { get; set; }
}