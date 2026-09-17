using Application.DTOs.Resoluciones;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/reportes/{reporteId}/resolucion")]
[Authorize]
public sealed class ResolucionesController(IServicioResolucion servicio) : ControladorBase
{
    [HttpGet]
    public async Task<IActionResult> Resolucion(string reporteId, CancellationToken ct)
    {
        return Responder(await servicio.PorReporteAsync(reporteId, ct));
    }

    [Authorize(Roles = Roles.Tecnico)]
    [HttpPost]
    public async Task<IActionResult> Registrar(string reporteId, CrearResolucionDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.RegistrarAsync(reporteId, solicitud, ct));
    }
}
