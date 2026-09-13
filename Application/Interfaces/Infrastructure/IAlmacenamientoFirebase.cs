namespace Application.Interfaces.Infrastructure
{

    public sealed record ArchivoAlmacenado(string Url, string Nombre, string TipoContenido);
    public interface IAlmacenamientoFirebase
    {
        Task<ArchivoAlmacenado> SubirEvidenciaAsync(Stream contenido, string nombre, string tipoContenido, CancellationToken cancellationToken);
    }
}
