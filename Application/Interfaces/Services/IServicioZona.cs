using ApagonYa.Application.Common;
using Application.DTOs.Zonas;

namespace Application.Interfaces.Services;

public interface IServicioZona
{
    Task<Respuesta<IReadOnlyCollection<ZonaDto>>> ListadoAsync(bool incluirInactivas, CancellationToken cancellationToken);
    Task<Respuesta<DetalleZonaDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<ZonaDto>> CrearAsync(CrearZonaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ZonaDto>> ActualizarAsync(string id, ActualizarZonaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ZonaDto>> CambiarEstadoAsync(string id, CambiarEstadoZonaDto solicitud, CancellationToken cancellationToken);
}
