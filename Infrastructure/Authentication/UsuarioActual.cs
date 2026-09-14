using System.Security.Claims;
using Application.Interfaces.Infrastructure;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication
{
    public sealed class UsuarioActual(IHttpContextAccessor contexto) : IUsuarioActual
    {
        private ClaimsPrincipal Usuario => contexto.HttpContext?.User ?? new ClaimsPrincipal();
        public bool Autenticado => Usuario.Identity?.IsAuthenticated == true;
        public string UsuarioId => Usuario.FindFirstValue(NombresClaims.UsuarioId) ?? string.Empty;
        public string FirebaseUid => Usuario.FindFirstValue(NombresClaims.FirebaseUid) ?? string.Empty;
        public string Email => Usuario.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        public string Rol => Usuario.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        public string? ZonaId => Usuario.FindFirstValue(NombresClaims.ZonaId);
    }
}
