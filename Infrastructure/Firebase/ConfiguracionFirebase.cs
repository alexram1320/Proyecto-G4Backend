using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Infrastructure.Firebase
{
    public sealed class ConfiguracionFirebase(IOptions<ConfiguracionFirebase.OpcionesFirebase> opciones, IHostEnvironment entorno)
    {
        public sealed class OpcionesFirebase
        {
            public string ProyectoId { get; set; } = string.Empty;
            public string Bucket { get; set; } = string.Empty;
            public string? RutaCredenciales { get; set; }
            public string ApiKeyWeb { get; set; } = string.Empty;
        }

        public OpcionesFirebase Opciones { get; } = opciones.Value;

        public GoogleCredential Credencial => string.IsNullOrWhiteSpace(Opciones.RutaCredenciales)
            ? GoogleCredential.GetApplicationDefault()
            : CredentialFactory
                .FromFile<ServiceAccountCredential>(ObtenerRutaCredenciales())
                .ToGoogleCredential();

        public FirebaseApp CrearApp()
        {
            return FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = Credencial,
                ProjectId = Opciones.ProyectoId
            });
        }

        public FirestoreDb CrearFirestore()
        {
            return new FirestoreDbBuilder
            {
                ProjectId = Opciones.ProyectoId,
                Credential = Credencial
            }.Build();
        }

        public StorageClient CrearStorage()
        {
            return StorageClient.Create(Credencial);
        }

        private string ObtenerRutaCredenciales()
        {
            var ruta = Opciones.RutaCredenciales ??
                throw new InvalidOperationException("No se configuro la ruta de credenciales Firebase");

            return Path.IsPathRooted(ruta)
                ? ruta
                : Path.GetFullPath(Path.Combine(entorno.ContentRootPath, ruta));
        }
    }
}
