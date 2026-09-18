using Application.Interfaces.Services;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApagonYa.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<IServicioAutenticacion, ServicioAutenticacion>()
        .AddScoped<IServicioProvisionamientoAdministrador, ServicioProvisionamientoAdministrador>()
        .AddScoped<IServicioZona, ServicioZona>()
        .AddScoped<IServicioHorario, ServicioHorario>()
        .AddScoped<IServicioTecnico, ServicioTecnico>()
        .AddScoped<IServicioReporte, ServicioReporte>()
        .AddScoped<IServicioVerificacion, ServicioVerificacion>()
        .AddScoped<IServicioConfirmacion, ServicioConfirmacion>()
        .AddScoped<IServicioResolucion, ServicioResolucion>()
        .AddScoped<IServicioEstadisticas, ServicioEstadisticas>()
        .AddScoped<IServicioNotificacion, ServicioNotificacion>();
}