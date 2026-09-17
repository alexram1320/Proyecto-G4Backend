using ApagonYa.Application.Common;
using Application.DTOs.Zonas;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;         
using Domain.Entities;

namespace Application.Services;

public sealed class ServicioZona(IRepositorioZona zonas) : IServicioZona
{
    public async Task<Respuesta<IReadOnlyCollection<ZonaDto>>> ListadoAsync(
        bool incluirInactivas,
        CancellationToken ct)
    {
        var zonasEncontradas = await zonas.ListadoAsync(incluirInactivas, ct);
        var resultado = zonasEncontradas.Select(zona => zona.Dto()).ToArray();

        return Respuesta<IReadOnlyCollection<ZonaDto>>.Correcta(resultado);
    }

    public async Task<Respuesta<DetalleZonaDto>> PorIdAsync(
        string id,
        CancellationToken ct)
    {
        var zona = await zonas.PorIdAsync(id, ct);

        if (zona is null)
        {
            return Respuesta<DetalleZonaDto>.Fallida("Zona no encontrada", 404);
        }

        var detalle = new DetalleZonaDto(
            zona.Id,
            zona.Nombre,
            zona.Descripcion,
            zona.Activa,
            zona.FechaCreacion,
            zona.FechaActualizacion);

        return Respuesta<DetalleZonaDto>.Correcta(detalle);
    }

    public async Task<Respuesta<ZonaDto>> CrearAsync(
        CrearZonaDto solicitud,
        CancellationToken ct)
    {
        var ahora = DateTime.UtcNow;
        var zona = new Zona
        {
            Id = Guid.NewGuid().ToString("N"),
            Nombre = solicitud.Nombre.Trim(),
            Descripcion = solicitud.Descripcion.Trim(),
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        await zonas.CrearAsync(zona, ct);
        return Respuesta<ZonaDto>.Correcta(zona.Dto(), "Zona creada", 201);
    }

    public async Task<Respuesta<ZonaDto>> ActualizarAsync(
        string id,
        ActualizarZonaDto solicitud,
        CancellationToken ct)
    {
        var zona = await zonas.PorIdAsync(id, ct);

        if (zona is null)
        {
            return Respuesta<ZonaDto>.Fallida("Zona no encontrada", 404);
        }

        zona.Nombre = solicitud.Nombre.Trim();
        zona.Descripcion = solicitud.Descripcion.Trim();
        zona.FechaActualizacion = DateTime.UtcNow;

        await zonas.ActualizarAsync(zona, ct);
        return Respuesta<ZonaDto>.Correcta(zona.Dto(), "Zona actualizada");
    }

    public async Task<Respuesta<ZonaDto>> CambiarEstadoAsync(
        string id,
        CambiarEstadoZonaDto solicitud,
        CancellationToken ct)
    {
        var zona = await zonas.PorIdAsync(id, ct);

        if (zona is null)
        {
            return Respuesta<ZonaDto>.Fallida("Zona no encontrada", 404);
        }

        zona.Activa = solicitud.Activa;
        zona.FechaActualizacion = DateTime.UtcNow;

        await zonas.ActualizarAsync(zona, ct);
        return Respuesta<ZonaDto>.Correcta(zona.Dto(), "Estado actualizado");
    }
}
