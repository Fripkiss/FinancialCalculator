namespace FinancialCalculator.Services
{
    public interface IFinancialCalculator
    {
        (decimal monthlyPayment, decimal totalPayment, decimal overpayment) CalculateCredit(
            decimal amount, int months, decimal annualRate);

        decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency);

        (decimal income, decimal totalAmount) CalculateDeposit(
            decimal amount, int months, decimal annualRate, bool withCapitalization);
    }
}