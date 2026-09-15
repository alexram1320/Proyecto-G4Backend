using ApagonYa.Domain.Entities;

namespace ApagonYa.Application.Interfaces.Repositories;

public interface IRepositorioHorario
{
    Task<IReadOnlyCollection<HorarioCorte>> ListadoAsync(
        string? zonaId,
        bool incluirInactivos,
        CancellationToken cancellationToken);
    Task<HorarioCorte?> PorIdAsync(string id, CancellationToken cancellationToken);
    Task CrearAsync(HorarioCorte horario, CancellationToken cancellationToken);
    Task ActualizarAsync(HorarioCorte horario, CancellationToken cancellationToken);
}
