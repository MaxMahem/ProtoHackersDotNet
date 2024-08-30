using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProtoHackersDotNet.Servers.Helpers;

public static class NumberHelper
{
    public static bool IsPrime(this double number)
    {

        if (number <= 1 || !double.IsInteger(number) || !double.IsNormal(number))
            return false;
        if (number is 2 or 3 or 5)
            return true;
        if (number % 2 == 0 || number % 3 == 0 || number % 5 == 0)
            return false;

        var maxFactor = double.Floor(double.Sqrt(number));

        // At this point, all remaining primes primes must fit the formula 6k ± 1.
        // Since only a prime can be a factor, test those factors.
        for (int factor = 6; factor <= maxFactor; factor += 6)
            if (number % (factor - 1) == 0 || number % (factor + 1) == 0)
                return false;

        return true;
    }
}