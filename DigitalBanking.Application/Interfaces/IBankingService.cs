using DigitalBanking.Application.DTOs;

namespace DigitalBanking.Application.Interfaces;

public interface IBankingService
{
    Task<ClienteDto> CreateCustomerAsync(CrearClienteDto dto);
    Task<CuentaBancariaDto> CreateAccountAsync(CrearCuentaDto dto);
    Task<decimal> GetBalanceAsync(string accountNumber);
    Task<TransaccionDto> DepositAsync(string accountNumber, decimal amount);
    Task<TransaccionDto> WithdrawAsync(string accountNumber, decimal amount);
    Task<IEnumerable<TransaccionDto>> GetTransactionHistoryAsync(string accountNumber);
}