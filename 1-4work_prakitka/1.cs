using System;
using System.Text;
public class Program
{
    public static void Main()
    {
        var calculator = new CompoundInterestCalculator();
        var result = calculator.CalculateCompoundInterest(1000, 3, 10);
        Console.WriteLine(result);
    }
}
public class CompoundInterestCalculator
{
    public string CalculateCompoundInterest(double initialDeposit, int years, double interestRate)
    {
        var result = new StringBuilder();
        var currentAmount = initialDeposit;

        for (var year = 1; year <= years; year++)
        {
            currentAmount *= (1 + interestRate / 100);
            result.AppendLine($"Год {year}: {currentAmount:F2} руб.");
        }
        return result.ToString();
    }
}