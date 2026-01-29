namespace FactorialIterative
{
    public static class Math
    {
        public static int Factorial(int n)
        {
            int result = 1;
            for (int i = 2; i <= n; i++)
            {
                result = result * i;
            }
            return result;
        }
    }
}
