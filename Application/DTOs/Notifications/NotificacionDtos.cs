using ApagonYa.Domain.Enums;

namespace ApagonYa.Application.DTOs.Notificaciones;

public sealed record CrearNotificacionDto(
    string UsuarioId,
    string? ReporteId,
    TipoNotificacion Tipo,
    string Titulo,
    string Mensaje,
    string Prioridad = "NORMAL");

public sealed record NotificacionDto(
    string Id,
    string UsuarioId,
    string? ReporteId,
    string Tipo,
    string Titulo,
    string Mensaje,
    string Prioridad,
    bool Leida,
    DateTime FechaCreacion);
