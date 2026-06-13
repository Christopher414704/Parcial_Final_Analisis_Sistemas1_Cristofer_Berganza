using MensajeriaApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MensajeriaApi.Controllers;

[ApiController]
[Route("api/oficinas")]
public class OficinasController : ControllerBase
{
    private readonly IEnvioService _envioService;

    public OficinasController(IEnvioService envioService)
    {
        _envioService = envioService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var oficinas = await _envioService.ListarOficinasAsync();
        return Ok(oficinas);
    }
}