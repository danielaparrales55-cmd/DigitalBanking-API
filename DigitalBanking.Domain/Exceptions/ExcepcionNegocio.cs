namespace DigitalBanking.Domain.Exceptions;

public class ExcepcionNegocio : Exception
{
    public ExcepcionNegocio (string mensaje) : base(mensaje) { }
}