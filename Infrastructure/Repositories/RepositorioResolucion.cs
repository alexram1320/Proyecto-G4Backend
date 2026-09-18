using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace Infrastructure.Repositories;

public sealed class RepositorioResolucion(ContextoFirestore contexto) : IRepositorioResolucion
{
    private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Resoluciones);

    public async Task<Resolucion?> PorReporteAsync(string reporteId, CancellationToken ct)
    {
        var documento = await Coleccion.Document(reporteId).GetSnapshotAsync(ct);
        
        return documento.Exists ? MapeoFirestore.Entidad<Resolucion>(documento) : null;
    }

    public Task RegistrarAsync(
        Resolucion resolucion,
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken ct)
    {
        var documentoResolucion = Coleccion.Document(reporte.Id);
        var documentoReporte = contexto.Coleccion(ColeccionesFirestore.Reportes).Document(reporte.Id);
        var documentoHistorial = contexto.Coleccion(ColeccionesFirestore.HistorialReportes)
            .Document(historial.Id);
        var documentoBloqueo = contexto.Coleccion(ColeccionesFirestore.BloqueosReportes)
            .Document(reporte.ZonaId);

        return contexto.BaseDatos.RunTransactionAsync(async transaccion =>
        {
            var existente = await transaccion.GetSnapshotAsync(documentoResolucion);

            if (existente.Exists)
            {
                throw new InvalidOperationException("El reporte ya tiene una resolucion inmutable");
            }

            var snapshotReporte = await transaccion.GetSnapshotAsync(documentoReporte);
            
            if (!snapshotReporte.Exists)
            {
                throw new InvalidOperationException("El reporte ya no existe");
            }
            
            var reporteActual = MapeoFirestore.Entidad<ReporteCorteEnergia>(snapshotReporte);
            
            if (reporteActual.Estado == EstadoReporte.RESOLVED)
            {
                throw new InvalidOperationException("El reporte ya fue resuelto");
            }
            
            if (reporteActual.TecnicoId != resolucion.TecnicoId)
            {
                throw new InvalidOperationException("El reporte fue reasignado a otro tecnico");
            }
            
            if (reporteActual.Estado is not (EstadoReporte.CONFIRMED or EstadoReporte.IN_VERIFICATION))
            {
                throw new InvalidOperationException("El reporte cambio de estado y ya no puede resolverse");
            }

            transaccion.Create(documentoResolucion, MapeoFirestore.Diccionario(resolucion));
            transaccion.Update(documentoReporte, new Dictionary<string, object>
            {
                [nameof(ReporteCorteEnergia.Estado)] = reporte.Estado.ToString(),
                [nameof(ReporteCorteEnergia.FechaActualizacion)] = reporte.FechaActualizacion
            });
            
            transaccion.Create(documentoHistorial, MapeoFirestore.Diccionario(historial));
            transaccion.Delete(documentoBloqueo);

            return true;
        }, cancellationToken: ct);
    }
}
