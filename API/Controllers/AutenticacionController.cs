using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacionController(IServicioAutenticacion servicio) : ControladorBase
    {
        [AllowAnonymous]
        [HttpPost("registro")]
        public async Task<IActionResult> Registrar(RegistrarUsuarioDto solicitud, CancellationToken ct)
        {
            return Responder(await servicio.RegistrarAsync(solicitud, ct));
        }

        [AllowAnonymous]
        [HttpPost("sesion")]
        public async Task<IActionResult> IniciarSesion(IniciarSesionDto solicitud, CancellationToken ct)
        {
            return Responder(await servicio.IniciarSesionAsync(solicitud, ct));
        }

        [AllowAnonymous]
        [HttpPost("renovar")]
        public async Task<IActionResult> Renovar(RenovarTokenDto solicitud, CancellationToken ct)
        {
            return Responder(await servicio.RenovarAsync(solicitud, ct));
        }

        [AllowAnonymous]
        [HttpPost("recuperar-contrasena")]
        public async Task<IActionResult> RecuperarContrasena(RecuperarContrasenaDto solicitud, CancellationToken ct)
        {
            return Responder(await servicio.RecuperarContrasenaAsync(solicitud, ct));
        }

        [Authorize]
        [HttpPost("cerrar-sesion")]
        public async Task<IActionResult> CerrarSesion(CancellationToken ct)
        {
            return Responder(await servicio.CerrarSesionAsync(ct));
        }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil(CancellationToken ct)
        {
            return Responder(await servicio.PerfilAsync(ct));
        }
    }
}
