using System.Globalization;
using ApagonYa.Application.Common;
using ApagonYa.Application.DTOs.Estadisticas;
using ApagonYa.Application.Interfaces.Repositories;
using ApagonYa.Application.Interfaces.Services;
using ApagonYa.Domain.Entities;
using ApagonYa.Domain.Enums;

namespace ApagonYa.Application.Services;

public sealed class ServicioEstadisticas(
    IRepositorioReporte reportes,
    IRepositorioZona zonas,
    IRepositorioTecnico tecnicos,
    IRepositorioUsuario usuarios,
    IRepositorioResolucion resoluciones) : IServicioEstadisticas
{
    public async Task<Respuesta<EstadisticasPanelDto>> PanelAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        var datos = await ObtenerDatosAsync(filtro, ct);
        var activos = datos.Count(reporte => reporte.Estado != EstadoReporte.RESOLVED);
        var resueltos = datos.Count(reporte => reporte.Estado == EstadoReporte.RESOLVED);
        var horasResolucion = await CalcularHorasResolucionAsync(datos, ct);
        var promedioPorTipo = await CalcularPromedioPorTipoCorteAsync(datos, ct);
        var porEstado = datos
            .GroupBy(reporte => reporte.Estado)
            .Select(grupo => new PuntoGraficoDto(grupo.Key.ToString(), grupo.Count()))
            .ToArray();
        var porZona = datos
            .GroupBy(reporte => reporte.ZonaId)
            .Select(grupo => new PuntoGraficoDto(grupo.Key, grupo.Count()))
            .ToArray();
        var promedioHoras = horasResolucion.Count == 0
            ? 0
            : horasResolucion.Average();
        var panel = new EstadisticasPanelDto(
            datos.Count,
            activos,
            resueltos,
            datos.Count(reporte => reporte.Estado == EstadoReporte.NEW),
            promedioHoras,
            porEstado,
            porZona,
            promedioPorTipo);

        return Respuesta<EstadisticasPanelDto>.Correcta(panel);
    }

    public async Task<Respuesta<IReadOnlyCollection<EstadisticasZonaDto>>> ZonasAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        var datos = await ObtenerDatosAsync(filtro, ct);
        var nombres = (await zonas.ListadoAsync(true, ct))
            .ToDictionary(zona => zona.Id, zona => zona.Nombre);
        var resultado = datos
            .GroupBy(reporte => reporte.ZonaId)
            .Select(grupo => CrearEstadisticaZona(grupo, nombres))
            .ToArray();

        return Respuesta<IReadOnlyCollection<EstadisticasZonaDto>>.Correcta(resultado);
    }

    public async Task<Respuesta<IReadOnlyCollection<EstadisticasTecnicoDto>>> TecnicosAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        var datos = await ObtenerDatosAsync(filtro, ct);
        var tecnicosEncontrados = await tecnicos.ListadoAsync(ct);
        var resultado = new List<EstadisticasTecnicoDto>();
        foreach (var tecnico in tecnicosEncontrados)
        {
            var usuario = await usuarios.PorIdAsync(tecnico.UsuarioId, ct);
            resultado.Add(CrearEstadisticaTecnico(tecnico, usuario?.Nombre ?? tecnico.Id, datos));
        }

        return Respuesta<IReadOnlyCollection<EstadisticasTecnicoDto>>.Correcta(resultado);
    }

    public async Task<Respuesta<EstadisticasTendenciaDto>> TendenciasAsync(
        FiltroEstadisticasDto filtro,
        string agrupacion,
        CancellationToken ct)
    {
        var datos = await ObtenerDatosAsync(filtro, ct);
        if (!agrupacion.Equals("semana", StringComparison.OrdinalIgnoreCase) &&
            !agrupacion.Equals("mes", StringComparison.OrdinalIgnoreCase))
        {
            return Respuesta<EstadisticasTendenciaDto>.Fallida("La agrupacion debe ser semana o mes", 400);
        }

        var reportesPorPeriodo = AgruparPorPeriodo(datos, agrupacion)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());
        var resolucionesPorPeriodo = new Dictionary<string, int>();
        foreach (var reporte in datos.Where(reporte => reporte.Estado == EstadoReporte.RESOLVED))
        {
            var resolucion = await resoluciones.PorReporteAsync(reporte.Id, ct);
            if (resolucion is null) continue;
            var periodo = ClavePeriodo(resolucion.FechaRestablecimiento, agrupacion);
            resolucionesPorPeriodo[periodo] = resolucionesPorPeriodo.GetValueOrDefault(periodo) + 1;
        }

        var etiquetas = reportesPorPeriodo.Keys
            .Concat(resolucionesPorPeriodo.Keys)
            .Distinct()
            .OrderBy(etiqueta => etiqueta)
            .ToArray();
        var tendencia = new EstadisticasTendenciaDto(
            etiquetas,
            etiquetas.Select(etiqueta => reportesPorPeriodo.GetValueOrDefault(etiqueta)).ToArray(),
            etiquetas.Select(etiqueta => resolucionesPorPeriodo.GetValueOrDefault(etiqueta)).ToArray());

        return Respuesta<EstadisticasTendenciaDto>.Correcta(tendencia);
    }

    private async Task<IReadOnlyCollection<ReporteCorteEnergia>> ObtenerDatosAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken ct)
    {
        if (filtro.FechaInicial is not null && filtro.FechaFinal is not null &&
            filtro.FechaInicial > filtro.FechaFinal)
        {
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final");
        }

        var tieneEstado = Enum.TryParse<EstadoReporte>(
            filtro.Estado,
            true,
            out var estado);

        return await reportes.TodosAsync(
            filtro.FechaInicial?.ToUniversalTime(),
            filtro.FechaFinal?.ToUniversalTime(),
            tieneEstado ? estado : null,
            filtro.ZonaId,
            filtro.TecnicoId,
            ct);
    }

    private async Task<List<double>> CalcularHorasResolucionAsync(
        IReadOnlyCollection<ReporteCorteEnergia> datos,
        CancellationToken ct)
    {
        var horas = new List<double>();

        foreach (var reporte in datos.Where(reporteActual =>
                     reporteActual.Estado == EstadoReporte.RESOLVED))
        {
            var resolucion = await resoluciones.PorReporteAsync(reporte.Id, ct);

            if (resolucion is not null)
            {
                horas.Add((resolucion.FechaRestablecimiento - reporte.FechaHoraInicio).TotalHours);
            }
        }

        return horas;
    }

    private async Task<IReadOnlyCollection<PuntoGraficoDto>> CalcularPromedioPorTipoCorteAsync(
        IReadOnlyCollection<ReporteCorteEnergia> datos,
        CancellationToken ct)
    {
        var tiempos = new List<(string Tipo, double Horas)>();
        foreach (var reporte in datos.Where(reporte => reporte.Estado == EstadoReporte.RESOLVED))
        {
            var resolucion = await resoluciones.PorReporteAsync(reporte.Id, ct);
            if (resolucion is null) continue;
            tiempos.Add((resolucion.Causa, (resolucion.FechaRestablecimiento - reporte.FechaHoraInicio).TotalHours));
        }

        return tiempos
            .GroupBy(item => item.Tipo, StringComparer.OrdinalIgnoreCase)
            .Select(grupo => new PuntoGraficoDto(grupo.Key, Math.Round((decimal)grupo.Average(item => item.Horas), 2)))
            .OrderBy(item => item.Etiqueta)
            .ToArray();
    }

    private static EstadisticasZonaDto CrearEstadisticaZona(
        IGrouping<string, ReporteCorteEnergia> grupo,
        IReadOnlyDictionary<string, string> nombres)
    {
        var total = grupo.Count();
        var resueltos = grupo.Count(reporte => reporte.Estado == EstadoReporte.RESOLVED);
        var porcentaje = total == 0
            ? 0
            : Math.Round(100m * resueltos / total, 2);

        return new EstadisticasZonaDto(
            grupo.Key,
            nombres.GetValueOrDefault(grupo.Key, grupo.Key),
            total,
            resueltos,
            porcentaje);
    }

    private static EstadisticasTecnicoDto CrearEstadisticaTecnico(
        Tecnico tecnico,
        string nombre,
        IReadOnlyCollection<ReporteCorteEnergia> datos)
    {
        var asignados = datos
            .Where(reporte => reporte.TecnicoId == tecnico.Id)
            .ToArray();
        var resueltos = asignados.Count(reporte => reporte.Estado == EstadoReporte.RESOLVED);
        var porcentaje = asignados.Length == 0
            ? 0
            : Math.Round(100m * resueltos / asignados.Length, 2);

        return new EstadisticasTecnicoDto(
            tecnico.Id,
            nombre,
            asignados.Length,
            resueltos,
            porcentaje);
    }

    private static IEnumerable<IGrouping<string, ReporteCorteEnergia>> AgruparPorPeriodo(
        IReadOnlyCollection<ReporteCorteEnergia> datos,
        string agrupacion)
    {
        return datos.GroupBy(reporte => ClavePeriodo(reporte.FechaCreacion, agrupacion));
    }

    private static string ClavePeriodo(DateTime fecha, string agrupacion) =>
        agrupacion.Equals("mes", StringComparison.OrdinalIgnoreCase)
            ? fecha.ToString("yyyy-MM")
            : $"{ISOWeek.GetYear(fecha)}-S{ISOWeek.GetWeekOfYear(fecha):00}";
}
