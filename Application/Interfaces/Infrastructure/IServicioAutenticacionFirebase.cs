namespace Application.Interfaces.Infrastructure
{
    public sealed record SesionFirebase(string Uid, string IdToken, string TokenRenovacion, int ExpiraEnSegundos);
    public sealed record UsuarioFirebase(string Uid, string Email);
    public interface IServicioAutenticacionFirebase
    {
        Task ActualizarEmailAsync(string uid, string email, CancellationToken cancellationToken);
        Task<string> CrearUsuarioAsync(string email, string contrasena, string nombre, string rol, CancellationToken cancellationToken);
        Task<UsuarioFirebase?> BuscarUsuarioPorEmailAsync(string email, CancellationToken cancellationToken);
        Task<SesionFirebase> IniciarSesionAsync(string email, string contrasena, CancellationToken cancellationToken);
        Task<SesionFirebase> RenovarAsync(string tokenRenovacion, CancellationToken cancellationToken);
        Task<string> CrearTokenPersonalizadoAsync(string uid, CancellationToken cancellationToken);
        Task EnviarRecuperacionAsync(string email, CancellationToken cancellationToken);
        Task AsignarClaimsAsync(string uid, IReadOnlyDictionary<string, object> claims, CancellationToken cancellationToken);
        Task RevocarTokensAsync(string uid, CancellationToken cancellationToken);
        Task DeshabilitarAsync(string uid, bool deshabilitado, CancellationToken cancellationToken);
    }
}
