using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces.Infrastructure;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication
{
    public sealed class ServicioJwt(IOptions<ConfiguracionJwt> opciones) : IServicioJwt
    {
        public (string Token, DateTime ExpiraEn) Crear(Usuario usuario)
        {
            var configuracion = opciones.Value;
            var expira = DateTime.UtcNow.AddMinutes(configuracion.MinutosExpiracion);
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(NombresClaims.UsuarioId, usuario.Id),
            new(NombresClaims.FirebaseUid, usuario.FirebaseUid),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Rol.ToString()),
            new(NombresClaims.Rol, usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

            if (usuario.ZonaId is not null)
            {
                claims.Add(new Claim(NombresClaims.ZonaId, usuario.ZonaId));
            }

            var clave = Encoding.UTF8.GetBytes(configuracion.Clave);
            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(clave),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                configuracion.Emisor,
                configuracion.Audiencia,
                claims,
                DateTime.UtcNow,
                expira,
                credenciales);

            return (new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}
