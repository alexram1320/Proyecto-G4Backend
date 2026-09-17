using System.ComponentModel.DataAnnotations;
using ApagonYa.Application.Common;
using Application.DTOs.Reportes;
using Application.Interfaces.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/reportes")]
[Authorize]
public sealed class ReportesController(IServicioReporte servicio) : ControladorBase
{
    [HttpGet]
    public async Task<IActionResult> Reportes([FromQuery] FiltroReporteDto filtro, CancellationToken ct)
    {
        return Responder(await servicio.ListadoAsync(filtro, ct));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> PorId(string id, CancellationToken ct)
    {
        return Responder(await servicio.PorIdAsync(id, ct));
    }

    [HttpGet("{id}/historial")]
    public async Task<IActionResult> Historial(string id, CancellationToken ct)
    {
        return Responder(await servicio.HistorialAsync(id, ct));
    }

    [Authorize(Roles = Roles.Ciudadano)]
    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    
    public async Task<IActionResult> Crear([FromForm] SolicitudReporteFormulario solicitud, CancellationToken ct)
    {
        if (solicitud.Evidencia is { Length: > 8_000_000 })
        {
            return Responder(Respuesta<object>.Fallida("La evidencia no puede superar 8 MB", 400));
        }

        var tiposPermitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };
        
        if (solicitud.Evidencia is not null &&
            (solicitud.Evidencia.Length == 0 || !tiposPermitidos.Contains(solicitud.Evidencia.ContentType)))
        {
            return Responder(Respuesta<object>.Fallida("La evidencia debe ser una imagen JPG, PNG o WEBP valida", 400));
        }

        await using var contenido = solicitud.Evidencia?.OpenReadStream();
        var reporte = new CrearReporteDto
        {
            ZonaId = solicitud.ZonaId,
            DireccionAproximada = solicitud.DireccionAproximada,
            FechaHoraInicio = solicitud.FechaHoraInicio,
            Evidencia = contenido,
            NombreEvidencia = solicitud.Evidencia?.FileName,
            TipoContenidoEvidencia = solicitud.Evidencia?.ContentType
        };

        return Responder(await servicio.CrearAsync(reporte, ct));
    }

    [Authorize(Roles = Roles.Ciudadano)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, ActualizarReporteDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.ActualizarAsync(id, solicitud, ct));
    }

    [Authorize(Roles = Roles.Administrador)]
    [HttpPost("{id}/asignar")]
    public async Task<IActionResult> AsignarTecnico(string id, AsignarTecnicoDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.AsignarTecnicoAsync(id, solicitud, ct));
    }

    [Authorize(Roles = Roles.Tecnico)]
    [HttpPost("{id}/aceptar")]
    public async Task<IActionResult> Aceptar(string id, CancellationToken ct)
    {
        return Responder(await servicio.AceptarAsync(id, ct));
    }

    [Authorize(Roles = Roles.Tecnico)]
    [HttpPost("{id}/reasignar")]
    public async Task<IActionResult> Reasignar(string id, AsignarTecnicoDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.ReasignarAsync(id, solicitud, ct));
    }

    [Authorize(Roles = $"{Roles.Administrador},{Roles.Tecnico}")]
    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(string id, CambiarEstadoReporteDto solicitud, CancellationToken ct)
    {
        return Responder(await servicio.CambiarEstadoAsync(id, solicitud, ct));
    }
}

public sealed class SolicitudReporteFormulario
{
    [Required]
    public string ZonaId { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string DireccionAproximada { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }
    public IFormFile? Evidencia { get; set; }
}
