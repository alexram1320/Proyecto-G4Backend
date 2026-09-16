using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/reportes/{reporteId}/confirmaciones")]
[Authorize]
public sealed class ConfirmacionesController(IServicioConfirmacion servicio) : ControladorBase
{
    [HttpGet]
    public async Task<IActionResult> Confirmaciones(string reporteId, CancellationToken ct)
    {
        return Responder(await servicio.PorReporteAsync(reporteId, ct));
    }

    [Authorize(Roles = Roles.Ciudadano)]
    [HttpPost]
    public async Task<IActionResult> Confirmar(string reporteId, CancellationToken ct)
    {
        return Responder(await servicio.ConfirmarAsync(reporteId, ct));
    }
}
