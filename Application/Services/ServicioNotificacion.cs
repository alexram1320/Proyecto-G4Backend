using ApagonYa.Application.Common;
using Application.DTOs.Notificaciones;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public sealed class ServicioNotificacion(
    IRepositorioNotificacion repositorio,
    IUsuarioActual usuarioActual) : IServicioNotificacion
{
    public Task RegistrarAsync(
        CrearNotificacionDto solicitud,
        CancellationToken ct)
    {
        var notificacion = new Notificacion
        {
            Id = Guid.NewGuid().ToString("N"),
            UsuarioId = solicitud.UsuarioId,
            ReporteId = solicitud.ReporteId,
            Tipo = solicitud.Tipo,
            Titulo = solicitud.Titulo,
            Mensaje = solicitud.Mensaje,
            Prioridad = solicitud.Prioridad.Equals(
                "ALTA",
                StringComparison.OrdinalIgnoreCase)
                ? "ALTA"
                : "NORMAL",
            FechaCreacion = DateTime.UtcNow
        };

        return repositorio.CrearAsync(notificacion, ct);
    }

    public async Task<Respuesta<IReadOnlyCollection<NotificacionDto>>> ListadoAsync(
        int limite,
        CancellationToken ct)
    {
        var datos = await repositorio.PorUsuarioAsync(
            usuarioActual.UsuarioId,
            limite,
            ct);

        var resultado = datos.Select(notificacion => new NotificacionDto(
            notificacion.Id,
            notificacion.UsuarioId,
            notificacion.ReporteId,
            notificacion.Tipo.ToString(),
            notificacion.Titulo,
            notificacion.Mensaje,
            notificacion.Prioridad,
            notificacion.Leida,
            notificacion.FechaCreacion))
            .ToArray();

        return Respuesta<IReadOnlyCollection<NotificacionDto>>
            .Correcta(resultado);
    }

    public async Task<Respuesta<bool>> MarcarLeidaAsync(
        string id,
        CancellationToken ct)
    {
        var actualizada = await repositorio.MarcarLeidaAsync(
            id,
            usuarioActual.UsuarioId,
            ct);

        return actualizada
            ? Respuesta<bool>.Correcta(
                true,
                "Notificacion marcada como leida")
            : Respuesta<bool>.Fallida(
                "Notificacion no encontrada",
                404);
    }
}