using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace Infrastructure.Repositories;

public sealed class RepositorioZona(ContextoFirestore contexto) : IRepositorioZona
{
    private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Zonas);

    public async Task<Zona?> PorIdAsync(string id, CancellationToken ct)
    {
        var documento = await Coleccion.Document(id).GetSnapshotAsync(ct);
        return documento.Exists ? MapeoFirestore.Entidad<Zona>(documento) : null;
    }

    public async Task<IReadOnlyCollection<Zona>> ListadoAsync(
        bool incluirInactivas,
        CancellationToken ct)
    {
        Query consulta = Coleccion;

        if (!incluirInactivas)
        {
            consulta = consulta.WhereEqualTo(nameof(Zona.Activa), true);
        }

        var resultado = await consulta.GetSnapshotAsync(ct);

        return resultado.Documents
            .Select(MapeoFirestore.Entidad<Zona>)
            .OrderBy(zona => zona.Nombre, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public Task CrearAsync(Zona zona, CancellationToken ct)
    {
        return Coleccion.Document(zona.Id)
            .CreateAsync(MapeoFirestore.Diccionario(zona), ct);
    }

    public Task ActualizarAsync(Zona zona, CancellationToken ct)
    {
        return Coleccion.Document(zona.Id)
            .SetAsync(MapeoFirestore.Diccionario(zona), cancellationToken: ct);
    }
}
