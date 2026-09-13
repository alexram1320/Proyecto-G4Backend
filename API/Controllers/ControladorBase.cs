using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public abstract class ControladorBase : ControllerBase
    {
        protected IActionResult Responder<T>(Respuesta<T> respuesta) => StatusCode(respuesta.CodigoEstado, respuesta);
    }
}
