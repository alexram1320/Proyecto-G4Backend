using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace Infrastructure.Repositories;

public sealed class RepositorioConfirmacion(ContextoFirestore contexto) : IRepositorioConfirmacion
{
    private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Confirmaciones);

    public async Task<IReadOnlyCollection<Confirmacion>> PorReporteAsync(string reporteId, CancellationToken ct)
    {
        var resultado = await Coleccion.WhereEqualTo(nameof(Confirmacion.ReporteId), reporteId)
            .GetSnapshotAsync(ct);

        return resultado.Documents.Select(MapeoFirestore.Entidad<Confirmacion>)
            .OrderBy(confirmacion => confirmacion.FechaCreacion).ToArray();
    }

    public Task<ResultadoConfirmacion> ConfirmarAsync(string reporteId, string ciudadanoId,
        HistorialReporte historial,
        CancellationToken ct)
    {
        var documentoConfirmacion = Coleccion.Document($"{reporteId}_{ciudadanoId}");
        var documentoReporte = contexto.Coleccion(ColeccionesFirestore.Reportes).Document(reporteId);
        var documentoHistorial = contexto.Coleccion(ColeccionesFirestore.HistorialReportes).Document(historial.Id);

        return contexto.BaseDatos.RunTransactionAsync(async transaccion =>
        {
            var confirmacionExistente = await transaccion.GetSnapshotAsync(documentoConfirmacion);

            if (confirmacionExistente.Exists)
            {
                return new ResultadoConfirmacion(false, null, null);
            }

            var snapshotReporte = await transaccion.GetSnapshotAsync(documentoReporte);

            if (!snapshotReporte.Exists)
            {
                return new ResultadoConfirmacion(false, null, null);
            }

            var reporte = MapeoFirestore.Entidad<ReporteCorteEnergia>(snapshotReporte);
            
            if (reporte.Estado == EstadoReporte.RESOLVED)
            {
                return new ResultadoConfirmacion(false, null, reporte);
            }

            var confirmacion = new Confirmacion
            {
                Id = documentoConfirmacion.Id,
                ReporteId = reporteId,
                CiudadanoId = ciudadanoId,
                FechaCreacion = DateTime.UtcNow
            };

            var estadoTransaccional = reporte.Estado is EstadoReporte.NEW or EstadoReporte.IN_VERIFICATION
                ? EstadoReporte.CONFIRMED : reporte.Estado;
            historial.EstadoAnterior = reporte.Estado;
            historial.EstadoNuevo = estadoTransaccional;
            reporte.CantidadConfirmaciones++;
            reporte.Estado = estadoTransaccional;
            reporte.FechaActualizacion = DateTime.UtcNow;

            transaccion.Create(documentoConfirmacion, MapeoFirestore.Diccionario(confirmacion));
            transaccion.Set(documentoReporte, MapeoFirestore.Diccionario(reporte));
            transaccion.Create(documentoHistorial, MapeoFirestore.Diccionario(historial));

            return new ResultadoConfirmacion(true, confirmacion, reporte);
        }, cancellationToken: ct);
    }
}
