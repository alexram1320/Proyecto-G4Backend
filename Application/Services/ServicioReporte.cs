using ApagonYa.Application.Common;
using Application.DTOs.Notificaciones;
using Application.DTOs.Reportes;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ServicioReporte(
    IRepositorioReporte reportes,
    IRepositorioZona zonas,
    IRepositorioTecnico tecnicos,
    IRepositorioUsuario usuarios,
    IUsuarioActual usuarioActual,
    IAlmacenamientoFirebase almacenamiento,
    IServicioNotificacion notificaciones,
    IServicioVerificacion verificacion) : IServicioReporte
{
    public async Task<RespuestaPaginada<ReporteDto>> ListadoAsync(
        FiltroReporteDto filtro,
        CancellationToken ct)
    {
        if (usuarioActual.Rol == RolUsuario.TECHNICIAN.ToString())
        {
            filtro.ZonaId = usuarioActual.ZonaId;
        }
        else if (usuarioActual.Rol == RolUsuario.CITIZEN.ToString())
        {
            filtro.ZonaId = usuarioActual.ZonaId;
        }

        var (datos, total) = await reportes.ListadoAsync(filtro, ct);
        var resultado = datos.Select(reporte => reporte.Dto()).ToArray();

        return RespuestaPaginada<ReporteDto>.Crear(
            resultado,
            filtro.Pagina,
            filtro.TamanoPagina,
            total);
    }

    public async Task<Respuesta<DetalleReporteDto>> PorIdAsync(
        string id,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);

        if (reporte is null)
        {
            return Respuesta<DetalleReporteDto>.Fallida("Reporte no encontrado", 404);
        }

        if (!PuedeVer(reporte))
        {
            return Respuesta<DetalleReporteDto>.Fallida("No autorizado para este reporte", 403);
        }

        var detalle = new DetalleReporteDto(reporte.Dto(), reporte.FechaActualizacion);
        return Respuesta<DetalleReporteDto>.Correcta(detalle);
    }

    public async Task<Respuesta<ReporteDto>> CrearAsync(
        CrearReporteDto solicitud,
        CancellationToken ct)
    {
        if (usuarioActual.Rol != RolUsuario.CITIZEN.ToString())
        {
            return Respuesta<ReporteDto>.Fallida(
                "Solo un ciudadano puede crear reportes",
                403);
        }

        if (usuarioActual.ZonaId != solicitud.ZonaId)
        {
            return Respuesta<ReporteDto>.Fallida(
                "El ciudadano solo puede reportar cortes en su zona",
                403);
        }

        if (await zonas.PorIdAsync(solicitud.ZonaId, ct) is not { Activa: true })
        {
            return Respuesta<ReporteDto>.Fallida("Zona invalida", 400);
        }

        if (solicitud.FechaHoraInicio == default)
        {
            return Respuesta<ReporteDto>.Fallida("La hora de inicio es obligatoria", 400);
        }

        var reporteActivo = await reportes.ActivoPorZonaAsync(solicitud.ZonaId, ct);

        if (reporteActivo is not null)
        {
            return CrearRespuestaDuplicado(reporteActivo);
        }

        if (solicitud.FechaHoraInicio.ToUniversalTime() > DateTime.UtcNow.AddMinutes(5))
        {
            return Respuesta<ReporteDto>.Fallida(
                "La hora de inicio no puede estar en el futuro",
                400);
        }

        ArchivoAlmacenado? archivo = null;

        if (solicitud.Evidencia is not null)
        {
            archivo = await almacenamiento.SubirEvidenciaAsync(
                solicitud.Evidencia,
                solicitud.NombreEvidencia ?? "evidencia.jpg",
                solicitud.TipoContenidoEvidencia ?? "image/jpeg",
                ct);
        }

        var ahora = DateTime.UtcNow;
        var reporte = new ReporteCorteEnergia
        {
            Id = Guid.NewGuid().ToString("N"),
            ZonaId = solicitud.ZonaId,
            CiudadanoId = usuarioActual.UsuarioId,
            DireccionAproximada = solicitud.DireccionAproximada.Trim(),
            FechaHoraInicio = solicitud.FechaHoraInicio.ToUniversalTime(),
            UrlEvidencia = archivo?.Url,
            NombreEvidencia = archivo?.Nombre,
            TipoContenidoEvidencia = archivo?.TipoContenido,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };
        var historial = CrearHistorial(
            reporte.Id,
            EstadoReporte.NEW,
            EstadoReporte.NEW,
            "CREACION",
            "Reporte creado");
        var resultado = await reportes.CrearSiZonaLibreAsync(reporte, historial, ct);

        if (!resultado.Creado)
        {
            return CrearRespuestaDuplicado(resultado.Reporte);
        }

        var notificacion = new CrearNotificacionDto(
            usuarioActual.UsuarioId,
            reporte.Id,
            TipoNotificacion.REPORTE_CREADO,
            "Reporte creado",
            "El reporte fue registrado");

        await notificaciones.RegistrarAsync(notificacion, ct);
        await NotificarResponsablesDeZonaAsync(reporte, ct);
        return Respuesta<ReporteDto>.Correcta(reporte.Dto(), "Reporte creado", 201);
    }

    public async Task<Respuesta<ReporteDto>> ActualizarAsync(
        string id,
        ActualizarReporteDto solicitud,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);

        if (reporte is null)
        {
            return Respuesta<ReporteDto>.Fallida("Reporte no encontrado", 404);
        }

        var perteneceAlUsuario = reporte.CiudadanoId == usuarioActual.UsuarioId;

        if (!perteneceAlUsuario || reporte.Estado != EstadoReporte.NEW)
        {
            return Respuesta<ReporteDto>.Fallida("El reporte ya no puede editarse", 403);
        }

        if (solicitud.FechaHoraInicio == default)
        {
            return Respuesta<ReporteDto>.Fallida("La hora de inicio es obligatoria", 400);
        }

        if (solicitud.FechaHoraInicio.ToUniversalTime() > DateTime.UtcNow.AddMinutes(5))
        {
            return Respuesta<ReporteDto>.Fallida("La hora de inicio no puede estar en el futuro", 400);
        }

        reporte.DireccionAproximada = solicitud.DireccionAproximada.Trim();
        reporte.FechaHoraInicio = solicitud.FechaHoraInicio.ToUniversalTime();
        reporte.FechaActualizacion = DateTime.UtcNow;

        await reportes.ActualizarAsync(reporte, ct);
        return Respuesta<ReporteDto>.Correcta(reporte.Dto());
    }

    public async Task<Respuesta<ReporteDto>> AsignarTecnicoAsync(
        string id,
        AsignarTecnicoDto solicitud,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);

        if (reporte is null)
        {
            return Respuesta<ReporteDto>.Fallida("Reporte no encontrado", 404);
        }

        if (reporte.Estado == EstadoReporte.RESOLVED)
        {
            return Respuesta<ReporteDto>.Fallida("No se puede asignar un reporte resuelto", 409);
        }

        var tecnico = await tecnicos.PorIdAsync(solicitud.TecnicoId, ct);
        var tecnicoDisponible = tecnico is { Activo: true, Disponible: true };

        if (!tecnicoDisponible || tecnico!.ZonaId != reporte.ZonaId)
        {
            return Respuesta<ReporteDto>.Fallida(
                "Tecnico no disponible para la zona",
                400);
        }

        reporte.TecnicoId = tecnico.Id;
        reporte.FechaActualizacion = DateTime.UtcNow;

        var historial = CrearHistorial(
            reporte.Id,
            reporte.Estado,
            reporte.Estado,
            "ASIGNACION",
            $"Tecnico {tecnico.Id} asignado");

        await reportes.AsignarAsync(reporte, historial, ct);

        var notificacion = new CrearNotificacionDto(
            tecnico.UsuarioId,
            reporte.Id,
            TipoNotificacion.TECNICO_ASIGNADO,
            "Reporte asignado",
            "Tiene un nuevo reporte asignado");

        await notificaciones.RegistrarAsync(notificacion, ct);
        return Respuesta<ReporteDto>.Correcta(reporte.Dto());
    }

    public async Task<Respuesta<ReporteDto>> AceptarAsync(string id, CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);
        if (reporte is null)
        {
            return Respuesta<ReporteDto>.Fallida("Reporte no encontrado", 404);
        }

        var tecnico = await tecnicos.PorUsuarioIdAsync(usuarioActual.UsuarioId, ct);
        if (usuarioActual.Rol != RolUsuario.TECHNICIAN.ToString() ||
            tecnico is not { Activo: true } || reporte.TecnicoId != tecnico.Id)
        {
            return Respuesta<ReporteDto>.Fallida("El reporte no esta asignado al tecnico actual", 403);
        }

        if (reporte.Estado == EstadoReporte.RESOLVED)
        {
            return Respuesta<ReporteDto>.Fallida("El reporte ya fue resuelto", 409);
        }

        var anterior = reporte.Estado;
        if (reporte.Estado == EstadoReporte.NEW)
        {
            reporte.Estado = EstadoReporte.IN_VERIFICATION;
        }
        reporte.FechaActualizacion = DateTime.UtcNow;
        var historial = CrearHistorial(reporte.Id, anterior, reporte.Estado, "ACEPTACION", "Reporte aceptado por el tecnico asignado");
        await reportes.CambiarEstadoAsync(reporte, historial, ct);
        return Respuesta<ReporteDto>.Correcta(reporte.Dto(), "Reporte aceptado");
    }

    public async Task<Respuesta<ReporteDto>> ReasignarAsync(
        string id,
        AsignarTecnicoDto solicitud,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);
        if (reporte is null)
        {
            return Respuesta<ReporteDto>.Fallida("Reporte no encontrado", 404);
        }

        var tecnicoActual = await tecnicos.PorUsuarioIdAsync(usuarioActual.UsuarioId, ct);
        if (usuarioActual.Rol != RolUsuario.TECHNICIAN.ToString() ||
            tecnicoActual is not { Activo: true } || reporte.TecnicoId != tecnicoActual.Id)
        {
            return Respuesta<ReporteDto>.Fallida("Solo el tecnico asignado puede reasignar el reporte", 403);
        }

        if (solicitud.TecnicoId == tecnicoActual.Id)
        {
            return Respuesta<ReporteDto>.Fallida("Seleccione un tecnico diferente", 400);
        }

        return await AsignarTecnicoAsync(id, solicitud, ct);
    }

    public async Task<Respuesta<ReporteDto>> CambiarEstadoAsync(
        string id,
        CambiarEstadoReporteDto solicitud,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);

        if (reporte is null)
        {
            return Respuesta<ReporteDto>.Fallida("Reporte no encontrado", 404);
        }

        if (!await PuedeGestionarAsync(reporte, ct))
        {
            return Respuesta<ReporteDto>.Fallida(
                "No autorizado para modificar este reporte",
                403);
        }

        var transicionValida = verificacion.TransicionValida(
            reporte.Estado,
            solicitud.Estado);

        if (!transicionValida || solicitud.Estado == EstadoReporte.RESOLVED)
        {
            return Respuesta<ReporteDto>.Fallida(
                "Transicion de estado no permitida",
                409);
        }

        if (solicitud.Estado == EstadoReporte.CONFIRMED &&
            reporte.CantidadConfirmaciones < 1)
        {
            return Respuesta<ReporteDto>.Fallida(
                "El reporte requiere al menos una confirmacion ciudadana adicional",
                409);
        }

        var estadoAnterior = reporte.Estado;
        reporte.Estado = solicitud.Estado;
        reporte.FechaActualizacion = DateTime.UtcNow;

        var historial = CrearHistorial(
            reporte.Id,
            estadoAnterior,
            reporte.Estado,
            "CAMBIO_ESTADO",
            solicitud.Descripcion ?? "Estado actualizado");

        await reportes.CambiarEstadoAsync(reporte, historial, ct);

        var notificacion = new CrearNotificacionDto(
            reporte.CiudadanoId,
            reporte.Id,
            TipoNotificacion.ESTADO_CAMBIADO,
            "Estado actualizado",
            $"Nuevo estado: {reporte.Estado}");

        await notificaciones.RegistrarAsync(notificacion, ct);
        return Respuesta<ReporteDto>.Correcta(reporte.Dto());
    }

    public async Task<Respuesta<IReadOnlyCollection<HistorialReporteDto>>> HistorialAsync(
        string id,
        CancellationToken ct)
    {
        var reporte = await reportes.PorIdAsync(id, ct);
        if (reporte is null)
        {
            return Respuesta<IReadOnlyCollection<HistorialReporteDto>>.Fallida("Reporte no encontrado", 404);
        }
        if (!PuedeVer(reporte))
        {
            return Respuesta<IReadOnlyCollection<HistorialReporteDto>>.Fallida("No autorizado para este reporte", 403);
        }

        var historial = await reportes.HistorialAsync(id, ct);
        var resultado = historial.Select(item => new HistorialReporteDto(
            item.Id,
            item.ReporteId,
            item.EstadoAnterior.ToString(),
            item.EstadoNuevo.ToString(),
            item.UsuarioCambioId,
            item.FechaHora,
            item.Accion,
            item.Descripcion)).ToArray();
        return Respuesta<IReadOnlyCollection<HistorialReporteDto>>.Correcta(resultado);
    }

    private bool PuedeVer(ReporteCorteEnergia reporte)
    {
        var esAdministrador = usuarioActual.Rol == "ADMIN";
        var esCiudadanoDeLaZona = usuarioActual.Rol == "CITIZEN" && reporte.ZonaId == usuarioActual.ZonaId;
        var esTecnicoDeLaZona = usuarioActual.Rol == "TECHNICIAN" && reporte.ZonaId == usuarioActual.ZonaId;

        return esAdministrador || esCiudadanoDeLaZona || esTecnicoDeLaZona;
    }

    private async Task<bool> PuedeGestionarAsync(ReporteCorteEnergia reporte, CancellationToken ct)
    {
        if (usuarioActual.Rol == "ADMIN")
        {
            return true;
        }

        if (usuarioActual.Rol != "TECHNICIAN" || reporte.ZonaId != usuarioActual.ZonaId)
        {
            return false;
        }

        var tecnico = await tecnicos.PorUsuarioIdAsync(usuarioActual.UsuarioId, ct);
        return tecnico is { Activo: true } && reporte.TecnicoId == tecnico.Id;
    }

    private async Task NotificarResponsablesDeZonaAsync(ReporteCorteEnergia reporte, CancellationToken ct)
    {
        var destinatarios = new HashSet<string>();
        foreach (var usuario in await usuarios.ListadoAsync(ct))
        {
            if (usuario.Activo && usuario.Rol == RolUsuario.ADMIN)
            {
                destinatarios.Add(usuario.Id);
            }
        }

        foreach (var tecnico in await tecnicos.ListadoAsync(ct))
        {
            if (tecnico.Activo && tecnico.ZonaId == reporte.ZonaId)
            {
                destinatarios.Add(tecnico.UsuarioId);
            }
        }

        destinatarios.Remove(usuarioActual.UsuarioId);
        foreach (var usuarioId in destinatarios)
        {
            await notificaciones.RegistrarAsync(new CrearNotificacionDto(
                usuarioId,
                reporte.Id,
                TipoNotificacion.REPORTE_CREADO,
                "Nuevo corte reportado",
                "Se registro un corte en una zona bajo su responsabilidad"), ct);
        }
    }

    private HistorialReporte CrearHistorial(
        string reporteId,
        EstadoReporte estadoAnterior,
        EstadoReporte estadoNuevo,
        string accion,
        string descripcion)
    {
        return new HistorialReporte
        {
            Id = Guid.NewGuid().ToString("N"),
            ReporteId = reporteId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = estadoNuevo,
            UsuarioCambioId = usuarioActual.UsuarioId,
            FechaHora = DateTime.UtcNow,
            Accion = accion,
            Descripcion = descripcion
        };
    }

    private static Respuesta<ReporteDto> CrearRespuestaDuplicado(
        ReporteCorteEnergia reporte)
    {
        return Respuesta<ReporteDto>.Fallida(
            $"Ya existe un reporte activo en la zona. Reporte: {reporte.Id}",
            409,
            reporte.Id);
    }
}
