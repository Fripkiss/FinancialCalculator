using System;
using System.Collections.Generic;
using System.Globalization;

namespace FinancialCalculator
{
    class Program
    {
        private static readonly Dictionary<string, decimal> exchangeRates = new Dictionary<string, decimal>
            {
               {"USD_RUB", 90.0m},
               {"EUR_RUB", 98.5m},
               {"EUR_USD", 1.9m},
               {"RUB_USD", 1/90.0m},
               {"RUB_EUR", 1/98.5m},
               {"USD_EUR", 1/1.09m}
            };

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (true)
            {
                ShowMainMenu();
                var choice = GetInput("Выберите опцию: ");

                switch (choice)
                {
                    case "1":
                        CreditCalculator();
                        break;
                    case "2":
                        CurrencyConverter();
                        break;
                    case "3":
                        DepositCalculator();
                        break;
                    case "4":
                        Console.WriteLine("Выход из программы...");
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("ФИНАНСОВЫЙ КАЛЬКУЛЯТОР");
            Console.WriteLine("=======================");
            Console.WriteLine("1. Расчет кредита");
            Console.WriteLine("2. Конвертер валют");
            Console.WriteLine("3. Калькулятор вкладов");
            Console.WriteLine("4. Выход");
        }

        static void CreditCalculator()
        {
            Console.WriteLine("\n=== РАСЧЕТ КРЕДИТА ===");

            try
            {
                decimal amount = GetValidatedDecimal("Сумма кредита (руб): ", 1000, 10000000);
                int months = GetValidatedInt("Срок кредита (месяцев): ", 1, 360);
                decimal rate = GetValidatedDecimal("Процентная ставка (% годовых): ", 0.1m, 99.9m);

                decimal monthlyRate = rate / 12 / 100;
                decimal coefficient = monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), months) /
                                    ((decimal)Math.Pow((double)(1 + monthlyRate), months) - 1);

                decimal monthlyPayment = amount * coefficient;
                decimal totalPayment = monthlyPayment * months;
                decimal overpayment = totalPayment - amount;

                Console.WriteLine("\n=== РЕЗУЛЬТАТЫ РАСЧЕТА ===");
                Console.WriteLine($"Ежемесячный платеж: {monthlyPayment:F2} руб");
                Console.WriteLine($"Общая сумма выплат: {totalPayment:F2} руб");
                Console.WriteLine($"Переплата по кредиту: {overpayment:F2} руб");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static void CurrencyConverter()
        {
            Console.WriteLine("\n=== КОНВЕРТЕР ВАЛЮТ ===");
            Console.WriteLine("Доступные валюты: RUB, USD, EUR");

            try
            {
                string fromCurrency = GetValidatedCurrency("Исходная валюта: ");
                string toCurrency = GetValidatedCurrency("Целевая валюта: ");
                decimal amount = GetValidatedDecimal("Сумма для конвертации: ", 0.01m, 1000000m);

                if (fromCurrency == toCurrency)
                {
                    Console.WriteLine($"Результат: {amount:F2} {toCurrency}");
                    return;
                }

                decimal result = ConvertCurrency(amount, fromCurrency, toCurrency);
                Console.WriteLine($"Результат: {result:F2} {toCurrency}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static void DepositCalculator()
        {
            Console.WriteLine("\n=== КАЛЬКУЛЯТОР ВКЛАДОВ ===");

            try
            {
                decimal amount = GetValidatedDecimal("Сумма вклада (руб): ", 1000, 10000000);
                int months = GetValidatedInt("Срок вклада (месяцев): ", 1, 60);
                decimal rate = GetValidatedDecimal("Процентная ставка (% годовых): ", 0.1m, 99.9m);

                string depositType = GetDepositType();

                decimal income = 0;
                decimal totalAmount = 0;

                if (depositType == "1") 
                {
                    income = amount * rate * months / 12 / 100;
                    totalAmount = amount + income;
                }
                else 
                {
                    decimal monthlyRate = rate / 12 / 100;
                    totalAmount = amount * (decimal)Math.Pow((double)(1 + monthlyRate), months);
                    income = totalAmount - amount;
                }

                Console.WriteLine("\n=== РЕЗУЛЬТАТЫ РАСЧЕТА ===");
                Console.WriteLine($"Доход по вкладу: {income:F2} руб");
                Console.WriteLine($"Итоговая сумма: {totalAmount:F2} руб");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        static decimal ConvertCurrency(decimal amount, string from, string to)
        {
            string key = $"{from}_{to}";

            if (exchangeRates.ContainsKey(key))
            {
                return amount * exchangeRates[key];
            }

            string reverseKey = $"{to}_{from}";
            if (exchangeRates.ContainsKey(reverseKey))
            {
                return amount / exchangeRates[reverseKey];
            }

            if (from != "RUB" && to != "RUB")
            {
                decimal toRub = ConvertCurrency(amount, from, "RUB");
                return ConvertCurrency(toRub, "RUB", to);
            }

            throw new ArgumentException("Курс для указанных валют не найден");
        }

        static string GetInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine().Trim().ToUpper();
        }

        static decimal GetValidatedDecimal(string prompt, decimal min, decimal max)
        {
            while (true)
            {
                try
                {
                    Console.Write(prompt);
                    string input = Console.ReadLine().Trim();

                    if (string.IsNullOrEmpty(input))
                        throw new ArgumentException("Значение не может быть пустым");

                    if (decimal.TryParse(input.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                    {
                        if (result < min)
                            throw new ArgumentException($"Значение не может быть меньше {min}");
                        if (result > max)
                            throw new ArgumentException($"Значение не может быть больше {max}");

                        return result;
                    }
                    else
                    {
                        throw new ArgumentException("Неверный формат числа");
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.WriteLine("Попробуйте снова.");
                }
            }
        }

        static int GetValidatedInt(string prompt, int min, int max)
        {
            while (true)
            {
                try
                {
                    Console.Write(prompt);
                    string input = Console.ReadLine().Trim();

                    if (string.IsNullOrEmpty(input))
                        throw new ArgumentException("Значение не может быть пустым");

                    if (int.TryParse(input, out int result))
                    {
                        if (result < min)
                            throw new ArgumentException($"Значение не может быть меньше {min}");
                        if (result > max)
                            throw new ArgumentException($"Значение не может быть больше {max}");

                        return result;
                    }
                    else
                    {
                        throw new ArgumentException("Неверный формат числа");
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.WriteLine("Попробуйте снова.");
                }
            }
        }

        static string GetValidatedCurrency(string prompt)
        {
            string[] validCurrencies = { "RUB", "USD", "EUR" };

            while (true)
            {
                Console.Write(prompt);
                string currency = Console.ReadLine().Trim().ToUpper();

                if (Array.Exists(validCurrencies, c => c == currency))
                {
                    return currency;
                }
                else
                {
                    Console.WriteLine("Неверная валюта. Допустимые значения: RUB, USD, EUR");
                }
            }
        }

        static string GetDepositType()
        {
            while (true)
            {
                Console.WriteLine("Тип вклада:");
                Console.WriteLine("1. Без капитализации");
                Console.WriteLine("2. С капитализацией");
                Console.Write("Выберите тип: ");

                string choice = Console.ReadLine().Trim();

                if (choice == "1" || choice == "2")
                {
                    return choice;
                }
                else
                {
                    Console.WriteLine("Неверный выбор. Введите 1 или 2.");
                }
            }
        }
    }
}