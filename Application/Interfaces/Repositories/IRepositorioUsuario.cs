using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IRepositorioUsuario
    {
        Task<Usuario?> PorIdAsync(string id, CancellationToken cancellationToken);
        Task<Usuario?> PorFirebaseUidAsync(string uid, CancellationToken cancellationToken);
        Task<Usuario?> PorEmailAsync(string email, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Usuario>> ListadoAsync(CancellationToken cancellationToken);
        Task CrearAsync(Usuario usuario, CancellationToken cancellationToken);
        Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken);
    }
}
