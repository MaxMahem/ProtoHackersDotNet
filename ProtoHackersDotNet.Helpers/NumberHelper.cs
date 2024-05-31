namespace ProtoHackersDotNet.Servers.Helpers;

public static class NumberHelper
{
    public static bool IsPrime(this double number)
    {
        if (number <= 1 || !double.IsInteger(number) || !double.IsNormal(number))
            return false;
        if (number == 2 || number == 3 || number == 5)
            return true;
        if (number % 2 == 0 || number % 3 == 0 || number % 5 == 0)
            return false;

        var boundary = double.Floor(double.Sqrt(number));

        // You can do less work by observing that at this point, all primes 
        // other than 2 and 3 leave a remainder of either 1 or 5 when divided by 6. 
        // The other possible remainders have been taken care of.
        int i = 6; // start from 6, since others below have been handled.
        while (i <= boundary) {
            if (number % (i + 1) == 0 || number % (i + 5) == 0)
                return false;

            i += 6;
        }

        return true;
    }
}