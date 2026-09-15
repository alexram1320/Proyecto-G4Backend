namespace Domain.Entities;

public class HorarioCorte
{
    public int Id { get; set; }
    public int ZonaId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "Programado";
    public string? Observaciones { get; set; }

    public Zona? Zona { get; set; }
}
