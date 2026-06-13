using MensajeriaApi.DTOs;
using MensajeriaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MensajeriaApi.Controllers;

[ApiController]
[Route("api/envios")]
public class EnviosController : ControllerBase
{
    private readonly IEnvioService _envioService;

    public EnviosController(IEnvioService envioService)
    {
        _envioService = envioService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var envios = await _envioService.ListarEnviosAsync();
        return Ok(envios);
    }

    [HttpGet("{codigoRastreo}")]
    public async Task<IActionResult> ObtenerPorCodigo(string codigoRastreo)
    {
        var envio = await _envioService.ObtenerPorCodigoAsync(codigoRastreo);

        if (envio == null)
            return NotFound(new { mensaje = "No se encontró el envío." });

        return Ok(envio);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearEnvioRequest request)
    {
        try
        {
            var envio = await _envioService.CrearEnvioAsync(request);
            return Ok(envio);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{idEnvio}/estado")]
    public async Task<IActionResult> ActualizarEstado(int idEnvio, ActualizarEstadoRequest request)
    {
        try
        {
            await _envioService.ActualizarEstadoAsync(idEnvio, request);
            return Ok(new { mensaje = "Estado actualizado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{idEnvio}/intento-entrega")]
    public async Task<IActionResult> RegistrarIntento(int idEnvio, RegistrarIntentoRequest request)
    {
        try
        {
            await _envioService.RegistrarIntentoEntregaAsync(idEnvio, request);
            return Ok(new { mensaje = "Intento de entrega registrado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}