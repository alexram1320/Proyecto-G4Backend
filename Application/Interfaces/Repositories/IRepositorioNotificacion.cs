using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IRepositorioNotificacion
{
    Task CrearAsync(Notificacion notificacion, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Notificacion>> PorUsuarioAsync(string usuarioId, int limite, CancellationToken cancellationToken);
    Task<bool> MarcarLeidaAsync(string id, string usuarioId, CancellationToken cancellationToken);
}