using Domain.Enums;

namespace Domain.Entities;

public sealed class Tecnico
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string? ZonaId { get; set; }
    public bool Disponible { get; set; } = true;
    public EstadoTecnico Estado { get; set; } = EstadoTecnico.DISPONIBLE;
    public int CargaReportesActivos { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

