using ApagonYa.Application.Common;

using Application.DTOs.Tecnicos;

namespace Application.Interfaces.Services;

public interface IServicioTecnico
{
    Task<Respuesta<IReadOnlyCollection<TecnicoDto>>> ListadoAsync(CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> ActualAsync(CancellationToken cancellationToken);
    Task<Respuesta<DetalleTecnicoDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> CrearAsync(CrearTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> ActualizarAsync(string id, ActualizarTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> CambiarDisponibilidadAsync(
        string id,
        DisponibilidadTecnicoDto solicitud,
        CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> AsignarZonaAsync(string id, AsignarZonaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> CambiarEstadoAsync(string id, CambiarEstadoTecnicoDto solicitud, CancellationToken cancellationToken);
}
