using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IRepositorioHorario
{
    Task<IEnumerable<HorarioCorte>> ObtenerTodosAsync();
    Task<IEnumerable<HorarioCorte>> ObtenerPorZonaIdAsync(int zonaId);
    Task<HorarioCorte?> ObtenerPorIdAsync(int id);
    Task<HorarioCorte> CrearAsync(HorarioCorte horario);
    Task ActualizarAsync(HorarioCorte horario);
    Task EliminarAsync(int id);
}
