namespace DigitalBanking.Application.DTOs;

public record TransaccionDto(
    string CodigoTransaccion,
    string Tipo,
    decimal Monto,
    DateTime MarcaTemoral,
    decimal SaldoRestante
);