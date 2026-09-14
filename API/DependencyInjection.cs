using System.Text.Json.Serialization;
using ApagonYa.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ApagonYa.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(opciones =>
            {
                opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(opciones =>
            {
                opciones.InvalidModelStateResponseFactory = contexto =>
                {
                    var errores = contexto.ModelState.Values
                        .SelectMany(valor => valor.Errors)
                        .Select(error => error.ErrorMessage)
                        .ToArray();
                    var respuesta = new Respuesta<object>
                    {
                        Exito = false,
                        Mensaje = "Solicitud invalida",
                        Errores = errores,
                        CodigoEstado = 400
                    };

                    return new BadRequestObjectResult(respuesta);
                };
            });

        return services;
    }
}
