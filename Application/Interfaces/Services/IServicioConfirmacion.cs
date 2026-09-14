using ApagonYa.Application.Common;
using Application.DTOs.Confirmaciones;


namespace Application.Interfaces.Services;

public interface IServicioConfirmacion
{
    Task<Respuesta<IReadOnlyCollection<ConfirmacionDto>>> PorReporteAsync(string reporteId, CancellationToken cancellationToken);
    Task<Respuesta<ConfirmacionDto>> ConfirmarAsync(string reporteId, CancellationToken cancellationToken);
}
