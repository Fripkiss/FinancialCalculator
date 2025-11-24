using System;
using Moq;
using FinancialCalculator.Services;
using Xunit;

namespace FinancialCalculator.Tests
{
    public class FinancialCalculatorTests
    {
        private readonly FinancialCalculatorService _calculator;

        public FinancialCalculatorTests()
        {
            _calculator = new FinancialCalculatorService();
        }

        // 1. Тест с атрибутом [Fact] - базовый тест кредитного калькулятора
        [Fact]
        public void CalculateCredit_WithValidData_ReturnsCorrectResults()
        {
            // Arrange
            decimal amount = 100000m;
            int months = 12;
            decimal rate = 10m;

            // Act
            var result = _calculator.CalculateCredit(amount, months, rate);

            // Assert
            Assert.Equal(8791.59m, result.monthlyPayment);
            Assert.Equal(105499.08m, result.totalPayment);
            Assert.Equal(5499.08m, result.overpayment);
        }

        // 2. Тест с атрибутом [Theory] и [InlineData] - параметризованный тест конвертера валют
        [Theory]
        [InlineData(100, "USD", "RUB", 9000.00)]
        [InlineData(100, "EUR", "USD", 109.00)]
        [InlineData(9850, "RUB", "EUR", 100.00)]
        [InlineData(100, "USD", "USD", 100.00)]
        public void ConvertCurrency_WithDifferentCurrencies_ReturnsCorrectResults(
            decimal amount, string from, string to, decimal expected)
        {
            // Act
            var result = _calculator.ConvertCurrency(amount, from, to);

            // Assert
            Assert.Equal(expected, result);
        }

        // 3. Тест с Mock и Verify - проверка вызова методов
        [Fact]
        public void CurrencyConverter_CallsServiceMethod_WithCorrectParameters()
        {
            // Arrange
            var mockCalculator = new Mock<IFinancialCalculator>();
            mockCalculator.Setup(x => x.ConvertCurrency(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(9000m);

            decimal testAmount = 100m;
            string testFrom = "USD";
            string testTo = "RUB";

            // Act
            var result = mockCalculator.Object.ConvertCurrency(testAmount, testFrom, testTo);

            // Assert
            mockCalculator.Verify(x => x.ConvertCurrency(testAmount, testFrom, testTo), Times.Once);
            Assert.Equal(9000m, result);
        }

        // 4. Тест обработки исключений с атрибутом [Fact]
        [Fact]
        public void CalculateCredit_WithNegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            decimal amount = -1000m;
            int months = 12;
            decimal rate = 10m;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _calculator.CalculateCredit(amount, months, rate));

            Assert.Contains("положительной", exception.Message);
        }

        // 5. Тест калькулятора вкладов без капитализации
        [Fact]
        public void CalculateDeposit_WithoutCapitalization_ReturnsCorrectResults()
        {
            // Arrange
            decimal amount = 100000m;
            int months = 12;
            decimal rate = 8m;
            bool withCapitalization = false;

            // Act
            var result = _calculator.CalculateDeposit(amount, months, rate, withCapitalization);

            // Assert
            Assert.Equal(8000.00m, result.income);
            Assert.Equal(108000.00m, result.totalAmount);
        }

        // 6. Дополнительный тест с [Theory] для граничных значений
        [Theory]
        [InlineData(1000, 1, 0.1)]  // минимальные значения
        [InlineData(10000000, 360, 99.9)]  // максимальные значения
        public void CalculateCredit_WithBoundaryValues_ReturnsValidResults(
            decimal amount, int months, decimal rate)
        {
            // Act
            var result = _calculator.CalculateCredit(amount, months, rate);

            // Assert
            Assert.True(result.monthlyPayment > 0);
            Assert.True(result.totalPayment >= amount);
            Assert.True(result.overpayment >= 0);
        }
    }
}