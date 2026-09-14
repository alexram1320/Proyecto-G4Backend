using ApagonYa.Application.Common;
using Application.DTOs.Resoluciones;


namespace Application.Interfaces.Services;

public interface IServicioResolucion
{
    Task<Respuesta<DetalleResolucionDto>> PorReporteAsync(string reporteId, CancellationToken cancellationToken);
    Task<Respuesta<ResolucionDto>> RegistrarAsync(string reporteId, CrearResolucionDto solicitud, CancellationToken cancellationToken);
}
