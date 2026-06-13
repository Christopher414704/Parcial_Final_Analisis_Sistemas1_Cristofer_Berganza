namespace MensajeriaApi.Models;

public class Oficina
{
    public int IdOficina { get; set; }
    public string Nombre { get; set; } = "";
    public string Departamento { get; set; } = "";
    public string Direccion { get; set; } = "";
    public bool Activa { get; set; }
}