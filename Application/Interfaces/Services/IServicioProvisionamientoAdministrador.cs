using Application.DTOs.Auth;

namespace Application.Interfaces.Services
{
    public interface IServicioProvisionamientoAdministrador
    {
        Task<UsuarioDto> ProvisionarAsync(string nombre, string email, string contrasena, CancellationToken cancellationToken);
    }
}
