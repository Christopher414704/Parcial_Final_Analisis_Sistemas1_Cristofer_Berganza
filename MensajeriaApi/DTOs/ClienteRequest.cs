namespace MensajeriaApi.DTOs;

public class ClienteRequest
{
    public string Nombre { get; set; } = "";
    public string? Telefono { get; set; }
    public string Direccion { get; set; } = "";
    public string Departamento { get; set; } = "";
    public string? NIT { get; set; }
}