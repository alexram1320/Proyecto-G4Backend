namespace ApagonYa.Application.DTOs.Zones;

public record ZonaDto(
    int Id,
    string Nombre,
    string Departamento,
    string Municipio,
    string? CodigoPostal
);

public record CrearZonaDto(
    string Nombre,
    string Departamento,
    string Municipio,
    string? CodigoPostal
);

public record ActualizarZonaDto(
    string Nombre,
    string Departamento,
    string Municipio,
    string? CodigoPostal
);
