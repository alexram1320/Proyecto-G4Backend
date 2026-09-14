using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Resoluciones;

public sealed record CrearResolucionDto(
    [Required, StringLength(300)] string Causa,
    [Required, StringLength(1000)] string Descripcion,
    DateTime? FechaEstimadaResolucion,
    DateTime FechaRestablecimiento);

public sealed record ResolucionDto(
    string Id,
    string ReporteId,
    string TecnicoId,
    string Causa,
    string Descripcion,
    DateTime? FechaEstimadaResolucion,
    DateTime FechaRestablecimiento,
    DateTime FechaCreacion);

public sealed record DetalleResolucionDto(
    ResolucionDto Resolucion,
    string Estado);
