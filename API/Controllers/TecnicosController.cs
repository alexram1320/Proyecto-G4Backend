using Application.DTOs.Tecnicos;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/tecnicos")]
[Authorize(Roles = $"{Roles.Administrador},{Roles.Tecnico}")]
public sealed class TecnicosController(IServicioTecnico servicio) : ControladorBase
{
    [HttpGet]
    public async Task<IActionResult> Tecnicos(CancellationToken ct)
    {
        return Responder(await servicio.ListadoAsync(ct));
    }

    [Authorize(Roles = Roles.Tecnico)]
    [HttpGet("actual")]
    public async Task<IActionResult> Actual(CancellationToken ct)
    {
        return Responder(await servicio.ActualAsync(ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> PorId(string id, CancellationToken ct)
    {
        return Responder(await servicio.PorIdAsync(id, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost]
    public async Task<IActionResult> Crear(CrearTecnicoDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.CrearAsync(solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, ActualizarTecnicoDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.ActualizarAsync(id, solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPatch("{id}/disponibilidad")]
    public async Task<IActionResult> CambiarDisponibilidad(string id, DisponibilidadTecnicoDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.CambiarDisponibilidadAsync(id, solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPatch("{id}/zona")]
    public async Task<IActionResult> AsignarZona(string id, AsignarZonaDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.AsignarZonaAsync(id, solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(string id, CambiarEstadoTecnicoDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.CambiarEstadoAsync(id, solicitud, ct));
    }
}

