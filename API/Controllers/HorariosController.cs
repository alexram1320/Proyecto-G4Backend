using Application.DTOs.Horarios;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[Route("api/horarios")]
public sealed class HorariosController(IServicioHorario servicio) : ControladorBase
{
    [HttpGet]
    public async Task<IActionResult> Listado(
        [FromQuery] string? zonaId,
        [FromQuery] bool incluirInactivos,
        CancellationToken ct) =>
        Responder(await servicio.ListadoAsync(zonaId, incluirInactivos, ct));

    [HttpGet("{id}")]
    public async Task<IActionResult> PorId(string id, CancellationToken ct) =>
        Responder(await servicio.PorIdAsync(id, ct));

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Crear(CrearHorarioDto solicitud, CancellationToken ct) =>
        Responder(await servicio.CrearAsync(solicitud, ct));

    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(
        string id,
        ActualizarHorarioDto solicitud,
        CancellationToken ct) =>
        Responder(await servicio.ActualizarAsync(id, solicitud, ct));

    [Authorize(Roles = Roles.Administrador)]
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(
        string id,
        CambiarEstadoHorarioDto solicitud,
        CancellationToken ct) =>
        Responder(await servicio.CambiarEstadoAsync(id, solicitud, ct));
}
