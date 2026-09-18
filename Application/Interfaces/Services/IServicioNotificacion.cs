using ApagonYa.Application.Common;
using Application.DTOs.Notificaciones;

namespace Application.Interfaces.Services;

public interface IServicioNotificacion
{
    Task RegistrarAsync(
        CrearNotificacionDto solicitud,
        CancellationToken cancellationToken);

    Task<Respuesta<IReadOnlyCollection<NotificacionDto>>> ListadoAsync(
        int limite,
        CancellationToken cancellationToken);

    Task<Respuesta<bool>> MarcarLeidaAsync(
        string id,
        CancellationToken cancellationToken);
}
