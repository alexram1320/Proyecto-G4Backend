using ApagonYa.Application.Common;
using Application.DTOs.Auth;
using Infrastructure.Firebase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/configuracion")]
public sealed class ConfiguracionController(ConfiguracionFirebase firebase) : ControladorBase
{
    [AllowAnonymous]
    [HttpGet("firebase")]
    public IActionResult Firebase()
    {
        var opciones = firebase.Opciones;
        if (string.IsNullOrWhiteSpace(opciones.ApiKeyWeb) || string.IsNullOrWhiteSpace(opciones.ProyectoId))
        {
            return Responder(Respuesta<ConfiguracionFirebasePublicaDto>.Fallida(
                "La configuracion web de Firebase no esta disponible",
                503));
        }

        var datos = new ConfiguracionFirebasePublicaDto(
            opciones.ApiKeyWeb,
            opciones.ProyectoId,
            $"{opciones.ProyectoId}.firebaseapp.com",
            opciones.Bucket);
        return Responder(Respuesta<ConfiguracionFirebasePublicaDto>.Correcta(datos));
    }
}