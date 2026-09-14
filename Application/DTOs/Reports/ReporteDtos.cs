using System.ComponentModel.DataAnnotations;
using ApagonYa.Application.Common;
using Domain.Enums;

namespace Application.DTOs.Reportes;

public sealed class CrearReporteDto
{
    [Required]
    public string ZonaId { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string DireccionAproximada { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }
    public Stream? Evidencia { get; set; }
    public string? NombreEvidencia { get; set; }
    public string? TipoContenidoEvidencia { get; set; }
}

public sealed record ActualizarReporteDto(
    [Required, StringLength(300)] string DireccionAproximada,
    DateTime FechaHoraInicio);

public sealed record AsignarTecnicoDto(
    [Required] string TecnicoId);

public sealed record CambiarEstadoReporteDto(
    EstadoReporte Estado,
    [StringLength(500)] string? Descripcion);

public sealed class FiltroReporteDto : ParametrosPaginacion
{
    public DateTime? FechaInicial { get; set; }
    public DateTime? FechaFinal { get; set; }
    public EstadoReporte? Estado { get; set; }
    public string? ZonaId { get; set; }
    public string? TecnicoId { get; set; }
}

public sealed record ReporteDto(
    string Id,
    string ZonaId,
    string CiudadanoId,
    string? TecnicoId,
    string DireccionAproximada,
    DateTime FechaHoraInicio,
    string Estado,
    string? UrlEvidencia,
    int CantidadConfirmaciones,
    DateTime FechaCreacion);

public sealed record DetalleReporteDto(
    ReporteDto Reporte,
    DateTime FechaActualizacion);

public sealed record ReporteDuplicadoDto(
    string Id,
    string ZonaId,
    string Estado,
    int CantidadConfirmaciones);

public sealed record HistorialReporteDto(
    string Id,
    string ReporteId,
    string EstadoAnterior,
    string EstadoNuevo,
    string UsuarioCambioId,
    DateTime FechaHora,
    string Accion,
    string Descripcion);
