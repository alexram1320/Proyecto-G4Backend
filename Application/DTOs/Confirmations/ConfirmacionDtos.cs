namespace Application.DTOs.Confirmaciones;

public sealed record CrearConfirmacionDto;

public sealed record ConfirmacionDto(
    string Id, 
    string ReporteId, 
    string CiudadanoId, 
    DateTime FechaCreacion);

public sealed record ResumenConfirmacionDto(
    string ReporteId, 
    int Cantidad, 
    bool ConfirmadoPorUsuarioActual);

