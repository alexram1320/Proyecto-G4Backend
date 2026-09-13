
using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.Auth;
using Domain.Entities;

namespace Application.Services
{
    internal static class Mapeos
    {
        public static UsuarioDto Dto(this Usuario usuario)
        {
            return new UsuarioDto(
                usuario.Id,
                usuario.FirebaseUid,
                usuario.Nombre,
                usuario.Email,
                usuario.Rol.ToString(),
                usuario.ZonaId,
                usuario.Activo);
        }
    }
}
