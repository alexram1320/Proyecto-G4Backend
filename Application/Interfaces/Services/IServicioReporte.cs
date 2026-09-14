using ApagonYa.Application.Common;
using Application.DTOs.Reportes;

namespace Application.Interfaces.Services;

public interface IServicioReporte
{
    Task<RespuestaPaginada<ReporteDto>> ListadoAsync(FiltroReporteDto filtro, CancellationToken cancellationToken);
    Task<Respuesta<DetalleReporteDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> CrearAsync(CrearReporteDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> ActualizarAsync(string id, ActualizarReporteDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> AsignarTecnicoAsync(string id, AsignarTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> AceptarAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> ReasignarAsync(string id, AsignarTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> CambiarEstadoAsync(string id, CambiarEstadoReporteDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<IReadOnlyCollection<HistorialReporteDto>>> HistorialAsync(string id, CancellationToken cancellationToken);
}
