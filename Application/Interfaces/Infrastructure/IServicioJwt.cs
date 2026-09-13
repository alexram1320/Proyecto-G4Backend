using Domain.Entities;

namespace Application.Interfaces.Infrastructure
{
    public interface IServicioJwt
    {
        (string Token, DateTime ExpiraEn) Crear(Usuario usuario);
    }
}
