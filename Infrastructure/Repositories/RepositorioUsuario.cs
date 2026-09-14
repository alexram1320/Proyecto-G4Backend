using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Google.Cloud.Firestore;
using Infrastructure.Firebase;

namespace Infrastructure.Repositories
{
    public sealed class RepositorioUsuario(ContextoFirestore contexto) : IRepositorioUsuario
    {
        private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Usuarios);

        public async Task<Usuario?> PorIdAsync(string id, CancellationToken ct)
        {
            var documento = await Coleccion.Document(id).GetSnapshotAsync(ct);
            return documento.Exists ? MapeoFirestore.Entidad<Usuario>(documento) : null;
        }

        public Task<Usuario?> PorFirebaseUidAsync(string uid, CancellationToken ct)
        {
            var consulta = Coleccion.WhereEqualTo(nameof(Usuario.FirebaseUid), uid);
            return PrimeroAsync(consulta, ct);
        }

        public Task<Usuario?> PorEmailAsync(string email, CancellationToken ct)
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            var consulta = Coleccion.WhereEqualTo(nameof(Usuario.Email), emailNormalizado);

            return PrimeroAsync(consulta, ct);
        }

        public async Task<IReadOnlyCollection<Usuario>> ListadoAsync(CancellationToken ct)
        {
            var resultado = await Coleccion.GetSnapshotAsync(ct);
            return resultado.Documents.Select(MapeoFirestore.Entidad<Usuario>).ToArray();
        }

        public Task CrearAsync(Usuario usuario, CancellationToken ct)
        {
            return Coleccion.Document(usuario.Id)
                .CreateAsync(MapeoFirestore.Diccionario(usuario), ct);
        }

        public Task ActualizarAsync(Usuario usuario, CancellationToken ct)
        {
            return Coleccion.Document(usuario.Id)
                .SetAsync(MapeoFirestore.Diccionario(usuario), cancellationToken: ct);
        }

        private static async Task<Usuario?> PrimeroAsync(Query consulta, CancellationToken ct)
        {
            var resultado = await consulta.Limit(1).GetSnapshotAsync(ct);

            return resultado.Count == 0
                ? null
                : MapeoFirestore.Entidad<Usuario>(resultado.Documents[0]);
        }
    }
}
