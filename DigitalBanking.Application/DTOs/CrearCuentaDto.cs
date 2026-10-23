namespace DigitalBanking.Application.DTOs;

public record CrearCuentaDto(
    int CodigoCliente,
    decimal SaldoInicial
);