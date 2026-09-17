using ApagonYa.Application.Common;
using Application.DTOs.Horarios;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ServicioHorario(
    IRepositorioHorario horarios,
    IRepositorioZona zonas,
    IUsuarioActual usuarioActual) : IServicioHorario
{
    public async Task<Respuesta<IReadOnlyCollection<HorarioDto>>> ListadoAsync(
        string? zonaId,
        bool incluirInactivos,
        CancellationToken ct)
    {
        if (usuarioActual.Rol != RolUsuario.ADMIN.ToString())
        {
            zonaId = usuarioActual.ZonaId;
            incluirInactivos = false;
        }

        var encontrados = await horarios.ListadoAsync(zonaId, incluirInactivos, ct);
        return Respuesta<IReadOnlyCollection<HorarioDto>>.Correcta(encontrados.Select(CrearDto).ToArray());
    }

    public async Task<Respuesta<HorarioDto>> PorIdAsync(string id, CancellationToken ct)
    {
        var horario = await horarios.PorIdAsync(id, ct);
        if (horario is null)
        {
            return Respuesta<HorarioDto>.Fallida("Horario no encontrado", 404);
        }

        if (usuarioActual.Rol != RolUsuario.ADMIN.ToString() &&
            (horario.ZonaId != usuarioActual.ZonaId || !horario.Activo))
        {
            return Respuesta<HorarioDto>.Fallida("No autorizado para consultar este horario", 403);
        }

        return Respuesta<HorarioDto>.Correcta(CrearDto(horario));
    }

    public async Task<Respuesta<HorarioDto>> CrearAsync(CrearHorarioDto solicitud, CancellationToken ct)
    {
        var error = await ValidarAsync(
            solicitud.ZonaId,
            solicitud.Titulo,
            solicitud.FechaHoraInicio,
            solicitud.FechaHoraFin,
            ct);
        if (error is not null)
        {
            return Respuesta<HorarioDto>.Fallida(error, 400);
        }

        var ahora = DateTime.UtcNow;
        var horario = new HorarioCorte
        {
            Id = Guid.NewGuid().ToString("N"),
            ZonaId = solicitud.ZonaId,
            Titulo = solicitud.Titulo.Trim(),
            Descripcion = solicitud.Descripcion.Trim(),
            FechaHoraInicio = solicitud.FechaHoraInicio.ToUniversalTime(),
            FechaHoraFin = solicitud.FechaHoraFin.ToUniversalTime(),
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        await horarios.CrearAsync(horario, ct);
        return Respuesta<HorarioDto>.Correcta(CrearDto(horario), "Horario creado", 201);
    }

    public async Task<Respuesta<HorarioDto>> ActualizarAsync(
        string id,
        ActualizarHorarioDto solicitud,
        CancellationToken ct)
    {
        var horario = await horarios.PorIdAsync(id, ct);
        if (horario is null)
        {
            return Respuesta<HorarioDto>.Fallida("Horario no encontrado", 404);
        }

        var error = await ValidarAsync(
            solicitud.ZonaId,
            solicitud.Titulo,
            solicitud.FechaHoraInicio,
            solicitud.FechaHoraFin,
            ct);
        if (error is not null)
        {
            return Respuesta<HorarioDto>.Fallida(error, 400);
        }

        horario.ZonaId = solicitud.ZonaId;
        horario.Titulo = solicitud.Titulo.Trim();
        horario.Descripcion = solicitud.Descripcion.Trim();
        horario.FechaHoraInicio = solicitud.FechaHoraInicio.ToUniversalTime();
        horario.FechaHoraFin = solicitud.FechaHoraFin.ToUniversalTime();
        horario.FechaActualizacion = DateTime.UtcNow;
        await horarios.ActualizarAsync(horario, ct);

        return Respuesta<HorarioDto>.Correcta(CrearDto(horario), "Horario actualizado");
    }

    public async Task<Respuesta<HorarioDto>> CambiarEstadoAsync(
        string id,
        CambiarEstadoHorarioDto solicitud,
        CancellationToken ct)
    {
        var horario = await horarios.PorIdAsync(id, ct);
        if (horario is null)
        {
            return Respuesta<HorarioDto>.Fallida("Horario no encontrado", 404);
        }

        horario.Activo = solicitud.Activo;
        horario.FechaActualizacion = DateTime.UtcNow;
        await horarios.ActualizarAsync(horario, ct);
        return Respuesta<HorarioDto>.Correcta(CrearDto(horario), "Estado del horario actualizado");
    }

    private async Task<string?> ValidarAsync(
        string zonaId,
        string titulo,
        DateTime inicio,
        DateTime fin,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(titulo)) return "El titulo es obligatorio";
        if (inicio == default || fin == default) return "Las fechas de inicio y fin son obligatorias";
        if (fin.ToUniversalTime() <= inicio.ToUniversalTime()) return "La fecha de fin debe ser posterior al inicio";
        return await zonas.PorIdAsync(zonaId, ct) is { Activa: true }
            ? null
            : "La zona no existe o esta inactiva";
    }

    private static HorarioDto CrearDto(HorarioCorte horario) => new(
        horario.Id,
        horario.ZonaId,
        horario.Titulo,
        horario.Descripcion,
        horario.FechaHoraInicio,
        horario.FechaHoraFin,
        horario.Activo,
        horario.FechaCreacion,
        horario.FechaActualizacion);
}
