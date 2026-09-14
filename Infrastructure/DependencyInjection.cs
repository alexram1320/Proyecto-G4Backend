using Application.Interfaces.Infrastructure;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using FirebaseAdmin.Auth;
using Infrastructure.Authentication;
using Infrastructure.Firebase;
using Infrastructure.Repositories;
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


        return services;
    }
}
