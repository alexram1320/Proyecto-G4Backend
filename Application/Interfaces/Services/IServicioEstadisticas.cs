using ApagonYa.Application.Common;
using Application.DTOs.Estadisticas;

namespace Application.Interfaces.Services;

public interface IServicioEstadisticas
{
    Task<Respuesta<EstadisticasPanelDto>> PanelAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken cancellationToken);

    Task<Respuesta<IReadOnlyCollection<EstadisticasZonaDto>>> ZonasAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken cancellationToken);

    Task<Respuesta<IReadOnlyCollection<EstadisticasTecnicoDto>>> TecnicosAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken cancellationToken);

    Task<Respuesta<EstadisticasTendenciaDto>> TendenciasAsync(
        FiltroEstadisticasDto filtro,
        string agrupacion,
        CancellationToken cancellationToken);
}