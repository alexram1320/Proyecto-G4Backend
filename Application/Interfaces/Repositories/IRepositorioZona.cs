using ApagonYa.Domain.Entities;

namespace ApagonYa.Application.Interfaces.Repositories;

public interface IRepositorioZona
{
    Task<Zona?> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Zona>> ListadoAsync(bool incluirInactivas, CancellationToken cancellationToken);
    Task CrearAsync(Zona zona, CancellationToken cancellationToken);
    Task ActualizarAsync(Zona zona, CancellationToken cancellationToken);
}
