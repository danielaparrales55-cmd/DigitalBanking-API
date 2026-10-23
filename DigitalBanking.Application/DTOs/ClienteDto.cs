namespace DigitalBanking.Application.DTOs;

public record ClienteDto(
    int CodigoCliente,
    string NombreCompleto,
    DateTime FechaNacimiento,
    string Genero,
    decimal IngresosMensuales
);