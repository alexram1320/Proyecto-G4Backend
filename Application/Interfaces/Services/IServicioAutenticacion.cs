using Application.Common;
using Application.DTOs.Auth;

namespace Application.Interfaces.Services
{
    public interface IServicioAutenticacion
    {
        Task<Respuesta<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto solicitud, CancellationToken cancellationToken);
        Task<Respuesta<SesionDto>> IniciarSesionAsync(IniciarSesionDto solicitud, CancellationToken cancellationToken);
        Task<Respuesta<SesionDto>> RenovarAsync(RenovarTokenDto solicitud, CancellationToken cancellationToken);
        Task<Respuesta<bool>> RecuperarContrasenaAsync(RecuperarContrasenaDto solicitud, CancellationToken cancellationToken);
        Task<Respuesta<bool>> CerrarSesionAsync(CancellationToken cancellationToken);
        Task<Respuesta<UsuarioDto>> PerfilAsync(CancellationToken cancellationToken);
    }
}
