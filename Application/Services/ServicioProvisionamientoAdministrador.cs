using Application.DTOs.Auth;
using Application.Interfaces.Infrastructure;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public sealed class ServicioProvisionamientoAdministrador(IRepositorioUsuario usuarios, IServicioAutenticacionFirebase firebase) : IServicioProvisionamientoAdministrador
    {
        public async Task<UsuarioDto> ProvisionarAsync(string nombre, string email, string contrasena, CancellationToken ct)
        {
            nombre = nombre.Trim();
            email = email.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("AdminInicial:Nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("AdminInicial:Email es obligatorio");

            var usuarioFirebase = await firebase.BuscarUsuarioPorEmailAsync(email, ct);
            var usuario = await usuarios.PorEmailAsync(email, ct);

            if (usuarioFirebase is null)
            {
                if (contrasena.Length < 8)
                    throw new ArgumentException("AdminInicial:Contrasena debe tener al menos 8 caracteres");

                var uid = await firebase.CrearUsuarioAsync(email, contrasena, nombre, RolUsuario.ADMIN.ToString(), ct);
                usuarioFirebase = new UsuarioFirebase(uid, email);
            }

            if (usuario is not null && usuario.FirebaseUid != usuarioFirebase.Uid)
                throw new InvalidOperationException("El correo pertenece a usuarios distintos en Firebase Authentication y Firestore");

            var ahora = DateTime.UtcNow;
            usuario ??= new Usuario
            {
                Id = Guid.NewGuid().ToString("N"),
                FirebaseUid = usuarioFirebase.Uid,
                Email = email,
                FechaCreacion = ahora
            };

            usuario.Nombre = nombre;
            usuario.Rol = RolUsuario.ADMIN;
            usuario.Activo = true;
            usuario.FechaActualizacion = ahora;

            if (await usuarios.PorEmailAsync(email, ct) is null)
                await usuarios.CrearAsync(usuario, ct);
            else
                await usuarios.ActualizarAsync(usuario, ct);

            await firebase.DeshabilitarAsync(usuario.FirebaseUid, false, ct);
            await firebase.AsignarClaimsAsync(usuario.FirebaseUid, new Dictionary<string, object>
            {
                ["role"] = RolUsuario.ADMIN.ToString(),
                ["user_id"] = usuario.Id,
                ["zone_id"] = usuario.ZonaId ?? string.Empty
            }, ct);

            return usuario.Dto();
        }
    }
}
