using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using ApagonYa.Infrastructure.Repositories;
using FirebaseAdmin.Auth;
using Infrastructure.Authentication;
using Infrastructure.Email;
using Infrastructure.Firebase;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApagonYa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<OpcionesCorreo>()
            .Bind(configuration.GetSection("Correo"))
            .Validate(o => !o.Habilitado ||
                (!string.IsNullOrWhiteSpace(o.Usuario) &&
                 !string.IsNullOrWhiteSpace(o.ContrasenaAplicacion) &&
                 !string.IsNullOrWhiteSpace(o.Servidor) &&
                 o.Puerto == 587),
                "Complete Correo en correo.Local.json y use el puerto 587 con STARTTLS.")
            .ValidateOnStart();

        services.AddHostedService<EmisorCorreo>();

        services.Configure<ConfiguracionFirebase.OpcionesFirebase>(
            configuration.GetSection("Firebase"));

        services.Configure<ConfiguracionJwt>(
            configuration.GetSection("JWT"));

        services.AddSingleton<ConfiguracionFirebase>();

        services.AddSingleton(sp =>
            sp.GetRequiredService<ConfiguracionFirebase>().CrearApp());

        services.AddSingleton(sp =>
            FirebaseAuth.GetAuth(
                sp.GetRequiredService<FirebaseAdmin.FirebaseApp>()));

        services.AddSingleton(sp =>
            sp.GetRequiredService<ConfiguracionFirebase>().CrearFirestore());

        services.AddSingleton(sp =>
            sp.GetRequiredService<ConfiguracionFirebase>().CrearStorage());

        services.AddSingleton<ContextoFirestore>();

        services.AddHttpContextAccessor();

        services.AddHttpClient<
            IServicioAutenticacionFirebase,
            ServicioAutenticacionFirebase>();

        services.AddScoped<IUsuarioActual, UsuarioActual>();
        services.AddSingleton<IServicioJwt, ServicioJwt>();
        services.AddSingleton<IAlmacenamientoFirebase, AlmacenamientoFirebase>();

        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioZona, RepositorioZona>();
        services.AddScoped<IRepositorioHorario, RepositorioHorario>();
        services.AddScoped<IRepositorioTecnico, RepositorioTecnico>();
        services.AddScoped<IRepositorioReporte, RepositorioReporte>();
        services.AddScoped<IRepositorioConfirmacion, RepositorioConfirmacion>();
        services.AddScoped<IRepositorioResolucion, RepositorioResolucion>();
        services.AddScoped<IRepositorioNotificacion, RepositorioNotificacion>();

        return services;
    }
}