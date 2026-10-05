using Module14.Tasks;

namespace Module14.Tests;

// Задача 14.1 (см. exercises.md). Два теста — ОБРАЗЦЫ: структура AAA и имя Method_Condition_Expected.
// Допиши свои тесты ниже (Theory + MemberData, граничные значения, исключения).
// В PriceCalculator есть баг — твои тесты должны его найти.
public class PriceCalculatorTests
{
    [Fact]
    public void CalculateTotal_SingleLine_ReturnsQuantityTimesPrice()
    {
        // Arrange — подготовка данных
        var lines = new[] { new OrderLine("BOOK-1", 2, 150m) };

        // Act — одно действие, которое проверяем
        var total = PriceCalculator.CalculateTotal(lines);

        // Assert — проверка результата
        Assert.Equal(300m, total);
    }

    [Fact]
    public void CalculateTotal_EmptyLines_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PriceCalculator.CalculateTotal([]));
    }

    // TODO: твои тесты.
}
