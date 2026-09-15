using ApagonYa.Application.Common;
using Application.DTOs.Notificaciones;
using Application.DTOs.Resoluciones;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ServicioResolucion(
    IRepositorioResolucion resoluciones,
    IRepositorioReporte reportes,
    IRepositorioTecnico tecnicos,
    IRepositorioConfirmacion confirmaciones,
    IUsuarioActual usuarioActual,
    IServicioNotificacion notificaciones,
    IServicioVerificacion verificacion) : IServicioResolucion
{
    public async Task<Respuesta<DetalleResolucionDto>> PorReporteAsync(
        string reporteId,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(reporteId, ct);
        if (reporte is null)
        {
            return Respuesta<DetalleResolucionDto>.Fallida("Reporte no encontrado", 404);
        }

        var puedeVer = usuarioActual.Rol == RolUsuario.ADMIN.ToString() ||
            reporte.ZonaId == usuarioActual.ZonaId;
        if (!puedeVer)
        {
            return Respuesta<DetalleResolucionDto>.Fallida(
                "No autorizado para consultar la resolucion de este reporte", 403);
        }

        var resolucion = await resoluciones.PorReporteAsync(reporteId, ct);

        if (resolucion is null)
        {
            return Respuesta<DetalleResolucionDto>.Fallida(
                "Resolucion no encontrada", 404);
        }

        var detalle = new DetalleResolucionDto(
            resolucion.Dto(),
            resolucion.Estado.ToString());

        return Respuesta<DetalleResolucionDto>.Correcta(detalle);
    }

    public async Task<Respuesta<ResolucionDto>> RegistrarAsync(
        string reporteId,
        CrearResolucionDto solicitud,
        CancellationToken ct)
    {
        if (usuarioActual.Rol != RolUsuario.TECHNICIAN.ToString())
        {
            return Respuesta<ResolucionDto>.Fallida("Solo un tecnico puede resolver", 403);
        }

        var reporte = await reportes.PorIdAsync(reporteId, ct);

        if (reporte is null)
        {
            return Respuesta<ResolucionDto>.Fallida("Reporte no encontrado", 404);
        }

        var tecnico = await tecnicos.PorUsuarioIdAsync(usuarioActual.UsuarioId, ct);
        var esTecnicoAsignado = tecnico is { Activo: true } &&
            reporte.TecnicoId == tecnico.Id &&
            reporte.ZonaId == tecnico.ZonaId;

        if (!esTecnicoAsignado)
        {
            return Respuesta<ResolucionDto>.Fallida(
                "El tecnico no esta asignado al reporte", 403);
        }

        if (!verificacion.PuedeResolver(reporte.Estado))
        {
            return Respuesta<ResolucionDto>.Fallida(
                "El reporte no puede resolverse en su estado actual", 409);
        }

        if (solicitud.FechaEstimadaResolucion is null)
        {
            return Respuesta<ResolucionDto>.Fallida("La fecha estimada de resolucion es obligatoria", 400);
        }

        var fechaRestablecimiento = solicitud.FechaRestablecimiento.ToUniversalTime();
        var fechaEstimada = solicitud.FechaEstimadaResolucion.Value.ToUniversalTime();

        if (fechaRestablecimiento>DateTime.UtcNow.AddMinutes(5))
        {
            return Respuesta<ResolucionDto>.Fallida(
                "La fecha de restablecimiento no puede estar en el futuro", 400);
        }

        if (fechaRestablecimiento<reporte.FechaHoraInicio)
        {
            return Respuesta<ResolucionDto>.Fallida(
                "La fecha de restablecimiento no puede ser anterior al inicio del corte", 400);
        }

        if (fechaEstimada<reporte.FechaHoraInicio)
        {
            return Respuesta<ResolucionDto>.Fallida(
                "La fecha estimada no puede ser anterior al inicio del corte", 400);
        }

        var ahora = DateTime.UtcNow;
        var estadoAnterior = reporte.Estado;
        var resolucion = new Resolucion
        {
            Id = Guid.NewGuid().ToString("N"),
            ReporteId = reporteId,
            TecnicoId = tecnico!.Id,
            Causa = solicitud.Causa.Trim(),
            Descripcion = solicitud.Descripcion.Trim(),
            FechaEstimadaResolucion = fechaEstimada,
            FechaRestablecimiento = fechaRestablecimiento,
            FechaCreacion = ahora
        };

        reporte.Estado = EstadoReporte.RESOLVED;
        reporte.FechaActualizacion = ahora;

        var historial = new HistorialReporte
        {
            Id = Guid.NewGuid().ToString("N"),
            ReporteId = reporteId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = EstadoReporte.RESOLVED,
            UsuarioCambioId = usuarioActual.UsuarioId,
            FechaHora = ahora,
            Accion = "RESOLUCION",
            Descripcion = "Servicio electrico restablecido"
        };

        await resoluciones.RegistrarAsync(resolucion, reporte, historial, ct);

        var destinatarios = (await confirmaciones.PorReporteAsync(reporteId, ct))
            .Select(confirmacion => confirmacion.CiudadanoId)
            .Append(reporte.CiudadanoId)
            .Distinct();
        
        foreach (var ciudadanoId in destinatarios)
        {
            await notificaciones.RegistrarAsync(new CrearNotificacionDto(
                ciudadanoId,
                reporteId,
                TipoNotificacion.REPORTE_RESUELTO,
                "Reporte resuelto",
                "El servicio fue restablecido"), ct);
        }

        return Respuesta<ResolucionDto>.Correcta(
            resolucion.Dto(),
            "Resolucion registrada", 201);
    }
}
