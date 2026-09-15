namespace ApagonYa.Application.DTOs.Schedules;

public record HorarioCorteDto(
    int Id,
    int ZonaId,
    string? NombreZona,
    DateTime FechaInicio,
    DateTime FechaFin,
    string Estado,
    string? Observaciones
);

public record CrearHorarioCorteDto(
    int ZonaId,
    DateTime FechaInicio,
    DateTime FechaFin,
    string? Observaciones
);

public record ActualizarHorarioCorteDto(
    DateTime FechaInicio,
    DateTime FechaFin,
    string Estado,
    string? Observaciones
);
