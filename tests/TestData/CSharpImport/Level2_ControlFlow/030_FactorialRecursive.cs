namespace FactorialRecursive
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
    }
}
