namespace Domain.Entities;

public sealed class Confirmacion
{
    public string Id { get; set; } = string.Empty;
    public string ReporteId { get; set; } = string.Empty;
    public string CiudadanoId { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
