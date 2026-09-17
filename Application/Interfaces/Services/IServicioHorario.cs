using ApagonYa.Application.Common;
using Application.DTOs.Horarios;

namespace Application.Interfaces.Services;

public interface IServicioHorario
{
    Task<Respuesta<IReadOnlyCollection<HorarioDto>>> ListadoAsync(
        string? zonaId,
        bool incluirInactivos,
        CancellationToken cancellationToken);
    Task<Respuesta<HorarioDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<HorarioDto>> CrearAsync(CrearHorarioDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<HorarioDto>> ActualizarAsync(string id, ActualizarHorarioDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<HorarioDto>> CambiarEstadoAsync(string id, CambiarEstadoHorarioDto solicitud, CancellationToken cancellationToken);
}
