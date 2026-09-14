using Domain.Enums;

namespace Domain.Entities;

public sealed class Resolucion
{
    public string Id { get; set; } = string.Empty;
    public string ReporteId { get; set; } = string.Empty;
    public string TecnicoId { get; set; } = string.Empty;
    public string Causa { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaEstimadaResolucion { get; set; }
    public DateTime FechaRestablecimiento { get; set; }
    public DateTime FechaCreacion { get; set; }
    public EstadoResolucion Estado { get; set; } = EstadoResolucion.REGISTRADA;
}
