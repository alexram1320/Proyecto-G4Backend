namespace Application.Interfaces.Infrastructure
{
    public interface IUsuarioActual
    {
        bool Autenticado
        {
            get;
        }
        string UsuarioId
        {
            get;
        }
        string FirebaseUid
        {
            get;
        }
        string Email
        {
            get;
        }
        string Rol
        {
            get;
        }
        string? ZonaId
        {
            get;
        }
    }
}
