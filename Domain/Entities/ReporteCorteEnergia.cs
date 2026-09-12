using ApagonYa.Domain.Enums;

namespace ApagonYa.Domain.Entities;

public sealed class ReporteCorteEnergia
{
    public string Id { get; set; } = string.Empty;
    public string ZonaId { get; set; } = string.Empty;
    public string CiudadanoId { get; set; } = string.Empty;
    public string? TecnicoId { get; set; }
    public string DireccionAproximada { get; set; } = string.Empty;
    public DateTime FechaHoraInicio { get; set; }
    public EstadoReporte Estado { get; set; } = EstadoReporte.NEW;
    public string? UrlEvidencia { get; set; }
    public string? NombreEvidencia { get; set; }
    public string? TipoContenidoEvidencia { get; set; }
    public int CantidadConfirmaciones { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

