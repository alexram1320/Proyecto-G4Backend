using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IRepositorioZona
{
    Task<IEnumerable<Zona>> ObtenerTodasAsync();
    Task<Zona?> ObtenerPorIdAsync(int id);
    Task<Zona> CrearAsync(Zona zona);
    Task ActualizarAsync(Zona zona);
    Task EliminarAsync(int id);
}
