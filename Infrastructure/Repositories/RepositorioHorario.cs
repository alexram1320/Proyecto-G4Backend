using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace ApagonYa.Infrastructure.Repositories;

public sealed class RepositorioHorario(ContextoFirestore contexto) : IRepositorioHorario
{
    private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Horarios);

    public async Task<IReadOnlyCollection<HorarioCorte>> ListadoAsync(
        string? zonaId,
        bool incluirInactivos,
        CancellationToken ct)
    {
        var resultado = await Coleccion.GetSnapshotAsync(ct);
        var horarios = resultado.Documents.Select(MapeoFirestore.Entidad<HorarioCorte>);

        if (!string.IsNullOrWhiteSpace(zonaId))
        {
            horarios = horarios.Where(horario => horario.ZonaId == zonaId);
        }

        if (!incluirInactivos)
        {
            horarios = horarios.Where(horario => horario.Activo);
        }

        return horarios.OrderBy(horario => horario.FechaHoraInicio).ToArray();
    }

    public async Task<HorarioCorte?> PorIdAsync(string id, CancellationToken ct)
    {
        var documento = await Coleccion.Document(id).GetSnapshotAsync(ct);
        return documento.Exists ? MapeoFirestore.Entidad<HorarioCorte>(documento) : null;
    }

    public Task CrearAsync(HorarioCorte horario, CancellationToken ct) =>
        Coleccion.Document(horario.Id).CreateAsync(MapeoFirestore.Diccionario(horario), ct);

    public Task ActualizarAsync(HorarioCorte horario, CancellationToken ct) =>
        Coleccion.Document(horario.Id).SetAsync(MapeoFirestore.Diccionario(horario), cancellationToken: ct);
}
