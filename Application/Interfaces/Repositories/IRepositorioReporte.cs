using ApagonYa.Application.DTOs.Reportes;
using ApagonYa.Domain.Entities;
using ApagonYa.Domain.Enums;

namespace ApagonYa.Application.Interfaces.Repositories;

public sealed record ResultadoCreacionReporte(
    bool Creado,
    ReporteCorteEnergia Reporte);

public interface IRepositorioReporte
{
    Task<ReporteCorteEnergia?> PorIdAsync(
        string id,
        CancellationToken cancellationToken);

    Task<ReporteCorteEnergia?> ActivoPorZonaAsync(
        string zonaId,
        CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<ReporteCorteEnergia> Datos, int Total)> ListadoAsync(
        FiltroReporteDto filtro,
        CancellationToken cancellationToken);

    Task<ResultadoCreacionReporte> CrearSiZonaLibreAsync(
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken cancellationToken);

    Task ActualizarAsync(
        ReporteCorteEnergia reporte,
        CancellationToken cancellationToken);

    Task CambiarEstadoAsync(
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken cancellationToken);

    Task AsignarAsync(
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReporteCorteEnergia>> TodosAsync(
        DateTime? desde,
        DateTime? hasta,
        EstadoReporte? estado,
        string? zonaId,
        string? tecnicoId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<HistorialReporte>> HistorialAsync(
        string reporteId,
        CancellationToken cancellationToken);
}
