using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    internal class AutenticacionDtos
    {
    }

    public sealed record IniciarSesionDto(
    [Required, EmailAddress] string Email,
    [Required] string Contrasena);

    public sealed record RegistrarUsuarioDto(
        [Required, StringLength(120)] string Nombre,
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Contrasena,
        [Required] string ZonaId);

    public sealed record RenovarTokenDto(
        [Required] string TokenRenovacion);

    public sealed record RecuperarContrasenaDto(
        [Required, EmailAddress] string Email);

    public sealed record SesionDto(
        string Token,
        string TokenRenovacion,
        string TokenFirebase,
        DateTime ExpiraEn,
        UsuarioDto Usuario);

    public sealed record ConfiguracionFirebasePublicaDto(
        string ApiKey,
        string ProjectId,
        string AuthDomain,
        string StorageBucket);

    public sealed record UsuarioDto(
        string Id,
        string FirebaseUid,
        string Nombre,
        string Email,
        string Rol,
        string? ZonaId,
        bool Activo);

}
