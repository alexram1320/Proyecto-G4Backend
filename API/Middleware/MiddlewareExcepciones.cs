using Application.Common;

namespace API.Middleware
{
    public sealed class MiddlewareExcepciones(RequestDelegate siguiente, ILogger<MiddlewareExcepciones> logger, IHostEnvironment entorno)
    {
        public async Task InvokeAsync(HttpContext contexto)
        {
            try
            {
                await siguiente(contexto);
            }
            catch (UnauthorizedAccessException ex)
            {
                var detalle = entorno.IsDevelopment() ? ex.Message : null;
                await EscribirAsync(
                    contexto,
                    StatusCodes.Status401Unauthorized,
                    "No fue posible autenticar la solicitud",
                    detalle);
            }
            catch (ArgumentException ex)
            {
                await EscribirAsync(
                    contexto,
                    StatusCodes.Status400BadRequest,
                    ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await EscribirAsync(
                    contexto,
                    StatusCodes.Status409Conflict,
                    ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error no controlado con identificador {Id}",
                    contexto.TraceIdentifier);

                var detalle = entorno.IsDevelopment() ? ex.Message : null;
                await EscribirAsync(
                    contexto,
                    StatusCodes.Status500InternalServerError,
                    "Ocurrio un error inesperado",
                    detalle);
            }
        }

        private static Task EscribirAsync(
            HttpContext contexto,
            int codigo,
            string mensaje,
            string? detalle = null)
        {
            contexto.Response.StatusCode = codigo;
            contexto.Response.ContentType = "application/json";

            var errores = detalle is null ? [] : new[] { detalle };
            var respuesta = Respuesta<object>.Fallida(mensaje, codigo, errores);

            return contexto.Response.WriteAsJsonAsync(respuesta);
        }
    }
}
