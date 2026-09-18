using Domain.Enums;

namespace Domain.Entities;

public sealed class Notificacion
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string? ReporteId { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "NORMAL";
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
}