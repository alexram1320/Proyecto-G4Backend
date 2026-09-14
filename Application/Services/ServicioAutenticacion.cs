using ApagonYa.Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public sealed class ServicioAutenticacion(IRepositorioUsuario usuarios, IServicioAutenticacionFirebase firebase, IServicioJwt jwt, IUsuarioActual usuarioActual) : IServicioAutenticacion
    {

        public async Task<Respuesta<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto solicitud, CancellationToken ct)
        {
            var email = solicitud.Email.Trim().ToLowerInvariant();

            //Aqui implentar la interface de zonas par avalidar si esta activa o no
          

            if (await usuarios.PorEmailAsync(email, ct) is not null ||
                await firebase.BuscarUsuarioPorEmailAsync(email, ct) is not null)
            {
                return Respuesta<UsuarioDto>.Fallida("El correo ya esta registrado", 409);
            }

            var uid = await firebase.CrearUsuarioAsync(
                email,
                solicitud.Contrasena,
                solicitud.Nombre,
                RolUsuario.CITIZEN.ToString(),
                ct);
            var ahora = DateTime.UtcNow;
            var usuario = new Usuario
            {
                Id = Guid.NewGuid().ToString("N"),
                FirebaseUid = uid,
                Nombre = solicitud.Nombre.Trim(),
                Email = email,
                Rol = RolUsuario.CITIZEN,
                ZonaId = solicitud.ZonaId,
                FechaCreacion = ahora,
                FechaActualizacion = ahora
            };

            await usuarios.CrearAsync(usuario, ct);
            await firebase.AsignarClaimsAsync(
                uid,
                new Dictionary<string, object>
                {
                    ["role"] = usuario.Rol.ToString(),
                    ["user_id"] = usuario.Id,
                    ["zone_id"] = usuario.ZonaId ?? string.Empty
                },
                ct);

            return Respuesta<UsuarioDto>.Correcta(usuario.Dto(), "Usuario registrado", 201);
        }

        public async Task<Respuesta<SesionDto>> IniciarSesionAsync(IniciarSesionDto solicitud, CancellationToken ct)
        {
            var sesionFirebase = await firebase.IniciarSesionAsync(solicitud.Email, solicitud.Contrasena, ct);
            var usuario = await usuarios.PorFirebaseUidAsync(sesionFirebase.Uid, ct);
            if (usuario is null || !usuario.Activo)
            {
                return Respuesta<SesionDto>.Fallida("Usuario no habilitado", 403);
            }

            var token = jwt.Crear(usuario);
            var tokenFirebase = await firebase.CrearTokenPersonalizadoAsync(usuario.FirebaseUid, ct);
            var sesion = new SesionDto(
                token.Token,
                sesionFirebase.TokenRenovacion,
                tokenFirebase,
                token.ExpiraEn,
                usuario.Dto());

            return Respuesta<SesionDto>.Correcta(sesion, "Sesion iniciada");
        }

        public async Task<Respuesta<SesionDto>> RenovarAsync(RenovarTokenDto solicitud, CancellationToken ct)
        {
            var sesionFirebase = await firebase.RenovarAsync(solicitud.TokenRenovacion, ct);
            var usuario = await usuarios.PorFirebaseUidAsync(sesionFirebase.Uid, ct);
            if (usuario is null || !usuario.Activo)
            {
                return Respuesta<SesionDto>.Fallida("Usuario no habilitado", 403);
            }

            var token = jwt.Crear(usuario);
            var tokenFirebase = await firebase.CrearTokenPersonalizadoAsync(usuario.FirebaseUid, ct);
            var sesion = new SesionDto(
                token.Token,
                sesionFirebase.TokenRenovacion,
                tokenFirebase,
                token.ExpiraEn,
                usuario.Dto());

            return Respuesta<SesionDto>.Correcta(sesion);
        }

        public async Task<Respuesta<bool>> RecuperarContrasenaAsync(RecuperarContrasenaDto solicitud, CancellationToken ct)
        {
            await firebase.EnviarRecuperacionAsync(solicitud.Email, ct);
            return Respuesta<bool>.Correcta(true, "Si el correo existe, recibira instrucciones");
        }

        public async Task<Respuesta<bool>> CerrarSesionAsync(CancellationToken ct)
        {
            await firebase.RevocarTokensAsync(usuarioActual.FirebaseUid, ct);
            return Respuesta<bool>.Correcta(true, "Sesion revocada");
        }

        public async Task<Respuesta<UsuarioDto>> PerfilAsync(CancellationToken ct)
        {
            var usuario = await usuarios.PorIdAsync(usuarioActual.UsuarioId, ct);
            return usuario is null
                ? Respuesta<UsuarioDto>.Fallida("Usuario no encontrado", 404)
                : Respuesta<UsuarioDto>.Correcta(usuario.Dto());
        }
    }
}

