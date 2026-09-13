using Domain.Enums;

namespace Domain.Entities
{
    public sealed class Usuario
    {
        public string Id { get; set; } = string.Empty;
        public string FirebaseUid { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; } = RolUsuario.CITIZEN;
        public string? ZonaId { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
