using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services;

public sealed class ServicioVerificacion : IServicioVerificacion
{
    public bool TransicionValida(EstadoReporte actual, EstadoReporte nuevo) => (actual, nuevo) is
        (EstadoReporte.NEW, EstadoReporte.IN_VERIFICATION) or
        (EstadoReporte.NEW, EstadoReporte.CONFIRMED) or
        (EstadoReporte.IN_VERIFICATION, EstadoReporte.CONFIRMED);

    public bool PuedeResolver(EstadoReporte estado) => estado == EstadoReporte.CONFIRMED;
}
