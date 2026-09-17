using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Zonas;

public sealed record CrearZonaDto(
    [Required, StringLength(100)] string Nombre,
    [StringLength(500)] string Descripcion);

public sealed record ActualizarZonaDto(
    [Required, StringLength(100)] string Nombre,
    [StringLength(500)] string Descripcion);

public sealed record CambiarEstadoZonaDto(bool Activa);

public sealed record ZonaDto(
    string Id,
    string Nombre,
    string Descripcion,
    bool Activa,
    DateTime FechaCreacion);

public sealed record DetalleZonaDto(
    string Id,
    string Nombre,
    string Descripcion,
    bool Activa,
    DateTime FechaCreacion,
    DateTime FechaActualizacion);

