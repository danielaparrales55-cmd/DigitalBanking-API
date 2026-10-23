namespace DigitalBanking.Application.DTOs;

public record CuentaBancariaDto(
    int CodigoCuenta,
    string NumeroCuenta,
    decimal Saldo,
    int CodigoCliente
);