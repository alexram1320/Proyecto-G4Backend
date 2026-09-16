using Application.DTOs.Estadisticas;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize(Roles = nameof(RolUsuario.ADMIN))]
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