using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Tecnicos;

public sealed record CrearTecnicoDto(
    [Required] string Nombre,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Contrasena,
    [Required] string ZonaId,
    bool Disponible);

public sealed record ActualizarTecnicoDto(
    [Required] string Nombre,
    [Required, EmailAddress] string Email);

public sealed record DisponibilidadTecnicoDto(bool Disponible);

public sealed record AsignarZonaDto(
    [Required] string ZonaId);

public sealed record CambiarEstadoTecnicoDto(bool Activo);

public sealed record TecnicoDto(
    string Id,
    string UsuarioId,
    string Nombre,
    string Email,
    string? ZonaId,
    bool Disponible,
    string Estado,
    int CargaReportesActivos,
    bool Activo);

public sealed record DetalleTecnicoDto(
    TecnicoDto Tecnico,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);
