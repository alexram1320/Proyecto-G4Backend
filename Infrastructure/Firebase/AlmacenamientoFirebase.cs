using Application.Interfaces.Infrastructure;
using Google.Cloud.Storage.V1;

namespace Infrastructure.Firebase
{
    public sealed class AlmacenamientoFirebase(StorageClient cliente, ConfiguracionFirebase configuracion) : IAlmacenamientoFirebase
    {
        public async Task<ArchivoAlmacenado> SubirEvidenciaAsync(Stream contenido, string nombre, string tipoContenido, CancellationToken ct)
        {
            if (!tipoContenido.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("La evidencia debe ser una imagen");
            }

            var nombreSeguro = Path.GetFileName(nombre);
            var objeto = $"reports/evidence/{Guid.NewGuid():N}-{nombreSeguro}";

            await cliente.UploadObjectAsync(
                configuracion.Opciones.Bucket,
                objeto,
                tipoContenido,
                contenido,
                cancellationToken: ct);

            var bucket = Uri.EscapeDataString(configuracion.Opciones.Bucket);
            var ruta = string.Join(
                '/',
                objeto.Split('/').Select(Uri.EscapeDataString));
            var url = $"https://storage.googleapis.com/{bucket}/{ruta}";

            return new(url, nombreSeguro, tipoContenido);
        }
    }
}
