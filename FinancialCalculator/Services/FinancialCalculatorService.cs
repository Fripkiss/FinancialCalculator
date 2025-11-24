using System;
using System.Collections.Generic;

namespace FinancialCalculator.Services
{
    public class FinancialCalculatorService : IFinancialCalculator
    {
        private readonly Dictionary<string, decimal> _exchangeRates;

        public FinancialCalculatorService()
        {
            _exchangeRates = new Dictionary<string, decimal>
            {
                {"USD_RUB", 90.0m},
                {"EUR_RUB", 98.5m},
                {"EUR_USD", 1.09m},
                {"RUB_USD", 1/90.0m},
                {"RUB_EUR", 1/98.5m},
                {"USD_EUR", 1/1.09m}
            };
        }

        public (decimal monthlyPayment, decimal totalPayment, decimal overpayment) CalculateCredit(
            decimal amount, int months, decimal annualRate)
        {
            if (amount <= 0) throw new ArgumentException("Сумма кредита должна быть положительной");
            if (months <= 0) throw new ArgumentException("Срок кредита должен быть положительным");
            if (annualRate < 0) throw new ArgumentException("Процентная ставка не может быть отрицательной");

            decimal monthlyRate = annualRate / 12 / 100;
            decimal coefficient = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months) /
                                ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1);

            decimal monthlyPayment = amount * coefficient;
            decimal totalPayment = monthlyPayment * months;
            decimal overpayment = totalPayment - amount;

            return (Math.Round(monthlyPayment, 2), Math.Round(totalPayment, 2), Math.Round(overpayment, 2));
        }

        public decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (amount <= 0) throw new ArgumentException("Сумма должна быть положительной");

            fromCurrency = fromCurrency.ToUpper();
            toCurrency = toCurrency.ToUpper();

            if (fromCurrency == toCurrency)
                return amount;

            string key = $"{fromCurrency}_{toCurrency}";

            if (_exchangeRates.ContainsKey(key))
            {
                return Math.Round(amount * _exchangeRates[key], 2);
            }

            string reverseKey = $"{toCurrency}_{fromCurrency}";
            if (_exchangeRates.ContainsKey(reverseKey))
            {
                return Math.Round(amount / _exchangeRates[reverseKey], 2);
            }

            if (fromCurrency != "RUB" && toCurrency != "RUB")
            {
                decimal toRub = ConvertCurrency(amount, fromCurrency, "RUB");
                return ConvertCurrency(toRub, "RUB", toCurrency);
            }

            throw new ArgumentException($"Курс для валют {fromCurrency} -> {toCurrency} не найден");
        }

        public (decimal income, decimal totalAmount) CalculateDeposit(
            decimal amount, int months, decimal annualRate, bool withCapitalization)
        {
            if (amount <= 0) throw new ArgumentException("Сумма вклада должна быть положительной");
            if (months <= 0) throw new ArgumentException("Срок вклада должен быть положительным");
            if (annualRate < 0) throw new ArgumentException("Процентная ставка не может быть отрицательной");

            decimal income, totalAmount;

            if (!withCapitalization)
            {
                income = amount * annualRate * months / 12 / 100;
                totalAmount = amount + income;
            }
            else
            {
                decimal monthlyRate = annualRate / 12 / 100;
                totalAmount = amount * (decimal)Math.Pow((double)(1 + monthlyRate), months);
                income = totalAmount - amount;
            }

            return (Math.Round(income, 2), Math.Round(totalAmount, 2));
        }
    }
}