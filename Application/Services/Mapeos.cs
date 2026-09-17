
using System;
using System.Collections.Generic;
using System.Text;
using Application.DTOs.Auth;
using Domain.Entities;
using Application.DTOs.Reportes;
using Application.DTOs.Resoluciones;
using Application.DTOs.Zonas;

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
        
        public static ReporteDto Dto(this ReporteCorteEnergia reporte)
        {
            return new ReporteDto(
                reporte.Id,
                reporte.ZonaId,
                reporte.CiudadanoId,
                reporte.TecnicoId,
                reporte.DireccionAproximada,
                reporte.FechaHoraInicio,
                reporte.Estado.ToString(),
                reporte.UrlEvidencia,
                reporte.CantidadConfirmaciones,
                reporte.FechaCreacion);
        }

        public static ResolucionDto Dto(this Resolucion resolucion)
        {
            return new ResolucionDto(
                resolucion.Id,
                resolucion.ReporteId,
                resolucion.TecnicoId,
                resolucion.Causa,
                resolucion.Descripcion,
                resolucion.FechaEstimadaResolucion,
                resolucion.FechaRestablecimiento,
                resolucion.FechaCreacion);
        }
            public static ZonaDto Dto(this Zona zona)
            {
                return new ZonaDto(
                    zona.Id,
                    zona.Nombre,
                    zona.Descripcion,
                    zona.Activa,
                    zona.FechaCreacion);
            }

    }
}
