namespace Domain.Entities;

public class Zona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string? CodigoPostal { get; set; }
    
    // Propiedad de navegación
    public ICollection<HorarioCorte> Horarios { get; set; } = new List<HorarioCorte>();
}
