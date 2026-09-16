using Application.DTOs.Reportes;
using Application.Interfaces.Repositories;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Firebase;
using Google.Cloud.Firestore;

namespace Infrastructure.Repositories;

public sealed class RepositorioReporte(ContextoFirestore contexto) : IRepositorioReporte
{
    private CollectionReference Coleccion => contexto.Coleccion(ColeccionesFirestore.Reportes);

    public async Task<ReporteCorteEnergia?> PorIdAsync(string id, CancellationToken ct)
    {
        var documento = await Coleccion.Document(id).GetSnapshotAsync(ct);

        return documento.Exists ? MapeoFirestore.Entidad<ReporteCorteEnergia>(documento) : null;
    }

    public async Task<ReporteCorteEnergia?> ActivoPorZonaAsync(string zonaId, CancellationToken ct)
    {
        var resultado = await Coleccion.WhereEqualTo(nameof(ReporteCorteEnergia.ZonaId), zonaId).GetSnapshotAsync(ct);

        return resultado.Documents.Select(MapeoFirestore.Entidad<ReporteCorteEnergia>).FirstOrDefault(reporte => reporte.Estado is
                EstadoReporte.NEW or
                EstadoReporte.IN_VERIFICATION or
                EstadoReporte.CONFIRMED);
    }

    public async Task<(IReadOnlyCollection<ReporteCorteEnergia> Datos, int Total)> ListadoAsync(FiltroReporteDto filtro,
        CancellationToken ct)
    {
        var resultado = await Coleccion.GetSnapshotAsync(ct);
        var reportes = resultado.Documents.Select(MapeoFirestore.Entidad<ReporteCorteEnergia>);
        var todos = AplicarFiltros(
                reportes,
                filtro.FechaInicial,
                filtro.FechaFinal,
                filtro.Estado,
                filtro.ZonaId,
                filtro.TecnicoId)
            .OrderByDescending(reporte => reporte.FechaCreacion).ToArray();
        
        var datos = todos
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToArray();

        return (datos, todos.Length);
    }

    public Task<ResultadoCreacionReporte> CrearSiZonaLibreAsync(
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken ct)
    {
        var bloqueo = contexto.Coleccion(ColeccionesFirestore.BloqueosReportes).Document(reporte.ZonaId);
        var documentoReporte = Coleccion.Document(reporte.Id);
        var documentoHistorial = contexto.Coleccion(ColeccionesFirestore.HistorialReportes).Document(historial.Id);

        return contexto.BaseDatos.RunTransactionAsync(async transaccion =>
        {
            var bloqueoActual = await transaccion.GetSnapshotAsync(bloqueo);

            if (bloqueoActual.Exists)
            {
                var reporteId = bloqueoActual.GetValue<string>("ReporteId");
                var existente = await transaccion.GetSnapshotAsync(Coleccion.Document(reporteId));

                if (existente.Exists)
                {
                    var reporteExistente = MapeoFirestore.Entidad<ReporteCorteEnergia>(existente);
                    if (reporteExistente.Estado is EstadoReporte.NEW or
                        EstadoReporte.IN_VERIFICATION or EstadoReporte.CONFIRMED)
                    {
                        return new ResultadoCreacionReporte(false, reporteExistente);
                    }
                }
            }

            var datosBloqueo = new Dictionary<string, object>
            {
                ["ReporteId"] = reporte.Id,
                ["FechaCreacion"] = DateTime.UtcNow
            };

            transaccion.Create(documentoReporte, MapeoFirestore.Diccionario(reporte));
            transaccion.Set(bloqueo, datosBloqueo);
            transaccion.Create(documentoHistorial, MapeoFirestore.Diccionario(historial));

            return new ResultadoCreacionReporte(true, reporte);
        }, cancellationToken: ct);
    }

    public Task ActualizarAsync(ReporteCorteEnergia reporte, CancellationToken ct)
    {
        return Coleccion.Document(reporte.Id).SetAsync(MapeoFirestore.Diccionario(reporte), cancellationToken: ct);
    }

    public Task CambiarEstadoAsync(
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken ct)
    {
        var documentoReporte = Coleccion.Document(reporte.Id);
        var documentoHistorial = contexto.Coleccion(ColeccionesFirestore.HistorialReportes).Document(historial.Id);
        return contexto.BaseDatos.RunTransactionAsync(async transaccion =>
        {
            var snapshot = await transaccion.GetSnapshotAsync(documentoReporte);
            if (!snapshot.Exists)
            {
                throw new InvalidOperationException("El reporte ya no existe");
            }
            var actual = MapeoFirestore.Entidad<ReporteCorteEnergia>(snapshot);
            if (actual.Estado != historial.EstadoAnterior)
            {
                throw new InvalidOperationException("El reporte cambio de estado; actualice la informacion e intente nuevamente");
            }

            transaccion.Update(documentoReporte, new Dictionary<string, object>
            {
                [nameof(ReporteCorteEnergia.Estado)] = reporte.Estado.ToString(),
                [nameof(ReporteCorteEnergia.FechaActualizacion)] = reporte.FechaActualizacion
            });
            transaccion.Create(documentoHistorial, MapeoFirestore.Diccionario(historial));
            return true;
        }, cancellationToken: ct);
    }

    public Task AsignarAsync(
        ReporteCorteEnergia reporte,
        HistorialReporte historial,
        CancellationToken ct)
    {
        var documentoReporte = Coleccion.Document(reporte.Id);
        var documentoHistorial = contexto.Coleccion(ColeccionesFirestore.HistorialReportes).Document(historial.Id);
        return contexto.BaseDatos.RunTransactionAsync(async transaccion =>
        {
            var snapshot = await transaccion.GetSnapshotAsync(documentoReporte);
            if (!snapshot.Exists)
            {
                throw new InvalidOperationException("El reporte ya no existe");
            }
            var actual = MapeoFirestore.Entidad<ReporteCorteEnergia>(snapshot);
            if (actual.Estado == EstadoReporte.RESOLVED)
            {
                throw new InvalidOperationException("No se puede asignar un reporte resuelto");
            }

            transaccion.Update(documentoReporte, new Dictionary<string, object>
            {
                [nameof(ReporteCorteEnergia.TecnicoId)] = reporte.TecnicoId!,
                [nameof(ReporteCorteEnergia.FechaActualizacion)] = reporte.FechaActualizacion
            });
            transaccion.Create(documentoHistorial, MapeoFirestore.Diccionario(historial));
            return true;
        }, cancellationToken: ct);
    }

    public async Task<IReadOnlyCollection<ReporteCorteEnergia>> TodosAsync(
        DateTime? desde,
        DateTime? hasta,
        EstadoReporte? estado,
        string? zonaId,
        string? tecnicoId,
        CancellationToken ct)
    {
        var resultado = await Coleccion.GetSnapshotAsync(ct);
        var reportes = resultado.Documents.Select(MapeoFirestore.Entidad<ReporteCorteEnergia>);

        return AplicarFiltros(reportes, desde, hasta, estado, zonaId, tecnicoId).ToArray();
    }

    public async Task<IReadOnlyCollection<HistorialReporte>> HistorialAsync(
        string reporteId,
        CancellationToken ct)
    {
        var resultado = await contexto
            .Coleccion(ColeccionesFirestore.HistorialReportes)
            .WhereEqualTo(nameof(HistorialReporte.ReporteId), reporteId)
            .GetSnapshotAsync(ct);

        return resultado.Documents
            .Select(MapeoFirestore.Entidad<HistorialReporte>)
            .OrderBy(historial => historial.FechaHora)
            .ToArray();
    }

    private static IEnumerable<ReporteCorteEnergia> AplicarFiltros(
        IEnumerable<ReporteCorteEnergia> reportes,
        DateTime? desde,
        DateTime? hasta,
        EstadoReporte? estado,
        string? zonaId,
        string? tecnicoId)
    {
        if (zonaId is not null)
        {
            reportes = reportes.Where(reporte => reporte.ZonaId == zonaId);
        }

        if (tecnicoId is not null)
        {
            reportes = reportes.Where(reporte => reporte.TecnicoId == tecnicoId);
        }

        if (estado is not null)
        {
            reportes = reportes.Where(reporte => reporte.Estado == estado);
        }

        if (desde is not null)
        {
            var fechaInicialUtc = desde.Value.ToUniversalTime();
            reportes = reportes.Where(reporte => reporte.FechaCreacion >= fechaInicialUtc);
        }

        if (hasta is not null)
        {
            var fechaFinalUtc = hasta.Value.ToUniversalTime();
            reportes = reportes.Where(reporte => reporte.FechaCreacion <= fechaFinalUtc);
        }

        return reportes;
    }

}
