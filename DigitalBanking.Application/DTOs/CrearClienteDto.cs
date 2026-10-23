namespace DigitalBanking.Application.DTOs;

public record CrearClienteDto(
    string NombreCompleto,
    DateTime FechaNacimiento,
    string Genero,
    decimal IngresosMensuales
);