using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[Route("api/notificaciones")]
public sealed class NotificacionesController(IServicioNotificacion servicio) : ControladorBase
{
    [HttpGet]
    public async Task<IActionResult> Listado([FromQuery] int limite = 30, CancellationToken ct = default)
    {
        return Responder(await servicio.ListadoAsync(limite, ct));
    }

    [HttpPatch("{id}/leida")]
    public async Task<IActionResult> MarcarLeida(string id, CancellationToken ct)
    {
        return Responder(await servicio.MarcarLeidaAsync(id, ct));
    }
}