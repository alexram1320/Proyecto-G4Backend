using ApagonYa.Domain.Entities;

namespace ApagonYa.Application.Interfaces.Repositories;

public interface IRepositorioTecnico
{
    Task<Tecnico?> PorIdAsync(
        string id,
        CancellationToken cancellationToken);
    
    Task<Tecnico?> PorUsuarioIdAsync(
        string usuarioId, 
        CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<Tecnico>> ListadoAsync(
        CancellationToken cancellationToken);
    
    Task CrearAsync(
        Tecnico tecnico, 
        CancellationToken cancellationToken);
    
    Task ActualizarAsync(
        Tecnico tecnico, 
        CancellationToken cancellationToken);
}
