using Orders.Services;

namespace Orders.Tests;

public class DiscountCalculatorTests
{
    private readonly DiscountCalculator _sut = new();

    // 8. decimal нельзя передать в атрибут, поэтому значения передаются как double
    [Theory]
    [InlineData(0, 0)]
    [InlineData(999.99, 0)]
    [InlineData(1000, 100)]
    [InlineData(2000, 200)]
    public void Calculate_ReturnsExpected(double total, double expected)
    {
        // Act
        var discount = _sut.Calculate((decimal)total);

        // Assert
        Assert.Equal((decimal)expected, discount);
    }

    // 9
    [Fact]
    public void Calculate_NegativeTotal_Throws()
    {
        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Calculate(-1m));
    }
}
