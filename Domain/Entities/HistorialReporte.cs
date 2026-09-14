using Domain.Enums;

namespace Domain.Entities;

public sealed class HistorialReporte
{
    public string Id { get; set; } = string.Empty;
    public string ReporteId { get; set; } = string.Empty;
    public EstadoReporte EstadoAnterior { get; set; }
    public EstadoReporte EstadoNuevo { get; set; }
    public string UsuarioCambioId { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
