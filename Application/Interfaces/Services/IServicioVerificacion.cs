

namespace Application.Interfaces.Services;

public interface IServicioVerificacion
{
    bool TransicionValida(Domain.Enums.EstadoReporte estadoActual, Domain.Enums.EstadoReporte estadoNuevo);
    bool PuedeResolver(Domain.Enums.EstadoReporte estado);
}
