using System.Text;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class AutenticacionExtensions
    {
        public static IServiceCollection AddAutenticacionJwt(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var configuracion = configuration
                .GetSection("JWT")
                .Get<ConfiguracionJwt>() ?? new ConfiguracionJwt();
            var clave = Encoding.UTF8.GetBytes(configuracion.Clave);
            var parametrosValidacion = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuracion.Emisor,
                ValidateAudience = true,
                ValidAudience = configuracion.Audiencia,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(clave)
            };

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones =>
                {
                    opciones.TokenValidationParameters = parametrosValidacion;
                });
            services.AddAuthorization();

            return services;
        }
    }
}
