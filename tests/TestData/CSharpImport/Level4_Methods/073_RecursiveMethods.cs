namespace RecursiveMethods
{
    public static class Math
    {
        public static int Factorial(int n)
        {
            if (n <= 1)
            {
                return 1;
            }
            return n * Factorial(n - 1);
        }

        public static int Gcd(int a, int b)
        {
            if (b == 0)
            {
                return a;
            }
            return Gcd(b, a % b);
        }

        public static int Power(int baseNum, int exp)
        {
            if (exp == 0)
            {
                return 1;
            }
            return baseNum * Power(baseNum, exp - 1);
        }
    }
}
