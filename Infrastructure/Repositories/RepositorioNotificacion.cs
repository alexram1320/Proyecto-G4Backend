using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace Infrastructure.Repositories;

public sealed class RepositorioNotificacion(
    ContextoFirestore contexto) : IRepositorioNotificacion
{
    private CollectionReference Coleccion =>
        contexto.Coleccion(ColeccionesFirestore.Notificaciones);

    public Task CrearAsync(
        Notificacion notificacion,
        CancellationToken ct)
    {
        return Coleccion
            .Document(notificacion.Id)
            .CreateAsync(
                MapeoFirestore.Diccionario(notificacion),
                ct);
    }

    public async Task<IReadOnlyCollection<Notificacion>> PorUsuarioAsync(
        string usuarioId,
        int limite,
        CancellationToken ct)
    {
        var resultado = await Coleccion
            .WhereEqualTo(
                nameof(Notificacion.UsuarioId),
                usuarioId)
            .GetSnapshotAsync(ct);

        return resultado.Documents
            .Select(MapeoFirestore.Entidad<Notificacion>)
            .OrderByDescending(
                notificacion => notificacion.FechaCreacion)
            .Take(Math.Clamp(limite, 1, 100))
            .ToArray();
    }

    public Task<bool> MarcarLeidaAsync(
        string id,
        string usuarioId,
        CancellationToken ct)
    {
        var referencia = Coleccion.Document(id);

        return contexto.BaseDatos.RunTransactionAsync(
            async transaccion =>
            {
                var documento =
                    await transaccion.GetSnapshotAsync(referencia);

                if (!documento.Exists ||
                    documento.GetValue<string>(
                        nameof(Notificacion.UsuarioId)) != usuarioId)
                {
                    return false;
                }

                transaccion.Update(
                    referencia,
                    nameof(Notificacion.Leida),
                    true);

                return true;
            },
            cancellationToken: ct);
    }
}