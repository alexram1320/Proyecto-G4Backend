using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IRepositorioResolucion
{
    Task<Resolucion?> PorReporteAsync(
        string reporteId,
        CancellationToken cancellationToken);

    Task RegistrarAsync(
        Resolucion resolucion,
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken cancellationToken);
}
