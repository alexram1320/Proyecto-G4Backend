using ApagonYa.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public abstract class ControladorBase : ControllerBase
    {
        protected IActionResult Responder<T>(Respuesta<T> respuesta) => StatusCode(respuesta.CodigoEstado, respuesta);
    }
}
