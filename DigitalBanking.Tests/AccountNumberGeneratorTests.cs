using DigitalBanking.Infrastructure.Services;
using Xunit;

public class AccountNumberGeneratorTests
{
    [Fact]
    public void Generate_ShouldReturnValidFormat()
    {
        // Arrange
        var generator = new AccountNumberGenerator();
        string todayPrefix = $"ACC-{DateTime.UtcNow:yyyyMMdd}-";

        // Act
        string NumeroCuenta = generator.Generate();

        // Assert
        Assert.StartsWith(todayPrefix, NumeroCuenta);
        Assert.Equal(19, NumeroCuenta.Length); // ACC- (4) + YYYYMMDD (8) + - (1) + XXXX (4) = 17?
    }
}