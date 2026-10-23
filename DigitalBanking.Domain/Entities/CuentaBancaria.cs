namespace DigitalBanking.Domain.Entities;

public class CuentaBancaria
{
    public int CodigoCuenta { get; set; }
    public string NumeroCuenta { get; set; } = string.Empty;
    public decimal Saldo { get; set; }
    public int CodigoCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;
    
    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}