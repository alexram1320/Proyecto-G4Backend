using ApagonYa.Application.Interfaces.Infrastructure;
using ApagonYa.Application.Interfaces.Repositories;
using ApagonYa.Infrastructure.Authentication;
using ApagonYa.Infrastructure.Firebase;
using ApagonYa.Infrastructure.Repositories;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApagonYa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConfiguracionFirebase.OpcionesFirebase>(configuration.GetSection("Firebase"));
        services.Configure<ConfiguracionJwt>(configuration.GetSection("JWT"));
        services.AddSingleton<ConfiguracionFirebase>();
        services.AddSingleton(sp => sp.GetRequiredService<ConfiguracionFirebase>().CrearApp());
        services.AddSingleton(sp => FirebaseAuth.GetAuth(sp.GetRequiredService<FirebaseAdmin.FirebaseApp>()));
        services.AddSingleton(sp => sp.GetRequiredService<ConfiguracionFirebase>().CrearFirestore());
        services.AddSingleton(sp => sp.GetRequiredService<ConfiguracionFirebase>().CrearStorage());
        services.AddSingleton<ContextoFirestore>();
        services.AddHttpContextAccessor();
        services.AddHttpClient<IServicioAutenticacionFirebase, ServicioAutenticacionFirebase>();
        services.AddScoped<IUsuarioActual, UsuarioActual>();
        services.AddSingleton<IServicioJwt, ServicioJwt>();
        services.AddSingleton<IAlmacenamientoFirebase, AlmacenamientoFirebase>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioZona, RepositorioZona>();
        services.AddScoped<IRepositorioTecnico, RepositorioTecnico>();
        services.AddScoped<IRepositorioReporte, RepositorioReporte>();
        services.AddScoped<IRepositorioConfirmacion, RepositorioConfirmacion>();
        services.AddScoped<IRepositorioResolucion, RepositorioResolucion>();
        services.AddScoped<IRepositorioNotificacion, RepositorioNotificacion>();
        return services;
    }
}