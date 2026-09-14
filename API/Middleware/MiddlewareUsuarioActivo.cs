using ApagonYa.Application.Common;
using Application.Interfaces.Repositories;
using Domain.Enums;

namespace API.Middleware
{
    public sealed class MiddlewareUsuarioActivo(RequestDelegate siguiente)
    {
        public async Task InvokeAsync(
        HttpContext contexto,
        IServiceProvider servicios)
        {
            if (contexto.User.Identity?.IsAuthenticated == true)
            {
                var usuarios = servicios.GetRequiredService<IRepositorioUsuario>();
                var usuarioId = contexto.User.FindFirst(NombresClaims.UsuarioId)?.Value;
                var usuario = string.IsNullOrWhiteSpace(usuarioId)
                    ? null
                    : await usuarios.PorIdAsync(usuarioId, contexto.RequestAborted);

                if (usuario is not { Activo: true })
                {
                    contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    contexto.Response.ContentType = "application/json";
                    await contexto.Response.WriteAsJsonAsync(
                        Respuesta<object>.Fallida("La sesion ya no esta habilitada", 401),
                        contexto.RequestAborted);
                    return;
                }
            }

            await siguiente(contexto);
        }
    }
}
