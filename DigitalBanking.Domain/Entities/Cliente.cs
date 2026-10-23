namespace DigitalBanking.Domain.Entities;

public class Cliente
{
    public int CodigoCliente { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public decimal IngresosMensuales { get; set; }
    
    // Relación con cuentas bancarias
    public ICollection<CuentaBancaria> CuentasBancarias { get; set; } = new List<CuentaBancaria>();
}