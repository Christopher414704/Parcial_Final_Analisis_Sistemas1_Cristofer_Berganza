using MensajeriaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MensajeriaApi.Controllers;

[ApiController]
[Route("api/estados")]
public class EstadosController : ControllerBase
{
    private readonly IEnvioService _envioService;

    public EstadosController(IEnvioService envioService)
    {
        _envioService = envioService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var estados = await _envioService.ListarEstadosAsync();
        return Ok(estados);
    }

    [HttpGet("{idEstadoActual}/siguientes")]
    public async Task<IActionResult> ListarSiguientes(int idEstadoActual)
    {
        var estados = await _envioService.ListarEstadosSiguientesAsync(idEstadoActual);
        return Ok(estados);
    }
}