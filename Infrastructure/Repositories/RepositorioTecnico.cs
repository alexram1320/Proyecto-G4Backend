using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace Infrastructure.Repositories;

public sealed class RepositorioTecnico(ContextoFirestore contexto) : IRepositorioTecnico
{
    private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Tecnicos);

    public async Task<Tecnico?> PorIdAsync(string id, CancellationToken ct)
    {
        var documento = await Coleccion.Document(id).GetSnapshotAsync(ct);
        
        return documento.Exists ? MapeoFirestore.Entidad<Tecnico>(documento) : null;
    }

    public async Task<Tecnico?> PorUsuarioIdAsync(string usuarioId, CancellationToken ct)
    {
        var resultado = await Coleccion
            .WhereEqualTo(nameof(Tecnico.UsuarioId), usuarioId).Limit(1).GetSnapshotAsync(ct);

        return resultado.Count == 0 ? null : MapeoFirestore.Entidad<Tecnico>(resultado.Documents[0]);
    }

    public async Task<IReadOnlyCollection<Tecnico>> ListadoAsync(CancellationToken ct)
    {
        var resultado = await Coleccion.GetSnapshotAsync(ct);

        return resultado.Documents.Select(MapeoFirestore.Entidad<Tecnico>).ToArray();
    }

    public Task CrearAsync(Tecnico tecnico, CancellationToken ct)
    {
        return Coleccion.Document(tecnico.Id).CreateAsync(MapeoFirestore.Diccionario(tecnico), ct);
    }

    public Task ActualizarAsync(Tecnico tecnico, CancellationToken ct)
    {
        return Coleccion.Document(tecnico.Id).SetAsync(MapeoFirestore.Diccionario(tecnico), cancellationToken: ct);
    }
}
