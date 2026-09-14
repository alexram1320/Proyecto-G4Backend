using Domain.Entities;

namespace Application.Interfaces.Repositories;

public sealed record ResultadoConfirmacion(
    bool Creada,
    Confirmacion? Confirmacion,
    ReporteCorteEnergia? Reporte);

public interface IRepositorioConfirmacion
{
    Task<IReadOnlyCollection<Confirmacion>> PorReporteAsync(
        string reporteId,
        CancellationToken cancellationToken);

    Task<ResultadoConfirmacion> ConfirmarAsync(
        string reporteId,
        string ciudadanoId,
        HistorialReporte historial,
        CancellationToken cancellationToken);
}
