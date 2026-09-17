using ApagonYa.Application.Common;
using ApagonYa.Application.DTOs.Autenticacion;
using ApagonYa.Application.DTOs.Confirmaciones;
using ApagonYa.Application.DTOs.Estadisticas;
using ApagonYa.Application.DTOs.Notificaciones;
using ApagonYa.Application.DTOs.Reportes;
using ApagonYa.Application.DTOs.Resoluciones;
using ApagonYa.Application.DTOs.Tecnicos;
using ApagonYa.Application.DTOs.Zonas;

namespace ApagonYa.Application.Interfaces.Services;

public interface IServicioAutenticacion
{
    Task<Respuesta<UsuarioDto>> RegistrarAsync(RegistrarUsuarioDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<SesionDto>> IniciarSesionAsync(IniciarSesionDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<SesionDto>> RenovarAsync(RenovarTokenDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<bool>> RecuperarContrasenaAsync(RecuperarContrasenaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<bool>> CerrarSesionAsync(CancellationToken cancellationToken);
    Task<Respuesta<UsuarioDto>> PerfilAsync(CancellationToken cancellationToken);
}
public interface IServicioProvisionamientoAdministrador
{
    Task<UsuarioDto> ProvisionarAsync(string nombre, string email, string contrasena, CancellationToken cancellationToken);
}
public interface IServicioZona
{
    Task<Respuesta<IReadOnlyCollection<ZonaDto>>> ListadoAsync(bool incluirInactivas, CancellationToken cancellationToken);
    Task<Respuesta<DetalleZonaDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<ZonaDto>> CrearAsync(CrearZonaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ZonaDto>> ActualizarAsync(string id, ActualizarZonaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ZonaDto>> CambiarEstadoAsync(string id, CambiarEstadoZonaDto solicitud, CancellationToken cancellationToken);
}
public interface IServicioTecnico
{
    Task<Respuesta<IReadOnlyCollection<TecnicoDto>>> ListadoAsync(CancellationToken cancellationToken);
    Task<Respuesta<DetalleTecnicoDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> CrearAsync(CrearTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> ActualizarAsync(string id, ActualizarTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> CambiarDisponibilidadAsync(
        string id,
        DisponibilidadTecnicoDto solicitud,
        CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> AsignarZonaAsync(string id, AsignarZonaDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<TecnicoDto>> CambiarEstadoAsync(string id, CambiarEstadoTecnicoDto solicitud, CancellationToken cancellationToken);
}
public interface IServicioReporte
{
    Task<RespuestaPaginada<ReporteDto>> ListadoAsync(FiltroReporteDto filtro, CancellationToken cancellationToken);
    Task<Respuesta<DetalleReporteDto>> PorIdAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> CrearAsync(CrearReporteDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> ActualizarAsync(string id, ActualizarReporteDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> AsignarTecnicoAsync(string id, AsignarTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> AceptarAsync(string id, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> ReasignarAsync(string id, AsignarTecnicoDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<ReporteDto>> CambiarEstadoAsync(string id, CambiarEstadoReporteDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<IReadOnlyCollection<HistorialReporteDto>>> HistorialAsync(string id, CancellationToken cancellationToken);
}
public interface IServicioVerificacion
{
    bool TransicionValida(ApagonYa.Domain.Enums.EstadoReporte estadoActual, ApagonYa.Domain.Enums.EstadoReporte estadoNuevo);
    bool PuedeResolver(ApagonYa.Domain.Enums.EstadoReporte estado);
}
public interface IServicioConfirmacion
{
    Task<Respuesta<IReadOnlyCollection<ConfirmacionDto>>> PorReporteAsync(string reporteId, CancellationToken cancellationToken);
    Task<Respuesta<ConfirmacionDto>> ConfirmarAsync(string reporteId, CancellationToken cancellationToken);
}
public interface IServicioResolucion
{
    Task<Respuesta<DetalleResolucionDto>> PorReporteAsync(string reporteId, CancellationToken cancellationToken);
    Task<Respuesta<ResolucionDto>> RegistrarAsync(string reporteId, CrearResolucionDto solicitud, CancellationToken cancellationToken);
}
public interface IServicioNotificacion
{
    Task RegistrarAsync(CrearNotificacionDto solicitud, CancellationToken cancellationToken);
    Task<Respuesta<IReadOnlyCollection<NotificacionDto>>> ListadoAsync(int limite, CancellationToken cancellationToken);
    Task<Respuesta<bool>> MarcarLeidaAsync(string id, CancellationToken cancellationToken);
}
public interface IServicioEstadisticas
{
    Task<Respuesta<EstadisticasPanelDto>> PanelAsync(FiltroEstadisticasDto filtro, CancellationToken cancellationToken);
    Task<Respuesta<IReadOnlyCollection<EstadisticasZonaDto>>> ZonasAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken cancellationToken);

    Task<Respuesta<IReadOnlyCollection<EstadisticasTecnicoDto>>> TecnicosAsync(
        FiltroEstadisticasDto filtro,
        CancellationToken cancellationToken);

    Task<Respuesta<EstadisticasTendenciaDto>> TendenciasAsync(
        FiltroEstadisticasDto filtro,
        string agrupacion,
        CancellationToken cancellationToken);
}