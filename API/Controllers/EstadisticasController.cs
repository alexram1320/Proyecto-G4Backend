using ApagonYa.Application.DTOs.Estadisticas;
using ApagonYa.Application.Interfaces.Services;
using ApagonYa.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApagonYa.API.Controllers;

[Authorize(Roles = Roles.Administrador)]
[Route("api/estadisticas")]
public sealed class EstadisticasController(IServicioEstadisticas servicio) : ControladorBase
{
    [HttpGet("panel")]
    public async Task<IActionResult> Panel(
        [FromQuery] FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        return Responder(await servicio.PanelAsync(filtro, ct));
    }

    [HttpGet("zonas")]
    public async Task<IActionResult> Zonas(
        [FromQuery] FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        return Responder(await servicio.ZonasAsync(filtro, ct));
    }

    [HttpGet("tecnicos")]
    public async Task<IActionResult> Tecnicos(
        [FromQuery] FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        return Responder(await servicio.TecnicosAsync(filtro, ct));
    }

    [HttpGet("tendencias")]
    public async Task<IActionResult> Tendencias(
        [FromQuery] FiltroEstadisticasDto filtro,
        [FromQuery] string agrupacion = "semana",
        CancellationToken ct = default)
    {
        return Responder(await servicio.TendenciasAsync(filtro, agrupacion, ct));
    }
}
