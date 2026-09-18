namespace ApagonYa.Application.DTOs.Estadisticas;

public sealed record PuntoGraficoDto(
    string Etiqueta,
    decimal Valor);

public sealed record EstadisticasPanelDto(
    int TotalReportes,
    int ReportesActivos,
    int ReportesResueltos,
    int SinVerificar,
    double TiempoPromedioResolucionHoras,
    IReadOnlyCollection<PuntoGraficoDto> PorEstado,
    IReadOnlyCollection<PuntoGraficoDto> PorZona,
    IReadOnlyCollection<PuntoGraficoDto> TiempoPromedioPorTipoCorte);

public sealed record EstadisticasZonaDto(
    string ZonaId,
    string Nombre,
    int Total,
    int Resueltos,
    decimal PorcentajeResuelto);

public sealed record EstadisticasTecnicoDto(
    string TecnicoId,
    string Nombre,
    int Total,
    int Resueltos,
    decimal PorcentajeResuelto);

public sealed record EstadisticasTendenciaDto(
    IReadOnlyCollection<string> Etiquetas,
    IReadOnlyCollection<int> Reportes,
    IReadOnlyCollection<int> Resoluciones);

public sealed record FiltroEstadisticasDto(
    DateTime? FechaInicial,
    DateTime? FechaFinal,
    string? Estado,
    string? ZonaId,
    string? TecnicoId);
