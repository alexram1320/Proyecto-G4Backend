using Application.DTOs.Zonas;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/zonas")]
public sealed class ZonasController(IServicioZona servicio) : ControladorBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Zonas(
        [FromQuery] bool incluirInactivas,
        CancellationToken ct)
    {
        var puedeIncluirInactivas = incluirInactivas &&
                                    User.IsInRole(Roles.Administrador);

        return Responder(await servicio.ListadoAsync(puedeIncluirInactivas, ct));
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> PorId(string id, CancellationToken ct)
    {
        return Responder(await servicio.PorIdAsync(id, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearZonaDto solicitud,
        CancellationToken ct)
    {
        return Responder(await servicio.CrearAsync(solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(
        string id,
        ActualizarZonaDto solicitud,
        CancellationToken ct)
    {
        return Responder(await servicio.ActualizarAsync(id, solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(
        string id,
        CambiarEstadoZonaDto solicitud,
        CancellationToken ct)
    {
        return Responder(await servicio.CambiarEstadoAsync(id, solicitud, ct));
    }
}
