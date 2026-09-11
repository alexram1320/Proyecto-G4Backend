using ApagonYa.Application.Interfaces.Infrastructure;
using ApagonYa.Application.Interfaces.Repositories;
using ApagonYa.Infrastructure.Authentication;
using ApagonYa.Infrastructure.Firebase;
using ApagonYa.Infrastructure.Email;
using ApagonYa.Infrastructure.Repositories;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApagonYa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        return services;
    }
}
