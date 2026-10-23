namespace DigitalBanking.Domain.Entities;

public class Transaccion
{
    public int Codigo { get; set; }
    public string CodigoTransaccion { get; set; } = Guid.NewGuid().ToString();
    public string Tipo { get; set; } = string.Empty; // "Deposito" o "Retiro"
    public decimal Monto { get; set; }
    public DateTime MarcaTemporal { get; set; } = DateTime.UtcNow;
    public decimal SaldoRestante { get; set; }
    
    public int CodigoCuenta { get; set; }
    public CuentaBancaria CuentaBancaria { get; set; } = null!;
}