using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Horarios;

public sealed record CrearHorarioDto(
    [Required] string ZonaId,
    [Required, StringLength(120)] string Titulo,
    [StringLength(800)] string Descripcion,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin);

public sealed record ActualizarHorarioDto(
    [Required] string ZonaId,
    [Required, StringLength(120)] string Titulo,
    [StringLength(800)] string Descripcion,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin);

public sealed record CambiarEstadoHorarioDto(bool Activo);

public sealed record HorarioDto(
    string Id,
    string ZonaId,
    string Titulo,
    string Descripcion,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    bool Activo,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);
