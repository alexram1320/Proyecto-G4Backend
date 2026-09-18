using ApagonYa.Application.Common;
using Application.DTOs.Tecnicos;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class ServicioTecnico(
    IRepositorioTecnico tecnicos,
    IRepositorioUsuario usuarios,
    IRepositorioZona zonas,
    IRepositorioReporte reportes,
    IUsuarioActual usuarioActual,
    IServicioAutenticacionFirebase firebase) : IServicioTecnico
{
    public async Task<Respuesta<IReadOnlyCollection<TecnicoDto>>> ListadoAsync(CancellationToken ct)
    {
        var resultado = new List<TecnicoDto>();
        var cargas = (await reportes.TodosAsync(null, null, null, null, null, ct))
            .Where(reporte => reporte.Estado != EstadoReporte.RESOLVED && reporte.TecnicoId is not null)
            .GroupBy(reporte => reporte.TecnicoId!)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());

        var listado = await tecnicos.ListadoAsync(ct);
        if (usuarioActual.Rol == RolUsuario.TECHNICIAN.ToString())
        {
            listado = listado.Where(tecnico => tecnico.ZonaId == usuarioActual.ZonaId).ToArray();
        }

        foreach (var tecnico in listado)
        {
            resultado.Add(await CrearDtoAsync(tecnico, ct, cargas.GetValueOrDefault(tecnico.Id)));
        }

        return Respuesta<IReadOnlyCollection<TecnicoDto>>.Correcta(resultado);
    }

    public async Task<Respuesta<TecnicoDto>> ActualAsync(CancellationToken ct)
    {
        if (usuarioActual.Rol != RolUsuario.TECHNICIAN.ToString())
        {
            return Respuesta<TecnicoDto>.Fallida("El usuario actual no es tecnico", 403);
        }

        var tecnico = await tecnicos.PorUsuarioIdAsync(usuarioActual.UsuarioId, ct);
        if (tecnico is null)
        {
            return Respuesta<TecnicoDto>.Fallida(
                "No existe un perfil tecnico vinculado al usuario actual",
                404);
        }

        return Respuesta<TecnicoDto>.Correcta(await CrearDtoAsync(tecnico, ct));
    }

    public async Task<Respuesta<DetalleTecnicoDto>> PorIdAsync(string id, CancellationToken ct)
    {
        var tecnico = await tecnicos.PorIdAsync(id, ct);

        if (tecnico is null)
        {
            return Respuesta<DetalleTecnicoDto>.Fallida("Tecnico no encontrado", 404);
        }

        if (usuarioActual.Rol == RolUsuario.TECHNICIAN.ToString() && tecnico.ZonaId != usuarioActual.ZonaId)
        {
            return Respuesta<DetalleTecnicoDto>.Fallida("No autorizado para consultar este tecnico", 403);
        }

        var dto = await CrearDtoAsync(tecnico, ct);
        var detalle = new DetalleTecnicoDto(
            dto,
            tecnico.FechaCreacion,
            tecnico.FechaActualizacion);

        return Respuesta<DetalleTecnicoDto>.Correcta(detalle);
    }

    public async Task<Respuesta<TecnicoDto>> CrearAsync(
        CrearTecnicoDto solicitud,
        CancellationToken ct)
    {
        var email = solicitud.Email.Trim().ToLowerInvariant();

        if (await zonas.PorIdAsync(solicitud.ZonaId, ct) is not { Activa: true })
        {
            return Respuesta<TecnicoDto>.Fallida("Zona invalida", 400);
        }

        if (await usuarios.PorEmailAsync(email, ct) is not null ||
            await firebase.BuscarUsuarioPorEmailAsync(email, ct) is not null)
        {
            return Respuesta<TecnicoDto>.Fallida("El correo ya esta registrado", 409);
        }

        var uid = await firebase.CrearUsuarioAsync(
            email,
            solicitud.Contrasena,
            solicitud.Nombre,
            RolUsuario.TECHNICIAN.ToString(),
            ct);
        var ahora = DateTime.UtcNow;
        var usuario = new Usuario
        {
            Id = Guid.NewGuid().ToString("N"),
            FirebaseUid = uid,
            Nombre = solicitud.Nombre.Trim(),
            Email = email,
            Rol = RolUsuario.TECHNICIAN,
            ZonaId = solicitud.ZonaId,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };
        var tecnico = new Tecnico
        {
            Id = Guid.NewGuid().ToString("N"),
            UsuarioId = usuario.Id,
            ZonaId = solicitud.ZonaId,
            Disponible = solicitud.Disponible,
            Estado = solicitud.Disponible ? EstadoTecnico.DISPONIBLE : EstadoTecnico.NO_DISPONIBLE,
            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        await usuarios.CrearAsync(usuario, ct);
        await tecnicos.CrearAsync(tecnico, ct);
        await AsignarClaimsAsync(usuario, ct);

        var dto = await CrearDtoAsync(tecnico, ct);
        return Respuesta<TecnicoDto>.Correcta(dto, "Tecnico creado", 201);
    }

    public async Task<Respuesta<TecnicoDto>> ActualizarAsync(string id,
        ActualizarTecnicoDto solicitud,
        CancellationToken ct)
    {
        var tecnico = await tecnicos.PorIdAsync(id, ct);

        if (tecnico is null)
        {
            return Respuesta<TecnicoDto>.Fallida("Tecnico no encontrado", 404);
        }

        var usuario = await usuarios.PorIdAsync(tecnico.UsuarioId, ct);

        if (usuario is null)
        {
            return Respuesta<TecnicoDto>.Fallida("Usuario relacionado no encontrado", 409);
        }
        var email = solicitud.Email.Trim().ToLowerInvariant();

        if (!string.Equals(email, usuario.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await usuarios.PorEmailAsync(email, ct) is not null ||
                await firebase.BuscarUsuarioPorEmailAsync(email, ct) is not null)
            {
                return Respuesta<TecnicoDto>.Fallida("El correo ya esta registrado", 409);
            }

            await firebase.ActualizarEmailAsync(usuario.FirebaseUid, email, ct);
            usuario.Email = email;
        }

        usuario.Nombre = solicitud.Nombre.Trim();
        usuario.FechaActualizacion = DateTime.UtcNow;

        await usuarios.ActualizarAsync(usuario, ct);
        return Respuesta<TecnicoDto>.Correcta(await CrearDtoAsync(tecnico, ct));
    }

    public async Task<Respuesta<TecnicoDto>> CambiarDisponibilidadAsync(string id,
        DisponibilidadTecnicoDto solicitud, CancellationToken ct)
    {
        var tecnico = await tecnicos.PorIdAsync(id, ct);

        if (tecnico is null)
        {
            return Respuesta<TecnicoDto>.Fallida("Tecnico no encontrado", 404);
        }

        tecnico.Disponible = solicitud.Disponible;
        tecnico.Estado = solicitud.Disponible ? EstadoTecnico.DISPONIBLE : EstadoTecnico.NO_DISPONIBLE;
        tecnico.FechaActualizacion = DateTime.UtcNow;

        await tecnicos.ActualizarAsync(tecnico, ct);
        return Respuesta<TecnicoDto>.Correcta(await CrearDtoAsync(tecnico, ct));
    }

    public async Task<Respuesta<TecnicoDto>> AsignarZonaAsync(string id,
        AsignarZonaDto solicitud, CancellationToken ct)
    {
        var tecnico = await tecnicos.PorIdAsync(id, ct);

        if (tecnico is null)
        {
            return Respuesta<TecnicoDto>.Fallida("Tecnico no encontrado", 404);
        }

        if (await zonas.PorIdAsync(solicitud.ZonaId, ct) is not { Activa: true })
        {
            return Respuesta<TecnicoDto>.Fallida("Zona invalida", 400);
        }

        tecnico.ZonaId = solicitud.ZonaId;
        tecnico.FechaActualizacion = DateTime.UtcNow;
        await tecnicos.ActualizarAsync(tecnico, ct);

        var usuario = await usuarios.PorIdAsync(tecnico.UsuarioId, ct);

        if (usuario is not null)
        {
            usuario.ZonaId = solicitud.ZonaId;

            await usuarios.ActualizarAsync(usuario, ct);
            await AsignarClaimsAsync(usuario, ct);
        }

        return Respuesta<TecnicoDto>.Correcta(await CrearDtoAsync(tecnico, ct));
    }

    public async Task<Respuesta<TecnicoDto>> CambiarEstadoAsync(
        string id,
        CambiarEstadoTecnicoDto solicitud,
        CancellationToken ct)
    {
        var tecnico = await tecnicos.PorIdAsync(id, ct);

        if (tecnico is null)
        {
            return Respuesta<TecnicoDto>.Fallida("Tecnico no encontrado", 404);
        }

        tecnico.Activo = solicitud.Activo;
        tecnico.FechaActualizacion = DateTime.UtcNow;
        await tecnicos.ActualizarAsync(tecnico, ct);

        var usuario = await usuarios.PorIdAsync(tecnico.UsuarioId, ct);

        if (usuario is not null)
        {
            usuario.Activo = solicitud.Activo;

            await usuarios.ActualizarAsync(usuario, ct);
            await firebase.DeshabilitarAsync(usuario.FirebaseUid, !solicitud.Activo, ct);
        }

        return Respuesta<TecnicoDto>.Correcta(await CrearDtoAsync(tecnico, ct));
    }

    private async Task<TecnicoDto> CrearDtoAsync(Tecnico tecnico, CancellationToken ct, int? carga = null)
    {
        var usuario = await usuarios.PorIdAsync(tecnico.UsuarioId, ct);
        var cargaActiva = carga ?? (await reportes.TodosAsync(null, null, null, null, tecnico.Id, ct))
            .Count(reporte => reporte.Estado != EstadoReporte.RESOLVED);

        return new TecnicoDto(
            tecnico.Id,
            tecnico.UsuarioId,
            usuario?.Nombre ?? string.Empty,
            usuario?.Email ?? string.Empty,
            tecnico.ZonaId,
            tecnico.Disponible,
            tecnico.Estado.ToString(),
            cargaActiva,
            tecnico.Activo);
    }

    private Task AsignarClaimsAsync(Usuario usuario, CancellationToken ct)
    {
        var claims = new Dictionary<string, object>
        {
            ["role"] = RolUsuario.TECHNICIAN.ToString(),
            ["user_id"] = usuario.Id,
            ["zone_id"] = usuario.ZonaId ?? string.Empty
        };

        return firebase.AsignarClaimsAsync(usuario.FirebaseUid, claims, ct);
    }
}
