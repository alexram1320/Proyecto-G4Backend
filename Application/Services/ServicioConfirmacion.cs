using ApagonYa.Application.Common;
using Application.DTOs.Confirmaciones;
using Application.DTOs.Notificaciones;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ServicioConfirmacion(
    IRepositorioConfirmacion confirmaciones,
    IRepositorioReporte reportes,
    IRepositorioUsuario usuarios,
    IRepositorioTecnico tecnicos,
    IUsuarioActual usuarioActual,
    IServicioNotificacion notificaciones) : IServicioConfirmacion
{
    public async Task<Respuesta<IReadOnlyCollection<ConfirmacionDto>>> PorReporteAsync(
        string reporteId,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(reporteId, ct);
        if (reporte is null)
        {
            return Respuesta<IReadOnlyCollection<ConfirmacionDto>>.Fallida("Reporte no encontrado", 404);
        }

        var puedeVer = usuarioActual.Rol == RolUsuario.ADMIN.ToString() ||
            reporte.ZonaId == usuarioActual.ZonaId;
        if (!puedeVer)
        {
            return Respuesta<IReadOnlyCollection<ConfirmacionDto>>.Fallida(
                "No esta autorizado para consultar las confirmaciones de este reporte", 403);
        }

        var confirmacionesEncontradas = await confirmaciones.PorReporteAsync(reporteId, ct);
        var resultado = confirmacionesEncontradas
            .Select(CrearDto)
            .ToArray();

        return Respuesta<IReadOnlyCollection<ConfirmacionDto>>.Correcta(resultado);
    }

    public async Task<Respuesta<ConfirmacionDto>> ConfirmarAsync(
        string reporteId,
        CancellationToken ct)
    {
        if (usuarioActual.Rol != RolUsuario.CITIZEN.ToString())
        {
            return Respuesta<ConfirmacionDto>.Fallida(
                "Solo ciudadanos pueden confirmar", 403);
        }

        var reporte = await reportes.PorIdAsync(reporteId, ct);

        if (reporte is null)
        {
            return Respuesta<ConfirmacionDto>.Fallida("Reporte no encontrado", 404);
        }

        if (reporte.CiudadanoId == usuarioActual.UsuarioId)
        {
            return Respuesta<ConfirmacionDto>.Fallida(
                "El creador no puede confirmar su propio reporte", 409);
        }

        var usuario = await usuarios.PorIdAsync(usuarioActual.UsuarioId, ct);

        if (usuario?.ZonaId != reporte.ZonaId)
        {
            return Respuesta<ConfirmacionDto>.Fallida(
                "Solo ciudadanos de la zona pueden confirmar", 403);
        }

        if (reporte.Estado == EstadoReporte.RESOLVED)
        {
            return Respuesta<ConfirmacionDto>.Fallida("El reporte ya fue resuelto", 409);
        }

        var estadoNuevo = reporte.Estado is EstadoReporte.NEW or EstadoReporte.IN_VERIFICATION
            ? EstadoReporte.CONFIRMED
            : reporte.Estado;
        var historial = new HistorialReporte
        {
            Id = Guid.NewGuid().ToString("N"),
            ReporteId = reporteId,
            EstadoAnterior = reporte.Estado,
            EstadoNuevo = estadoNuevo,
            UsuarioCambioId = usuarioActual.UsuarioId,
            FechaHora = DateTime.UtcNow,
            Accion = "CONFIRMACION_COMUNITARIA",
            Descripcion = "Confirmacion ciudadana registrada"
        };
        var resultado = await confirmaciones.ConfirmarAsync(
            reporteId,
            usuarioActual.UsuarioId,
            historial,
            ct);

        if (!resultado.Creada || resultado.Confirmacion is null)
        {
            return Respuesta<ConfirmacionDto>.Fallida(
                "El usuario ya confirmo este reporte", 409);
        }

        if (historial.EstadoAnterior != EstadoReporte.CONFIRMED &&
            historial.EstadoNuevo == EstadoReporte.CONFIRMED)
        {
            await notificaciones.RegistrarAsync(new CrearNotificacionDto(
                reporte.CiudadanoId,
                reporteId,
                TipoNotificacion.REPORTE_CONFIRMADO,
                "Reporte confirmado",
                "La comunidad confirmo el corte"), ct);

            var tecnicosZona = await tecnicos.ListadoAsync(ct);
            var destinatarios = reporte.TecnicoId is null
                ? tecnicosZona.Where(tecnico => tecnico.Activo && tecnico.ZonaId == reporte.ZonaId)
                : tecnicosZona.Where(tecnico => tecnico.Activo && tecnico.Id == reporte.TecnicoId);

            foreach (var tecnico in destinatarios)
            {
                await notificaciones.RegistrarAsync(new CrearNotificacionDto(
                    tecnico.UsuarioId,
                    reporteId,
                    TipoNotificacion.REPORTE_CONFIRMADO,
                    "Reporte confirmado con prioridad",
                    "La comunidad confirmo un corte que requiere atencion",
                    "ALTA"), ct);
            }
        }

        return Respuesta<ConfirmacionDto>.Correcta(
            CrearDto(resultado.Confirmacion),
            "Confirmacion registrada", 201);
    }

    private static ConfirmacionDto CrearDto(Confirmacion confirmacion)
    {
        return new ConfirmacionDto(
            confirmacion.Id,
            confirmacion.ReporteId,
            confirmacion.CiudadanoId,
            confirmacion.FechaCreacion);
    }
}
