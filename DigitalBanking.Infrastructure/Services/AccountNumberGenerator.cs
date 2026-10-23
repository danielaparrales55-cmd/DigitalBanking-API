using DigitalBanking.Application.Interfaces;

namespace DigitalBanking.Infrastructure.Services;

public class AccountNumberGenerator : IAccountNumberGenerator
{
    public string Generate()
    {
        string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        string randomPart = new Random().Next(1000, 9999).ToString();
        return $"ACC-{datePart}-{randomPart}";
    }
}